import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CeAuditFiltersComponent } from './audit-filters.component';
import type {
  AuditFilters,
  EntryState,
  SubjectType,
} from '../../../features/entry-log/entry-log.service';

describe('CeAuditFiltersComponent', () => {
  let fixture: ComponentFixture<CeAuditFiltersComponent>;
  let component: CeAuditFiltersComponent;

  const baseFilters: AuditFilters = { skip: 0, take: 50 };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeAuditFiltersComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(CeAuditFiltersComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('filters', baseFilters);
    fixture.detectChanges();
  });

  it('exposes 2 categories dropdown options for each filter', () => {
    const selects = fixture.nativeElement.querySelectorAll('select');
    expect(selects.length).toBe(2);
    expect(component.categoryOptions.length).toBeGreaterThan(1);
    expect(component.stateOptions.length).toBeGreaterThan(1);
  });

  it('emits updated filters with skip=0 when category changes', () => {
    let emitted: AuditFilters | undefined;
    component.filtersChanged.subscribe((f) => (emitted = f));

    const categorySelect =
      fixture.nativeElement.querySelectorAll('select')[0] as HTMLSelectElement;
    categorySelect.value = 'visitor';
    categorySelect.dispatchEvent(new Event('change'));

    expect(emitted).toBeDefined();
    expect(emitted!.subjectType).toBe('visitor' as SubjectType);
    expect(emitted!.skip).toBe(0);
  });

  it('emits updated filters with entryState when state changes', () => {
    let emitted: AuditFilters | undefined;
    component.filtersChanged.subscribe((f) => (emitted = f));

    const stateSelect =
      fixture.nativeElement.querySelectorAll('select')[1] as HTMLSelectElement;
    stateSelect.value = 'entered_override';
    stateSelect.dispatchEvent(new Event('change'));

    expect(emitted).toBeDefined();
    expect(emitted!.entryState).toBe('entered_override' as EntryState);
    expect(emitted!.skip).toBe(0);
  });

  it('emits fromUtc as ISO when From date changes', () => {
    let emitted: AuditFilters | undefined;
    component.filtersChanged.subscribe((f) => (emitted = f));

    const dateInputs =
      fixture.nativeElement.querySelectorAll('input[type=date]');
    const fromInput = dateInputs[0] as HTMLInputElement;
    fromInput.value = '2026-01-15';
    fromInput.dispatchEvent(new Event('change'));

    expect(emitted).toBeDefined();
    expect(emitted!.fromUtc).toBeTruthy();
    expect(new Date(emitted!.fromUtc!).toISOString()).toContain('2026-01-15');
  });

  it('reset emits fresh filter with skip=0 and original take', () => {
    let emitted: AuditFilters | undefined;
    component.filtersChanged.subscribe((f) => (emitted = f));

    const resetBtn = fixture.nativeElement.querySelector(
      'button',
    ) as HTMLButtonElement;
    resetBtn.click();

    expect(emitted).toBeDefined();
    expect(emitted!.skip).toBe(0);
    expect(emitted!.take).toBe(50);
  });

  it('exposes computed fromDate and toDate as YYYY-MM-DD slices', () => {
    fixture.componentRef.setInput('filters', {
      ...baseFilters,
      fromUtc: '2026-02-01T00:00:00.000Z',
      toUtc: '2026-02-15T23:59:59.000Z',
    });
    fixture.detectChanges();
    expect(component.fromDate()).toBe('2026-02-01');
    expect(component.toDate()).toBe('2026-02-15');
  });
});