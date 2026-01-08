Shader "Custom/FlashToColor_Premultiplied"
{
    Properties{
        _MainTex("Sprite", 2D) = "white" {}
        _Flash("Flash", Range(0,1)) = 0
        _FlashColor("Flash Color", Color) = (1,1,1,1) // 默认白色
    }
    SubShader{
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off
        ZWrite Off
        // Use premultiplied alpha blending
        Blend One OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Flash;
            fixed4 _FlashColor; // 新增

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; fixed4 color : COLOR; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; fixed4 color : COLOR; };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 tex = tex2D(_MainTex, i.uv);        // 原贴图
                // 考虑顶点/Renderer tint
                fixed3 tintRGB = i.color.rgb;
                float tintA = i.color.a;

                // 实际像素（未 premultiply）
                fixed3 srcRGB = tex.rgb * tintRGB;
                float srcA = tex.a * tintA;

                // premultiplied 源与目标（指定颜色）
                fixed3 premSrc = srcRGB * srcA;
                fixed3 premFlash = _FlashColor.rgb * srcA; // 这里替换成指定颜色

                // 在 premultiplied 空间做插值
                fixed3 outPrem = lerp(premSrc, premFlash, _Flash);

                // 输出 premultiplied 颜色和原 alpha
                return fixed4(outPrem, srcA);
            }
            ENDCG
        }
    }
}
