Shader "Custom/CeilingShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Transparency("Transparency", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Transparent" }
            LOD 200

            CGPROGRAM
            #pragma surface surf Standard alpha:fade

            sampler2D _MainTex;
            half _Transparency;

            struct Input
            {
                float2 uv_MainTex;
                float3 viewDir;
            };

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                half alpha = 1.0;

                // ƒJƒƒ‰‚ÌY²•ûŒü‚ğŠî€‚É“§–¾“x‚ğİ’è
                if (IN.viewDir.y > 0) // ƒJƒƒ‰‚ªã‚©‚çŒ©‚Ä‚¢‚éê‡
                {
                    alpha = _Transparency;
                }

                fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * alpha;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
            ENDCG
        }
            FallBack "Diffuse"
}