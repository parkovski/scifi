// http://answers.unity3d.com/questions/521984/how-do-you-draw-2d-circles-and-primitives.html

Shader "Custom/Circle" {
    Properties {
        _Color ("Color", Color) = (1,0,0,0)
        _Thickness("Thickness", Range(0.0,0.5)) = 0.05
        _Radius("Radius", Range(0.0, 0.5)) = 0.4
        _Dropoff("Dropoff", Range(0.01, 4)) = 0.1
        _MainTex("Base (RGB)", 2D) = "white" { }
    }
    SubShader {
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
            AlphaToMask On
            Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
            LOD 200
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            
            fixed4 _Color; // low precision type is usually enough for colors
            float _Thickness;
            float _Radius;
            float _Dropoff;
            
            struct fragmentInput {
                float4 pos : SV_POSITION;
                float2 uv : TEXTCOORD0;
            };

            fragmentInput vert (appdata_base v)
            {
                fragmentInput o;

                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                o.uv = v.texcoord.xy - fixed2(0.5,0.5);

                return o;
            }

            // r = radius
            // d = distance
            // t = thickness
            // p = % thickness used for dropoff
            float antialias(float r, float d, float p) {
                if (d > r)
                    return 1 - pow(d - r, 2) / pow(p, 2);
                else
                    return 1.0;
            }
            
            fixed4 frag(fragmentInput i) : SV_Target {
                float distance = sqrt(pow(i.uv.x, 2) + pow(i.uv.y,2));
                float scale = _Radius * 3;
                float r = _Color.r * distance / scale;
                float g = _Color.g * distance / scale;
                float b = _Color.b * distance / scale;
                float a = _Color.a * antialias(_Radius, distance, _Dropoff);
                return fixed4(r, g, b, a);

                //return fixed4(_Color.r, _Color.g, _Color.b, _Color.a*antialias(_Radius, distance, _Thickness, _Dropoff));
            }
            
            
            ENDCG
        }
    }
}