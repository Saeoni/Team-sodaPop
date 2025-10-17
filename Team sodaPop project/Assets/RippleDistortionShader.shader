Shader "Custom/RippleDistortionShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0.07, 0.0, 0.1, 1) // Dark violet
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _RippleCenter("Ripple Center", Vector) = (0.5, 0.5, 0, 0)
        _RippleStrength("Ripple Strength", Float) = 0.05
        _RippleFrequency("Ripple Frequency", Float) = 20
        _RippleSpeed("Ripple Speed", Float) = 2
        _RippleFade("Ripple Fade", Float) = 1

        _GlowColor("Glow Color", Color) = (0.78, 0.0, 1.0, 1) // Electric violet
        _GlowIntensity("Glow Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float4 _RippleCenter;
                float _RippleStrength;
                float _RippleFrequency;
                float _RippleSpeed;
                float _RippleFade;
                half4 _GlowColor;
                float _GlowIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // Ripple distortion
                float2 rippleUV = uv - _RippleCenter.xy;
                float dist = length(rippleUV);
                float ripple = sin(dist * _RippleFrequency - _Time.y * _RippleSpeed);
                uv += normalize(rippleUV) * ripple * _RippleStrength;
                uv = clamp(uv, 0.0, 1.0); // Prevent UV overflow

                // Sample base texture
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

                // Distance-based fade
                float fade = saturate(1.0 - dist * _RippleFade);

                // Base color blend
                half3 baseTint = _BaseColor.rgb * fade;
                half alpha = _BaseColor.a * fade;

                // Glow blend with softened ramp
                half3 glow = _GlowColor.rgb * _GlowIntensity * pow(fade, 1.5);

                // Final color
                half3 finalRGB = texColor.rgb * baseTint + glow;
                return half4(finalRGB, alpha);
            }
            ENDHLSL
        }
    }
}

