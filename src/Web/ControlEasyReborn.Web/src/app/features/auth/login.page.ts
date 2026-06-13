import { Component, ChangeDetectionStrategy, inject, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import type { TenantLookupResponse } from '../../core/services/auth.service';
import { DemoInfoService } from '../../core/services/demo-info.service';

const DEMO_ACCOUNTS = [
  { label: 'Platform Admin', email: 'platform@controleasy.app' },
  { label: 'Administrador', email: 'admin@controleasy.app' },
  { label: 'Porteiro', email: 'porteiro@controleasy.app' },
  { label: 'Morador', email: 'morador@controleasy.app' },
  { label: 'Multi-condomínio', email: 'multi@controleasy.app' },
] as const;

@Component({
  selector: 'ce-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <main class="login-page" tabindex="-1">
      <div class="login-card">
        <div class="login-brand">
          <div class="login-brand-mark">CE</div>
          <div class="login-brand-text">ControlEasy</div>
        </div>
        <h1 class="login-title">Sign in to ControlEasy</h1>
        <p class="login-subtitle">Use your email and password to access the platform.</p>

        @if (loginError()) {
          <div class="login-error" role="alert" aria-live="assertive">
            <span>&#9888;</span>
            <span>{{ loginError() }}</span>
          </div>
        }

        @if (tenantPickerVisible()) {
          <div class="tenant-picker-section">
            <p class="tenant-picker-label">Select a condominium to continue:</p>
            <div class="tenant-picker-grid">
              @for (tenant of tenants(); track tenant.tenantId) {
                <button class="tenant-card"
                        type="button"
                        (click)="selectTenant(tenant)">
                  <div class="tenant-card-name">{{ tenant.displayName }}</div>
                  <div class="tenant-card-slug">{{ tenant.slug }}</div>
                </button>
              }
            </div>
            <button class="ce-button variant-ghost size-sm" type="button" (click)="backToLogin()" style="margin-top: var(--spacing-3);">
              &#8592; Use a different account
            </button>
          </div>
        } @else {
          <form class="login-form" [formGroup]="loginForm" (ngSubmit)="onSubmit()">
            <div class="ce-input-group">
              <label class="ce-input-label" for="login-email">Email</label>
              <div class="ce-input-wrapper" [class.has-error]="loginForm.get('email')?.invalid && loginForm.get('email')?.touched">
                <input id="login-email"
                       class="ce-input"
                       type="email"
                       formControlName="email"
                       autocomplete="email"
                       placeholder="you@controleasy.app"
                       (blur)="onEmailBlur()" />
              </div>
              @if (loginForm.get('email')?.invalid && loginForm.get('email')?.touched) {
                <div class="ce-input-error">Please enter a valid email address.</div>
              }
            </div>

            <div class="ce-input-group">
              <label class="ce-input-label" for="login-password">Password</label>
              <div class="ce-input-wrapper" [class.has-error]="loginForm.get('password')?.invalid && loginForm.get('password')?.touched">
                <input id="login-password"
                       class="ce-input"
                       [type]="showPassword() ? 'text' : 'password'"
                       formControlName="password"
                       autocomplete="current-password"
                       placeholder="••••••••" />
                <button type="button"
                        class="ce-input-suffix"
                        (click)="showPassword.set(!showPassword())"
                        [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'"
                        tabindex="-1">
                  {{ showPassword() ? '&#9673;' : '&#9678;' }}
                </button>
              </div>
              @if (loginForm.get('password')?.invalid && loginForm.get('password')?.touched) {
                <div class="ce-input-error">Password is required.</div>
              }
            </div>

            <label class="ce-checkbox">
              <input type="checkbox" formControlName="remember" />
              <span class="ce-checkbox-box"></span>
              <span>Remember my email</span>
            </label>

            <button class="ce-button variant-primary size-lg"
                    type="submit"
                    [class.disabled]="loginForm.invalid || submitting()"
                    [disabled]="loginForm.invalid || submitting()"
                    [attr.aria-busy]="submitting()"
                    style="margin-top: var(--spacing-2);">
              @if (submitting()) {
                <span class="ce-spinner tone-current size-sm" style="color: white;"></span>
              }
              Sign in
            </button>
          </form>

          @if (tenantHint()) {
            <div class="tenant-hint" style="margin-top: var(--spacing-3);">
              <span class="text-secondary text-xs">{{ tenantHint() }}</span>
            </div>
          }

          @if (demoInfo.enabled()) {
            <details class="demo-shortcuts">
              <summary>Try a demo account</summary>
              <p class="demo-shortcuts-hint">Password: <code>demo123</code> — click to fill, then Sign in.</p>
              <div class="demo-shortcuts-grid">
                @for (account of demoAccounts; track account.email) {
                  <button type="button" class="ce-button variant-ghost size-sm demo-shortcut-btn"
                          (click)="fillDemoAccount(account.email)">
                    {{ account.label }}
                  </button>
                }
              </div>
            </details>
          }
        }
      </div>
    </main>
  `,
  styles: [`
    .login-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      background: var(--color-background);
      padding: var(--spacing-4);
      position: relative;
    }
    .login-page::before {
      content: "";
      position: absolute;
      inset: 0;
      background:
        radial-gradient(ellipse 80% 50% at 50% -20%, color-mix(in oklch, var(--color-primary) 12%, transparent), transparent),
        radial-gradient(ellipse 60% 50% at 80% 100%, color-mix(in oklch, var(--color-info) 8%, transparent), transparent);
      pointer-events: none;
      z-index: 0;
    }
    .login-page > * { position: relative; z-index: 1; }

    .login-card {
      width: 100%;
      max-width: 28rem;
      background: var(--color-surface-elevated);
      border-radius: var(--radius-xl);
      padding: var(--spacing-8);
      box-shadow: var(--shadow-xl);
      border: 1px solid var(--color-border);
    }
    .login-brand {
      display: flex;
      align-items: center;
      gap: var(--spacing-3);
      margin-bottom: var(--spacing-6);
    }
    .login-brand-mark {
      width: 3rem;
      height: 3rem;
      background: linear-gradient(135deg, var(--color-primary), var(--color-primary-hover));
      color: white;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      border-radius: var(--radius-lg);
      font-weight: var(--font-weight-bold);
      font-size: 1.5rem;
      box-shadow: var(--shadow-primary-glow);
    }
    .login-brand-text {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-primary);
    }
    .login-title {
      font-size: var(--font-size-2xl);
      margin-bottom: var(--spacing-1);
    }
    .login-subtitle {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      margin-bottom: var(--spacing-6);
    }

    .login-form {
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
    .ce-input-suffix {
      display: flex;
      align-items: center;
      padding: 0 var(--spacing-3);
      color: var(--color-text-muted);
      background: transparent;
      border: 0;
      cursor: pointer;
      font-size: 1.1rem;
    }

    .ce-checkbox {
      display: inline-flex;
      align-items: center;
      gap: var(--spacing-2);
      cursor: pointer;
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
      user-select: none;
      min-height: 44px;
    }
    .ce-checkbox input { position: absolute; opacity: 0; pointer-events: none; }
    .ce-checkbox-box {
      width: 1.25rem;
      height: 1.25rem;
      border: 1.5px solid var(--color-border);
      background: var(--color-surface);
      border-radius: var(--radius-sm);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      transition: background var(--duration-fast), border-color var(--duration-fast);
      flex-shrink: 0;
    }
    .ce-checkbox input:checked + .ce-checkbox-box {
      background: var(--color-primary);
      border-color: var(--color-primary);
    }
    .ce-checkbox input:checked + .ce-checkbox-box::after {
      content: "";
      width: 0.4rem;
      height: 0.7rem;
      border: solid white;
      border-width: 0 2px 2px 0;
      transform: rotate(45deg) translateY(-1px);
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
    .ce-button.size-lg { height: 3rem; padding: 0 var(--spacing-6); font-size: var(--font-size-base); }
    .ce-button.variant-primary {
      background: var(--color-primary);
      color: white;
      box-shadow: var(--shadow-sm);
    }
    .ce-button.variant-primary:hover:not(:disabled) {
      background: var(--color-primary-hover);
      transform: translateY(-1px);
      box-shadow: var(--shadow-primary-glow);
    }
    .ce-button.variant-ghost { background: transparent; color: var(--color-text-primary); }
    .ce-button.variant-ghost:hover:not(:disabled) { background: var(--color-neutral-light); }
    .ce-button:disabled, .ce-button[aria-busy="true"] { opacity: 0.6; cursor: not-allowed; pointer-events: none; }
    .ce-button.disabled { opacity: 0.6; cursor: not-allowed; pointer-events: none; }

    .login-error {
      padding: var(--spacing-3) var(--spacing-4);
      background: var(--color-danger-light);
      border: 1px solid color-mix(in oklch, var(--color-danger) 30%, transparent);
      border-radius: var(--radius-md);
      color: var(--color-danger);
      font-size: var(--font-size-sm);
      display: flex;
      align-items: flex-start;
      gap: var(--spacing-2);
      font-weight: var(--font-weight-medium);
      margin-bottom: var(--spacing-4);
    }

    .tenant-picker-section {
      margin-top: var(--spacing-4);
    }
    .tenant-picker-label {
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
      margin-bottom: var(--spacing-3);
    }
    .tenant-picker-grid {
      display: grid;
      gap: var(--spacing-3);
    }
    @media (min-width: 640px) {
      .tenant-picker-grid { grid-template-columns: 1fr 1fr; }
    }
    .tenant-card {
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-left: 4px solid var(--color-primary);
      border-radius: var(--radius-lg);
      padding: var(--spacing-4);
      cursor: pointer;
      text-align: left;
      font-family: inherit;
      transition: transform var(--duration-fast), box-shadow var(--duration-fast), border-color var(--duration-fast);
    }
    .tenant-card:hover {
      transform: translateY(-2px);
      box-shadow: var(--shadow-md);
      border-color: var(--color-primary);
    }
    .tenant-card-name {
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-primary);
      margin-bottom: var(--spacing-1);
    }
    .tenant-card-slug {
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
    }

    .tenant-hint {
      text-align: center;
    }

    .text-secondary { color: var(--color-text-secondary); }
    .text-xs { font-size: var(--font-size-xs); }

    .ce-spinner {
      display: inline-block;
      border-radius: var(--radius-full);
      border: 2px solid currentColor;
      border-top-color: transparent;
      animation: spin-slow 1.4s linear infinite;
    }
    .ce-spinner.size-sm { width: 1rem; height: 1rem; border-width: 2px; }
    .ce-spinner.tone-current { color: currentColor; }

    @keyframes spin-slow { to { transform: rotate(360deg); } }

    .demo-shortcuts {
      margin-top: var(--spacing-5);
      padding-top: var(--spacing-4);
      border-top: 1px solid var(--color-border);
    }
    .demo-shortcuts summary {
      cursor: pointer;
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-medium);
      color: var(--color-primary);
    }
    .demo-shortcuts-hint {
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      margin: var(--spacing-2) 0 var(--spacing-3);
    }
    .demo-shortcuts-grid {
      display: flex;
      flex-wrap: wrap;
      gap: var(--spacing-2);
    }
    .demo-shortcut-btn { flex: 1 1 calc(50% - var(--spacing-2)); min-width: 8rem; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage implements OnDestroy {
  private readonly authService = inject(AuthService);
  readonly demoInfo = inject(DemoInfoService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly destroy$ = new Subject<void>();
  private readonly emailChange$ = new Subject<string>();

  readonly demoAccounts = DEMO_ACCOUNTS;

  loginForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    remember: [false],
  });

  loginError = signal<string | null>(null);
  submitting = signal(false);
  showPassword = signal(false);
  tenants = signal<TenantLookupResponse[]>([]);
  tenantPickerVisible = signal(false);
  tenantHint = signal<string | null>(null);

  constructor() {
    const remembered = this.authService.getRememberedEmail();
    if (remembered) {
      this.loginForm.patchValue({ email: remembered, remember: true });
    }

    this.emailChange$.pipe(
      debounceTime(400),
      distinctUntilChanged(),
      switchMap((email) => {
        if (!email || !this.isValidEmail(email)) {
          this.tenantHint.set(null);
          return [];
        }
        return this.authService.lookupTenants(email);
      }),
      takeUntil(this.destroy$),
    ).subscribe({
      next: (tenants) => {
        if (tenants.length > 0) {
          this.tenantHint.set(`${tenants.length} condominium${tenants.length > 1 ? 's' : ''} found for this email.`);
        } else {
          this.tenantHint.set(null);
        }
      },
      error: () => this.tenantHint.set(null),
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onEmailBlur(): void {
    const email = this.loginForm.get('email')?.value;
    if (email && this.isValidEmail(email)) {
      this.emailChange$.next(email);
    }
  }

  onSubmit(): void {
    if (this.loginForm.invalid) return;
    this.loginError.set(null);
    this.submitting.set(true);

    const { email, password, remember } = this.loginForm.value;
    this.authService.setRememberedEmail(email, remember);

    this.authService.login(email, password).subscribe({
      next: () => {
        this.authService.lookupTenants(email).subscribe({
          next: (tenantList) => {
            this.submitting.set(false);
            if (this.authService.mustChangePassword()) {
              this.router.navigate(['/change-password']);
              return;
            }
            if (tenantList.length > 1) {
              this.tenants.set(tenantList);
              this.tenantPickerVisible.set(true);
              return;
            }
            this.router.navigate(['/']);
          },
          error: () => {
            this.submitting.set(false);
            this.router.navigate(['/']);
          },
        });
      },
      error: (err) => {
        this.submitting.set(false);
        this.loginError.set(this.extractErrorMessage(err));
      },
    });
  }

  selectTenant(tenant: TenantLookupResponse): void {
    this.authService.setToken(this.authService.accessToken()!);
    this.submitting.set(true);
    this.authService.switchTenant(tenant.tenantId).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.loginError.set(this.extractErrorMessage(err));
      },
    });
  }

  backToLogin(): void {
    this.tenantPickerVisible.set(false);
    this.tenants.set([]);
    this.tenantHint.set(null);
  }

  fillDemoAccount(email: string): void {
    this.loginForm.patchValue({ email, password: 'demo123' });
    this.loginForm.markAsDirty();
    this.loginForm.updateValueAndValidity();
    this.loginError.set(null);
  }

  private isValidEmail(email: string): boolean {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  private extractErrorMessage(err: unknown): string {
    if (err && typeof err === 'object' && 'error' in err) {
      const httpErr = err as { error?: { detail?: string; title?: string }; status?: number; message?: string };
      if (httpErr.error?.detail) return httpErr.error.detail;
      if (httpErr.error?.title) return httpErr.error.title;
      if (httpErr.status === 401) return 'Invalid email or password.';
      if (httpErr.status === 0) return 'Unable to connect to the server. Please check your connection.';
    }
    return 'An unexpected error occurred. Please try again.';
  }
}