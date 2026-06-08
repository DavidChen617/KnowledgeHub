import { signal } from '@angular/core';

export interface ResizableOptions {
  defaultWidth: number | null;
  min: number;
  max?: number;
  /** 'right': drag right = wider (left-edge handle). 'left': drag left = wider (right-edge handle). */
  direction: 'right' | 'left';
}

export function createResizable(options: ResizableOptions) {
  const width = signal<number | null>(options.defaultWidth);
  let startX = 0;
  let startWidth = 0;

  const onMouseMove = (e: MouseEvent) => {
    const delta = options.direction === 'right'
      ? e.clientX - startX
      : startX - e.clientX;
    width.set(Math.max(options.min, Math.min(options.max ?? Infinity, startWidth + delta)));
  };

  const onMouseUp = () => {
    document.removeEventListener('mousemove', onMouseMove);
    document.removeEventListener('mouseup', onMouseUp);
    document.body.style.cursor = '';
    document.body.style.userSelect = '';
  };

  function startResize(e: MouseEvent, currentWidth?: number) {
    startX = e.clientX;
    startWidth = currentWidth ?? width() ?? options.min;
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
    document.body.style.cursor = 'col-resize';
    document.body.style.userSelect = 'none';
    e.preventDefault();
  }

  return { width, startResize };
}
