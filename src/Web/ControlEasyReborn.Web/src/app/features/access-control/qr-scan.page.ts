import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrowserMultiFormatReader, IScannerControls } from '@zxing/browser';
import { GatewayControlService } from './gateway-control.service';
import { AccessScanResultComponent } from './components/access-scan-result.component';
import { ScanResult, ScanRefusal } from './access-control.types';

interface UiState {
  qrPayload: string;
  direction: 'entrance' | 'exit';
  scanAttemptId: string;
  busy: boolean;
  result: ScanResult | null;
  refusal: ScanRefusal | null;
  error: string | null;
  cameraStatus: 'idle' | 'starting' | 'active' | 'unavailable';
}

const CAMERA_PERMISSION_DENIED = 'Camera permission denied. Use the text input below to paste the QR value.';
const NO_CAMERA = 'No camera detected — paste the QR value below.';
const CAMERA_IN_USE = 'Camera is in use by another app. Close it and reload, or paste the QR value below.';
const CAMERA_START_FAILED = 'Could not start the camera. Use the text input below to paste the QR value.';

const initialState = (): UiState => ({
  qrPayload: '',
  direction: 'entrance',
  scanAttemptId: cryptoRandom(),
  busy: false,
  result: null,
  refusal: null,
  error: null,
  cameraStatus: 'idle',
});

function cryptoRandom(): string {
  const c = globalThis.crypto;
  if (c?.randomUUID) return c.randomUUID();
  if (c?.getRandomValues) {
    const b = new Uint8Array(16);
    c.getRandomValues(b);
    const v6 = b[6] ?? 0;
    const v8 = b[8] ?? 0;
    b[6] = (v6 & 0x0f) | 0x40;
    b[8] = (v8 & 0x3f) | 0x80;
    const h = Array.from(b, x => x.toString(16).padStart(2, '0')).join('');
    return `${h.slice(0, 8)}-${h.slice(8, 12)}-${h.slice(12, 16)}-${h.slice(16, 20)}-${h.slice(20)}`;
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, ch => {
    const r = (Math.random() * 16) | 0;
    return (ch === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

@Component({
  selector: 'ce-qr-scan-page',
  standalone: true,
  imports: [CommonModule, FormsModule, AccessScanResultComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [
    `
      .qr-camera {
        position: relative;
        width: 100%;
        max-width: 480px;
        aspect-ratio: 1 / 1;
        background: #000;
        border-radius: 12px;
        overflow: hidden;
        margin: 0.75rem 0;
      }
      .qr-camera.is-hidden {
        display: none;
      }
      .qr-camera-video {
        width: 100%;
        height: 100%;
        object-fit: cover;
        display: block;
      }
      .qr-camera-reticle {
        position: absolute;
        inset: 18%;
        border: 2px solid rgba(255, 255, 255, 0.7);
        border-radius: 12px;
        pointer-events: none;
        box-shadow: 0 0 0 9999px rgba(0, 0, 0, 0.25);
      }
      .qr-camera-status {
        position: absolute;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        color: #fff;
        background: rgba(0, 0, 0, 0.5);
        margin: 0;
        font-size: 0.95rem;
      }
      .qr-scan-page .error {
        color: #b00020;
        background: #fdecea;
        padding: 0.5rem 0.75rem;
        border-radius: 6px;
        margin: 0.5rem 0;
      }
      .qr-scan-page .busy-hint {
        font-size: 0.85rem;
        color: #666;
        margin: 0.25rem 0;
      }
    `,
  ],
  template: `
    <section class="qr-scan-page">
      <h2>QR Scan</h2>
      <p class="hint">Point the camera at the credential QR. The raw QR is never stored.</p>

      <div class="qr-camera" [class.is-hidden]="!showCameraPanel()" data-testid="qr-camera-panel">
        <video #video class="qr-camera-video" playsinline muted></video>
        @if (showCameraPanel()) {
          <div class="qr-camera-reticle" aria-hidden="true"></div>
          @if (state().cameraStatus === 'starting') {
            <p class="qr-camera-status" role="status" aria-live="polite">Starting camera…</p>
          }
        }
      </div>

      @if (state().error) {
        <div class="error" role="alert" data-testid="qr-camera-error">{{ state().error }}</div>
      }

      <form (ngSubmit)="submit()" novalidate>
        <label>
          QR payload
          <input
            name="qrPayload"
            type="text"
            [ngModel]="state().qrPayload"
            (ngModelChange)="onQrPayloadChange($event)"
            placeholder="Paste scanned value"
            autocomplete="off"
            required
            data-testid="qr-payload-input"
          />
        </label>
        <label>
          Direction
          <select name="direction" [ngModel]="state().direction" (ngModelChange)="onDirectionChange($event)">
            <option value="entrance">Entrance</option>
            <option value="exit">Exit</option>
          </select>
        </label>
        <button
          type="submit"
          [disabled]="state().busy || !state().qrPayload"
          data-testid="qr-submit-button"
        >
          {{ state().busy ? 'Scanning...' : 'Submit scan' }}
        </button>
        @if (state().busy) {
          <p class="busy-hint" role="status" aria-live="polite" data-testid="qr-busy-hint">
            Already scanning — please wait.
          </p>
        }
      </form>

      <div aria-live="polite">
        @if (state().result) {
          <ce-access-scan-result
            [decision]="state().result!.decision"
            [subjectType]="state().result!.subjectType"
            [direction]="state().result!.direction"
            [destinationBlock]="state().result!.destinationBlock"
            [destinationUnit]="state().result!.destinationUnit"
          />
        } @else if (state().refusal) {
          <ce-access-scan-result
            [decision]="state().refusal!.decision"
            [failureCode]="state().refusal!.failureCode"
          />
        }
      </div>
    </section>
  `,
})
export class QrScanPage implements AfterViewInit, OnDestroy {
  private readonly gateway = inject(GatewayControlService);
  private readonly reader = new BrowserMultiFormatReader();
  private scannerControls: IScannerControls | null = null;
  private rearmHandle: ReturnType<typeof setTimeout> | null = null;
  private destroyed = false;

  @ViewChild('video', { static: false }) videoRef?: ElementRef<HTMLVideoElement>;

  readonly state = signal<UiState>(initialState());
  readonly showCameraPanel = computed(
    () => this.state().cameraStatus === 'starting' || this.state().cameraStatus === 'active',
  );

  ngAfterViewInit(): void {
    this.startScanner();
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.stopScanner();
    if (this.rearmHandle !== null) {
      clearTimeout(this.rearmHandle);
      this.rearmHandle = null;
    }
    const videoEl = this.videoRef?.nativeElement;
    if (videoEl) {
      videoEl.srcObject = null;
      videoEl.load();
    }
  }

  submit(): void {
    if (this.destroyed) return;
    const current = this.state();
    if (!current.qrPayload || current.busy) return;
    this.state.set({ ...current, busy: true, error: null, result: null, refusal: null });
    this.stopScanner();

    this.gateway
      .recordScan({
        qrPayload: current.qrPayload,
        direction: current.direction,
        scanAttemptId: current.scanAttemptId,
      })
      .subscribe({
        next: (result: ScanResult) => {
          if (this.destroyed) return;
          this.state.update(s => ({ ...s, busy: false, result, refusal: null, qrPayload: '' }));
          this.scheduleRearm();
        },
        error: (err: { error?: { failureCode?: ScanRefusal['failureCode']; decision?: ScanRefusal['decision'] } }) => {
          if (this.destroyed) return;
          const problem = err?.error;
          const refusal: ScanRefusal | null =
            problem?.failureCode && problem?.decision
              ? { failureCode: problem.failureCode, decision: problem.decision }
              : null;
          this.state.update(s => ({
            ...s,
            busy: false,
            refusal,
            result: null,
            error: refusal ? null : 'Scan service unavailable.',
          }));
          this.scheduleRearm();
        },
      });
  }

  onQrPayloadChange(value: string): void {
    this.state.update(s => ({ ...s, qrPayload: value }));
  }

  onDirectionChange(value: 'entrance' | 'exit'): void {
    this.state.update(s => ({ ...s, direction: value }));
  }

  private startScanner(): void {
    if (this.destroyed) return;
    if (this.scannerControls || this.state().cameraStatus === 'unavailable') return;
    const videoEl = this.videoRef?.nativeElement;
    if (!videoEl) {
      this.state.update(s => ({ ...s, cameraStatus: 'unavailable', error: NO_CAMERA }));
      return;
    }
    this.attachVideoLifecycleListeners(videoEl);
    this.state.update(s => ({ ...s, cameraStatus: 'starting' }));

    this.reader
      .decodeFromVideoDevice(undefined, videoEl, (result, _err, controls) => {
        if (this.destroyed) {
          controls.stop();
          return;
        }
        this.scannerControls = controls;
        if (this.state().cameraStatus !== 'unavailable') {
          this.state.update(s => ({ ...s, cameraStatus: 'active' }));
        }
        if (!result) {
          return;
        }
        if (this.state().busy) {
          return;
        }
        const text = result.getText();
        if (!text || text.length > 1024) return;
        const trimmed = text.trim();
        if (!trimmed || trimmed.length < 8) return;
        this.state.update(s => ({
          ...s,
          qrPayload: trimmed,
          error: null,
        }));
        this.submit();
      })
      .catch((err: unknown) => {
        if (this.destroyed) return;
        this.handleCameraError(err);
      });
  }

  private attachVideoLifecycleListeners(videoEl: HTMLVideoElement): void {
    videoEl.addEventListener(
      'ended',
      () => {
        if (!this.destroyed && this.state().cameraStatus !== 'unavailable') {
          this.handleCameraError({ name: 'NotReadableError' });
        }
      },
      { once: true },
    );
  }

  private handleCameraError(err: unknown): void {
    const name = (err as { name?: string } | null)?.name ?? '';
    let message = CAMERA_START_FAILED;
    let terminal = false;
    if (name === 'NotAllowedError' || name === 'PermissionDeniedError') {
      message = CAMERA_PERMISSION_DENIED;
      terminal = true;
    } else if (name === 'NotFoundError' || name === 'DevicesNotFoundError' || name === 'OverconstrainedError') {
      message = NO_CAMERA;
      terminal = true;
    } else if (name === 'NotReadableError') {
      message = CAMERA_IN_USE;
    }
    this.state.update(s => ({ ...s, cameraStatus: 'unavailable', error: message }));
    this.stopScanner();
    if (!terminal && !this.destroyed) {
      this.scheduleRetry();
    }
  }

  private stopScanner(): void {
    if (this.scannerControls) {
      this.scannerControls.stop();
      this.scannerControls = null;
    }
    const videoEl = this.videoRef?.nativeElement;
    if (videoEl) {
      const stream = videoEl.srcObject as MediaStream | null;
      if (stream) {
        stream.getTracks().forEach(t => {
          try {
            t.stop();
          } catch {
            /* ignore */
          }
        });
      }
      videoEl.srcObject = null;
      videoEl.load();
    }
  }

  private scheduleRearm(): void {
    if (this.rearmHandle !== null) {
      clearTimeout(this.rearmHandle);
    }
    this.rearmHandle = setTimeout(() => {
      this.rearmHandle = null;
      if (this.destroyed) return;
      this.state.update(s => ({
        ...s,
        scanAttemptId: cryptoRandom(),
        error: null,
      }));
      this.startScanner();
    }, 250);
  }

  private scheduleRetry(): void {
    if (this.rearmHandle !== null) {
      clearTimeout(this.rearmHandle);
    }
    this.rearmHandle = setTimeout(() => {
      this.rearmHandle = null;
      if (this.destroyed) return;
      this.state.update(s => ({ ...s, cameraStatus: 'idle', error: null }));
      this.startScanner();
    }, 1500);
  }
}
