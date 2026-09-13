import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
} from '@angular/core';
import { CeModalComponent } from '../modal/modal.component';
import { CeIconComponent } from '../icon/icon.component';
import type { OverrideReason } from '../../../features/entry-log/entry-log.service';

/**
 * Two-button sub-second decision modal for override reasons. Hardcoded
 * reasons per ROADMAP — no free-text input.
 *
 * Emits `reasonSelected` on tap; consumer closes the modal via the
 * `closed` event (tap outside or Esc) or reacts to the chosen reason.
 */
@Component({
  selector: 'ce-override-reason',
  standalone: true,
  imports: [CeModalComponent, CeIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <ce-modal [open]="open()" title="Why?" size="sm" (closed)="onClose()">
      <div class="reasons">
        <button
          type="button"
          class="reason-btn"
          data-reason="emergency"
          (click)="select('emergency')"
        >
          <ce-icon name="alert-triangle" [size]="32" />
          <span class="label">Emergency</span>
        </button>
        <button
          type="button"
          class="reason-btn"
          data-reason="vouched"
          (click)="select('vouched')"
        >
          <ce-icon name="user-check" [size]="32" />
          <span class="label">Vouched</span>
        </button>
      </div>
    </ce-modal>
  `,
  styles: [
    `
      :host { display: contents; }
      .reasons {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: var(--space-3, 12px);
        padding: var(--space-2, 8px) 0;
      }
      .reason-btn {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: var(--space-2, 8px);
        padding: var(--space-4, 16px);
        border: 2px solid var(--color-border, #d1d5db);
        border-radius: var(--radius-lg, 8px);
        background: var(--color-surface-elevated, #fff);
        color: var(--color-text-primary, #111827);
        cursor: pointer;
        min-height: 120px;
        font-family: inherit;
        transition: transform 100ms ease-out, box-shadow 100ms ease-out;
      }
      .reason-btn:hover {
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }
      .reason-btn:active {
        transform: scale(0.95);
      }
      .reason-btn:focus-visible {
        outline: 2px solid var(--color-primary, #0066cc);
        outline-offset: 2px;
      }
      .reason-btn[data-reason='emergency'] {
        border-color: var(--color-danger, #991b1b);
        color: var(--color-danger, #991b1b);
      }
      .reason-btn[data-reason='vouched'] {
        border-color: var(--color-warning, #92400e);
        color: var(--color-warning, #92400e);
      }
      .label {
        font-size: var(--font-size-base, 18px);
        font-weight: var(--font-weight-semibold, 600);
      }
      @media (prefers-reduced-motion: reduce) {
        .reason-btn:hover,
        .reason-btn:active { transform: none; }
      }
    `,
  ],
})
export class CeOverrideReasonComponent {
  open = input<boolean>(false);

  reasonSelected = output<OverrideReason>();
  closed = output<void>();

  select(reason: OverrideReason): void {
    this.reasonSelected.emit(reason);
  }

  onClose(): void {
    this.closed.emit();
  }
}