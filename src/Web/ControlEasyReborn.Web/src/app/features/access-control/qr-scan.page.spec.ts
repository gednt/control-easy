import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { QrScanPage } from './qr-scan.page';
import { AccessScanResultComponent } from './components/access-scan-result.component';
import { GatewayControlService } from './gateway-control.service';

describe('QrScanPage', () => {
  let fixture: ComponentFixture<QrScanPage>;
  let component: QrScanPage;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QrScanPage, AccessScanResultComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), GatewayControlService],
    }).compileComponents();

    fixture = TestBed.createComponent(QrScanPage);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('renders the QR form', () => {
    fixture.detectChanges();
    const html = (fixture.nativeElement as HTMLElement).innerHTML;
    expect(html).toContain('QR Scan');
    expect(html).toContain('Submit scan');
  });

  it('records a successful scan and surfaces the recorded outcome', () => {
    fixture.detectChanges();

    component.state.update(s => ({ ...s, qrPayload: 'opaque-token', direction: 'entrance' }));
    fixture.detectChanges();

    component['submit']();

    const req = httpMock.expectOne('/api/v1/access-events/scans');
    req.flush({
      decision: 'recorded',
      accessEventId: '00000000-0000-0000-0000-000000000001',
      subjectType: 'resident',
      subjectId: '00000000-0000-0000-0000-000000000002',
      credentialId: null,
      lookupAuditId: null,
      accessMethod: 'qr',
      direction: 'entrance',
      policyOutcome: 'permit',
      destinationApartmentId: '00000000-0000-0000-0000-000000000003',
      destinationBlock: 'A',
      destinationUnit: '101',
    });

    fixture.detectChanges();
    const html = (fixture.nativeElement as HTMLElement).innerHTML;
    expect(html).toContain('Access recorded');
    expect(component.state().qrPayload).toBe('');
  });

  it('surfaces refusal with the safe failure code', () => {
    fixture.detectChanges();

    component.state.update(s => ({ ...s, qrPayload: 'revoked-token', direction: 'exit' }));
    fixture.detectChanges();

    component['submit']();

    const req = httpMock.expectOne('/api/v1/access-events/scans');
    req.flush(
      { failureCode: 'credential_inactive', decision: 'refused' },
      { status: 422, statusText: 'Unprocessable Entity' },
    );

    fixture.detectChanges();
    const html = (fixture.nativeElement as HTMLElement).innerHTML;
    expect(html).toContain('Access refused');
    expect(html).toContain('credential_inactive');
  });
});