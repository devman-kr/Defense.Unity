// 블롭 그림자(P04 S3 시험) — 캐릭터 발밑에 눕힌 쿼드에 둥근 그라데이션을 곱한다. 실시간 그림자 맵의 대안(모바일 캐주얼).
// 곱하기 블렌드라 바닥 색을 _BlobColor 쪽으로 어둡게 할 뿐 바닥 셰이딩을 바꾸지 않는다. 깊이는 쓰지 않는다.
Shader "Defense/BlobShadow"
{
    Properties
    {
        _BlobColor("Blob Color (multiplies ground)", Color) = (0.58, 0.62, 0.78, 1)
        _Opacity("Opacity", Range(0, 1)) = 0.8
        _Softness("Edge Softness (fraction of radius)", Range(0.01, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent-100" }
        Blend DstColor Zero
        ZWrite Off
        Cull Off

        Pass
        {
            Name "BlobShadow"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex BlobVertex
            #pragma fragment BlobFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BlobColor;
                half _Opacity;
                half _Softness;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings BlobVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 BlobFragment(Varyings input) : SV_Target
            {
                // 쿼드 가운데 0 · 가장자리 1 — 원 밖은 1(바닥 그대로)
                half radius = length(input.uv - 0.5h) * 2.0h;
                half blob = (1.0h - smoothstep(1.0h - _Softness, 1.0h, radius)) * _Opacity;
                return half4(lerp(half3(1.0h, 1.0h, 1.0h), _BlobColor.rgb, blob), 1.0h);
            }
            ENDHLSL
        }
    }
}
