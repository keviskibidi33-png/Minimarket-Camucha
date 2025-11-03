import { Component, ViewChild, AfterViewInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './shared/components/toast/toast.component';
import { ToastService } from './shared/services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent],
  template: `
    <router-outlet></router-outlet>
    <app-toast #toast></app-toast>
  `
})
export class AppComponent implements AfterViewInit {
  @ViewChild('toast') toastComponent!: ToastComponent;

  constructor(private toastService: ToastService) {}

  ngAfterViewInit(): void {
    this.toastService.register(this.toastComponent);
  }

  title = 'Minimarket Camucha';
}

