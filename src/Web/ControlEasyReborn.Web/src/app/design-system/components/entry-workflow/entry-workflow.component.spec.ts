import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
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

  function flushVisitorPolicy(photoRequired: boolean): void {
    const policyReq = httpMock.expectOne('/api/v1/consent-policy/visitors');
    expect(policyReq.request.method).toBe('GET');
    policyReq.flush({
      id: 'p-1',
      tenantId: 't-1',
      subjectCategory: 'visitors',
      photoRequired,
      createdAtUtc: new Date().toISOString(),
    });
  }

  function resident(overrides: Partial<{ id: string; name: string; cpf: string; active: boolean }> = {}) {
    return {
      id: 'b42c6db0-3c39-4f49-aec4-8ce8c98f3ad8',
      tenantId: 't-1',
      name: 'Ana Silva',
      cpf: '12345678909',
      email: 'ana@example.com',
      phone: '11999999999',
      apartmentId: 'a-1',
      active: true,
      createdAtUtc: new Date().toISOString(),
      ...overrides,
    };
  }

  it('renders entry and exit tiles', () => {
    const tiles: NodeListOf<HTMLButtonElement> = fixture.nativeElement.querySelectorAll('[data-action]');
    const actions = Array.from(tiles).map((t) => t.getAttribute('data-action'));
    expect(actions.sort()).toEqual(['denied', 'exit', 'gatehouse', 'override', 'register']);
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

  it('denied tile advances directly to subject info when the policy does not require a photo', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();
    flushVisitorPolicy(false);
    tick();

    expect(component.pendingState()).toBe('entered_without_consent');
    expect(component.showCapture()).toBe(false);
    expect(component.step()).toBe('subject-info');
    expect(component.selectedCategory()).toBe('visitor');
  }));

  it('denied tile opens photo capture when the policy requires a photo', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();
    flushVisitorPolicy(true);
    tick();

    expect(component.pendingState()).toBe('entered_without_consent');
    expect(component.showCapture()).toBe(true);
  }));

  it('gatehouse tile forces service_provider subject type', fakeAsync(() => {
    void component.onTileTap('gatehouse');
    tick();

    expect(component.pendingState()).toBe('gatehouse_only');
    expect(component.step()).toBe('subject-info');
    expect(component.selectedCategory()).toBe('service_provider');
  }));

  it('exit tile records resident or vehicle exits without requiring a photo', fakeAsync(() => {
    void component.onTileTap('exit');
    tick();

    expect(component.pendingState()).toBe('exited');
    expect(component.showCapture()).toBe(false);
    expect(component.step()).toBe('subject-info');
    expect(component.selectedCategory()).toBe('dweller');

    component.onCategorySelect('vehicle');
    expect(component.selectedCategory()).toBe('vehicle');
    component.onCategorySelect('visitor');
    expect(component.selectedCategory()).toBe('vehicle');
  }));

  it('finds an active resident by formatted CPF and fills the known record', fakeAsync(() => {
    void component.onTileTap('exit');
    tick();

    component.onResidentLookupInput('123.456.789-09');
    void component.findResident();
    tick();

    const lookupRequest = httpMock.expectOne(
      (request) =>
        request.url === '/api/v1/residents' &&
        request.params.get('search') === '12345678909' &&
        request.params.get('skip') === '0' &&
        request.params.get('take') === '10',
    );
    lookupRequest.flush([resident()]);
    tick();

    expect(component.selectedResident()?.id).toBe('b42c6db0-3c39-4f49-aec4-8ce8c98f3ad8');
    expect(component.subjectName()).toBe('Ana Silva');
    expect(component.subjectDocument()).toBe('12345678909');
  }));

  it('finds an active resident directly by resident ID for QR lookup', fakeAsync(() => {
    void component.onTileTap('exit');
    tick();

    component.onResidentLookupInput('b42c6db0-3c39-4f49-aec4-8ce8c98f3ad8');
    void component.findResident();
    tick();

    const lookupRequest = httpMock.expectOne('/api/v1/residents/b42c6db0-3c39-4f49-aec4-8ce8c98f3ad8');
    expect(lookupRequest.request.method).toBe('GET');
    lookupRequest.flush(resident());
    tick();

    expect(component.selectedResident()?.name).toBe('Ana Silva');
    expect(component.subjectDocument()).toBe('12345678909');
  }));

  it('shows a clear lookup message when no active resident matches', fakeAsync(() => {
    void component.onTileTap('exit');
    tick();

    component.onResidentLookupInput('00000000000');
    void component.findResident();
    tick();

    const lookupRequest = httpMock.expectOne((request) => request.url === '/api/v1/residents');
    lookupRequest.flush([]);
    tick();

    expect(component.selectedResident()).toBeNull();
    expect(component.residentSearchError()).toContain('No active resident');
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
    flushVisitorPolicy(false);
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
    flushVisitorPolicy(false);
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
    expect(createReq.request.body.apartmentId).toBeUndefined();
    expect(createReq.request.body.residentId).toBeUndefined();

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

  it('continue sends the resident apartmentId and residentId when a resident is selected', fakeAsync(() => {
    void component.onTileTap('exit');
    tick();

    component.onResidentLookupInput('12345678909');
    void component.findResident();
    tick();

    const lookupRequest = httpMock.expectOne((request) => request.url === '/api/v1/residents');
    lookupRequest.flush([resident()]);
    tick();

    void component.continue();
    tick();

    const createReq = httpMock.expectOne('/api/v1/entry-log');
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.subjectType).toBe('dweller');
    expect(createReq.request.body.entryState).toBe('exited');
    expect(createReq.request.body.apartmentId).toBe('a-1');
    expect(createReq.request.body.residentId).toBe('b42c6db0-3c39-4f49-aec4-8ce8c98f3ad8');
    expect(createReq.request.body.vehicleId).toBeUndefined();

    createReq.flush({
      id: 'e-2',
      tenantId: 't-1',
      entryState: 'exited',
      subjectType: 'dweller',
      apartmentId: 'a-1',
      recordedAt: new Date().toISOString(),
    });
    tick();
  }));

  it('continue surfaces toast and stays open on error', fakeAsync(() => {
    void component.onTileTap('denied');
    tick();
    flushVisitorPolicy(false);
    tick();

    void component.continue();
    tick();

    const createReq = httpMock.expectOne('/api/v1/entry-log');
    createReq.flush({ detail: 'Invalid request' }, { status: 400, statusText: 'Bad Request' });
    tick();

    expect(component.loading()).toBe(false);
    expect(component.step()).toBe('subject-info');
  }));
});
