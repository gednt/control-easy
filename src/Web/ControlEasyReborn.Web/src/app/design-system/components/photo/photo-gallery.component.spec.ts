import { ComponentFixture, TestBed } from '@angular/core/testing';
import { importProvidersFrom } from '@angular/core';
import { CePhotoGalleryComponent } from './photo-gallery.component';
import type { PhotoResponse } from '../../../features/photos/photos-api.service';
import { CE_LUCIDE_ICONS } from '../icon/icon.registry';

function makePhoto(id: string): PhotoResponse {
  return {
    id,
    tenantId: 'tenant-1',
    filePath: `/photos/${id}.jpg`,
    thumbnailPath: null,
    mimeType: 'image/jpeg',
    sizeBytes: 1024,
    capturedAtUtc: null,
    createdAtUtc: '2026-09-12T10:00:00Z',
    deletedAtUtc: null,
  };
}

describe('CePhotoGalleryComponent', () => {
  let fixture: ComponentFixture<CePhotoGalleryComponent>;
  let component: CePhotoGalleryComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CePhotoGalleryComponent],
      providers: [importProvidersFrom(CE_LUCIDE_ICONS)],
    }).compileComponents();

    fixture = TestBed.createComponent(CePhotoGalleryComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('photos', []);
    fixture.componentRef.setInput('canAdd', false);
    fixture.componentRef.setInput('canDelete', false);
    fixture.detectChanges();
  });

  it('shows empty state when photos is empty and canAdd is false', () => {
    const empty = fixture.nativeElement.querySelector('.empty');
    expect(empty?.textContent).toContain('No photos yet');
  });

  it('shows add tile when canAdd=true and no photos', () => {
    fixture.componentRef.setInput('canAdd', true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.add-tile')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('.empty')).toBeNull();
  });

  it('renders one ce-photo per photo', () => {
    fixture.componentRef.setInput('photos', [makePhoto('a'), makePhoto('b')]);
    fixture.detectChanges();
    const tiles = fixture.nativeElement.querySelectorAll('ce-photo');
    expect(tiles.length).toBe(2);
  });

  it('emits addRequested when add tile is clicked', () => {
    fixture.componentRef.setInput('canAdd', true);
    fixture.detectChanges();
    let fired = false;
    component.addRequested.subscribe(() => (fired = true));
    component.onAdd();
    expect(fired).toBe(true);
  });

  it('opens the lightbox at the clicked index', () => {
    fixture.componentRef.setInput('photos', [makePhoto('a'), makePhoto('b'), makePhoto('c')]);
    fixture.detectChanges();
    component.openLightbox(2);
    expect(component.lightboxIndex()).toBe(2);
    expect(component.lightboxOpen()).toBe(true);
  });

  it('forwards photoDeleted and closes the lightbox', () => {
    fixture.componentRef.setInput('photos', [makePhoto('a')]);
    fixture.componentRef.setInput('canDelete', true);
    fixture.detectChanges();
    let emittedId: string | null = null;
    component.photoDeleted.subscribe((id: string) => (emittedId = id));
    component.lightboxOpen.set(true);
    component.onDelete('a');
    expect(emittedId as string | null).toBe('a');
    expect(component.lightboxOpen()).toBe(false);
  });

  it('does NOT render the add tile when canAdd=false and photos exist', () => {
    fixture.componentRef.setInput('photos', [makePhoto('a')]);
    fixture.componentRef.setInput('canAdd', false);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.add-tile')).toBeNull();
  });
});
