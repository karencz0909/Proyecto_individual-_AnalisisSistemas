import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MiSolicitudComponent } from './mi-solicitud.component';

describe('MiSolicitudComponent', () => {
  let component: MiSolicitudComponent;
  let fixture: ComponentFixture<MiSolicitudComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MiSolicitudComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MiSolicitudComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
