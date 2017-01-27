const fs = require('fs');
const yaml = require('js-yaml');

function cutUnityTag(text) {
    let match = /(%.+\n)(%.+\n.+\n)/.exec(text);
    let yamlTag = match[1];
    let unityTag = match[2];
    return {
        doc: yamlTag + '---\n' + text.substr(match[0].length),
        tag: yamlTag + unityTag,
    };
}

function pasteUnityTag(text, tag) {
    text = tag + text;
    // this janky yaml parser quotes the letter y
    text = text.replace(/\'y\'/gm, 'y');
    return text;
}

/// Returns {tag:string, doc:object}
function load(filename) {
    let file = cutUnityTag(fs.readFileSync(filename, 'utf-8'));
    file.doc = yaml.load(file.doc);
    return file;
}

/// file is the same as returned from load.
function save(filename, file) {
    fs.writeFileSync(filename, pasteUnityTag(yaml.dump(file.doc), file.tag), { encoding: 'utf-8' });
}

module.exports = {
    load: load,
    save: save,
};