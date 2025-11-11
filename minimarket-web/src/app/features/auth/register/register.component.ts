import { Component, signal, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { BrandSettingsService } from '../../../core/services/brand-settings.service';
import { ToastService } from '../../../shared/services/toast.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
  animations: [fadeSlideAnimation]
})
export class RegisterComponent implements OnInit, AfterViewInit {
  registerForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);
  storeName = signal('Minimarket Camucha');

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private brandSettingsService: BrandSettingsService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.registerForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      dni: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]],
      phone: ['+51 ', [Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]],
      acceptTerms: [false, [Validators.requiredTrue]],
      acceptAdditionalPurposes: [false, [Validators.requiredTrue]]
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const credentials = {
      email: this.registerForm.get('email')?.value,
      password: this.registerForm.get('password')?.value,
      firstName: this.registerForm.get('firstName')?.value,
      lastName: this.registerForm.get('lastName')?.value,
      dni: this.registerForm.get('dni')?.value,
      phone: this.registerForm.get('phone')?.value
    };

    this.authService.register(credentials).subscribe({
      next: (response) => {
        this.toastService.success('Cuenta creada exitosamente');
        // Si el perfil no está completo, redirigir a completar perfil
        if (response.profileCompleted === false) {
          this.router.navigate(['/auth/complete-profile']);
        } else {
          this.router.navigate(['/perfil']);
        }
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || error.error?.errors?.[0] || 'Error al crear la cuenta');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(value => !value);
  }

  ngOnInit(): void {
    // Inicializar Google Sign-In cuando el componente se carga
    this.authService.initializeGoogleSignIn();
    
    // Cargar nombre de la tienda
    this.brandSettingsService.get().subscribe({
      next: (settings) => {
        if (settings?.storeName) {
          this.storeName.set(settings.storeName);
        }
      },
      error: (error) => {
        console.error('Error loading brand settings:', error);
        // Mantener el valor por defecto si hay error
      }
    });
  }

  ngAfterViewInit(): void {
    // Renderizar el botón de Google después de que la vista se inicialice
    setTimeout(() => {
      this.authService.renderGoogleButton('google-signin-button-register');
    }, 500);
  }

  registerWithGoogle(): void {
    // Este método ya no es necesario, el botón renderizado maneja todo
    this.authService.loginWithGoogle();
  }

  goToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}

