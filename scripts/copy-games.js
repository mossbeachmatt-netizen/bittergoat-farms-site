import { cpSync } from 'fs';
import { join } from 'path';

const src = join(process.cwd(), 'public', 'games');
const dest = join(process.cwd(), 'dist', 'games');

console.log('Copying games folder to dist...');
cpSync(src, dest, { recursive: true });
console.log('Done copying games!');
