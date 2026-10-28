# Test strategy

The project has four layers of tests. All automated layers run in CI on every push.

| Layer | Tool | Where | What it proves |
|---|---|---|---|
| Unit | xUnit (C#), Vitest (TypeScript) | `tests/`, `web/tests/` | The DICOM windowing formula, input validation, phantom values and PNG encoding are correct |
| Regression | xUnit + Vitest | `testdata/lut-golden.json` | The lookup tables for every preset have not changed, and C# and TypeScript produce **byte-identical** output |
| Acceptance (end-to-end) | Playwright (Chromium) | `web/e2e/` | A user can change presets, sliders, keyboard and mouse drag, and the image on screen changes correctly |
| Manual | Checklist below | this file | Things that are hard to automate: visual quality, feel of the drag interaction, accessibility with a screen reader |

## Running the tests

```bash
# C# unit + regression tests
dotnet test

# TypeScript unit + regression tests
cd web && npm ci && npm test

# Browser acceptance tests (starts the dev server automatically)
npx playwright install chromium   # first time only
npm run test:e2e
```

## Key test cases

**Windowing boundaries (both languages).** Values at or below the lower bound are black, values above the upper bound are white, and the window center maps to mid gray. Width 1 behaves as a threshold, which is a DICOM edge case.

**Lookup table vs. direct formula.** For every preset, all 65,536 possible 16-bit inputs are checked against the direct formula. This guards the performance optimisation (lookup instead of floating-point math per pixel) against off-by-one errors.

**Cross-language regression.** `imgwin golden` records a SHA-256 hash of each preset's lookup table. Both test suites recompute the hash and compare. If one implementation changes rounding, the build fails.

**Updating the golden file.** Only when an intended change alters the output:

```bash
dotnet run --project src/ImageWindowing.Cli -- golden
```

Commit the new `testdata/lut-golden.json` together with the change and explain why in the commit message.

## Manual test checklist

Run before a release, in Chrome and Firefox, on Windows and Linux.

| # | Step | Expected result | Pass |
|---|---|---|---|
| 1 | Open the viewer (`npm run dev`) | Brain preset is shown; readout says `C 40 / W 80`; skull is white, ventricles black | ☐ |
| 2 | Click each preset button | Sliders and readout jump to the preset; image changes immediately with no flicker | ☐ |
| 3 | Drag right / left on the image | Contrast decreases / increases smoothly; width never goes below 1 | ☐ |
| 4 | Drag up / down on the image | Image gets brighter / darker | ☐ |
| 5 | Release the mouse outside the image while dragging | Dragging stops; the next move over the image does not change the window | ☐ |
| 6 | Tab to a slider and use the arrow keys | Value changes by 1 HU per key press; focus outline is visible | ☐ |
| 7 | Resize the window to phone width (375 px) | Image and controls fit without horizontal scrolling | ☐ |
| 8 | Turn on a screen reader | Sliders are announced as "Center (HU)" and "Width (HU)"; presets as a button group | ☐ |
