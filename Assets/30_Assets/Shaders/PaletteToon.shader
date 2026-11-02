// 팔레트 텍스처 한 장을 쓰는 카툰 셰이더(URP 17) — PaletteCel 을 바탕으로 실제 게임이 쓰는 기법을 토글로 더한 시험판(P04 S3).
// 기본 모양은 PaletteCel 과 같다: 주 조명 N·L 을 2~3단 램프로 끊어 어두운 단은 그림자 색조 · 앰비언트(SH) · SSAO · 안개.
// 더한 것:
//   _SHADOW_BANDS — 받는 그림자 감쇠를 N·L 과 따로 한 단으로 끊는다(부드러운 그림자 가장자리가 중간 단 고리를 만들지 않게).
//   _ShadowSaturation — 어두운 단 색의 채도를 올린다(0 이면 PaletteCel 과 같다).
//   Outline 패스 — 뒤집은 외피(Cull Front, 법선 방향 확장, 화면 높이 기준 두께). LightMode SRPDefaultUnlit 이라
//     Material.SetShaderPassEnabled("SRPDefaultUnlit", false) 로 끈다(키워드 · 변형 없음).
// 받지 않는 것: 추가 조명(등불 점광) · 라이트맵 · 라이트 레이어(PaletteCel 과 같다).
Shader "Defense/PaletteToon"
{
    Properties
    {
        [MainTexture] _BaseMap("Palette", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _ShadowTint("Shadow Tint", Color) = (0.58, 0.62, 0.78, 1)
        _LowStep("Low Step (N·L)", Range(0, 1)) = 0.05
        _HighStep("High Step (N·L)", Range(0, 1)) = 0.5
        _MidLevel("Mid Band Level (1 = two tone)", Range(0, 1)) = 0.6
        _StepSoftness("Step Softness", Range(0.001, 0.2)) = 0.02
        _AmbientStrength("Ambient Strength", Range(0, 1)) = 0.35

        [Header(Shadow)]
        [Toggle(_SHADOW_BANDS)] _ShadowBands("Quantize Received Shadow", Float) = 1
        _ShadowStep("Received Shadow Step", Range(0.01, 0.99)) = 0.5
        _ShadowSaturation("Dark Band Saturation Boost", Range(0, 1)) = 0.3

        [Header(Outline)]
        _OutlineWidth("Outline Width (per mille of screen height)", Range(0, 8)) = 2
        _OutlineColor("Outline Color (multiplies palette)", Color) = (0.32, 0.30, 0.42, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        // SRP Batcher — 모든 패스가 같은 머티리얼 버퍼를 본다
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _ShadowTint;
            half _LowStep;
            half _HighStep;
            half _MidLevel;
            half _StepSoftness;
            half _AmbientStrength;
            half _ShadowStep;
            half _ShadowSaturation;
            half _OutlineWidth;
            half4 _OutlineColor;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        ENDHLSL

        Pass
        {
            Name "ForwardToon"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment
            #pragma shader_feature_local_fragment _SHADOW_BANDS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half fogFactor : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings ToonVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(position.positionCS.z);
                return output;
            }

            // 휘도는 그대로 두고 채도만 (1 + boost) 배 — 어두운 단이 회색으로 죽지 않게
            half3 BoostSaturation(half3 color, half boost)
            {
                half luma = dot(color, half3(0.299h, 0.587h, 0.114h));
                return max(lerp(luma.xxx, color, 1.0h + boost), 0.0h);
            }

            half4 ToonFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 normalWS = normalize(input.normalWS);

                // 위치 · 그림자 마스크를 넘기는 판이 그림자 거리 끝의 페이드를 한다(URP Lit 과 같은 호출)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS), input.positionWS, half4(1.0h, 1.0h, 1.0h, 1.0h));
                half nDotL = saturate(dot(normalWS, mainLight.direction));
                #if defined(_SHADOW_BANDS)
                    // 그림자는 따로 한 단 — 그림자 안은 어두운 단, 밖은 N·L 단 그대로
                    half lightAmount = nDotL;
                    half shadowBand = smoothstep(_ShadowStep - _StepSoftness, _ShadowStep + _StepSoftness, mainLight.shadowAttenuation);
                #else
                    half lightAmount = nDotL * mainLight.shadowAttenuation;
                    half shadowBand = 1.0h;
                #endif
                half low = smoothstep(_LowStep - _StepSoftness, _LowStep + _StepSoftness, lightAmount);
                half high = smoothstep(_HighStep - _StepSoftness, _HighStep + _StepSoftness, lightAmount);
                // 어두운 단 0 · 중간 단 _MidLevel · 밝은 단 1 — _MidLevel 이 1 이면 두 단
                half ramp = (low * _MidLevel + high * (1.0h - _MidLevel)) * shadowBand;

                // _ShadowSaturation 0 이면 albedo * lerp(_ShadowTint, 1, ramp) — PaletteCel 과 같은 값
                half3 dark = BoostSaturation(albedo * _ShadowTint.rgb, _ShadowSaturation);
                half3 direct = lerp(dark, albedo, ramp) * mainLight.color;
                half3 ambient = albedo * SampleSH(normalWS) * _AmbientStrength;
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor occlusion = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(input.positionCS));
                    direct *= occlusion.directAmbientOcclusion;
                    ambient *= occlusion.indirectAmbientOcclusion;
                #endif
                half3 color = MixFog(direct + ambient, input.fogFactor);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            // 외곽선 (A) — 뒤집은 외피. 두께는 화면 높이의 천분율이라 해상도 · 거리와 무관하게 같은 화면 폭
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half fogFactor : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings OutlineVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 normalCS = mul((float3x3)UNITY_MATRIX_VP, TransformObjectToWorldNormal(input.normalOS));
                // 클립 공간에서 법선의 화면 방향으로 민다 — w 를 곱해 원근 나눗셈 뒤에도 같은 NDC 폭, x 는 화면 비율로 보정
                float2 direction = normalize(normalCS.xy + 1e-5);
                float2 offset = direction * (_OutlineWidth * 0.002);
                offset.x *= _ScreenParams.y / _ScreenParams.x;
                positionCS.xy += offset * positionCS.w;
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.fogFactor = ComputeFogFactor(positionCS.z);
                return output;
            }

            half4 OutlineFragment(Varyings input) : SV_Target
            {
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                return half4(MixFog(albedo * _OutlineColor.rgb, input.fogFactor), 1.0h);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            // 렌더러의 SSAO(Source = Depth Normals) · 외곽선 (B) 엣지 검출이 이 패스로 _CameraNormalsTexture 를 만든다
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
