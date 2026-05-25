Shader "Custom/CloudsShader_EdgeFade_RandomPerObject"
{
    Properties
    {
        _CloudsColour ("Clouds Colour", Color) = (1,1,1,1)
        _CloudsScale ("Clouds Scale", Float) = 120
        _CloudsPower ("Clouds Power", Float) = 1.23
        _CloudsAlpha ("Clouds Alpha", Float) = 0.55
        _CloudsSpeed ("Clouds Speed", Vector) = (0.01,0.02,0,0)

        _DistortScale ("Distort Scale", Float) = 0
        _DistortSpeed ("Distort Speed", Vector) = (0.05,0.05,0,0)
        _VertexOffset ("Vertex Offset", Float) = 0.5

        _EdgeFadeWidth ("Edge Fade Width", Range(0.01,0.5)) = 0.28
        _EdgeFadePower ("Edge Fade Power", Range(0.25,5)) = 1.4
        _NoiseCutoff ("Noise Cutoff", Range(0,1)) = 0.18
        _NoiseSoftness ("Noise Softness", Range(0.01,1)) = 0.55

        _RandomOffsetStrength ("Random Offset Strength", Float) = 35
        _RandomScaleVariation ("Random Scale Variation", Range(0,1)) = 0.25
        _RandomSpeedVariation ("Random Speed Variation", Range(0,1)) = 0.35
        _RandomTimeOffset ("Random Time Offset", Float) = 50
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "CloudsEdgeFadeRandomPerObject"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _CloudsColour;
                float _CloudsScale;
                float _CloudsPower;
                float _CloudsAlpha;
                float4 _CloudsSpeed;
                float _DistortScale;
                float4 _DistortSpeed;
                float _VertexOffset;
                float _EdgeFadeWidth;
                float _EdgeFadePower;
                float _NoiseCutoff;
                float _NoiseSoftness;
                float _RandomOffsetStrength;
                float _RandomScaleVariation;
                float _RandomSpeedVariation;
                float _RandomTimeOffset;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 randomOffset : TEXCOORD3;
                float randomScale : TEXCOORD4;
                float randomSpeed : TEXCOORD5;
                float randomTime : TEXCOORD6;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float hash31(float3 p)
            {
                p = frac(p * float3(0.1031, 0.11369, 0.13787));
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            float2 hash32(float3 p)
            {
                float n1 = hash31(p);
                float n2 = hash31(p + float3(17.17, 31.31, 47.47));
                return float2(n1, n2);
            }

            float3 GetObjectWorldPosition()
            {
                return float3(UNITY_MATRIX_M[0][3], UNITY_MATRIX_M[1][3], UNITY_MATRIX_M[2][3]);
            }

            float valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 uv)
            {
                float n = 0.0;
                float amp = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    n += valueNoise(uv) * amp;
                    uv *= 2.03;
                    amp *= 0.5;
                }
                return saturate(n);
            }

            float edgeFade(float2 uv)
            {
                float2 edgeDistance = min(uv, 1.0 - uv);
                float boxFade = saturate(min(edgeDistance.x, edgeDistance.y) / max(_EdgeFadeWidth, 0.0001));
                boxFade = smoothstep(0.0, 1.0, boxFade);
                return pow(boxFade, _EdgeFadePower);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 objectWS = GetObjectWorldPosition();

                // These values change automatically for each object based on world position.
                OUT.randomOffset = (hash32(objectWS) - 0.5) * _RandomOffsetStrength;
                OUT.randomScale = 1.0 + ((hash31(objectWS + 5.13) - 0.5) * 2.0 * _RandomScaleVariation);
                OUT.randomSpeed = 1.0 + ((hash31(objectWS + 11.71) - 0.5) * 2.0 * _RandomSpeedVariation);
                OUT.randomTime = hash31(objectWS + 23.29) * _RandomTimeOffset;

                float2 animatedUV =
                    IN.uv * max(_CloudsScale * OUT.randomScale, 0.001)
                    + (_Time.y + OUT.randomTime) * _CloudsSpeed.xy * OUT.randomSpeed
                    + OUT.randomOffset;

                float displacement = (fbm(animatedUV) - 0.5) * _VertexOffset;

                float3 posOS = IN.positionOS.xyz + IN.normalOS * displacement;
                VertexPositionInputs posInputs = GetVertexPositionInputs(posOS);
                OUT.positionHCS = posInputs.positionCS;
                OUT.uv = IN.uv;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float cloudScale = max(_CloudsScale * IN.randomScale, 0.001);
                float timeValue = (_Time.y + IN.randomTime) * IN.randomSpeed;

                float2 distortUV =
                    IN.uv * cloudScale
                    + timeValue * _DistortSpeed.xy
                    + IN.randomOffset;

                float distortion = (fbm(distortUV) - 0.5) * _DistortScale;

                float2 cloudUV =
                    IN.uv * cloudScale
                    + timeValue * _CloudsSpeed.xy
                    + distortion
                    + IN.randomOffset;

                float cloudNoise = fbm(cloudUV);
                cloudNoise = pow(saturate(cloudNoise), max(_CloudsPower, 0.001));
                cloudNoise = smoothstep(_NoiseCutoff, _NoiseCutoff + _NoiseSoftness, cloudNoise);

                float fade = edgeFade(IN.uv);

                float fresnel = 1.0 - saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS)));
                float viewSoftness = saturate(1.0 - fresnel * 0.35);

                float alpha = cloudNoise * fade * _CloudsAlpha * _CloudsColour.a * viewSoftness;

                return half4(_CloudsColour.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
