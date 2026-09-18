import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import {
  HttpClient,
  provideHttpClient,
  withInterceptors,
  HttpErrorResponse,
} from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { of, throwError } from 'rxjs';
import { errorInterceptor } from './error.interceptor';
import { AuthService } from '../services/auth.service';

describe('errorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: Router;
  let auth: jasmine.SpyObj<Pick<AuthService, 'refreshAuth' | 'logout' | 'accessToken'>>;
  let navigateSpy: jasmine.Spy;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'refreshAuth',
      'logout',
      'accessToken',
    ]);
    auth.accessToken.and.returnValue('stub-token');

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    navigateSpy = spyOn(router, 'navigate').and.callThrough();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('navigates to /access-denied on a 403 response', () => {
    Object.defineProperty(router, 'url', { configurable: true, value: '/gatehouse/manual' });

    http.get('/api/v1/access-subjects/search').subscribe({
      next: () => fail('expected 403 error path'),
      error: () => {},
    });

    httpMock.expectOne('/api/v1/access-subjects/search').flush(
      { detail: 'Caller lacks Access.Access.Operate.' },
      { status: 403, statusText: 'Forbidden' },
    );

    expect(navigateSpy).toHaveBeenCalledWith(['/access-denied']);
  });

  it('does NOT navigate to /access-denied when the current URL is empty (pre-bootstrap)', () => {
    Object.defineProperty(router, 'url', { configurable: true, value: '' });

    http.get('/api/v1/access-events/scans').subscribe({
      next: () => fail('expected 403 error path'),
      error: () => {},
    });

    httpMock.expectOne('/api/v1/access-events/scans').flush(
      { detail: 'still forbidden' },
      { status: 403, statusText: 'Forbidden' },
    );

    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('logs out when a 401 refresh attempt throws', () => {
    Object.defineProperty(router, 'url', { configurable: true, value: '/gatehouse/qr' });

    auth.refreshAuth.and.returnValue(throwError(() => new Error('refresh failed')));

    http.get('/api/v1/access-events/scans').subscribe({
      next: () => fail('expected 401 error path'),
      error: () => {},
    });

    httpMock.expectOne('/api/v1/access-events/scans').flush(
      { detail: 'token expired' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(auth.refreshAuth).toHaveBeenCalled();
    expect(auth.logout).toHaveBeenCalledTimes(1);
  });

  it('logs out when refresh resolves with null (no new token)', () => {
    Object.defineProperty(router, 'url', { configurable: true, value: '/gatehouse/qr' });

    auth.refreshAuth.and.returnValue(of(null));

    http.get('/api/v1/access-events/scans').subscribe({
      next: () => fail('expected 401 error path'),
      error: () => {},
    });

    httpMock.expectOne('/api/v1/access-events/scans').flush(
      { detail: 'token expired' },
      { status: 401, statusText: 'Unauthorized' },
    );

    expect(auth.logout).toHaveBeenCalledTimes(1);
  });
});
