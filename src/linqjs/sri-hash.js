const fs = require('fs');
const crypto = require('crypto');

const filePath = process.argv[2];
if (!filePath) {
  console.error("Kullanım: node sri-hash.js <dosya yolu>");
  process.exit(1);
}

const buffer = fs.readFileSync(filePath);
const hash = crypto.createHash('sha384').update(buffer).digest('base64');

console.log(`\n✅ SRI Hash:\n`);
console.log(`integrity="sha384-${hash}"`);