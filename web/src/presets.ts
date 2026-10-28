import { createWindow, type WindowLevel } from './windowing';

/** Common CT window presets, the same values as src/ImageWindowing/Presets.cs. */
export const PRESETS: Readonly<Record<string, WindowLevel>> = {
  brain: createWindow(40, 80),
  'soft-tissue': createWindow(40, 400),
  lung: createWindow(-600, 1500),
  bone: createWindow(400, 1800),
};

export const PRESET_LABELS: Readonly<Record<string, string>> = {
  brain: 'Brain',
  'soft-tissue': 'Soft tissue',
  lung: 'Lung',
  bone: 'Bone',
};
