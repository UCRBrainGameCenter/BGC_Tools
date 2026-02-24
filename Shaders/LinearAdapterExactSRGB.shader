Shader "BGCTools/LinearAdapterExactSRGB"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Mode ("Mode (0=Exact_sRGB_LinearToSRGB, 1=Approx_PowInvGamma, 2=PassThrough)", Float) = 0.0
        _GammaExponent ("Approx Inv-Gamma Exponent", Float) = 2.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
        ZWrite Off ZTest Always Cull Off Lighting Off Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Mode;
            float _GammaExponent;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            // Exact sRGB forward OETF: linear -> sRGB
            float3 LinearToSRGB_Exact(float3 c)
            {
                c = max(c, 0.0);
                float3 lo = 12.92 * c;
                float3 hi = 1.055 * pow(c, 1.0/2.4) - 0.055;
                float3 cond = step(0.0031308, c); // 0 if c<0.0031308, 1 if c>=...
                // For c < 0.0031308 use lo; else hi.
                return lerp(lo, hi, cond);
            }

            float3 InvGammaApprox(float3 c, float exp)
            {
                // Brighten midtones using inverse gamma (approximate).
                return pow(saturate(c), 1.0/exp);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);

                if (_Mode < 0.5)
                {
                    // Exact sRGB forward (linear->sRGB) curve for brightening.
                    c.rgb = LinearToSRGB_Exact(c.rgb);
                    return c;
                }
                else if (_Mode < 1.5)
                {
                    // Approximate brighten using inverse gamma.
                    c.rgb = InvGammaApprox(c.rgb, _GammaExponent);
                    return c;
                }
                else
                {
                    return c; // Pass-through
                }
            }
            ENDCG
        }
    }
    FallBack Off
}