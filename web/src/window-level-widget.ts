import { applyWindow, createWindow, type WindowLevel } from './windowing';
import { PRESET_LABELS, PRESETS } from './presets';

export interface WidgetOptions {
  /** Square image in Hounsfield units. */
  pixels: Int16Array;
  size: number;
  initial?: WindowLevel;
}

const CENTER_RANGE = { min: -1200, max: 3000 } as const;
const WIDTH_RANGE = { min: 1, max: 4000 } as const;

/**
 * Window/level control for a CT image: center and width sliders, preset buttons, and
 * mouse drag on the image (horizontal changes width, vertical changes center), the way
 * radiology viewers usually work. Dispatches a "windowchange" event on every change.
 */
export class WindowLevelWidget extends EventTarget {
  readonly element: HTMLElement;

  private readonly canvas: HTMLCanvasElement;
  private readonly context: CanvasRenderingContext2D;
  private readonly imageData: ImageData;
  private readonly displayBuffer: Uint8Array;
  private readonly centerInput: HTMLInputElement;
  private readonly widthInput: HTMLInputElement;
  private readonly readout: HTMLOutputElement;
  private current: WindowLevel;

  constructor(private readonly options: WidgetOptions) {
    super();
    if (options.pixels.length !== options.size * options.size) {
      throw new RangeError('pixels must contain size * size values.');
    }

    this.current = options.initial ?? PRESETS.brain;
    this.element = document.createElement('section');
    this.element.className = 'wl-widget';
    this.element.setAttribute('aria-label', 'Window/level viewer');

    this.canvas = document.createElement('canvas');
    this.canvas.width = options.size;
    this.canvas.height = options.size;
    this.canvas.dataset.testid = 'image';
    this.canvas.setAttribute('role', 'img');
    this.canvas.setAttribute('aria-label', 'Synthetic CT head slice');
    const context = this.canvas.getContext('2d');
    if (!context) {
      throw new Error('2D canvas is not supported in this browser.');
    }
    this.context = context;
    this.imageData = context.createImageData(options.size, options.size);
    this.displayBuffer = new Uint8Array(options.pixels.length);

    this.centerInput = this.createSlider('Center (HU)', 'center', CENTER_RANGE);
    this.widthInput = this.createSlider('Width (HU)', 'width', WIDTH_RANGE);
    this.readout = document.createElement('output');
    this.readout.dataset.testid = 'readout';

    this.element.append(this.canvas, this.createPresetBar(), this.label(this.centerInput), this.label(this.widthInput), this.readout);
    this.attachDragHandling();
    this.setWindow(this.current);
  }

  get window(): WindowLevel {
    return this.current;
  }

  setWindow(window: WindowLevel): void {
    this.current = createWindow(
      clamp(Math.round(window.center), CENTER_RANGE.min, CENTER_RANGE.max),
      clamp(Math.round(window.width), WIDTH_RANGE.min, WIDTH_RANGE.max),
    );
    this.centerInput.value = String(this.current.center);
    this.widthInput.value = String(this.current.width);
    this.readout.value = `C ${this.current.center} / W ${this.current.width}`;
    this.render();
    this.dispatchEvent(new CustomEvent('windowchange', { detail: this.current }));
  }

  private render(): void {
    applyWindow(this.options.pixels, this.current, this.displayBuffer);
    const rgba = this.imageData.data;
    for (let i = 0; i < this.displayBuffer.length; i++) {
      const gray = this.displayBuffer[i];
      rgba[i * 4] = gray;
      rgba[i * 4 + 1] = gray;
      rgba[i * 4 + 2] = gray;
      rgba[i * 4 + 3] = 255;
    }
    this.context.putImageData(this.imageData, 0, 0);
  }

  private createSlider(name: string, key: 'center' | 'width', range: { min: number; max: number }): HTMLInputElement {
    const input = document.createElement('input');
    input.type = 'range';
    input.min = String(range.min);
    input.max = String(range.max);
    input.step = '1';
    input.name = key;
    input.dataset.testid = key;
    input.setAttribute('aria-label', name);
    input.addEventListener('input', () => {
      const value = Number(input.value);
      this.setWindow(key === 'center' ? { ...this.current, center: value } : { ...this.current, width: value });
    });
    return input;
  }

  private label(input: HTMLInputElement): HTMLLabelElement {
    const label = document.createElement('label');
    label.textContent = input.getAttribute('aria-label');
    label.append(input);
    return label;
  }

  private createPresetBar(): HTMLElement {
    const bar = document.createElement('div');
    bar.className = 'wl-presets';
    bar.setAttribute('role', 'group');
    bar.setAttribute('aria-label', 'Presets');
    for (const [key, window] of Object.entries(PRESETS)) {
      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = PRESET_LABELS[key] ?? key;
      button.dataset.preset = key;
      button.addEventListener('click', () => this.setWindow(window));
      bar.append(button);
    }
    return bar;
  }

  private attachDragHandling(): void {
    let start: { x: number; y: number; window: WindowLevel } | undefined;

    this.canvas.addEventListener('pointerdown', (event) => {
      start = { x: event.clientX, y: event.clientY, window: this.current };
      this.canvas.setPointerCapture(event.pointerId);
    });
    this.canvas.addEventListener('pointermove', (event) => {
      if (!start) {
        return;
      }
      // 4 HU per screen pixel feels natural for CT ranges of a few thousand HU.
      this.setWindow({
        center: start.window.center - (event.clientY - start.y) * 4,
        width: start.window.width + (event.clientX - start.x) * 4,
      });
    });
    const stop = () => (start = undefined);
    this.canvas.addEventListener('pointerup', stop);
    this.canvas.addEventListener('pointercancel', stop);
  }
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, value));
}
