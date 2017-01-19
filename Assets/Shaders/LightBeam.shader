Shader "SciFi/Items/LightBeam" {
    Properties {
        _Color("Color", Color) = (1,0,0,0)
        _Width("Width", Range(0.0, 0.5)) = 0.2
        _Angle("Angle", Float) = 0
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

            fixed4 _Color;
            float _Width;
            float _Angle;

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

            fixed4 frag(v2f IN) : SV_Target {
                if (IN.uv.x < 0) {
                    return fixed4(0, 0, 0, 0);
                }
                if (IN.uv.y < .2 && IN.uv.y > -.2) {
                    fixed alpha;
                    alpha = _Color.a * (1 - abs(5 * IN.uv.y));
                    if (IN.uv.x > .4) {
                        alpha = min(alpha, (.5 - IN.uv.x) * 10);
                    }
                    return fixed4(_Color.r, _Color.g, _Color.b, alpha);
                }
                return fixed4(0, 0, 0, 0);
            }

            ENDCG
        }
    }
}