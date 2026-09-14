import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { AdministrationPage } from './administration.page';
import {
  AdministrationApiService,
  CondominiumSettingsDto,
  AuditLogResponse,
} from './administration-api.service';

describe('AdministrationPage', () => {
  let component: AdministrationPage;
  let fixture: ComponentFixture<AdministrationPage>;
  let apiMock: jasmine.SpyObj<AdministrationApiService>;

  const mockSettings: CondominiumSettingsDto = {
    id: 'test-settings-id',
    tenantId: 'test-tenant-id',
    visitDurationMinutes: 120,
    requireShiftHandoverNotes: true,
    defaultShiftLengthHours: 8,
    emergencyContactPhone: '+55 11 99999-0000',
    allowedVisitorStartHour: '06:00',
    allowedVisitorEndHour: '22:00',
    autoCheckoutAtMidnight: true,
    maxActiveVisitorsPerUnit: 5,
    photoRequiredVisitors: true,
    photoRequiredProviders: true,
    photoRequiredResidents: false,
    allowOverrideOnRefusal: true,
    overdueVisitAlertMinutes: 15,
    createdAtUtc: '2026-09-13T00:00:00Z',
    updatedAtUtc: null,
  };

  const mockLogs: AuditLogResponse[] = [
    {
      id: 'log-1',
      tenantId: 'test-tenant-id',
      category: 'Gatehouse',
      action: 'OverrideAuthorized',
      entityType: 'GatehouseEntry',
      entityId: 'entry-1',
      severity: 'SecurityAlert',
      performedByName: 'Attendant John',
      details: 'Override authorized for visitor',
      metadataJson: '{"reason":"emergency"}',
      createdAtUtc: '2026-09-13T12:00:00Z',
    },
    {
      id: 'log-2',
      tenantId: 'test-tenant-id',
      category: 'Settings',
      action: 'SettingsUpdated',
      entityType: 'CondominiumSettings',
      entityId: 'settings-1',
      severity: 'Info',
      performedByName: 'Admin Alice',
      details: 'Condominium settings updated',
      metadataJson: null,
      createdAtUtc: '2026-09-13T11:00:00Z',
    },
  ];

  beforeEach(async () => {
    apiMock = jasmine.createSpyObj<AdministrationApiService>('AdministrationApiService', [
      'getSettings',
      'updateSettings',
      'listAuditLogs',
      'getAuditLogById',
    ]);

    apiMock.getSettings.and.returnValue(of(mockSettings));
    apiMock.listAuditLogs.and.returnValue(of(mockLogs));
    apiMock.updateSettings.and.returnValue(of({ ...mockSettings, visitDurationMinutes: 180 }));

    await TestBed.configureTestingModule({
      imports: [AdministrationPage],
      providers: [
        { provide: AdministrationApiService, useValue: apiMock },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdministrationPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create and default to settings tab', () => {
    expect(component).toBeTruthy();
    expect(component.activeTab()).toBe('settings');
    expect(apiMock.getSettings).toHaveBeenCalled();
  });

  it('should populate settingsForm from loaded settings', () => {
    expect(component.settingsForm.value.visitDurationMinutes).toBe(120);
    expect(component.settingsForm.value.defaultShiftLengthHours).toBe(8);
    expect(component.settingsForm.value.allowedVisitorStartHour).toBe('06:00');
    expect(component.settingsForm.value.photoRequiredVisitors).toBe(true);
  });

  it('should switch tabs between settings and audit', () => {
    component.setTab('audit');
    expect(component.activeTab()).toBe('audit');

    component.setTab('settings');
    expect(component.activeTab()).toBe('settings');
  });

  it('should call updateSettings when valid form is saved', () => {
    component.settingsForm.patchValue({ visitDurationMinutes: 180 });
    component.settingsForm.markAsDirty();

    component.onSaveSettings();

    expect(apiMock.updateSettings).toHaveBeenCalled();
    expect(component.statusMessage()?.type).toBe('success');
  });

  it('should filter audit logs by category', () => {
    const event = { target: { value: 'Gatehouse' } } as unknown as Event;
    component.onCategoryChange(event);

    expect(component.selectedCategory()).toBe('Gatehouse');
    expect(apiMock.listAuditLogs).toHaveBeenCalled();
  });

  it('should inspect and close an audit event', () => {
    component.inspectEvent(mockLogs[0]!);
    expect(component.inspectedEvent()).toEqual(mockLogs[0]!);

    component.closeInspect();
    expect(component.inspectedEvent()).toBeNull();
  });

  it('should show an audit-load error and recover when retry succeeds', () => {
    apiMock.listAuditLogs.and.returnValue(throwError(() => new Error('Audit service unavailable')));

    component.loadAuditLogs();
    component.setTab('audit');
    fixture.detectChanges();

    expect(component.auditLogs()).toEqual([]);
    expect(component.auditLoadError()).toBe('Unable to load audit records. Please retry.');
    expect(fixture.nativeElement.textContent).toContain('Unable to load audit records');
    expect(fixture.nativeElement.textContent).not.toContain('No audit records found');

    apiMock.listAuditLogs.and.returnValue(of(mockLogs));
    component.loadAuditLogs();
    fixture.detectChanges();

    expect(component.auditLoadError()).toBeNull();
    expect(component.auditLogs()).toEqual(mockLogs);
    expect(fixture.nativeElement.textContent).toContain('OverrideAuthorized');
  });

  it('should ignore an older audit request that fails after a newer one succeeds', () => {
    const olderRequest = new Subject<AuditLogResponse[]>();
    const newerRequest = new Subject<AuditLogResponse[]>();
    apiMock.listAuditLogs.calls.reset();
    apiMock.listAuditLogs.and.returnValues(olderRequest, newerRequest);

    component.loadAuditLogs();
    component.loadAuditLogs();
    newerRequest.next(mockLogs);
    olderRequest.error(new Error('Stale request failed'));

    expect(component.auditLogs()).toEqual(mockLogs);
    expect(component.auditLoadError()).toBeNull();
    expect(component.auditLoading()).toBeFalse();
  });

  it('should display a safe ProblemDetails title without exposing its detail', () => {
    apiMock.listAuditLogs.and.returnValue(
      throwError(() => ({ error: { title: 'Audit service unavailable', detail: 'Connection details are private' } })),
    );

    component.loadAuditLogs();

    expect(component.auditLoadError()).toBe('Unable to load audit records: Audit service unavailable. Please retry.');
  });
});
