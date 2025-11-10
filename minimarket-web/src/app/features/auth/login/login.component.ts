import { Component, signal, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  animations: [fadeSlideAnimation]
})
export class LoginComponent implements OnInit, AfterViewInit {
  loginForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Inicializar Google Sign-In cuando el componente se carga
    this.authService.initializeGoogleSignIn();
  }

  ngAfterViewInit(): void {
    // Renderizar el botón de Google después de que la vista se inicialice
    setTimeout(() => {
      this.authService.renderGoogleButton('google-signin-button');
    }, 500);
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const credentials = {
      username: this.loginForm.get('email')?.value, // Usar email como username
      password: this.loginForm.get('password')?.value
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        // Verificar si es administrador
        const isAdmin = response.roles?.includes('Administrador');
        
        // Si el perfil no está completo
        if (response.profileCompleted === false) {
          // Si es admin, redirigir a admin-setup, sino a complete-profile
          if (isAdmin) {
            this.router.navigate(['/auth/admin-setup']);
          } else {
            this.router.navigate(['/auth/complete-profile']);
          }
        } else {
          // Si el perfil está completo, redirigir según el rol
          if (isAdmin) {
            this.router.navigate(['/admin']);
          } else {
            this.router.navigate(['/']);
          }
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.errors?.[0] || 'Error al iniciar sesión');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(value => !value);
  }

  loginWithGoogle(): void {
    this.isLoading.set(true);
    try {
      this.authService.loginWithGoogle();
    } catch (error) {
      this.isLoading.set(false);
      this.errorMessage.set('Error al iniciar sesión con Google');
    }
  }

  goToForgotPassword(): void {
    this.router.navigate(['/auth/forgot-password']);
  }

  goToRegister(): void {
    this.router.navigate(['/auth/register']);
  }
}

