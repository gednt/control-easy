import { ComponentFixture, TestBed } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { CeEntryStateBadgeComponent } from './entry-state-badge.component';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';
import type { EntryState } from '../../../features/entry-log/entry-log.service';

describe('CeEntryStateBadgeComponent', () => {
  let fixture: ComponentFixture<CeEntryStateBadgeComponent>;
  let component: CeEntryStateBadgeComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeEntryStateBadgeComponent],
      providers: [importProvidersFrom(CE_LUCIDE_ICONS)],
    }).compileComponents();

    fixture = TestBed.createComponent(CeEntryStateBadgeComponent);
    component = fixture.componentInstance;
  });

  const cases: { state: EntryState; label: string; tone: string }[] = [
    { state: 'entered_with_consent', label: 'With consent', tone: 'success' },
    { state: 'entered_override', label: 'Override', tone: 'warning' },
    { state: 'gatehouse_only', label: 'No entry', tone: 'neutral' },
    { state: 'entered_without_consent', label: 'Refused', tone: 'danger' },
  ];

  for (const c of cases) {
    it(`renders label "${c.label}" with tone "${c.tone}" for state "${c.state}"`, () => {
      fixture.componentRef.setInput('state', c.state);
      fixture.detectChanges();

      const host: HTMLElement = fixture.nativeElement;
      const badge = host.querySelector('.ce-entry-state-badge')!;
      expect(badge).toBeTruthy();
      expect(badge.getAttribute('data-tone')).toBe(c.tone);
      expect(badge.getAttribute('aria-label')).toBe(c.label);
      expect(badge.querySelector('.label')?.textContent).toContain(c.label);
    });
  }

  it('updates reactively when state input changes', () => {
    fixture.componentRef.setInput('state', 'entered_without_consent');
    fixture.detectChanges();
    expect(component.meta().label).toBe('Refused');
    expect(component.meta().tone).toBe('danger');

    fixture.componentRef.setInput('state', 'entered_with_consent');
    fixture.detectChanges();
    expect(component.meta().label).toBe('With consent');
    expect(component.meta().tone).toBe('success');
  });
});
