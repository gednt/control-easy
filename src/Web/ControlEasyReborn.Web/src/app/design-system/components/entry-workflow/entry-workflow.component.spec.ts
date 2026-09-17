import { ComponentFixture, TestBed, fakeAsync, tick, flushMicrotasks } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { CeEntryWorkflowComponent } from './entry-workflow.component';
import { CeToastHostComponent } from '../toast/toast.component';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';

describe('CeEntryWorkflowComponent', () => {
  let fixture: ComponentFixture<CeEntryWorkflowComponent>;
  let component: CeEntryWorkflowComponent;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeEntryWorkflowComponent, CeToastHostComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        importProvidersFrom(CE_LUCIDE_ICONS),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(CeEntryWorkflowComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('renders 4 tiles', () => {
    const tiles: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('[data-action]');
    const actions = Array.from(tiles).map((t) =>
      t.getAttribute('data-action'),
    );
    expect(actions.sort()).toEqual(
      ['denied', 'gatehouse', 'override', 'register'],
    );
  });

  it('register tile opens photo capture when consent policy requires photo', fakeAsync(() => {
    void component.onTileTap('register');
    tick();

    const policyReq = httpMock.expectOne('/api/v1/consent-policy/visitors');
    expect(policyReq.request.method).toBe('GET');
    policyReq.flush({
      id: 'p-1',
      tenantId: 't-1',
      subjectCategory: 'visitors',
      photoRequired: true,
      createdAtUtc: new Date().toISOString(),
    });
    tick();

    expect(component.showCapture()).toBe(true);
    expect(component.pendingState()).toBe('entered_with_consent');
  }));

  it('register tile skips photo capture when consent policy does not require photo', fakeAsync(() => {
    void component.onTileTap('register');
    tick();

    const policyReq = httpMock.expectOne('/api/v1/consent-policy/visitors');
    expect(policyReq.request.method).toBe('GET');
    policyReq.flush({
      id: 'p-1',
      tenantId: 't-1',
      subjectCategory: 'visitors',
      photoRequired: false,
      createdAtUtc: new Date().toISOString(),
    });
    tick();

    expect(component.showCapture()).toBe(false);
    expect(component.pendingState()).toBe('entered_with_consent');
    expect(component.step()).toBe('subject-info');
  }));

  it('denied tile bypasses photo capture and advances directly to subject info', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();

    expect(component.pendingState()).toBe('entered_without_consent');
    expect(component.showCapture()).toBe(false);
    expect(component.step()).toBe('subject-info');
    expect(component.selectedCategory()).toBe('visitor');
  }));

  it('gatehouse tile forces service_provider subject type', fakeAsync(() => {
    void component.onTileTap('gatehouse');
    tick();

    expect(component.pendingState()).toBe('gatehouse_only');
    expect(component.step()).toBe('subject-info');
    expect(component.selectedCategory()).toBe('service_provider');
  }));

  it('override tile opens reason modal without opening camera', fakeAsync(() => {
    void component.onTileTap('override');
    tick();

    expect(component.pendingState()).toBe('entered_override');
    expect(component.showOverrideReason()).toBe(true);
    expect(component.showCapture()).toBe(false);
  }));

  it('close method emits closed event to host', () => {
    let closed = false;
    component.closed.subscribe(() => (closed = true));
    component.close();
    expect(closed).toBe(true);
  });

  it('allows category selection in subject-info for general entries', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();

    expect(component.step()).toBe('subject-info');
    component.onCategorySelect('dweller');
    expect(component.selectedCategory()).toBe('dweller');
  }));

  it('prevents selecting disallowed categories in gatehouse-only mode', fakeAsync(() => {
    void component.onTileTap('gatehouse');
    tick();

    expect(component.selectedCategory()).toBe('service_provider');
    component.onCategorySelect('visitor');
    // Category should remain service_provider because gatehouse_only only allows service_provider
    expect(component.selectedCategory()).toBe('service_provider');
  }));

  it('continue closes the dashboard workflow after a successful entry without photo on refusal', fakeAsync(() => {
    let closed = false;
    component.closed.subscribe(() => (closed = true));
    fixture.componentRef.setInput('closeOnEntry', true);

    void component.onTileTap('denied');
    tick();

    component.subjectName.set('Refused Person');
    void component.continue();
    tick();

    const createReq = httpMock.expectOne('/api/v1/entry-log');
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.subjectType).toBe('visitor');
    expect(createReq.request.body.entryState).toBe('entered_without_consent');
    expect(createReq.request.body.subjectName).toBe('Refused Person');
    expect(createReq.request.body.photoId).toBeUndefined();

    createReq.flush({
      id: 'e-1',
      tenantId: 't-1',
      entryState: 'entered_without_consent',
      subjectType: 'visitor',
      recordedAt: new Date().toISOString(),
    });
    tick();

    expect(component.step()).toBe('tiles');
    expect(closed).toBe(true);
  }));

  it('continue surfaces toast and stays open on error', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();

    void component.continue();
    tick();

    const createReq = httpMock.expectOne('/api/v1/entry-log');
    createReq.flush(
      { detail: 'Invalid request' },
      { status: 400, statusText: 'Bad Request' },
    );
    tick();

    expect(component.loading()).toBe(false);
    expect(component.step()).toBe('subject-info');
  }));

  it('exposes a polite live region for screen-reader announcements', () => {
    const region: HTMLElement | null =
      fixture.nativeElement.querySelector('[data-testid="entry-workflow-live-region"]');
    expect(region).not.toBeNull();
    expect(region!.getAttribute('role')).toBe('status');
    expect(region!.getAttribute('aria-live')).toBe('polite');
    expect(region!.getAttribute('aria-atomic')).toBe('true');
  });

  it('category selector uses role=radiogroup with roving tabindex for keyboard users', fakeAsync(() => {
    // Advance to subject-info step.
    component['step'].set('subject-info');
    fixture.detectChanges();
    tick();

    const group: HTMLElement | null =
      fixture.nativeElement.querySelector('[role="radiogroup"]');
    expect(group).not.toBeNull();
    expect(group!.getAttribute('aria-labelledby')).toBe('category-label');

    const radios: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('[role="radio"]');
    expect(radios.length).toBe(4);

    // Only the active radio is in the tab sequence.
    let focusableCount = 0;
    radios.forEach((r) => {
      if (r.getAttribute('tabindex') === '0') focusableCount++;
    });
    expect(focusableCount).toBe(1);

    // ArrowRight advances selection and ARIA checked state.
    const initial = component.selectedCategory();
    const event = new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true });
    group!.dispatchEvent(event);
    tick();
    fixture.detectChanges();

    expect(component.selectedCategory()).not.toBe(initial);
    const newActive = fixture.nativeElement.querySelector(
      `[role="radio"][aria-checked="true"]`,
    ) as HTMLButtonElement | null;
    expect(newActive).not.toBeNull();
  }));

  it('announces category selection through the live region', fakeAsync(() => {
    component['step'].set('subject-info');
    fixture.detectChanges();
    tick();

    component.onCategorySelect('dweller');
    tick();
    fixture.detectChanges();
    flushMicrotasks();
    fixture.detectChanges();

    const region: HTMLElement =
      fixture.nativeElement.querySelector('[data-testid="entry-workflow-live-region"]');
    expect(region.textContent || '').toContain('Resident');
  }));
});
