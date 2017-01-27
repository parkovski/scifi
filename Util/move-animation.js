const unityYaml = require('./lib/unity-yaml');

if (!process.argv[2]) {
    console.error("usage: node move-animation.js [input] [xyz=delta]...");
} else {
    const diffs = getDiffs(process.argv, 3);
    main(process.argv[2], diffs);
}

function getDiffs(array, first) {
    let i = first;
    let vals = { x: 0, y: 0, z: 0 };
    while (array[i]) {
        let match = /(.)=(.+)/.exec(array[i]);
        if (!match) {
            throw 'expected [xyz]=number, found' + array[i];
        }
        vals[match[1]] = +match[2];
        i++;
    }
    return vals;
}

function main(filename, deltas) {
    let file = unityYaml.load(filename);
    let anim = file.doc;
    for (let i = 0; i < anim.AnimationClip.m_PositionCurves.length; i++) {
        let points = anim.AnimationClip.m_PositionCurves[i].curve.m_Curve;
        for (let j = 0; j < points.length; j++) {
            Object.keys(deltas).forEach(axis => {
                points[j].value[axis] = +points[j].value[axis];
                points[j].value[axis] += deltas[axis];
            });
        }
    }

    let editorCurves = anim.AnimationClip.m_EditorCurves;
    for (let i = 0; i < editorCurves.length; i++) {
        if (!/m_LocalPosition\.[xyz]/.test(editorCurves[i].attribute)) {
            continue;
        }
        let curveAxis = editorCurves[i].attribute;
        curveAxis = curveAxis[curveAxis.length - 1];
        let points = editorCurves[i].curve.m_Curve;
        for (let j = 0; j < points.length; j++) {
            points[j].value = +points[j].value;
            points[j].value += deltas[curveAxis];
        }
    }
    unityYaml.save(filename, file);
}