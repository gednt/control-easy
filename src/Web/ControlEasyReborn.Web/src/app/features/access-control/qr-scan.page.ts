import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
}

const initialState = (): UiState => ({
  qrPayload: '',
  direction: 'entrance',
  scanAttemptId: cryptoRandom(),
  busy: false,
  result: null,
  refusal: null,
  error: null,
});

function cryptoRandom(): string {
  if (typeof globalThis.crypto?.randomUUID === 'function') {
    return globalThis.crypto.randomUUID();
  }
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
    const r = (Math.random() * 16) | 0;
    return (c === 'x' ? r : (r & 0x3) | 0x8).toString(16);
  });
}

@Component({
  selector: 'ce-qr-scan-page',
  standalone: true,
  imports: [CommonModule, FormsModule, AccessScanResultComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <section class="qr-scan-page">
      <h2>QR Scan</h2>
      <p class="hint">Enter or scan a credential value. The raw QR is never stored.</p>
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
          />
        </label>
        <label>
          Direction
          <select name="direction" [ngModel]="state().direction" (ngModelChange)="onDirectionChange($event)">
            <option value="entrance">Entrance</option>
            <option value="exit">Exit</option>
          </select>
        </label>
        <button type="submit" [disabled]="state().busy || !state().qrPayload">
          {{ state().busy ? 'Scanning...' : 'Submit scan' }}
        </button>
      </form>

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
      } @else if (state().error) {
        <div class="error" role="alert">{{ state().error }}</div>
      }
    </section>
  `,
})
export class QrScanPage {
  private readonly gateway = inject(GatewayControlService);

  readonly state = signal<UiState>(initialState());

  submit(): void {
    const current = this.state();
    if (!current.qrPayload || current.busy) return;
    this.state.set({ ...current, busy: true, error: null, result: null, refusal: null });

    this.gateway
      .recordScan({
        qrPayload: current.qrPayload,
        direction: current.direction,
        scanAttemptId: current.scanAttemptId,
      })
      .subscribe({
        next: (result: ScanResult) => {
          this.state.update(s => ({ ...s, busy: false, result, refusal: null, qrPayload: '' }));
        },
        error: (err: { error?: { failureCode?: ScanRefusal['failureCode']; decision?: ScanRefusal['decision'] } }) => {
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
        },
      });
  }

  onQrPayloadChange(value: string): void {
    this.state.update(s => ({ ...s, qrPayload: value }));
  }

  onDirectionChange(value: 'entrance' | 'exit'): void {
    this.state.update(s => ({ ...s, direction: value }));
  }
}