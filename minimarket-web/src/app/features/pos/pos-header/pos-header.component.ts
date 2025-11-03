import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-pos-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pos-header.component.html',
  styleUrl: './pos-header.component.css'
})
export class PosHeaderComponent {
  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  goToDashboard(): void {
    this.router.navigate(['/']);
  }
}

