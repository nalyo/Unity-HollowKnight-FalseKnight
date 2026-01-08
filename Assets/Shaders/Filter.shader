Shader "Custom/BlueFilter_Premultiplied"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Saturation("Saturation", Range(0,2)) = 1.0
        _Contrast("Contrast", Range(0,2)) = 1.0
        _HueShift("Hue Shift", Range(-180,180)) = 0.0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "FullscreenFilter"
            ZWrite Off
            Cull Off
            Blend One OneMinusSrcAlpha   // Premultiplied alpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _Saturation;
            float _Contrast;
            float _HueShift;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            // 色相旋转
            float3 ShiftHue(float3 color, float hueDegrees)
            {
                float angle = radians(hueDegrees);
                float s = sin(angle);
                float c = cos(angle);

                float3x3 toYIQ = float3x3(
                    0.299,     0.587,     0.114,
                    0.595716, -0.274453, -0.321263,
                    0.211456, -0.522591,  0.311135
                );

                float3x3 toRGB = float3x3(
                    1.0,  0.9563,  0.6210,
                    1.0, -0.2721, -0.6474,
                    1.0, -1.1070,  1.7046
                );

                float3 yiq = mul(toYIQ, color);
                float2x2 rot = float2x2(c, -s, s, c);
                yiq.yz = mul(rot, yiq.yz);

                return mul(toRGB, yiq);
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // ---- 只在 alpha > 0 时进行处理 ----
                if (col.a > 0.0001)
                {
                    // 色相
                    col.rgb = ShiftHue(col.rgb, _HueShift);

                    // 饱和度
                    float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    col.rgb = lerp(gray.xxx, col.rgb, _Saturation);

                    // 对比度
                    col.rgb = (col.rgb - 0.5) * _Contrast + 0.5;
                }

                // ---- Premultiplied: RGB = RGB * A ----
                col.rgb *= col.a;

                return col;
            }

            ENDHLSL
        }
    }

    FallBack "Unlit/Transparent"
}
