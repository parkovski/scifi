console.log('input in the form of .-.-');
console.log('output described by MorseCode.cs');
process.stdin.resume();
process.stdin.setEncoding('utf8');

process.stdin.on('data', function(text) {
  text = text.trim();
  if (text == '') {
    process.exit();
  }
  var value = 0;
  var len = text.length;
  for (var i = 0; i < len; i++) {
    if (text[i] == '-') {
      value |= (1 << i);
    }
  }

  len = len.toString(16);
  value = value.toString(16);
  console.log('0x' + len + value);
});