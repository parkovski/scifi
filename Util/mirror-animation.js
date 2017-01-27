const unityYaml = require('./lib/unity-yaml');

const axis = process.argv[2];
const input = process.argv[3];
const output = process.argv[4];

if (!input || !output || !axis) {
    console.error("usage: node mirror-animation.js [axis] <input.yaml> <output.yaml>");
    console.error("axis is any combination of x, y, z");
} else {
    main(input, output, axis);
}

function main(input, output, axis) {
    let file = unityYaml.load(input);
    mirror(file.doc, getAxis(axis));
    unityYaml.save(output, file);
}

function getAxis(str) {
    let axis = { x: false, y: false, z: false };
    for (let i = 0; i < str.length; i++) {
        if (str[i] === 'x') {
            axis.x = true;
        } else if (str[i] === 'y') {
            axis.y = true;
        } else if (str[i] === 'z') {
            axis.z = true;
        }
    }

    return axis;
}

function mirror(anim, axis) {
    for (let h = 1; h < anim.AnimationClip.m_PositionCurves.length; h++) {
        let points = anim.AnimationClip.m_PositionCurves[h].curve.m_Curve;
        let firstPoint = points[0];
        for (let i = 1; i < points.length; i++) {
            if (axis.x) {
                mirrorAxis(points[i], firstPoint, 'x');
            }
            if (axis.y) {
                mirrorAxis(points[i], firstPoint, 'y');
            }
            if (axis.z) {
                mirrorAxis(points[i], firstPoint, 'z');
            }
        }
    }

    let editorCurves = anim.AnimationClip.m_EditorCurves;
    for (let i = 0; i < editorCurves.length; i++) {
        if (!/m_LocalPosition\.[xyz]/.test(editorCurves[i].attribute)) {
            continue;
        }
        let curveAxis = editorCurves[i].attribute;
        curveAxis = curveAxis[curveAxis.length - 1];
        if (axis[curveAxis]) {
            mirrorEditorCurves(editorCurves[i].curve.m_Curve);
        }
    }
}

function mirrorAxis(pt, lastPt, axis) {
    let delta = pt.value[axis] - lastPt.value[axis];
    pt.value[axis] = lastPt.value[axis] - delta;
    pt.inSlope[axis] = -pt.inSlope[axis];
    pt.outSlope[axis] = -pt.outSlope[axis];
}

function mirrorEditorCurves(editorCurves) {
    let firstCurve = editorCurves[0];
    for (let i = 1; i < editorCurves.length; i++) {
        let curve = editorCurves[i];
        let delta = curve.value - firstCurve.value;
        curve.value = firstCurve.value - delta;
        curve.inSlope = -curve.inSlope;
        curve.outSlope = -curve.outSlope;
    }
}