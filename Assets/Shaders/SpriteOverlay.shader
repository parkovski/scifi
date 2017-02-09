Shader "SciFi/Effects/SpriteOverlay" {
    Properties {
        _Color("Color", Color) = (0, 0, 0, 1)
        _MainTex("Base (RGB), Alpha (A)", 2D) = "white" { }
    }
    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            LOD 100
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            half4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v) {
                v2f o;

                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy;

                return o;
            }

            half4 frag(v2f i) : SV_Target {
                float4 c = tex2D(_MainTex, i.uv);
                float inverseAlpha = 1 - _Color.a;
                c.r = _Color.r * _Color.a + c.r * inverseAlpha;
                c.g = _Color.g * _Color.a + c.g * inverseAlpha;
                c.b = _Color.b * _Color.a + c.b * inverseAlpha;

                return c;
            }

            ENDCG
        }
    }
}