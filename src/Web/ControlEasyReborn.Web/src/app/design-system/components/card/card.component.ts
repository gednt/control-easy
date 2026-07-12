import { Component, input, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'ce-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="ce-card" [class]="hostClass()">
      <ng-content select="[ce-card-header], [card-header]" />
      <div class="ce-card-body" [class.unpadded]="!padded()">
        <ng-content />
      </div>
      <ng-content select="[ce-card-footer], [card-footer]" />
    </div>
  `,
  styles: [`
    .ce-card {
      background: var(--color-surface-elevated);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-card);
      overflow: hidden;
    }
    .ce-card.accent-primary { border-left: 4px solid var(--color-primary); }
    .ce-card.accent-success { border-left: 4px solid var(--color-success); }
    .ce-card.accent-warning { border-left: 4px solid var(--color-warning); }
    .ce-card.accent-danger { border-left: 4px solid var(--color-danger); }
    .ce-card.accent-info { border-left: 4px solid var(--color-info); }
    .ce-card-body {
      padding: var(--space-6);
    }
    .ce-card-body.unpadded {
      padding: 0;
    }
    :host ::ng-deep [card-header], :host ::ng-deep [ce-card-header] {
      padding: var(--space-4) var(--space-6);
      border-bottom: 1px solid var(--color-border);
      font-weight: var(--font-weight-semibold);
    }
    :host ::ng-deep [card-footer], :host ::ng-deep [ce-card-footer] {
      padding: var(--space-4) var(--space-6);
      border-top: 1px solid var(--color-border);
    }
  `],
})
export class CeCardComponent {
  accent = input<'primary' | 'success' | 'warning' | 'danger' | 'info' | null>(null);
  padded = input(true);

  hostClass(): string {
    const classes: string[] = [];
    const a = this.accent();
    if (a) classes.push(`accent-${a}`);
    return classes.join(' ');
  }
}