/**
 * Maps Hounsfield values to 8-bit display values with the DICOM linear VOI function
 * (PS3.3 C.11.2.1.2). Mirrors src/ImageWindowing/WindowingTransform.cs; the golden
 * regression test checks that both produce identical lookup tables.
 */

export interface WindowLevel {
  readonly center: number;
  readonly width: number;
}

export const OUTPUT_MIN = 0;
export const OUTPUT_MAX = 255;

const INT16_MIN = -32768;
const LUT_SIZE = 65536;

export function createWindow(center: number, width: number): WindowLevel {
  if (!Number.isFinite(center)) {
    throw new RangeError(`Center must be a finite number, got ${center}.`);
  }
  if (Number.isNaN(width) || width < 1) {
    throw new RangeError(`Width must be at least 1, got ${width}.`);
  }
  return { center, width };
}

export function lowerBound(window: WindowLevel): number {
  return window.center - 0.5 - (window.width - 1) / 2;
}

export function upperBound(window: WindowLevel): number {
  return window.center - 0.5 + (window.width - 1) / 2;
}

/** Rounds half up so the result is identical to the C# implementation. */
export function toDisplay(value: number, window: WindowLevel): number {
  if (value <= lowerBound(window)) {
    return OUTPUT_MIN;
  }
  if (value > upperBound(window)) {
    return OUTPUT_MAX;
  }
  const normalized = (value - (window.center - 0.5)) / (window.width - 1) + 0.5;
  const scaled = normalized * (OUTPUT_MAX - OUTPUT_MIN) + OUTPUT_MIN;
  return Math.min(OUTPUT_MAX, Math.max(OUTPUT_MIN, Math.floor(scaled + 0.5)));
}

/** Precomputes the output for every 16-bit input, so rendering is one lookup per pixel. */
export function buildLookupTable(window: WindowLevel): Uint8Array {
  const table = new Uint8Array(LUT_SIZE);
  for (let i = 0; i < LUT_SIZE; i++) {
    table[i] = toDisplay(INT16_MIN + i, window);
  }
  return table;
}

export function applyWindow(pixels: Int16Array, window: WindowLevel, output: Uint8Array = new Uint8Array(pixels.length)): Uint8Array {
  if (output.length < pixels.length) {
    throw new RangeError('Output buffer is smaller than the input.');
  }
  const table = buildLookupTable(window);
  for (let i = 0; i < pixels.length; i++) {
    output[i] = table[pixels[i] - INT16_MIN];
  }
  return output;
}
