import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { CeEntryWorkflowComponent } from './entry-workflow.component';
import { CeToastHostComponent } from '../toast/toast.component';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';
import type {
  ConsentPolicyResponse,
} from '../../../features/consent-policy/consent-policy.service';

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

  it('register tile with photo-required policy opens ce-photo-capture', fakeAsync(() => {
    component['onTileTap']('register');
    tick();
    const consents = httpMock.match(
      (req) => req.url.includes('/api/v1/consent-policy/') && req.method === 'GET',
    );
    // Respond to all 4 per-category GETs with photoRequired=true for visitors.
    for (const req of consents) {
      const cat = req.request.url.split('/').pop();
      const body: ConsentPolicyResponse = {
        id: `id-${cat}`,
        tenantId: 'tenant-1',
        subjectCategory: cat as ConsentPolicyResponse['subjectCategory'],
        photoRequired: cat === 'visitors',
        createdAtUtc: new Date().toISOString(),
      };
      req.flush(body);
    }
    tick();
    fixture.detectChanges();

    expect(component['showCapture']()).toBe(true);
  }));

  it('register tile with no photo-required policy jumps to subject-info', fakeAsync(() => {
    component['onTileTap']('register');
    tick();
    const consents = httpMock.match(
      (req) => req.url.includes('/api/v1/consent-policy/') && req.method === 'GET',
    );
    // Visitors policy missing (404) -> no photo required
    for (const req of consents) {
      req.flush(null, { status: 404, statusText: 'Not Found' });
    }
    tick();
    fixture.detectChanges();

    expect(component['showCapture']()).toBe(false);
    expect(component['step']()).toBe('subject-info');
    expect(component['pendingState']()).toBe('entered_with_consent');
  }));

  it('denied tile skips camera and goes to subject-info', () => {
    component['onTileTap']('denied');
    expect(component['pendingState']()).toBe('denied');
    expect(component['step']()).toBe('subject-info');
    expect(component['showCapture']()).toBe(false);
    expect(component['selectedCategory']()).toBe('visitor');
  });

  it('gatehouse tile forces service-provider subject type', () => {
    component['onTileTap']('gatehouse');
    expect(component['pendingState']()).toBe('gatehouse_only');
    expect(component['step']()).toBe('subject-info');
    expect(component['selectedCategory']()).toBe('service-provider');
  });

  it('override tile opens reason modal', () => {
    component['onTileTap']('override');
    expect(component['pendingState']()).toBe('entered_override');
    expect(component['showOverrideReason']()).toBe(true);
  });

  it('continue POSTs the entry and emits entryLogged on success', fakeAsync(() => {
    component['onTileTap']('denied');
    component['onPhotoUploaded']?.({} as any);
    component['subjectName'].set('Refused Person');
    component['continue']();
    tick();
    const createReq = httpMock.expectOne('/api/v1/entry-log');
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.subjectType).toBe('visitor');
    expect(createReq.request.body.entryState).toBe('denied');
    expect(createReq.request.body.subjectName).toBe('Refused Person');
    createReq.flush({
      id: 'e-1',
      tenantId: 't-1',
      entryState: 'denied',
      subjectType: 'visitor',
      recordedAt: new Date().toISOString(),
    });
    tick();
    expect(component['step']()).toBe('tiles');
  }));

  it('continue surfaces toast and stays open on error', fakeAsync(() => {
    component['onTileTap']('denied');
    component['continue']();
    tick();
    const createReq = httpMock.expectOne('/api/v1/entry-log');
    createReq.flush(
      { detail: 'Invalid request' },
      { status: 400, statusText: 'Bad Request' },
    );
    tick();
    expect(component['loading']()).toBe(false);
    expect(component['step']()).toBe('subject-info');
  }));
});