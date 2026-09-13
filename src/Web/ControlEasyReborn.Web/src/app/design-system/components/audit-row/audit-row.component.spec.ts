import { ComponentFixture, TestBed } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { CeAuditRowComponent } from './audit-row.component';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';
import type {
  EntryLogResponse,
  EntryState,
} from '../../../features/entry-log/entry-log.service';

describe('CeAuditRowComponent', () => {
  let fixture: ComponentFixture<CeAuditRowComponent>;
  let component: CeAuditRowComponent;

  const baseEntry: EntryLogResponse = {
    id: 'e-1',
    tenantId: 't-1',
    entryState: 'entered_with_consent' as EntryState,
    subjectType: 'visitor',
    subjectName: 'Jane Doe',
    photoId: 'p-1',
    recordedAt: '2026-02-15T14:30:00.123Z',
    performedByProfileId: 'profile-1',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeAuditRowComponent],
      providers: [importProvidersFrom(CE_LUCIDE_ICONS)],
    }).compileComponents();

    fixture = TestBed.createComponent(CeAuditRowComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('entry', baseEntry);
    fixture.detectChanges();
  });

  it('renders the subject name', () => {
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Jane Doe');
  });

  it('renders the entry-state badge', () => {
    const badge: HTMLElement | null =
      fixture.nativeElement.querySelector('ce-entry-state-badge');
    expect(badge).toBeTruthy();
  });

  it('shows a photo for entries with photoId', () => {
    const photo = fixture.nativeElement.querySelector('ce-photo');
    expect(photo).toBeTruthy();
  });

  it('shows em-dash placeholder when no photoId', () => {
    fixture.componentRef.setInput('entry', {
      ...baseEntry,
      photoId: undefined,
    });
    fixture.detectChanges();
    const photo = fixture.nativeElement.querySelector('ce-photo');
    expect(photo).toBeNull();
    const noPhoto = fixture.nativeElement.querySelector('.no-photo');
    expect(noPhoto).toBeTruthy();
    expect(noPhoto.textContent).toContain('—');
  });

  it('applies override-row class only when entryState is entered_override', () => {
    fixture.componentRef.setInput('entry', {
      ...baseEntry,
      entryState: 'entered_override',
      overrideReason: 'emergency',
    });
    fixture.detectChanges();
    const row = fixture.nativeElement.querySelector('tr');
    expect(row.classList.contains('override-row')).toBe(true);
  });

  it('does NOT apply override-row class for other states', () => {
    fixture.componentRef.setInput('entry', {
      ...baseEntry,
       entryState: 'entered_without_consent',
    });
    fixture.detectChanges();
    const row = fixture.nativeElement.querySelector('tr');
    expect(row.classList.contains('override-row')).toBe(false);
  });

  it('exposes a photoClicked output that can be subscribed', () => {
    const clicked = jasmine.createSpy('clicked');
    component.photoClicked.subscribe(clicked);
    fixture.componentRef.setInput('entry', baseEntry);
    fixture.detectChanges();
    // The OutputEmitterRef is created and accepts subscribers. We assert
    // by binding a subscriber without error.
    expect(typeof component.photoClicked.subscribe).toBe('function');
  });

  it('formats subject type with leading capital', () => {
    expect(component.subjectTypeLabel()).toBe('Visitor');
    fixture.componentRef.setInput('entry', {
      ...baseEntry,
      subjectType: 'service_provider',
    });
    fixture.detectChanges();
    expect(component.subjectTypeLabel()).toBe('Service provider');
  });
});
