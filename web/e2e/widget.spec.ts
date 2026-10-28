import { expect, test, type Page } from '@playwright/test';

/** Reads the gray value of one pixel from the rendered canvas. */
async function grayAt(page: Page, x: number, y: number): Promise<number> {
  return page.getByTestId('image').evaluate(
    (canvas, [px, py]) => (canvas as HTMLCanvasElement).getContext('2d')!.getImageData(px, py, 1, 1).data[0],
    [x, y],
  );
}

// Pixel positions in the 256 x 256 phantom.
const SKULL = { x: 128, y: 12 };       // bone, 1000 HU
const BRAIN = { x: 128, y: 166 };      // brain tissue, 40 HU
const VENTRICLE = { x: 156, y: 128 };  // fluid, 0 HU

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test('opens with the brain preset', async ({ page }) => {
  await expect(page.getByTestId('readout')).toHaveText('C 40 / W 80');
  await expect(page.getByTestId('center')).toHaveValue('40');
  await expect(page.getByTestId('width')).toHaveValue('80');
});

test('brain window separates tissue from fluid and shows bone as white', async ({ page }) => {
  expect(await grayAt(page, SKULL.x, SKULL.y)).toBe(255);
  expect(await grayAt(page, BRAIN.x, BRAIN.y)).toBeGreaterThan(100);
  expect(await grayAt(page, VENTRICLE.x, VENTRICLE.y)).toBe(0);
});

test('preset buttons update the sliders and the image', async ({ page }) => {
  await page.getByRole('button', { name: 'Bone' }).click();

  await expect(page.getByTestId('readout')).toHaveText('C 400 / W 1800');
  // In the bone window, brain and fluid look almost the same.
  const difference = (await grayAt(page, BRAIN.x, BRAIN.y)) - (await grayAt(page, VENTRICLE.x, VENTRICLE.y));
  expect(difference).toBeLessThan(10);
});

test('dragging right on the image widens the window', async ({ page }) => {
  const box = (await page.getByTestId('image').boundingBox())!;
  const start = { x: box.x + box.width / 2, y: box.y + box.height / 2 };

  await page.mouse.move(start.x, start.y);
  await page.mouse.down();
  await page.mouse.move(start.x + 50, start.y, { steps: 5 });
  await page.mouse.up();

  await expect(page.getByTestId('width')).toHaveValue('280');
  await expect(page.getByTestId('center')).toHaveValue('40');
});

test('sliders can be operated with the keyboard', async ({ page }) => {
  await page.getByTestId('center').focus();
  await page.keyboard.press('ArrowRight');

  await expect(page.getByTestId('readout')).toHaveText('C 41 / W 80');
});
