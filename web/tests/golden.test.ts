import { createHash } from 'node:crypto';
import { describe, expect, it } from 'vitest';
import golden from '../../testdata/lut-golden.json';
import { buildLookupTable, createWindow } from '../src/windowing';
import { PRESETS } from '../src/presets';

/**
 * Regression tests shared with the C# library: both must produce lookup tables with the
 * hashes recorded in testdata/lut-golden.json, so the web viewer and the CLI render
 * identical images.
 */
describe('golden lookup tables', () => {
  it('covers every preset', () => {
    expect(Object.keys(golden).sort()).toEqual(Object.keys(PRESETS).sort());
  });

  it.each(Object.entries(golden))('%s matches the recorded hash', (_, entry) => {
    const table = buildLookupTable(createWindow(entry.center, entry.width));
    expect(createHash('sha256').update(table).digest('hex')).toBe(entry.sha256);
  });
});
