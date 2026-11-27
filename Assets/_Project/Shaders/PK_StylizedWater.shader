Shader "PulauKabut/StylizedWater"
{
    Properties
    {
        [Header(Colors)]
        _ShallowColor ("Shallow Color", Color) = (0.36, 0.78, 0.80, 0.72)
        _DeepColor    ("Deep Color", Color)    = (0.17, 0.44, 0.62, 0.94)
        _FoamColor    ("Foam Color", Color)    = (0.94, 0.98, 1.00, 0.92)
        _SkyTint      ("Fresnel Sky Tint", Color) = (0.72, 0.86, 0.96, 1.0)

        [Header(Depth And Refraction)]
        _DepthMaxDistance ("Depth Gradient Distance", Range(0.2, 20.0)) = 4.5
        _RefractionStrength ("Refraction Strength", Range(0.0, 0.05)) = 0.012
        _OpacityMin ("Min Shallow Alpha", Range(0.1, 1.0)) = 0.58

        [Header(Foam And Ripples)]
        _FoamDistance ("Shore Foam Width", Range(0.05, 4.0)) = 1.15
        _FoamCutoff   ("Foam Cutoff", Range(0.1, 0.95)) = 0.48
        _FoamSpeed    ("Foam Speed", Float) = 0.45
        _RippleScale  ("World Ripple Scale", Float) = 0.22
        _RippleSpeed  ("Ripple Speed", Float) = 0.28
        _RippleStrength ("Ripple Highlight", Range(0.0, 1.0)) = 0.34

        [Header(Waves And Lighting)]
        _WaveAmplitude ("Vertex Wave Height", Range(0.0, 0.25)) = 0.025
        _WaveFrequency ("Vertex Wave Frequency", Float) = 0.35
        _WaveSpeed     ("Vertex Wave Speed", Float) = 1.1
        _Smoothness    ("Sun Specular Tightness", Range(8.0, 256.0)) = 72.0
        _SpecularStrength ("Sun Specular Strength", Range(0.0, 2.0)) = 0.55
        _FresnelPower  ("Fresnel Power", Range(1.0, 8.0)) = 3.5
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
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ShallowColor;
                half4 _DeepColor;
                half4 _FoamColor;
                half4 _SkyTint;
                float _DepthMaxDistance;
                float _RefractionStrength;
                half _OpacityMin;
                float _FoamDistance;
                half _FoamCutoff;
                float _FoamSpeed;
                float _RippleScale;
                float _RippleSpeed;
                half _RippleStrength;
                float _WaveAmplitude;
                float _WaveFrequency;
                float _WaveSpeed;
                half _Smoothness;
                half _SpecularStrength;
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
                float3 normalWS   : TEXCOORD1;
                float  viewDepth  : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
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

            float RipplePattern(float2 xz, float time)
            {
                float2 uv1 = xz * _RippleScale + float2(time * _RippleSpeed, time * _RippleSpeed * 0.65);
                float2 uv2 = xz * (_RippleScale * 1.45) - float2(time * _RippleSpeed * 0.8, -time * _RippleSpeed * 0.5);

                float n1 = ValueNoise2D(uv1);
                float n2 = ValueNoise2D(uv2);
                float ridge = 1.0 - abs(n1 - n2) * 2.0;
                return saturate(pow(saturate(ridge), 3.2));
            }

            Varyings vert(Attributes input)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);

                if (_WaveAmplitude > 0.0001)
                {
                    float w1 = sin(posWS.x * _WaveFrequency + _Time.y * _WaveSpeed);
                    float w2 = cos(posWS.z * (_WaveFrequency * 0.85) - _Time.y * (_WaveSpeed * 0.9));
                    posWS.y += (w1 + w2) * 0.5 * _WaveAmplitude;
                }

                o.positionWS = posWS;
                o.positionCS = TransformWorldToHClip(posWS);
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.viewDepth = -TransformWorldToView(posWS).z;
                o.fogFactor = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = GetNormalizedScreenSpaceUV(input.positionCS);

                // Depth difference for shallow-to-deep gradient & shoreline foam
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float depthDiff = max(0.0, sceneEyeDepth - input.viewDepth);
                float depthT = saturate(depthDiff / max(0.05, _DepthMaxDistance));

                // Dual scrolling world-space ripples
                float ripple = RipplePattern(input.positionWS.xz, _Time.y);
                float rippleFine = RipplePattern(input.positionWS.xz * 1.9 + 11.3, _Time.y * 1.25);

                // Perturbed normal for cozy stylized lighting
                float2 grad = float2(
                    ValueNoise2D(input.positionWS.xz * (_RippleScale * 1.8) + _Time.y * 0.35) - 0.5,
                    ValueNoise2D(input.positionWS.xz * (_RippleScale * 1.8) - _Time.y * 0.29 + 7.1) - 0.5
                );
                float3 N = normalize(input.normalWS + float3(grad.x, 0.0, grad.y) * 0.22);
                float3 V = normalize(GetWorldSpaceViewDir(input.positionWS));

                // Base water gradient (uses hardware alpha blending instead of costly OpaqueTexture copy)
                half4 waterTint = lerp(_ShallowColor, _DeepColor, depthT);
                half waterAlpha = lerp(max(_OpacityMin, waterTint.a), _DeepColor.a, depthT);

                // Stylized ripple highlights
                half3 baseCol = waterTint.rgb;
                baseCol += (_RippleStrength * 0.28) * (ripple * 0.7 + rippleFine * 0.45);

                // Fresnel sky tint
                half fresnel = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
                baseCol = lerp(baseCol, _SkyTint.rgb, fresnel * 0.42);

                // Golden-hour directional light specular glints
                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);
                half ndotl = saturate(dot(N, L) * 0.5 + 0.5);
                half spec = pow(saturate(dot(N, H)), _Smoothness) * _SpecularStrength;
                baseCol *= lerp(0.82, 1.08, ndotl);
                baseCol += mainLight.color * (spec + ripple * spec * 0.8);

                // Animated stylized shoreline foam
                float foamNoise = ValueNoise2D(input.positionWS.xz * (_RippleScale * 3.1) + float2(_Time.y * _FoamSpeed, -_Time.y * _FoamSpeed * 0.7));
                float shoreMask = 1.0 - saturate(depthDiff / max(0.05, _FoamDistance));
                float foamBand = smoothstep(_FoamCutoff - 0.08, _FoamCutoff + 0.08, shoreMask + (foamNoise - 0.5) * 0.45);
                // Crisp secondary ripple ring right along the contact edge
                float contactRing = smoothstep(0.32, 0.02, depthDiff) * (0.65 + 0.35 * sin(depthDiff * 18.0 - _Time.y * 3.2));
                half foamFactor = saturate(max(foamBand, contactRing) * _FoamColor.a);

                half3 finalCol = lerp(baseCol, _FoamColor.rgb, foamFactor);
                half finalAlpha = saturate(max(waterAlpha, foamFactor * 0.95));

                finalCol = MixFog(finalCol, input.fogFactor);
                return half4(finalCol, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
