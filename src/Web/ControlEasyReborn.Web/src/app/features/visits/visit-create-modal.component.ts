import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ApartmentPickerComponent } from '../../shared/apartment-picker/apartment-picker.component';
import { CeButtonComponent, CeModalComponent } from '../../design-system';
import { getApiErrorMessage } from '../../core/utils/api-error.util';
import { VisitsApiService } from './visits-api.service';

/**
 * The single visit-registration form used by both the Visits register and the
 * dashboard. Keeping this workflow here prevents either entry point from
 * losing required visit details such as the destination apartment.
 */
@Component({
  selector: 'ce-visit-create-modal',
  standalone: true,
  imports: [ReactiveFormsModule, ApartmentPickerComponent, CeButtonComponent, CeModalComponent],
  template: `
    <ce-modal [open]="open()" title="Add visit" size="md" (openChange)="onOpenChange($event)">
      @if (createError()) {
        <div class="form-error-banner" role="alert">{{ createError() }}</div>
      }
      <form [formGroup]="form" (ngSubmit)="submit()" id="visit-create-form" class="visit-form">
        <label>Visitor name<input class="ce-input" formControlName="visitorName" /></label>
        <label>Document<input class="ce-input" formControlName="visitorDocument" /></label>
        <label>Phone<input class="ce-input" formControlName="visitorPhone" /></label>
        <ce-apartment-picker
          formControlName="apartmentId"
          label="Apartment"
          inputId="visit-apartment"
          placeholder="Select apartment..."
        />
        <label>Purpose<input class="ce-input" formControlName="purpose" /></label>
      </form>
      <div ce-modal-footer>
        <ce-button variant="ghost" size="sm" type="button" (click)="close()">Cancel</ce-button>
        <ce-button variant="primary" size="sm" type="button" (click)="submit()" [disabled]="form.invalid || creating()">
          {{ creating() ? 'Saving…' : 'Save visit' }}
        </ce-button>
      </div>
    </ce-modal>
  `,
  styles: [
    `
      .visit-form {
        display: grid;
        gap: var(--space-3);
      }
      label {
        display: block;
        font-size: var(--font-size-sm);
      }
      .ce-input {
        box-sizing: border-box;
        width: 100%;
        padding: var(--space-2) var(--space-3);
        border: 1px solid var(--color-border);
        border-radius: var(--radius-lg);
        margin-top: var(--space-1);
        font-family: inherit;
      }
      .form-error-banner {
        margin-bottom: var(--space-3);
        padding: var(--space-3);
        border-radius: var(--radius-lg);
        color: var(--color-danger);
        background: var(--color-danger-light);
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VisitCreateModalComponent {
  private readonly api = inject(VisitsApiService);
  private readonly fb = inject(FormBuilder);

  open = input(false);
  closed = output<void>();
  created = output<void>();

  creating = signal(false);
  createError = signal<string | null>(null);

  form = this.fb.group({
    visitorName: ['', Validators.required],
    visitorDocument: ['', Validators.required],
    visitorPhone: [''],
    apartmentId: [null, Validators.required],
    purpose: [''],
  });

  onOpenChange(open: boolean): void {
    if (!open) this.close();
  }

  close(): void {
    this.creating.set(false);
    this.createError.set(null);
    this.form.reset();
    this.closed.emit();
  }

  submit(): void {
    if (this.form.invalid || this.creating()) return;

    this.creating.set(true);
    this.createError.set(null);
    const value = this.form.getRawValue();
    this.api
      .create({
        visitorName: value.visitorName!,
        visitorDocument: value.visitorDocument!,
        visitorPhone: value.visitorPhone || null,
        apartmentId: value.apartmentId!,
        purpose: value.purpose || null,
      })
      .subscribe({
        next: () => {
          this.creating.set(false);
          this.form.reset();
          this.created.emit();
          this.closed.emit();
        },
        error: (err) => {
          this.creating.set(false);
          this.createError.set(getApiErrorMessage(err, 'Failed to create visit'));
        },
      });
  }
}
