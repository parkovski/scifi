Shader "SciFi/Effects/Explosion" {
    Properties {
        _Radius("Radius", Range(0.0, 0.5)) = 0.5
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
            ZWrite On
            LOD 1
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float _Radius;

            struct v2f {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_base v) {
                v2f o;

                o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy - fixed2(0.5, 0.5);

                return o;
            }

            float4 blend(float4 c1, float4 c2) {
                return float4(
                    (c1.r * c1.a + c2.r * c2.a) / 1,
                    (c1.g * c1.a + c2.g * c2.a) / 1,
                    (c1.b * c1.a + c2.b * c2.a) / 1,
                    max(c1.a, c2.a)
                );
            }

            fixed4 circle(
                float2 pt,
                float2 center,
                float radius,
                float4 prevColor
            ) {
                float x = abs(center.x) - abs(pt.x);
                float y = abs(center.y) - abs(pt.y);
                float distance = sqrt(pow(x, 2) + pow(y, 2));
                float g = 1;
                float b = 1;
                float a = 1;
                float yellowRadius = radius * .4;
                float opaqueRadius = radius * .5;
                float scale = .5 / radius;
                if (distance > yellowRadius) {
                    g = 1 - (distance - yellowRadius) * scale * 3;
                    b = .2;
                } else {
                    b = .8 - distance * scale * 2;
                }

                if (distance > opaqueRadius) {
                    a = 1 - (distance - opaqueRadius) * scale * 4;
                }

                //return blend(fixed4(1, g, b, a), prevColor);
                return fixed4(1, g, b, a);
            }

            fixed4 frag(v2f IN) : SV_Target {
                float4 color;
                color = circle(IN.uv, float2(0, 0), .5, float4(0, 0, 0, 0));

                return color;
            }

            ENDCG
        }
    }
}