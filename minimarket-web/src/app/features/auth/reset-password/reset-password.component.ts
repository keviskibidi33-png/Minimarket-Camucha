import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
  animations: [fadeSlideAnimation]
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  token = signal<string | null>(null);
  email = signal<string | null>(null);
  invalidToken = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toastService: ToastService
  ) {
    this.resetPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Obtener token y email de los query params
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const email = params['email'];

      if (!token || !email) {
        this.invalidToken.set(true);
        this.errorMessage.set('El enlace de recuperación no es válido o ha expirado');
        return;
      }

      this.token.set(token);
      this.email.set(email);
    });
  }

  passwordMatchValidator(group: FormGroup) {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.resetPasswordForm.invalid || !this.token()) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const newPassword = this.resetPasswordForm.get('password')?.value;

    this.authService.resetPassword(this.token()!, this.email()!, newPassword).subscribe({
      next: () => {
        this.toastService.success('Contraseña restablecida exitosamente');
        // Redirigir a página de éxito
        this.router.navigate(['/auth/success-reset-password'], {
          queryParams: { email: this.email() }
        });
      },
      error: (error) => {
        this.isLoading.set(false);
        const errorMsg = error.error?.message || error.error?.errors?.[0] || 'Error al restablecer la contraseña';
        this.errorMessage.set(errorMsg);
        
        // Si el token es inválido o expiró, marcar como inválido
        if (errorMsg.toLowerCase().includes('token') || errorMsg.toLowerCase().includes('inválido') || errorMsg.toLowerCase().includes('expirado')) {
          this.invalidToken.set(true);
        }
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(value => !value);
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword.update(value => !value);
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  goToForgotPassword(): void {
    this.router.navigate(['/auth/forgot-password']);
  }
}

