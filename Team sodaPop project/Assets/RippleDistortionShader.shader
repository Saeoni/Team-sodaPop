Shader "Custom/RippleDistortionShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
        _RippleCenter("Ripple Center", Vector) = (0.5, 0.5, 0, 0)
        _RippleStrength("Ripple Strength", Float) = 0.05
        _RippleFrequency("Ripple Frequency", Float) = 20
        _RippleSpeed("Ripple Speed", Float) = 2
        _RippleFade("Ripple Fade", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
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

    // Calculate distance from ripple center
    float2 rippleUV = uv - _RippleCenter.xy;
    float dist = length(rippleUV);

    // Animate ripple
    float ripple = sin(dist * _RippleFrequency - _Time.y * _RippleSpeed);

    // Apply distortion
    uv += normalize(rippleUV) * ripple * _RippleStrength;

    // Sample texture
    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;

    // Fade out based on distance
    float fade = saturate(1.0 - dist * _RippleFade);

    // Final color = texture * base color * fade
    half4 finalColor = color * _BaseColor;
    finalColor.rgb *= fade;
    finalColor.a *= fade;

    
    return finalColor;
}

            ENDHLSL
        }
    }
}
