Shader "BGCTools/LinearAdapter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GammaExponent ("Gamma Exponent", Float) = 2.2
        _Mode ("Mode (0=EncodeLinearToGamma, 1=ApproxLinearizeGamma, 2=PassThrough)", Float) = 0.0
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
            float _GammaExponent;
            float _Mode;

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

            float3 GammaToLinearCustom(float3 c, float exp)
            {
                // Convert gamma-encoded color to linear using custom exponent.
                // Default exp=2.2. Unity's GammaToLinearSpace uses sRGB piecewise approximation;
                // here we expose exponent for tuning parity with legacy PLFest.
                return pow(max(c, 0.0), exp);
            }

            float3 LinearToGammaCustom(float3 c, float invExp)
            {
                // Convert linear to gamma using inverse exponent (1/2.2 by default).
                return pow(saturate(c), invExp);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 c = tex2D(_MainTex, i.uv);

                // Mode 0: Encode linear->gamma (use when the source is already linear)
                // Mode 1: Approximate "linear look" in a Gamma project by lifting midtones.
                //         We intentionally *do not* encode back, to mimic the brighter midtones users expect.
                // Mode 2: Pass-through
                if (_Mode < 0.5)
                {
                    // linear->gamma using inverse exponent
                    float invExp = 1.0 / _GammaExponent;
                    c.rgb = LinearToGammaCustom(c.rgb, invExp);
                    return c;
                }
                else if (_Mode < 1.5)
                {
                    // gamma->linear using exponent (and do NOT encode back)
                    c.rgb = GammaToLinearCustom(c.rgb, _GammaExponent);
                    return c;
                }
                else
                {
                    return c;
                }
            }
            ENDCG
        }
    }
    FallBack Off
}