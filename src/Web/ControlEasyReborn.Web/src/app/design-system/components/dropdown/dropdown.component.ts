import { Component, input, output, ChangeDetectionStrategy, signal, inject, ElementRef } from '@angular/core';

@Component({
  selector: 'ce-dropdown',
  standalone: true,
  imports: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ce-dropdown" (click)="toggle()" (keydown.escape)="close()">
      <ng-content select="[ceDropdownTrigger], [dropdown-trigger]" />
      @if (isOpen()) {
        <div class="ce-dropdown-panel" role="menu" (keydown.escape)="close()" (click)="$event.stopPropagation()">
          <ng-content />
        </div>
      }
    </div>
  `,
  styles: [`
    .ce-dropdown { display: inline-block; position: relative; }
    .ce-dropdown-panel {
      position: absolute;
      top: 100%;
      left: 0;
      margin-top: var(--space-2);
      z-index: 50;
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-xl);
      padding: var(--space-1);
      min-width: 10rem;
      animation: fade-in var(--duration-fast) var(--ease-out);
    }
    :host ::ng-deep [ceDropdownTrigger], :host ::ng-deep [dropdown-trigger] {
      cursor: pointer;
    }
    :host ::ng-deep button[role="menuitem"],
    :host ::ng-deep a[role="menuitem"] {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      width: 100%;
      padding: var(--space-2) var(--space-3);
      border: 0;
      background: transparent;
      color: var(--color-text-primary);
      font-size: var(--font-size-sm);
      font-family: inherit;
      border-radius: var(--radius-md);
      cursor: pointer;
      text-align: left;
      transition: background var(--duration-fast) var(--ease-out);
    }
    :host ::ng-deep button[role="menuitem"]:hover,
    :host ::ng-deep a[role="menuitem"]:hover {
      background: var(--color-neutral-light);
    }
  `],
  host: {
    '(document:click)': 'onDocumentClick($event)',
  },
})
export class CeDropdownComponent {
  isOpen = signal(false);
  openChange = output<boolean>();

  private elementRef = inject(ElementRef);

  toggle(): void {
    this.isOpen.update(v => !v);
    this.openChange.emit(this.isOpen());
  }

  close(): void {
    if (this.isOpen()) {
      this.isOpen.set(false);
      this.openChange.emit(false);
    }
  }

  onDocumentClick(event: Event): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.close();
    }
  }
}