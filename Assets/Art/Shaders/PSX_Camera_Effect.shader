Shader "Hidden/PSX_Camera_Effect"
{
    Properties
    {
        // Vector stores (Width, Height, 0, 0)
        _Resolution ("Internal Resolution", Vector) = (320, 240, 0, 0)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            // Mandatory for Full Screen Pass in Unity 6
            ZWrite Off ZTest Always Blend Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // URP provides screen data via _BlitTexture 
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);
            float4 _Resolution;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // Pixelation logic: $uv_{new} = \frac{\lfloor uv \cdot Res \rfloor}{Res}$ [cite: 7, 8]
                uv.x = floor(uv.x * _Resolution.x) / _Resolution.x;
                uv.y = floor(uv.y * _Resolution.y) / _Resolution.y;

                // Sample using the automated URP buffer
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv);
                
                // PSX color depth reduction [cite: 9]
                color.rgb = floor(color.rgb * 16.0) / 16.0;
                
                return color;
            }
            ENDHLSL
        }
    }
}