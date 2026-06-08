const ffmpegPath = require('ffmpeg-static');
const ffmpeg = require('fluent-ffmpeg');
const path = require('path');
ffmpeg.setFfmpegPath(ffmpegPath);

const inPath = path.resolve(__dirname, '..', 'public', 'games', 'dragon-palace', 'gvehimpa.wav');
const outPath = path.resolve(__dirname, '..', 'public', 'games', 'dragon-palace', 'gvehimpa_converted.wav');

console.log('Converting', inPath, '->', outPath);

ffmpeg(inPath)
  .outputOptions(['-acodec pcm_s16le', '-ac 1', '-ar 44100'])
  .on('end', ()=>{
    console.log('Conversion finished');
  })
  .on('error', (err)=>{
    console.error('Conversion error', err.message);
    process.exit(1);
  })
  .save(outPath);
