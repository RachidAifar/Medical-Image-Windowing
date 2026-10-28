import { generatePhantom } from './phantom';
import { WindowLevelWidget } from './window-level-widget';
import './style.css';

const SIZE = 256;

const widget = new WindowLevelWidget({ pixels: generatePhantom(SIZE), size: SIZE });
document.querySelector('#app')!.append(widget.element);
