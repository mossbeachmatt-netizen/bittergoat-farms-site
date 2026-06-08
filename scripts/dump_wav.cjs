const fs = require('fs');
const path = require('path');
const p = path.resolve(__dirname,'..','public','games','dragon-palace','gvehimpa.wav');
console.log('path',p);
const b = fs.readFileSync(p);
console.log('size', b.length);
console.log('first 64 bytes:', b.slice(0,64).toString('hex'));
