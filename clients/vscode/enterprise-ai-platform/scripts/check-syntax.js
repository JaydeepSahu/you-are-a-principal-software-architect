'use strict';

const { spawnSync } = require('node:child_process');
const { readdirSync, statSync } = require('node:fs');
const { join } = require('node:path');

const root = join(__dirname, '..');
const files = collectJavaScriptFiles(root)
  .filter(file => !file.includes(`${join('node_modules')}`));

let hasFailure = false;
for (const file of files) {
  const result = spawnSync(process.execPath, ['--check', file], {
    encoding: 'utf8'
  });

  if (result.status !== 0) {
    hasFailure = true;
    process.stderr.write(result.stderr);
  }
}

if (hasFailure) {
  process.exitCode = 1;
}

function collectJavaScriptFiles(directory) {
  const files = [];
  for (const entry of readdirSync(directory)) {
    const fullPath = join(directory, entry);
    const stats = statSync(fullPath);
    if (stats.isDirectory()) {
      files.push(...collectJavaScriptFiles(fullPath));
      continue;
    }

    if (entry.endsWith('.js')) {
      files.push(fullPath);
    }
  }

  return files;
}
