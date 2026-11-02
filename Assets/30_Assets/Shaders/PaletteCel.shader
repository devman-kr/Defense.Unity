// 팔레트 텍스처 한 장을 쓰는 셀 셰이더(URP 17) — 주 조명 N·L 과 그림자 감쇠를 2~3단 램프로 끊어 팔레트 색에 곱한다.
// 받는 것: 주 조명 · 주 조명 그림자(거리 페이드 포함) · SSAO(렌더러 기능이 켜져 있을 때) · 앰비언트(SH) · 안개.
// 받지 않는 것: 추가 조명(등불 점광 — URP Lit 과 셰이딩 비교 때 이 차이가 남는다) · 라이트맵 · 라이트 레이어.
// 그림자 캐스터 · 깊이 · 깊이 노멀 패스는 URP 기본 패스를 include 한다(알파 테스트 없음).
Shader "Defense/PaletteCel"
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
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardCel"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex CelVertex
            #pragma fragment CelFragment
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

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

            Varyings CelVertex(Attributes input)
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

            half4 CelFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb * _BaseColor.rgb;
                half3 normalWS = normalize(input.normalWS);

                // 위치 · 그림자 마스크를 넘기는 판이 그림자 거리 끝의 페이드를 한다(URP Lit 과 같은 호출)
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS), input.positionWS, half4(1.0h, 1.0h, 1.0h, 1.0h));
                half lightAmount = saturate(dot(normalWS, mainLight.direction)) * mainLight.shadowAttenuation;
                half low = smoothstep(_LowStep - _StepSoftness, _LowStep + _StepSoftness, lightAmount);
                half high = smoothstep(_HighStep - _StepSoftness, _HighStep + _StepSoftness, lightAmount);
                // 어두운 단 0 · 중간 단 _MidLevel · 밝은 단 1 — _MidLevel 이 1 이면 두 단
                half ramp = low * _MidLevel + high * (1.0h - _MidLevel);

                half3 direct = lerp(_ShadowTint.rgb, half3(1.0h, 1.0h, 1.0h), ramp) * mainLight.color;
                half3 ambient = SampleSH(normalWS) * _AmbientStrength;
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    AmbientOcclusionFactor occlusion = GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(input.positionCS));
                    direct *= occlusion.directAmbientOcclusion;
                    ambient *= occlusion.indirectAmbientOcclusion;
                #endif
                half3 color = albedo * (direct + ambient);
                color = MixFog(color, input.fogFactor);
                return half4(color, 1.0h);
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
            // 렌더러의 SSAO 기능(Source = Depth Normals)이 이 패스를 쓴다
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
