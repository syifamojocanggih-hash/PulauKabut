Shader "PulauKabut/StylizedWaterfall"
{
    Properties
    {
        [Header(Colors)]
        _TopColor    ("Top Flow Color", Color)    = (0.48, 0.88, 0.95, 0.86)
        _BottomColor ("Bottom Flow Color", Color) = (0.30, 0.72, 0.84, 0.82)
        _FoamColor   ("Foam And Crest Color", Color) = (0.95, 0.99, 1.00, 0.96)
        _RimColor    ("Lantern Rim Tint", Color)  = (0.85, 0.97, 1.00, 1.0)

        [Header(Flow Streaks)]
        _FlowSpeed    ("Vertical Flow Speed", Float) = 2.6
        _StreakScaleU ("Horizontal Streak Scale", Float) = 1.6
        _StreakScaleV ("Vertical Streak Stretch", Float) = 0.28
        _StreakStrength ("Streak Brightness", Range(0.0, 1.5)) = 0.75

        [Header(Crest And Splash Foam)]
        _TopFoamHeight    ("Top Lip Foam Height (0..0.5)", Range(0.01, 0.35)) = 0.14
        _BottomFoamHeight ("Bottom Splash Height (0..0.5)", Range(0.01, 0.45)) = 0.22
        _SoftEdgeDepth    ("Depth Splash Blend", Range(0.05, 3.0)) = 1.15
        _EmissionIntensity ("Soft Glow Intensity", Range(0.0, 2.0)) = 0.42
        _FresnelPower     ("Edge Fresnel Power", Range(1.0, 6.0)) = 2.4
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "WaterfallForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _TopColor;
                half4 _BottomColor;
                half4 _FoamColor;
                half4 _RimColor;
                float _FlowSpeed;
                float _StreakScaleU;
                float _StreakScaleV;
                half _StreakStrength;
                float _TopFoamHeight;
                float _BottomFoamHeight;
                float _SoftEdgeDepth;
                half _EmissionIntensity;
                half _FresnelPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 positionOS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float  viewDepth  : TEXCOORD3;
                float  fogFactor  : TEXCOORD4;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            float ValueNoise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);

                // Subtle curtain sway near lower half of waterfall
                float lowerMask = saturate(0.5 - input.positionOS.y);
                posWS.x += sin(_Time.y * 2.4 + posWS.z * 1.4 + posWS.y * 0.5) * 0.045 * lowerMask;

                o.positionOS = input.positionOS.xyz;
                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.viewDepth = -TransformWorldToView(posWS).z;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                // Vertical coordinate along the cube height (-0.5 bottom .. +0.5 top -> 0 .. 1)
                float vNorm = saturate(input.positionOS.y + 0.5);
                // Horizontal coordinate in world space (works for any cube rotation/scale)
                float horizCoord = input.positionWS.x + input.positionWS.z;

                // Dual scrolling stylized waterfall streaks + bubbly foam veins
                float2 flowUV1 = float2(horizCoord * _StreakScaleU, input.positionWS.y * _StreakScaleV + _Time.y * _FlowSpeed);
                float2 flowUV2 = float2(horizCoord * (_StreakScaleU * 2.15) + 3.7, input.positionWS.y * (_StreakScaleV * 1.9) + _Time.y * (_FlowSpeed * 1.45));
                float2 flowUV3 = float2(horizCoord * (_StreakScaleU * 3.8) - 1.9, input.positionWS.y * (_StreakScaleV * 3.1) + _Time.y * (_FlowSpeed * 1.85));

                float n1 = ValueNoise2D(flowUV1);
                float n2 = ValueNoise2D(flowUV2);
                float n3 = ValueNoise2D(flowUV3);
                float streaks = smoothstep(0.50, 0.74, n1 * 0.60 + n2 * 0.40);
                float fineThreads = smoothstep(0.66, 0.84, n2 * 0.5 + n3 * 0.5);

                // Top crest & bottom churning splash foam
                float topDist = 1.0 - vNorm;
                float topFoam = smoothstep(_TopFoamHeight, 0.0, topDist + (n1 - 0.5) * 0.14);

                float splashNoise = ValueNoise2D(float2(horizCoord * 3.2, _Time.y * 3.6));
                float bottomFoam = smoothstep(_BottomFoamHeight, 0.0, vNorm + (splashNoise - 0.5) * 0.18);

                // Soft scene depth splash where waterfall enters water or touches rock
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthDiff = max(0.0, sceneEyeDepth - input.viewDepth);
                float depthSplash = (1.0 - saturate(depthDiff / max(0.05, _SoftEdgeDepth))) * (0.55 + 0.45 * splashNoise);

                // Combine richer water body + bright crisp stylized foam
                half4 bodyCol = lerp(_BottomColor, _TopColor, vNorm);
                half foamAmount = saturate(max(max(topFoam, bottomFoam), depthSplash * 0.78) + fineThreads * 0.52);

                half3 N = normalize(input.normalWS) * (facing >= 0 ? 1.0 : -1.0);
                float3 V = normalize(GetWorldSpaceViewDir(input.positionWS));
                half fresnel = pow(1.0 - abs(dot(N, V)), _FresnelPower);

                Light mainLight = GetMainLight();
                half ndotl = saturate(dot(N, normalize(mainLight.direction)) * 0.40 + 0.60);

                half3 col = bodyCol.rgb * ndotl;
                col = lerp(col, _FoamColor.rgb, streaks * _StreakStrength * 0.48);
                col += fresnel * _RimColor.rgb * 0.20;
                col = lerp(col, _FoamColor.rgb, foamAmount * _FoamColor.a);
                col += col * _EmissionIntensity * (0.25 + 0.75 * max(streaks * 0.5, foamAmount));

                // Soft alpha fade at very bottom splash edge so waterfall blends into water surface
                half bottomSoftAlpha = smoothstep(0.0, 0.06, vNorm);
                half alpha = saturate(max(bodyCol.a, foamAmount * 0.94)) * lerp(0.55, 1.0, bottomSoftAlpha);

                col = MixFog(col, input.fogFactor);
                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
