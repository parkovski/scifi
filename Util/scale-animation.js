const unityYaml = require('./lib/unity-yaml');

if (!process.argv[2]) {
    console.error("usage: node scale-animation.js [input] [xyz=scale]...");
} else {
    // do some black magic
    let moveAnimationScript = require('fs').readFileSync('./move-animation.js', 'utf-8');

    let lines = [
        {
            old: 'points[j].value[axis] += deltas[axis];',
            new: 'points[j].value[axis] *= deltas[axis];',
        },
        {
            old: 'points[j].value += deltas[curveAxis];',
            new: 'points[j].value *= deltas[curveAxis];',
        }
    ];

    lines.forEach(line => {
        if (moveAnimationScript.indexOf(line.old) === -1) {
            throw 'line not found!';
        }
        moveAnimationScript = moveAnimationScript.replace(line.old, line.new);
    });

    eval(moveAnimationScript);
}