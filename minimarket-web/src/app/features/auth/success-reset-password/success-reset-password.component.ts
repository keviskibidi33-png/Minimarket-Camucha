import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';

@Component({
  selector: 'app-success-reset-password',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './success-reset-password.component.html',
  styleUrl: './success-reset-password.component.css',
  animations: [fadeSlideAnimation]
})
export class SuccessResetPasswordComponent implements OnInit {
  email = signal<string | null>(null);

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Obtener email de los query params
    this.route.queryParams.subscribe(params => {
      const email = params['email'];
      if (email) {
        this.email.set(email);
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}

