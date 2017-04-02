const fetch = require('node-fetch');

const method = process.argv[2];
const url = process.argv[3];
if (!method || !url) {
    console.log('usage: node httprequest.js <method> <url>');
    process.exit(0);
}

fetch(url, { method }).then(res => res.text()).then(text => console.log(text));
