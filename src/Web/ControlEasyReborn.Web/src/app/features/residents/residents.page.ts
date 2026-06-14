import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ResidentsApiService, ResidentResponse } from './residents-api.service';
import { ApartmentPickerComponent } from '../../shared/apartment-picker/apartment-picker.component';
import {
  ApartmentsApiService,
  formatApartmentLabel,
} from '../apartments/apartments-api.service';
import { cpfValidator } from '../../core/validators/cpf.validator';
import { getApiErrorMessage } from '../../core/utils/api-error.util';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'ce-residents-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ApartmentPickerComponent],
  template: `
    <div class="page-header">
      <div class="page-title-block">
        <h1 class="page-title">Residents</h1>
        <p class="page-subtitle">{{ totalResidents() }} active residents across all apartments in this condominium.</p>
      </div>
      <div class="page-header-actions">
        <button class="ce-button variant-secondary size-md" (click)="refresh()">
          &#8635; Refresh
        </button>
        @if (canWrite()) {
          <button class="ce-button variant-primary size-md" (click)="openCreateModal()">
            &#43; Add resident
          </button>
        }
      </div>
    </div>

    @if (loading()) {
      <div class="loading-state">
        <div class="ce-spinner tone-primary size-lg"></div>
        <p>Loading residents...</p>
      </div>
    } @else if (residents().length === 0 && !searchTerm()) {
      <div class="ce-empty-state">
        <div class="ce-empty-state-icon">&#128101;</div>
        <div class="ce-empty-state-title">No residents yet</div>
        <div class="ce-empty-state-description text-secondary">
          Add your first resident to get started.
        </div>
        @if (canWrite()) {
          <button class="ce-button variant-primary size-md" (click)="openCreateModal()">
            &#43; Add resident
          </button>
        }
      </div>
    } @else {
      <div class="ce-card" style="padding: 0;">
        <div class="table-toolbar">
          <div class="search-wrapper">
            <span class="search-icon">&#128269;</span>
            <input type="search"
                   class="search-input"
                   placeholder="Search by name, apartment, or CPF..."
                   [value]="searchTerm()"
                   (input)="onSearch($event)" />
          </div>
          @if (searchTerm() && !loading()) {
            <span class="result-count">{{ residents().length }} result{{ residents().length !== 1 ? 's' : '' }}</span>
          }
        </div>
        <div class="ce-table-wrapper">
          <table class="ce-table" aria-label="Residents table">
            <thead>
              <tr>
                <th scope="col">Resident</th>
                <th scope="col">Apartment</th>
                <th scope="col">CPF</th>
                <th scope="col">Phone</th>
                <th scope="col">Status</th>
                <th scope="col" style="width: 1%;"></th>
              </tr>
            </thead>
            <tbody>
              @for (resident of residents(); track resident.id) {
                <tr>
                  <td>
                    <div class="resident-name-cell">
                      <div class="resident-avatar" [style]="getAvatarStyle(resident)">
                        {{ getInitials(resident.name) }}
                      </div>
                      <div>
                        <div class="font-semibold">{{ resident.name }}</div>
                        @if (resident.email) {
                          <div class="text-xs text-secondary">{{ resident.email }}</div>
                        }
                      </div>
                    </div>
                  </td>
                  <td>{{ getApartmentLabel(resident.apartmentId) }}</td>
                  <td>{{ resident.cpf }}</td>
                  <td>{{ resident.phone ?? '\u2014' }}</td>
                  <td>
                    <span class="ce-badge tone-success size-sm">{{ resident.active ? 'Active' : 'Inactive' }}</span>
                  </td>
                  <td>
                    @if (canWrite()) {
                      <div class="action-cell">
                        <button class="icon-btn-sm" aria-label="Edit resident" title="Edit" (click)="openEditModal(resident)">&#9998;</button>
                        @if (resident.active) {
                          <button class="icon-btn-sm action-deactivate" aria-label="Deactivate resident" title="Deactivate" (click)="onDeactivate(resident)">&#8855;</button>
                        }
                      </div>
                    }
                  </td>
                </tr>
              } @empty {
                <tr>
                  <td colspan="6">
                    <div class="ce-empty-state" style="padding: var(--spacing-8) var(--spacing-4);">
                      <div class="ce-empty-state-icon">&#128269;</div>
                      <div class="ce-empty-state-title">No residents match your filters</div>
                      <div class="ce-empty-state-description text-secondary">
                        Try clearing the search or selecting a different filter.
                      </div>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    }

    @if (createModalOpen()) {
      <div class="ce-modal-backdrop" (click)="closeCreateModal()">
        <div class="ce-modal size-md" role="dialog" aria-modal="true" aria-labelledby="add-resident-title" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title" id="add-resident-title">Add new resident</h3>
            <button class="ce-modal-close" (click)="closeCreateModal()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            @if (createError()) {
              <div class="form-error-banner">{{ createError() }}</div>
            }
            <form class="ce-form" [formGroup]="createForm" (ngSubmit)="onCreateResident()">
              <div class="ce-input-group">
                <label class="ce-input-label" for="ar-name">Full name</label>
                <div class="ce-input-wrapper" [class.has-error]="createForm.get('name')?.invalid && createForm.get('name')?.touched">
                  <input id="ar-name" class="ce-input" placeholder="e.g. Maria Silva" formControlName="name" />
                </div>
                @if (createForm.get('name')?.invalid && createForm.get('name')?.touched) {
                  <div class="ce-input-error">Name is required</div>
                }
              </div>
              <div class="ce-input-group">
                <label class="ce-input-label" for="ar-cpf">CPF</label>
                <div class="ce-input-wrapper" [class.has-error]="createForm.get('cpf')?.invalid && createForm.get('cpf')?.touched">
                  <input id="ar-cpf" class="ce-input" placeholder="000.000.000-00" formControlName="cpf" />
                </div>
                @if (createForm.get('cpf')?.hasError('required') && createForm.get('cpf')?.touched) {
                  <div class="ce-input-error">CPF is required</div>
                } @else if (createForm.get('cpf')?.hasError('invalidCpf') && createForm.get('cpf')?.touched) {
                  <div class="ce-input-error">CPF check digits are invalid</div>
                }
              </div>
              <ce-apartment-picker
                formControlName="apartmentId"
                label="Apartment"
                inputId="ar-apartment"
                placeholder="Select block and unit..."
                [hasError]="!!(createForm.get('apartmentId')?.invalid && createForm.get('apartmentId')?.touched)" />
              @if (createForm.get('apartmentId')?.invalid && createForm.get('apartmentId')?.touched) {
                <div class="ce-input-error">Apartment is required</div>
              }
              <div class="ce-input-group">
                <label class="ce-input-label" for="ar-phone">Phone</label>
                <div class="ce-input-wrapper">
                  <input id="ar-phone" class="ce-input" placeholder="(11) 99999-0000" formControlName="phone" />
                </div>
              </div>
            </form>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeCreateModal()">Cancel</button>
            <button class="ce-button variant-primary size-sm"
                    (click)="onCreateResident()"
                    [class.disabled]="createForm.invalid || creating()"
                    [attr.aria-busy]="creating()"
                    [disabled]="createForm.invalid || creating()">
              @if (creating()) {
                <span class="ce-spinner tone-current size-sm"></span>
              }
              Add resident
            </button>
          </div>
        </div>
      </div>
    }

    @if (editModalOpen()) {
      <div class="ce-modal-backdrop" (click)="closeEditModal()">
        <div class="ce-modal size-md" role="dialog" aria-modal="true" aria-labelledby="edit-resident-title" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title" id="edit-resident-title">Edit resident</h3>
            <button class="ce-modal-close" (click)="closeEditModal()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            @if (editError()) {
              <div class="form-error-banner">{{ editError() }}</div>
            }
            <form class="ce-form" [formGroup]="editForm" (ngSubmit)="onEditResident()">
              <div class="ce-input-group">
                <label class="ce-input-label" for="er-name">Full name</label>
                <div class="ce-input-wrapper" [class.has-error]="editForm.get('name')?.invalid && editForm.get('name')?.touched">
                  <input id="er-name" class="ce-input" placeholder="e.g. Maria Silva" formControlName="name" />
                </div>
                @if (editForm.get('name')?.invalid && editForm.get('name')?.touched) {
                  <div class="ce-input-error">Name is required</div>
                }
              </div>
              <div class="ce-input-group">
                <label class="ce-input-label" for="er-cpf">CPF</label>
                <div class="ce-input-wrapper" [class.has-error]="editForm.get('cpf')?.invalid && editForm.get('cpf')?.touched">
                  <input id="er-cpf" class="ce-input" placeholder="000.000.000-00" formControlName="cpf" />
                </div>
                @if (editForm.get('cpf')?.hasError('required') && editForm.get('cpf')?.touched) {
                  <div class="ce-input-error">CPF is required</div>
                } @else if (editForm.get('cpf')?.hasError('invalidCpf') && editForm.get('cpf')?.touched) {
                  <div class="ce-input-error">CPF check digits are invalid</div>
                }
              </div>
              <ce-apartment-picker
                formControlName="apartmentId"
                label="Apartment"
                inputId="er-apartment"
                placeholder="Select block and unit..."
                [hasError]="!!(editForm.get('apartmentId')?.invalid && editForm.get('apartmentId')?.touched)" />
              @if (editForm.get('apartmentId')?.invalid && editForm.get('apartmentId')?.touched) {
                <div class="ce-input-error">Apartment is required</div>
              }
              <div class="ce-input-group">
                <label class="ce-input-label" for="er-phone">Phone</label>
                <div class="ce-input-wrapper">
                  <input id="er-phone" class="ce-input" placeholder="(11) 99999-0000" formControlName="phone" />
                </div>
              </div>
            </form>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeEditModal()">Cancel</button>
            <button class="ce-button variant-primary size-sm"
                    (click)="onEditResident()"
                    [class.disabled]="editForm.invalid || saving()"
                    [attr.aria-busy]="saving()"
                    [disabled]="editForm.invalid || saving()">
              @if (saving()) {
                <span class="ce-spinner tone-current size-sm"></span>
              }
              Save changes
            </button>
          </div>
        </div>
      </div>
    }

    @if (confirmDeactivateOpen()) {
      <div class="ce-modal-backdrop" (click)="closeDeactivateConfirm()">
        <div class="ce-modal size-sm" role="dialog" aria-modal="true" aria-labelledby="deactivate-resident-title" (click)="$event.stopPropagation()">
          <div class="ce-modal-header">
            <h3 class="ce-modal-title" id="deactivate-resident-title">Deactivate resident</h3>
            <button class="ce-modal-close" (click)="closeDeactivateConfirm()" aria-label="Close">&#10005;</button>
          </div>
          <div class="ce-modal-body">
            <p class="text-secondary">Are you sure you want to deactivate <strong>{{ residentToDeactivate()?.name }}</strong>? They will no longer be able to access the condominium.</p>
          </div>
          <div class="ce-modal-footer">
            <button class="ce-button variant-ghost size-sm" (click)="closeDeactivateConfirm()">Cancel</button>
            <button class="ce-button variant-danger size-sm"
                    (click)="onConfirmDeactivate()"
                    [class.disabled]="deactivating()"
                    [disabled]="deactivating()">
              @if (deactivating()) {
                <span class="ce-spinner tone-current size-sm"></span>
              }
              Deactivate
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--spacing-4);
      flex-wrap: wrap;
      margin-bottom: var(--spacing-6);
    }
    .page-title-block { min-width: 0; }
    .page-title { font-size: var(--font-size-2xl); margin-bottom: var(--spacing-1); }
    .page-subtitle { color: var(--color-text-secondary); font-size: var(--font-size-sm); }
    .page-header-actions { display: flex; gap: var(--spacing-2); flex-wrap: wrap; }
    .loading-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: var(--spacing-12);
      gap: var(--spacing-4);
      color: var(--color-text-secondary);
    }

    .ce-button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: var(--spacing-2);
      font-weight: var(--font-weight-medium);
      border: 1px solid transparent;
      border-radius: var(--radius-lg);
      cursor: pointer;
      user-select: none;
      white-space: nowrap;
      transition: transform var(--duration-fast) var(--ease-out), box-shadow var(--duration-fast) var(--ease-out), background-color var(--duration-fast) var(--ease-out), border-color var(--duration-fast) var(--ease-out), color var(--duration-fast) var(--ease-out), opacity var(--duration-fast) var(--ease-out);
      text-decoration: none;
      font-family: inherit;
    }
    .ce-button:focus-visible { outline: 2px solid var(--color-primary); outline-offset: 2px; }
    .ce-button.size-sm { height: 2rem; padding: 0 var(--spacing-3); font-size: var(--font-size-sm); }
    .ce-button.size-md { height: 2.5rem; padding: 0 var(--spacing-4); font-size: var(--font-size-sm); }
    .ce-button.variant-primary { background: var(--color-primary); color: white; box-shadow: var(--shadow-sm); }
    .ce-button.variant-primary:hover:not(:disabled) { background: var(--color-primary-hover); transform: translateY(-1px); box-shadow: var(--shadow-primary-glow); }
    .ce-button.variant-secondary { background: var(--color-surface); color: var(--color-text-primary); border-color: var(--color-border); }
    .ce-button.variant-secondary:hover:not(:disabled) { background: var(--color-surface-elevated); }
    .ce-button.variant-ghost { background: transparent; color: var(--color-text-primary); }
    .ce-button.variant-ghost:hover:not(:disabled) { background: var(--color-neutral-light); }
    .ce-button.variant-danger { background: var(--color-danger); color: white; }
    .ce-button.variant-danger:hover:not(:disabled) { background: var(--color-danger-hover); }
    .ce-button:disabled, .ce-button[aria-busy="true"] { opacity: 0.6; cursor: not-allowed; pointer-events: none; }
    .ce-button.disabled { opacity: 0.6; cursor: not-allowed; pointer-events: none; }

    .ce-card {
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }

    .table-toolbar {
      padding: var(--spacing-4) var(--spacing-6);
      border-bottom: 1px solid var(--color-border);
      display: flex;
      gap: var(--spacing-3);
      flex-wrap: wrap;
      align-items: center;
    }
    .search-wrapper {
      flex: 1;
      max-width: 24rem;
      position: relative;
    }
    .search-icon {
      position: absolute;
      left: var(--spacing-3);
      top: 50%;
      transform: translateY(-50%);
      color: var(--color-text-muted);
      pointer-events: none;
      font-size: 1rem;
    }
    .search-input {
      width: 100%;
      height: 2.5rem;
      padding: 0 var(--spacing-3) 0 2.5rem;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      background: var(--color-background);
      font-size: var(--font-size-sm);
      color: var(--color-text-primary);
      font-family: inherit;
    }
    .search-input:focus {
      outline: none;
      border-color: var(--color-primary);
      box-shadow: 0 0 0 3px color-mix(in oklch, var(--color-primary) 15%, transparent);
    }
    .result-count {
      font-size: var(--font-size-xs);
      color: var(--color-text-muted);
      white-space: nowrap;
    }

    .ce-table-wrapper { overflow-x: auto; }
    .ce-table {
      width: 100%;
      border-collapse: collapse;
      font-size: var(--font-size-sm);
    }
    .ce-table thead { background: var(--color-neutral-light); position: sticky; top: 0; }
    .ce-table th {
      text-align: left;
      padding: var(--spacing-3) var(--spacing-4);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-secondary);
      font-size: var(--font-size-xs);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      border-bottom: 1px solid var(--color-border);
    }
    .ce-table td {
      padding: var(--spacing-3) var(--spacing-4);
      border-bottom: 1px solid var(--color-border);
      color: var(--color-text-primary);
    }
    .ce-table tbody tr:last-child td { border-bottom: 0; }
    .ce-table tbody tr:hover { background: color-mix(in oklch, var(--color-primary) 3%, transparent); }

    .resident-name-cell {
      display: flex;
      align-items: center;
      gap: var(--spacing-3);
    }
    .resident-avatar {
      width: 2.5rem;
      height: 2.5rem;
      border-radius: var(--radius-full);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-weight: var(--font-weight-semibold);
      font-size: 0.85rem;
      color: white;
      flex-shrink: 0;
    }

    .action-cell {
      display: flex;
      gap: var(--spacing-1);
    }

    .ce-badge {
      display: inline-flex;
      align-items: center;
      gap: var(--spacing-1);
      font-weight: var(--font-weight-medium);
      border-radius: var(--radius-full);
      border: 1px solid transparent;
      font-size: var(--font-size-xs);
      line-height: 1;
    }
    .ce-badge.size-sm { padding: var(--spacing-1) var(--spacing-2); font-size: 0.7rem; }
    .ce-badge.tone-success { background: var(--color-success-light); color: var(--color-success); border-color: color-mix(in oklch, var(--color-success) 30%, transparent); }
    .ce-badge.tone-neutral { background: var(--color-neutral-light); color: var(--color-text-secondary); border-color: var(--color-border); }

    .icon-btn-sm {
      width: 2rem;
      height: 2rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: transparent;
      border: 0;
      border-radius: var(--radius-md);
      cursor: pointer;
      color: var(--color-text-muted);
      font-size: 1rem;
    }
    .icon-btn-sm:hover { background: var(--color-neutral-light); color: var(--color-text-primary); }
    .action-deactivate:hover { background: var(--color-danger-light); color: var(--color-danger); }

    .ce-empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: var(--spacing-12) var(--spacing-6);
      gap: var(--spacing-3);
      color: var(--color-text-secondary);
    }
    .ce-empty-state-icon {
      width: 4rem;
      height: 4rem;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      background: var(--color-neutral-light);
      color: var(--color-text-muted);
      border-radius: var(--radius-full);
      margin-bottom: var(--spacing-2);
      font-size: 1.5rem;
    }
    .ce-empty-state-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); color: var(--color-text-primary); }
    .ce-empty-state-description { max-width: 24rem; }

    .ce-spinner {
      display: inline-block;
      border-radius: var(--radius-full);
      border: 2px solid currentColor;
      border-top-color: transparent;
      animation: spin-slow 1.4s linear infinite;
    }
    .ce-spinner.size-sm { width: 1rem; height: 1rem; border-width: 2px; }
    .ce-spinner.size-lg { width: 2rem; height: 2rem; border-width: 3px; }
    .ce-spinner.tone-primary { color: var(--color-primary); }
    .ce-spinner.tone-current { color: currentColor; }

    .ce-modal-backdrop {
      position: fixed;
      inset: 0;
      background: rgb(0 0 0 / 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 100;
      padding: var(--spacing-4);
      animation: fade-in var(--duration-base) var(--ease-out);
    }
    .ce-modal {
      background: var(--color-surface-elevated);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-xl);
      width: 100%;
      max-width: 32rem;
      max-height: calc(100vh - var(--spacing-8));
      overflow: auto;
      animation: zoom-in var(--duration-base) var(--ease-out);
    }
    .ce-modal.size-sm { max-width: 24rem; }
    .ce-modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--spacing-5) var(--spacing-6);
      border-bottom: 1px solid var(--color-border);
    }
    .ce-modal-title { font-size: var(--font-size-lg); font-weight: var(--font-weight-semibold); }
    .ce-modal-close {
      background: transparent;
      border: 0;
      color: var(--color-text-muted);
      cursor: pointer;
      padding: var(--spacing-1);
      border-radius: var(--radius-md);
      display: inline-flex;
      font-size: 1.25rem;
    }
    .ce-modal-close:hover { background: var(--color-neutral-light); color: var(--color-text-primary); }
    .ce-modal-body { padding: var(--spacing-6); }
    .ce-modal-footer {
      display: flex;
      justify-content: flex-end;
      gap: var(--spacing-2);
      padding: var(--spacing-4) var(--spacing-6);
      border-top: 1px solid var(--color-border);
    }

    .ce-form {
      display: flex;
      flex-direction: column;
      gap: var(--spacing-4);
    }
    .ce-input-group { display: flex; flex-direction: column; gap: var(--spacing-1); }
    .ce-input-label {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      color: var(--color-text-primary);
    }
    .ce-input-wrapper {
      display: flex;
      align-items: center;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      transition: border-color var(--duration-fast) var(--ease-out), box-shadow var(--duration-fast) var(--ease-out);
      overflow: hidden;
    }
    .ce-input-wrapper:focus-within {
      border-color: var(--color-primary);
      box-shadow: 0 0 0 3px color-mix(in oklch, var(--color-primary) 15%, transparent);
    }
    .ce-input-wrapper.has-error {
      border-color: var(--color-danger);
      box-shadow: 0 0 0 3px color-mix(in oklch, var(--color-danger) 15%, transparent);
    }
    .ce-input {
      flex: 1;
      border: 0;
      background: transparent;
      padding: var(--spacing-3);
      font-family: inherit;
      font-size: var(--font-size-sm);
      color: var(--color-text-primary);
      outline: none;
      min-height: 2.5rem;
    }
    .ce-input::placeholder { color: var(--color-text-muted); }
    .ce-input-error {
      font-size: var(--font-size-xs);
      color: var(--color-danger);
      font-weight: var(--font-weight-medium);
    }
    .form-row { display: flex; gap: var(--spacing-3); }
    .form-error-banner {
      margin-bottom: var(--spacing-4);
      padding: var(--spacing-3);
      border-radius: var(--radius-lg);
      background: var(--color-danger-light);
      color: var(--color-danger);
      font-size: var(--font-size-sm);
    }

    .font-semibold { font-weight: var(--font-weight-semibold); }
    .text-xs { font-size: var(--font-size-xs); }
    .text-secondary { color: var(--color-text-secondary); }

    @keyframes spin-slow { to { transform: rotate(360deg); } }
    @keyframes fade-in { from { opacity: 0; } to { opacity: 1; } }
    @keyframes zoom-in { from { opacity: 0; transform: scale(0.96); } to { opacity: 1; transform: scale(1); } }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResidentsPage {
  private readonly api = inject(ResidentsApiService);
  private readonly apartmentsApi = inject(ApartmentsApiService);
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);

  readonly canWrite = computed(() => this.auth.hasPermission('Residents.Write'));

  residents = signal<ResidentResponse[]>([]);
  apartmentLabels = signal<Record<string, string>>({});
  loading = signal(true);
  creating = signal(false);
  saving = signal(false);
  deactivating = signal(false);
  createModalOpen = signal(false);
  editModalOpen = signal(false);
  confirmDeactivateOpen = signal(false);
  createError = signal<string | null>(null);
  editError = signal<string | null>(null);
  searchTerm = signal('');
  residentToDeactivate = signal<ResidentResponse | null>(null);
  editingResident = signal<ResidentResponse | null>(null);
  totalResidents = computed(() => this.residents().filter(r => r.active).length);

  createForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    cpf: ['', [Validators.required, cpfValidator()]],
    apartmentId: [null, [Validators.required]],
    phone: [null],
  });

  editForm: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    cpf: ['', [Validators.required, cpfValidator()]],
    apartmentId: [null, [Validators.required]],
    phone: [null],
  });

  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    this.loadApartmentLabels();
    this.loadResidents();
  }

  loadApartmentLabels(): void {
    this.apartmentsApi.list(undefined, 0, 500).subscribe({
      next: (apartments) => {
        const labels: Record<string, string> = {};
        for (const apartment of apartments) {
          labels[apartment.id] = formatApartmentLabel(apartment);
        }
        this.apartmentLabels.set(labels);
      },
    });
  }

  getApartmentLabel(apartmentId: string | null): string {
    if (!apartmentId) return '\u2014';
    return this.apartmentLabels()[apartmentId] ?? apartmentId.slice(0, 8);
  }

  loadResidents(): void {
    this.loading.set(true);
    this.api.list().subscribe({
      next: (data) => {
        this.residents.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      },
    });
  }

  refresh(): void {
    this.loadResidents();
  }

  onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    const value = target.value;
    this.searchTerm.set(value);
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.loading.set(true);
      this.api.list(value || undefined).subscribe({
        next: (data) => {
          this.residents.set(data);
          this.loading.set(false);
        },
        error: () => this.loading.set(false),
      });
    }, 300);
  }

  openCreateModal(): void {
    this.createForm.reset();
    this.createError.set(null);
    this.createModalOpen.set(true);
  }

  closeCreateModal(): void {
    this.createModalOpen.set(false);
  }

  onCreateResident(): void {
    if (this.createForm.invalid) return;
    this.creating.set(true);
    this.createError.set(null);
    const value = this.createForm.value;
    this.api.create({
      name: value.name,
      cpf: value.cpf,
      phone: value.phone ?? null,
      apartmentId: value.apartmentId,
    }).subscribe({
      next: () => {
        this.creating.set(false);
        this.closeCreateModal();
        this.loadResidents();
      },
      error: (err) => {
        this.creating.set(false);
        this.createError.set(getApiErrorMessage(err, 'Failed to create resident'));
      },
    });
  }

  openEditModal(resident: ResidentResponse): void {
    this.editingResident.set(resident);
    this.editForm.patchValue({
      name: resident.name,
      cpf: resident.cpf,
      apartmentId: resident.apartmentId,
      phone: resident.phone,
    });
    this.editError.set(null);
    this.editModalOpen.set(true);
  }

  closeEditModal(): void {
    this.editModalOpen.set(false);
    this.editingResident.set(null);
  }

  onEditResident(): void {
    if (this.editForm.invalid) return;
    const resident = this.editingResident();
    if (!resident) return;
    this.saving.set(true);
    this.editError.set(null);
    const value = this.editForm.value;
    this.api.update(resident.id, {
      name: value.name,
      cpf: value.cpf,
      phone: value.phone ?? null,
      apartmentId: value.apartmentId,
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.closeEditModal();
        this.loadResidents();
      },
      error: (err) => {
        this.saving.set(false);
        this.editError.set(getApiErrorMessage(err, 'Failed to update resident'));
      },
    });
  }

  onDeactivate(resident: ResidentResponse): void {
    this.residentToDeactivate.set(resident);
    this.confirmDeactivateOpen.set(true);
  }

  closeDeactivateConfirm(): void {
    this.confirmDeactivateOpen.set(false);
    this.residentToDeactivate.set(null);
  }

  onConfirmDeactivate(): void {
    const resident = this.residentToDeactivate();
    if (!resident) return;
    this.deactivating.set(true);
    this.api.deactivate(resident.id).subscribe({
      next: () => {
        this.deactivating.set(false);
        this.closeDeactivateConfirm();
        this.loadResidents();
      },
      error: () => {
        this.deactivating.set(false);
      },
    });
  }

  getInitials(name: string): string {
    if (!name) return '?';
    const parts = name.trim().split(/\s+/);
    const first = parts[0];
    const last = parts[parts.length - 1];
    if (parts.length >= 2 && first && last) {
      return (first.charAt(0) + last.charAt(0)).toUpperCase();
    }
    return first?.charAt(0)?.toUpperCase() ?? '?';
  }

  private readonly avatarColors = [
    'linear-gradient(135deg, #4f46e5, #818cf8)',
    'linear-gradient(135deg, #ec4899, #f472b6)',
    'linear-gradient(135deg, #22c55e, #86efac)',
    'linear-gradient(135deg, #f59e0b, #fcd34d)',
    'linear-gradient(135deg, #3b82f6, #93c5fd)',
  ];

  getAvatarStyle(resident: ResidentResponse): string {
    const index = resident.name.charCodeAt(0) % this.avatarColors.length;
    return `background: ${this.avatarColors[index]}`;
  }
}