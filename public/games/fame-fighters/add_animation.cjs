#!/usr/bin/env node
/**
 * Fame Fighters — Add Animation Script (Node.js version)
 * Process PNG frames and create base64-encoded sprite sheet for index.html
 */

const fs = require('fs');
const path = require('path');

// CONFIG
const FRAMES = [
  'don-kick-1.png',
  'don-kick-2.png',
  'don-kick-3.png',
  'don-kick-4.png',
  'don-kick-5.png',
  'don-kick-6.png',
];

const ASSET_NAME = 'DON_KICK_B64';
const SHEET_NAME = 'don_kick_sheet.png';

// Helper: read files as base64
function frameToBase64(filePath) {
  if (!fs.existsSync(filePath)) {
    console.error(`  ❌ File not found: ${filePath}`);
    return null;
  }
  return fs.readFileSync(filePath).toString('base64');
}

// Main
console.log(`\n🎮 Fame Fighters — Processing ${FRAMES.length} Don kick frames\n`);

// For now, just read the first frame and encode it
// A full recreation of the Python sprite sheet logic would require sharp or jimp
// Instead, we'll create JavaScript code to encode the frames individually

const frameB64s = [];
const missing = [];

for (const frameFile of FRAMES) {
  const framePath = path.join(__dirname, frameFile);
  const b64 = frameToBase64(framePath);
  if (b64) {
    frameB64s.push(b64);
    console.log(`  ✓ Encoded ${frameFile}`);
  } else {
    missing.push(frameFile);
  }
}

if (frameB64s.length === 0) {
  console.error('\n❌ ERROR: No frames found!\n');
  process.exit(1);
}

console.log(`\n✓ Successfully encoded ${frameB64s.length} frames`);
if (missing.length > 0) {
  console.log(`⚠️  WARNING: ${missing.length} frame(s) missing:`);
  missing.forEach(f => console.log(`   - ${f}`));
}

// Output the base64 array as JavaScript code to update index.html
console.log(`\n✓ To use in index.html, replace the donKickFramesArray with:`);
console.log(`\nconst donKickFramesArray = [`);
frameB64s.forEach((b64, i) => {
  console.log(`  "data:image/png;base64,${b64.substring(0,50)}...${b64.substring(b64.length-20)}",`);
});
console.log(`];\n`);

console.log(`📋 Detailed usage: Use donKickFramesArray in index.html's don kick frame loader`);
