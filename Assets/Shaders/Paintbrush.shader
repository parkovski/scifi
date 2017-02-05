Shader "SciFi/Attacks/Paintbrush" {
    Properties {
        _Color("Color", Color) = (1,0,0,0)
        _Width("Width", Range(0.0, 0.5)) = 0.2
        _Height("Height", Range(0.0, 0.5)) = 0.5
        _Peaks("Peaks", Int) = 5
        _PeakMin("PeakMin", Range(0.0, 1.0)) = 0.5
        _BumpDist("BumpDist", Float) = 0.1
        _StartTime("StartTime", Float) = 0.0
        _AnimLength("AnimLength", Float) = 10.0
        _MainTex("Base (RGB), Alpha(A)", 2D) = "white" { }
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

            static const float PI = 3.1415926;

            fixed4 _Color;
            float _Width;
            float _Height;
            int _Peaks;
            float _PeakMin;
            float _BumpDist;
            float _StartTime;
            float _AnimLength;
            
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

            fixed animate(float y, fixed alpha) {
                y += _Height;
                float cutoff = _Height * 2 * (1 - (_Time.y - _StartTime) / _AnimLength);
                if (y < cutoff) {
                    return 0;
                } else {
                    return alpha;
                }
            }

            fixed4 frag(v2f i) : SV_Target {
                if (abs(i.uv.x) > _Width) {
                    return fixed4(0, 0, 0, 0);
                }
                if (abs(i.uv.y) > _Height) {
                    return fixed4(0, 0, 0, 0);
                }

                float peak = (1 - _PeakMin) * cos(PI * _Peaks * i.uv.x / _Width) / 2 + 0.5 + _PeakMin;
                float bump = (1 - _PeakMin) * cos(PI * i.uv.y / (_Height * _BumpDist)) / 2 + 0.5 + _PeakMin;
                bump = bump / 1.01;

                return fixed4(
                    _Color.r * max(peak, bump),
                    _Color.g * max(peak, bump),
                    _Color.b * max(peak, bump),
                    animate(i.uv.y, max(peak - .5, (bump - .5) / 1.2))
                );
            }

            ENDCG
        }
    }
}