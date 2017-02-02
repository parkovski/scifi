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
    for (let i = 1; i < anim.AnimationClip.m_PositionCurves.length; i++) {
        let points = anim.AnimationClip.m_PositionCurves[i].curve.m_Curve;
        let firstPoint = points[0];
        for (let j = 1; j < points.length; j++) {
            if (axis.x) {
                mirrorAxis(points[j], firstPoint, 'x');
            }
            if (axis.y) {
                mirrorAxis(points[j], firstPoint, 'y');
            }
            if (axis.z) {
                mirrorAxis(points[j], firstPoint, 'z');
            }
        }
    }

    for (let i = 0; i < anim.AnimationClip.m_EulerCurves.length; i++) {
        let points = anim.AnimationClip.m_EulerCurves[i].curve.m_Curve;
        // x is rotation around z,
        // y is rotation around x,
        // z is rotation around y
        for (let j = 0; j < points.length; j++) {
            if (axis.y) {
                points[j].value.x = -points[j].value.x;
            }
            if (axis.z) {
                points[j].value.y = -points[j].value.y;
            }
            if (axis.x) {
                points[j].value.z = -points[j].value.z;
            }
        }
    }

    let editorCurves = anim.AnimationClip.m_EditorCurves;
    for (let i = 0; i < editorCurves.length; i++) {
        if (/m_LocalPosition\.[xyz]/.test(editorCurves[i].attribute)) {
            let curveAxis = editorCurves[i].attribute;
            curveAxis = curveAxis[curveAxis.length - 1];
            if (axis[curveAxis]) {
                mirrorEditorCurves(editorCurves[i].curve.m_Curve);
            }
        } else if (/localEulerAnglesRaw\.[xyz]/.test(editorCurves[i].attribute)) {
            let curveAxis = editorCurves[i].attribute;
            curveAxis = curveAxis[curveAxis.length - 1];
            if (axis.y && curveAxis == 'x') {
                reverseEditorCurves(editorCurves[i].curve.m_Curve);
            }
            if (axis.z && curveAxis == 'y') {
                reverseEditorCurves(editorCurves[i].curve.m_Curve);
            }
            if (axis.x && curveAxis == 'z') {
                reverseEditorCurves(editorCurves[i].curve.m_Curve);
            }
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

function reverseEditorCurves(editorCurves) {
    for (let i = 1; i < editorCurves.length; i++) {
        editorCurves[i].value = -editorCurves[i].value;
        editorCurves[i].inSlope = -editorCurves[i].inSlope;
        editorCurves[i].outSlope = -editorCurves[i].outSlope;
    }
}