import { describe, expect, it } from 'vitest';
import { AIR, generatePhantom, valueAt } from '../src/phantom';

describe('head phantom', () => {
  it.each([
    [0.95, 0.95, -1000, 'air'],
    [0, 0.9, 1000, 'skull'],
    [0, -0.3, 40, 'brain'],
    [0.22, 0, 0, 'ventricle'],
    [0, 0.35, 60, 'dense structure'],
  ])('returns %s,%s -> %s HU (%s)', (x, y, expected) => {
    expect(valueAt(x, y)).toBe(expected);
  });

  it('generates a square image', () => {
    const pixels = generatePhantom(64);
    expect(pixels).toHaveLength(64 * 64);
    expect(pixels[0]).toBe(AIR);
  });

  it.each([0, 1, 2.5])('rejects size %s', (size) => {
    expect(() => generatePhantom(size)).toThrow(RangeError);
  });
});
