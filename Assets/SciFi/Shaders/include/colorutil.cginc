#ifndef SCIFI_COLORUTIL_CGINC
#define SCIFI_COLORUTIL_CGINC

half3 rgb2hsv(half3 rgb) {
    half3 hsv;
    half cmax = max(rgb.x, max(rgb.y, rgb.z));
    half cmin = min(rgb.x, min(rgb.y, rgb.z));
    half delta = cmax - cmin;

    hsv.z = cmax;
    if (cmax <= .00001) {
        hsv.x = hsv.y = 0;
        return hsv;
    }

    if (cmax > 0) {
        hsv.y = delta / cmax;
    } else {
        hsv.x = hsv.y = 0;
        return hsv;
    }

    if (rgb.x >= cmax) {
        // yellow - magenta
        hsv.x = (rgb.y - rgb.z) / delta;
    } else if (rgb.y >= cmax) {
        // cyan - yellow
        hsv.x = 2 + (rgb.z - rgb.x) / delta;
    } else {
        hsv.x = 4 + (rgb.x - rgb.y) / delta;
    }

    hsv.x *= 60; // [0-360]

    if (hsv.x < 0) {
        hsv.x += 360;
    }

    return hsv;
}

half3 hsv2rgb(half3 hsv) {
    half hh, p, q, t, ff;
    half3 rgb;

    if (hsv.y <= 0) {
        return half3(hsv.z, hsv.z, hsv.z);
    }

    hh = hsv.x;
    if (hh >= 360) {
        hh -= 360;
    } else if (hh < 0) {
        hh += 360;
    }
    ff = frac(hh * .0166667);

    p = hsv.z * (1 - hsv.y);
    q = hsv.z * (1 - hsv.y * ff);
    t = hsv.z * (1 - hsv.y * (1 - ff));

    if (hh < 60) {
        return half3(hsv.z, t, p);
    } else if (hh < 120) {
        return half3(q, hsv.z, p);
    } else if (hh < 180) {
        return half3(p, hsv.z, t);
    } else if (hh < 240) {
        return half3(p, q, hsv.z);
    } else if (hh < 300) {
        return half3(t, p, hsv.z);
    } else {
        return half3(hsv.z, p, q);
    }
}

#endif /* SCIFI_COLORUTIL_CGINC */
