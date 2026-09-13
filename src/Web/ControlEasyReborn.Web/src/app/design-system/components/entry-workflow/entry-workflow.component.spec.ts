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

  it('register tile opens photo capture for a consented visitor', () => {
    component['onTileTap']('register');

    expect(component['showCapture']()).toBe(true);
    expect(component['pendingState']()).toBe('entered_with_consent');
  });

  it('denied tile opens photo capture before subject info', () => {
    component['onTileTap']('denied');
    expect(component['pendingState']()).toBe('entered_without_consent');
    expect(component['showCapture']()).toBe(true);
    expect(component['selectedCategory']()).toBe('visitor');
  });

  it('allows upload fallback when camera access is unavailable', () => {
    component['onTileTap']('denied');
    component['onCaptureModeChange']('upload');

    expect(component['captureMode']()).toBe('upload');
  });

  it('gatehouse tile forces service_provider subject type (matches backend)', () => {
    component['onTileTap']('gatehouse');
    expect(component['pendingState']()).toBe('gatehouse_only');
    expect(component['step']()).toBe('subject-info');
    expect(component['selectedCategory']()).toBe('service_provider');
  });

  it('override tile opens reason modal', () => {
    component['onTileTap']('override');
    expect(component['pendingState']()).toBe('entered_override');
    expect(component['showOverrideReason']()).toBe(true);
  });

  it('continue closes the dashboard workflow after a successful entry', fakeAsync(() => {
    let closed = false;
    component.closed.subscribe(() => (closed = true));
    fixture.componentRef.setInput('closeOnEntry', true);
    component['onTileTap']('denied');
    component['onPhotoUploaded']({ id: 'photo-1' });
    component['subjectName'].set('Refused Person');
    component['continue']();
    tick();
    const createReq = httpMock.expectOne('/api/v1/entry-log');
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.subjectType).toBe('visitor');
    expect(createReq.request.body.entryState).toBe('entered_without_consent');
    expect(createReq.request.body.subjectName).toBe('Refused Person');
    expect(createReq.request.body.photoId).toBe('photo-1');
    createReq.flush({
      id: 'e-1',
      tenantId: 't-1',
      entryState: 'entered_without_consent',
      subjectType: 'visitor',
      recordedAt: new Date().toISOString(),
    });
    tick();
    expect(component['step']()).toBe('tiles');
    expect(closed).toBe(true);
  }));

  it('continue surfaces toast and stays open on error', fakeAsync(() => {
    component['onTileTap']('denied');
    component['onPhotoUploaded']({ id: 'photo-1' });
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
