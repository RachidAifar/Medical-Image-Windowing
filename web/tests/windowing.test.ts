import { describe, expect, it } from 'vitest';
import { applyWindow, buildLookupTable, createWindow, lowerBound, toDisplay, upperBound } from '../src/windowing';
import { PRESETS } from '../src/presets';

const brain = PRESETS.brain; // C=40, W=80 -> visible range 0..79 HU

describe('createWindow', () => {
  it.each([0, 0.5, -10, Number.NaN])('rejects width %s', (width) => {
    expect(() => createWindow(40, width)).toThrow(RangeError);
  });

  it.each([Number.NaN, Number.POSITIVE_INFINITY])('rejects center %s', (center) => {
    expect(() => createWindow(center, 80)).toThrow(RangeError);
  });

  it('follows the DICOM bounds', () => {
    expect(lowerBound(brain)).toBe(0);
    expect(upperBound(brain)).toBe(79);
  });
});

describe('toDisplay', () => {
  it.each([
    [-1000, 0],
    [0, 0],
    [79, 255],
    [80, 255],
    [1000, 255],
  ])('maps %s HU to %s', (hu, expected) => {
    expect(toDisplay(hu, brain)).toBe(expected);
  });

  it('maps the center to mid gray', () => {
    expect(toDisplay(40, brain)).toBeGreaterThanOrEqual(127);
    expect(toDisplay(40, brain)).toBeLessThanOrEqual(129);
  });

  it('never decreases as the input increases', () => {
    let previous = 0;
    for (let hu = -200; hu <= 300; hu++) {
      const current = toDisplay(hu, brain);
      expect(current).toBeGreaterThanOrEqual(previous);
      previous = current;
    }
  });

  it('acts as a threshold when the width is 1', () => {
    const threshold = createWindow(100, 1);
    expect(toDisplay(99, threshold)).toBe(0);
    expect(toDisplay(100, threshold)).toBe(255);
  });
});

describe('lookup table', () => {
  it.each(Object.entries(PRESETS))('matches direct computation for every input (%s)', (_, window) => {
    const table = buildLookupTable(window);
    expect(table).toHaveLength(65536);
    for (let value = -32768; value <= 32767; value++) {
      if (table[value + 32768] !== toDisplay(value, window)) {
        throw new Error(`Mismatch at ${value}`);
      }
    }
  });

  it('applies the window to every pixel', () => {
    const pixels = Int16Array.from([-1000, 0, 40, 79, 1000]);
    expect(Array.from(applyWindow(pixels, brain))).toEqual(Array.from(pixels, (p) => toDisplay(p, brain)));
  });

  it('rejects an output buffer that is too small', () => {
    expect(() => applyWindow(new Int16Array(10), brain, new Uint8Array(9))).toThrow(RangeError);
  });
});
