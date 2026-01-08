Shader "Custom/ReplaceWhiteColor"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _ReplaceColor("Replace Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Name "ForwardLit"
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _ReplaceColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);


                // 如果接近白色，就替换颜色（可调节阈值）
                return half4(_ReplaceColor.rgb, _ReplaceColor.a * texColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Unlit/Transparent"
}
