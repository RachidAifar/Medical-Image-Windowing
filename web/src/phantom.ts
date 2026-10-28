/**
 * Synthetic CT head slice in Hounsfield units (Shepp-Logan geometry).
 * Same ellipses as src/ImageWindowing/HeadPhantom.cs, so no patient data is needed.
 */

export const AIR = -1000;

// x0, y0, semi-axis a, semi-axis b, rotation (degrees), HU added inside the ellipse.
const ELLIPSES: ReadonlyArray<readonly [number, number, number, number, number, number]> = [
  [0, 0, 0.69, 0.92, 0, 2000],
  [0, -0.0184, 0.6624, 0.874, 0, -960],
  [0.22, 0, 0.11, 0.31, -18, -40],
  [-0.22, 0, 0.16, 0.41, 18, -40],
  [0, 0.35, 0.21, 0.25, 0, 20],
  [0, 0.1, 0.046, 0.046, 0, 20],
  [0, -0.1, 0.046, 0.046, 0, 20],
  [-0.08, -0.605, 0.046, 0.023, 0, 20],
  [0, -0.606, 0.023, 0.023, 0, 20],
  [0.06, -0.605, 0.023, 0.046, 0, 20],
];

export function valueAt(x: number, y: number): number {
  let value = AIR;
  for (const [x0, y0, a, b, angle, hu] of ELLIPSES) {
    const radians = (angle * Math.PI) / 180;
    const cos = Math.cos(radians);
    const sin = Math.sin(radians);
    const dx = x - x0;
    const dy = y - y0;
    const u = (dx * cos + dy * sin) / a;
    const v = (-dx * sin + dy * cos) / b;
    if (u * u + v * v <= 1) {
      value += hu;
    }
  }
  return value;
}

export function generatePhantom(size: number): Int16Array {
  if (!Number.isInteger(size) || size < 2) {
    throw new RangeError(`Size must be an integer of at least 2, got ${size}.`);
  }
  const pixels = new Int16Array(size * size);
  for (let row = 0; row < size; row++) {
    const y = 1 - (2 * row) / (size - 1);
    for (let col = 0; col < size; col++) {
      const x = (2 * col) / (size - 1) - 1;
      pixels[row * size + col] = valueAt(x, y);
    }
  }
  return pixels;
}
