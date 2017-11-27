// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// http://answers.unity3d.com/questions/521984/how-do-you-draw-2d-circles-and-primitives.html

Shader "SciFi/UI/ItemPicker" {
    Properties {
        _Radius ("Radius", Range(.3, 1)) = 1
        _Mode ("Mode", Int) = 0
        _MainTex("Base (RGB), Alpha (A)", 2D) = "white" { }
    }
    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha // Alpha blending
            Cull Off
            ZWrite On
            LOD 1
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "include/colorutil.cginc"
            #include "include/constants.cginc"

            fixed4 _SegColors[3];
            float _Radius;
            int _Mode;
            sampler2D _MainTex;

            static const float RADIUS_SMALL = 0.30;
            static const float INV_EDGE_PERCENT = 30;
            static const float NON_EDGE_PERCENT = 1 - 1 / INV_EDGE_PERCENT;
            static const float INNER_MARGIN_PERCENT = 0.4;
            static const float INNER_RADIUS_PERCENT = 0.166667;
            static const float INNER_EDGE_PERCENT_START = 0.95;

            static const float MODE_SMALL = 0;
            static const float MODE_EXPAND = 1;
            static const float MODE_SEG1_SELECTED = 2;
            static const float MODE_SEG2_SELECTED = 3;
            static const float MODE_SEG3_SELECTED = 4;

            struct fragmentInput {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            fragmentInput vert(appdata_base v) {
                fragmentInput o;

                o.pos = UnityObjectToClipPos(v.vertex);
                // (0, 0) in upper right corner
                // (1, 1) in bottom left.
                o.uv = float2(1, 1) - v.texcoord.xy;

                return o;
            }

            // The forumla here is derived from d = sqrt(x^2 + y^2)
            // where x is parallel to the line through the center of the segment.
            // This line is called 'h', the margin of the inner circle is 'm'.
            float inner_circle_distance(
                float inner_radius,
                float distance,
                float angle_center,
                float angle
            )
            {
                float m = inner_radius * INNER_MARGIN_PERCENT;
                float h = _Radius - inner_radius - m;
                float hsq = h * h;
                float dsq = distance * distance;
                float n1 = hsq + dsq;
                float n2 = 2 * h * distance * cos(angle_center - angle);
                if (n2 > n1) {
                    return 0;
                }
                return sqrt(n1 - n2);
            }

            fixed4 blend(fixed4 rgb1, fixed4 rgb2, half pct) {
                half pct1 = 1 - pct;
                return (fixed4)(((half4)rgb1) * pct1 + ((half4)rgb2) * pct);
            }

            static const int SEG_BACKGROUND = 0;
            static const int SEG_INNER_CIRCLE = 1;
            static const int SEG_INNER_CIRCLE_EDGE = 2;
            static const int SEG_SELECTED = 0x100;
            fixed4 modify_segment_color(fixed4 color, int part) {
                if (part == SEG_BACKGROUND) {
                    return color;
                }
                bool selected = part & SEG_SELECTED;
                part &= ~SEG_SELECTED;
                half3 hsv = rgb2hsv(color.rgb);
                if (selected) {
                    switch (part) {
                    case SEG_BACKGROUND:
                        hsv.z *= 2;
                        break;
                    case SEG_INNER_CIRCLE:
                        hsv.y = .20;
                        hsv.z = .95;
                        break;
                    case SEG_INNER_CIRCLE_EDGE:
                        hsv.y *= .75;
                        hsv.z *= 3;
                        break;
                    }
                } else {
                    switch (part) {
                    case SEG_INNER_CIRCLE:
                        hsv.y = .35;
                        hsv.z = .65;
                        break;
                    case SEG_INNER_CIRCLE_EDGE:
                        hsv.y *= .5;
                        hsv.z *= 2;
                        break;
                    }
                }
                return fixed4(hsv2rgb(hsv), color.a);
            }

            // first segment selected: mode |= SEG_SELECTED
            // second segment selected: mode |= SEG_SELECTED << 1
            fixed4 blend_segments(int seg1, int mode, half pct) {
                int mode1 = mode & 0xF | (mode & 0x100);
                int mode2 = mode & 0xF | ((mode & 0x200) >> 1);
                fixed4 c1 = modify_segment_color(_SegColors[seg1], mode1);
                fixed4 c2 = modify_segment_color(_SegColors[seg1 + 1], mode2);
                return blend(c1, c2, pct);
            }

            static const float SEG_BLEND_ANGLE = PI / 96;
            static const float INV_HALF_SEG_BLEND_ANGLE = 48 / PI;
            fixed4 segment(float distance, float angle, bool inner_circles) {
                float inner_radius = _Radius * INNER_RADIUS_PERCENT;

                float angle_center;
                if (angle > THIRD_PI) {
                    angle_center = 2.5 * SIXTH_PI;
                } else if (angle > SIXTH_PI) {
                    angle_center = 1.5 * SIXTH_PI;
                } else {
                    angle_center = .5 * SIXTH_PI;
                }

                int segment_mode;
                if (inner_circles) {
                    float dist_inner = inner_circle_distance(
                        inner_radius,
                        distance,
                        angle_center,
                        angle
                    );

                    if (dist_inner <= inner_radius) {
                        if (dist_inner >= INNER_EDGE_PERCENT_START * inner_radius) {
                            segment_mode = SEG_INNER_CIRCLE_EDGE;
                        } else {
                            segment_mode = SEG_INNER_CIRCLE;
                        }
                    } else {
                        segment_mode = SEG_BACKGROUND;
                    }
                } else {
                    segment_mode = SEG_BACKGROUND;
                }

                fixed4 color;
                int selectionflag = SEG_SELECTED << (int)(_Mode - MODE_SEG1_SELECTED);
                if (angle > THIRD_PI + SEG_BLEND_ANGLE) {
                    // Seg 3
                    segment_mode |= (selectionflag >> 2) & SEG_SELECTED;
                    color = modify_segment_color(_SegColors[2], segment_mode);
                } else if (angle > THIRD_PI - SEG_BLEND_ANGLE) {
                    // Seg 2 -> Seg 3
                    segment_mode |= (selectionflag >> 1) & (SEG_SELECTED | (SEG_SELECTED << 1));
                    half pct = (angle - THIRD_PI) * INV_HALF_SEG_BLEND_ANGLE + .5;
                    color = blend_segments(1, segment_mode, pct);
                } else if (angle > SIXTH_PI + SEG_BLEND_ANGLE) {
                    // Seg 2
                    segment_mode |= (selectionflag >> 1) & SEG_SELECTED;
                    color = modify_segment_color(_SegColors[1], segment_mode);
                } else if (angle > SIXTH_PI - SEG_BLEND_ANGLE) {
                    // Seg 1 -> Seg 2
                    segment_mode |= selectionflag & (SEG_SELECTED | (SEG_SELECTED << 1));
                    half pct = (angle - SIXTH_PI) * INV_HALF_SEG_BLEND_ANGLE + .5;
                    color = blend_segments(0, segment_mode, pct);
                } else {
                    // Seg 1
                    segment_mode |= selectionflag & SEG_SELECTED;
                    color = modify_segment_color(_SegColors[0], segment_mode);
                }

                return color;
            }

            // half for hsv calcs
            half parabolic(half base, half peak, half percent) {
                // y = 1 - 4(x-.5)^2
                percent -= .5;
                half amt = 1 - 4 * percent * percent;
                return base + (peak - base) * amt;
            }

            fixed4 draw_control(float distance, float angle, bool inner_circles) {
                if (distance > _Radius) return fixed4(0, 0, 0, 0);
                fixed4 color = segment(distance, angle, inner_circles);
                float edge_start = NON_EDGE_PERCENT * _Radius;
                if (distance > edge_start) {
                    half3 hsv = rgb2hsv(color.rgb);
                    hsv.z = parabolic(hsv.z, .85, (distance - edge_start) * INV_EDGE_PERCENT);
                    color.rgb = hsv2rgb(hsv);
                }
                return color;
            }

            fixed4 frag(fragmentInput i) : SV_Target {
                float distance = sqrt(i.uv.x * i.uv.x + i.uv.y * i.uv.y);
                float angle;
                if (i.uv.x == 0 && i.uv.y == 0) {
                    angle = QUARTER_PI;
                } else {
                    angle = atan2(i.uv.y, i.uv.x);
                }
                return draw_control(distance, angle, _Mode != MODE_SMALL);
            }

            ENDCG
        }
    }
}