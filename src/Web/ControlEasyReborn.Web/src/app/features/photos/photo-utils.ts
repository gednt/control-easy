import { Observable, firstValueFrom } from 'rxjs';

export interface CompressionOptions {
  /** Longest edge target in pixels (default 1280). */
  maxDimension: number;
  /** Hard ceiling on output blob size in bytes (default 500 KB). */
  maxBytes: number;
  /** First-pass JPEG quality (default 0.8). */
  initialQuality: number;
  /** Second-pass JPEG quality (default 0.6). */
  fallbackQuality: number;
  /** Floor JPEG quality (default 0.3) — last attempt before bailing. */
  floorQuality: number;
  mimeType: 'image/jpeg';
}

export const DEFAULT_COMPRESSION: CompressionOptions = {
  maxDimension: 1280,
  maxBytes: 500 * 1024,
  initialQuality: 0.8,
  fallbackQuality: 0.6,
  floorQuality: 0.3,
  mimeType: 'image/jpeg',
};

/**
 * Compress an image blob using the ROADMAP quality ladder (0.8 → 0.6 → 0.3).
 * Each pass redraws the source image onto an offscreen canvas which strips
 * all EXIF metadata (canvas → blob has no EXIF header).
 *
 * Throws `Error('FILE_TOO_LARGE_AFTER_COMPRESSION')` when even the floor
 * quality exceeds the byte budget. Callers should map this to a
 * user-visible toast.
 */
export async function compressImage(
  source: Blob,
  options: CompressionOptions = DEFAULT_COMPRESSION,
): Promise<Blob> {
  const img = await loadImage(source);
  const canvas = resize(img, options.maxDimension);

  let blob = await canvasToBlob(canvas, options.initialQuality, options.mimeType);
  if (blob.size > options.maxBytes) {
    blob = await canvasToBlob(canvas, options.fallbackQuality, options.mimeType);
  }
  if (blob.size > options.maxBytes) {
    blob = await canvasToBlob(canvas, options.floorQuality, options.mimeType);
  }
  if (blob.size > options.maxBytes) {
    throw new Error('FILE_TOO_LARGE_AFTER_COMPRESSION');
  }
  return blob;
}

/**
 * Generate a cover-fitted square thumbnail at the requested size (default 128).
 * Used by the upload modal to send a small preview alongside the source blob;
 * in Phase 12 the backend does not yet persist a thumbnail, so this is wired
 * but currently only kept in the local binding cache.
 */
export async function generateThumbnail(source: Blob, size = 128): Promise<Blob> {
  const img = await loadImage(source);
  const canvas = document.createElement('canvas');
  canvas.width = size;
  canvas.height = size;
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('CANVAS_CONTEXT_UNAVAILABLE');

  // Cover-fit: scale so the image fills the square, then center-crop.
  const scale = Math.max(size / img.naturalWidth, size / img.naturalHeight);
  const w = img.naturalWidth * scale;
  const h = img.naturalHeight * scale;
  const x = (size - w) / 2;
  const y = (size - h) / 2;
  ctx.fillStyle = '#000';
  ctx.fillRect(0, 0, size, size);
  ctx.drawImage(img, x, y, w, h);
  return canvasToBlob(canvas, 0.8, 'image/jpeg');
}

async function loadImage(blob: Blob): Promise<HTMLImageElement> {
  const url = URL.createObjectURL(blob);
  try {
    const img = new Image();
    img.src = url;
    await img.decode();
    return img;
  } finally {
    URL.revokeObjectURL(url);
  }
}

function resize(img: HTMLImageElement, maxDimension: number): HTMLCanvasElement {
  const { naturalWidth: w, naturalHeight: h } = img;
  const scale = Math.min(1, maxDimension / Math.max(w, h));
  const canvas = document.createElement('canvas');
  canvas.width = Math.max(1, Math.round(w * scale));
  canvas.height = Math.max(1, Math.round(h * scale));
  const ctx = canvas.getContext('2d');
  if (!ctx) throw new Error('CANVAS_CONTEXT_UNAVAILABLE');
  ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
  return canvas;
}

function canvasToBlob(canvas: HTMLCanvasElement, quality: number, mime: string): Promise<Blob> {
  return new Promise<Blob>((resolve, reject) => {
    canvas.toBlob(
      (blob) => (blob ? resolve(blob) : reject(new Error('CANVAS_TO_BLOB_FAILED'))),
      mime,
      quality,
    );
  });
}

export interface RetryOptions {
  maxAttempts: number;
  /** Per-attempt delay in ms (entry N waits delays[N] before retrying). */
  delays: number[];
}

export const DEFAULT_RETRY: RetryOptions = {
  maxAttempts: 3,
  delays: [1000, 2000, 4000],
};

/**
 * Wraps an Observable-returning upload function with retry + exponential backoff.
 * Retries on any thrown error; clients should treat 4xx validation errors as
 * terminal by short-circuiting on the server (no client-side skip in this util
 * because ProblemDetails payload inspection is upstream of retry logic).
 */
export async function uploadWithRetry<T>(
  upload: () => Observable<T>,
  options: RetryOptions = DEFAULT_RETRY,
): Promise<T> {
  let lastError: unknown;
  for (let attempt = 0; attempt < options.maxAttempts; attempt++) {
    try {
      return await firstValueFrom(upload());
    } catch (err) {
      lastError = err;
      if (attempt < options.maxAttempts - 1) {
        await sleep(options.delays[attempt] ?? options.delays[options.delays.length - 1] ?? 0);
      }
    }
  }
  throw lastError;
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
