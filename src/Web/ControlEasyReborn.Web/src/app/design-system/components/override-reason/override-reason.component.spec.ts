import { ComponentFixture, TestBed } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { CeOverrideReasonComponent } from './override-reason.component';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';
import type { OverrideReason } from '../../../features/entry-log/entry-log.service';

describe('CeOverrideReasonComponent', () => {
  let fixture: ComponentFixture<CeOverrideReasonComponent>;
  let component: CeOverrideReasonComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CeOverrideReasonComponent],
      providers: [importProvidersFrom(CE_LUCIDE_ICONS)],
    }).compileComponents();

    fixture = TestBed.createComponent(CeOverrideReasonComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('open', true);
    fixture.detectChanges();
  });

  it('renders Emergency and Vouched buttons', () => {
    const buttons: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('.reason-btn');
    expect(buttons.length).toBe(2);
    const btn0 = buttons[0]!;
    const btn1 = buttons[1]!;
    expect(btn0.getAttribute('data-reason')).toBe('emergency');
    expect(btn1.getAttribute('data-reason')).toBe('vouched');
    expect(btn0.textContent).toContain('Emergency');
    expect(btn1.textContent).toContain('Vouched');
  });

  it('emits "emergency" when Emergency button is clicked', () => {
    const reasonSelected = jasmine.createSpy('reasonSelected');
    component.reasonSelected.subscribe(reasonSelected);

    const buttons: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('.reason-btn');
    buttons[0]!.click();
    expect(reasonSelected).toHaveBeenCalledWith('emergency' as OverrideReason);
  });

  it('emits "vouched" when Vouched button is clicked', () => {
    const reasonSelected = jasmine.createSpy('reasonSelected');
    component.reasonSelected.subscribe(reasonSelected);

    const buttons: NodeListOf<HTMLButtonElement> =
      fixture.nativeElement.querySelectorAll('.reason-btn');
    buttons[1]!.click();
    expect(reasonSelected).toHaveBeenCalledWith('vouched' as OverrideReason);
  });

  it('renders the modal title "Why?"', () => {
    const title = fixture.nativeElement.querySelector('.ce-modal-title');
    expect(title?.textContent).toContain('Why?');
  });

  it('hides content when open is false', () => {
    fixture.componentRef.setInput('open', false);
    fixture.detectChanges();
    const modalCard: HTMLElement | null =
      fixture.nativeElement.querySelector('.ce-card');
    expect(modalCard).toBeNull();
  });
});