// 외곽선 (B) — 후처리 엣지 검출(URP 17 내장 Full Screen Pass Renderer Feature 용, P04 S3 시험).
// 깊이와 노멀을 Roberts cross(대각선 두 쌍)로 비교해 물체 외곽(깊이 차)과 안쪽 꺾임(노멀 차)에 선을 긋는다.
// 기능의 Requirements 에 Depth · Normal 이 있어야 _CameraDepthTexture · _CameraNormalsTexture 가 생긴다.
// 선 색은 그 자리 화면 색에 _EdgeColor 를 곱한 것 — 팔레트 색을 어둡게(외피 방식과 같은 규칙).
Shader "Defense/ToonEdgeDetect"
{
    Properties
    {
        _EdgeWidth("Edge Width (per mille of screen height)", Range(0.5, 6)) = 1.5
        _DepthThreshold("Depth Threshold (relative)", Range(0.001, 0.2)) = 0.03
        _NormalThreshold("Normal Threshold", Range(0.01, 2)) = 0.6
        _EdgeColor("Edge Color (multiplies screen)", Color) = (0.32, 0.30, 0.42, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "ToonEdgeDetect"

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment EdgeFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            float _EdgeWidth;
            float _DepthThreshold;
            float _NormalThreshold;
            half4 _EdgeColor;

            half4 EdgeFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float2 uv = input.texcoord;
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, uv);

                // 화면 높이 기준 두께 — 해상도가 바뀌어도 같은 화면 비율
                float2 texel = float2(_ScreenParams.y / _ScreenParams.x, 1.0) * (_EdgeWidth * 0.001 * 0.5);
                float2 uv0 = uv - texel;
                float2 uv1 = uv + texel;
                float2 uv2 = uv + float2(texel.x, -texel.y);
                float2 uv3 = uv + float2(-texel.x, texel.y);

                float depth0 = LinearEyeDepth(SampleSceneDepth(uv0), _ZBufferParams);
                float depth1 = LinearEyeDepth(SampleSceneDepth(uv1), _ZBufferParams);
                float depth2 = LinearEyeDepth(SampleSceneDepth(uv2), _ZBufferParams);
                float depth3 = LinearEyeDepth(SampleSceneDepth(uv3), _ZBufferParams);
                // 상대 차 — 먼 곳일수록 같은 깊이 차가 작은 간격이라 가까운 깊이로 나눈다
                float depthEdge = sqrt(pow(depth1 - depth0, 2) + pow(depth3 - depth2, 2)) / min(min(depth0, depth1), min(depth2, depth3));

                float3 normal0 = SampleSceneNormals(uv0);
                float3 normal1 = SampleSceneNormals(uv1);
                float3 normal2 = SampleSceneNormals(uv2);
                float3 normal3 = SampleSceneNormals(uv3);
                float normalEdge = sqrt(dot(normal1 - normal0, normal1 - normal0) + dot(normal3 - normal2, normal3 - normal2));

                half edge = (half)max(step(_DepthThreshold, depthEdge), step(_NormalThreshold, normalEdge));
                return half4(lerp(color.rgb, color.rgb * _EdgeColor.rgb, edge), color.a);
            }
            ENDHLSL
        }
    }
}
