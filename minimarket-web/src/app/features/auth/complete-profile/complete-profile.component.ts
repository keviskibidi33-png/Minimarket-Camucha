import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-complete-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './complete-profile.component.html',
  styleUrl: './complete-profile.component.css',
  animations: [fadeSlideAnimation]
})
export class CompleteProfileComponent implements OnInit {
  completeProfileForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  userName = signal<string>('');
  includePaymentMethod = signal(false);
  includeAddress = signal(false);
  existingAddresses = signal<any[]>([]);
  selectedAddressId = signal<string | null>(null);
  showNewAddressForm = signal(false);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private toastService: ToastService
  ) {
    this.completeProfileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      dni: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]],
      // Método de pago opcional
      cardHolderName: [''],
      cardNumber: [''],
      expiryMonth: [new Date().getMonth() + 1],
      expiryYear: [new Date().getFullYear()],
      isDefault: [true],
      // Dirección de envío opcional
      addressLabel: [''],
      addressFullName: [''],
      addressPhone: [''],
      addressAddress: [''],
      addressReference: [''],
      addressDistrict: [''],
      addressCity: [''],
      addressRegion: [''],
      addressPostalCode: [''],
      addressIsDefault: [true]
    });
  }

  async ngOnInit(): Promise<void> {
    // Verificar si hay un token en la URL (viene de Google OAuth)
    this.route.queryParams.subscribe(params => {
      if (params['token']) {
        this.authService.handleGoogleCallback(params['token']);
      }
    });

    const currentUser = this.authService.currentUser();
    if (currentUser) {
      this.userName.set(currentUser.username || 'Usuario');
    } else {
      // Si no hay usuario autenticado, redirigir al login
      this.router.navigate(['/auth/login']);
      return;
    }

    // Cargar direcciones existentes para mostrar selector
    try {
      const addresses = await firstValueFrom(this.authService.getAddresses());
      if (addresses && addresses.length > 0) {
        this.existingAddresses.set(addresses);
        // Seleccionar la dirección predeterminada si existe
        const defaultAddress = addresses.find(a => a.isDefault);
        if (defaultAddress) {
          this.selectedAddressId.set(defaultAddress.id);
        } else {
          // Si no hay predeterminada, seleccionar la primera
          this.selectedAddressId.set(addresses[0].id);
        }
      }
    } catch (error) {
      // Si no hay direcciones o hay un error, continuar sin direcciones
      console.log('No se encontraron direcciones existentes o error al cargar:', error);
      this.existingAddresses.set([]);
    }
  }

  onSubmit(): void {
    // Validar solo los campos básicos del perfil
    if (this.completeProfileForm.get('firstName')?.invalid ||
        this.completeProfileForm.get('lastName')?.invalid ||
        this.completeProfileForm.get('dni')?.invalid ||
        this.completeProfileForm.get('phone')?.invalid) {
      this.completeProfileForm.markAllAsTouched();
      return;
    }

    // Si se incluye método de pago, validar esos campos también
    if (this.includePaymentMethod()) {
      if (this.completeProfileForm.get('cardHolderName')?.invalid ||
          this.completeProfileForm.get('cardNumber')?.invalid) {
        this.completeProfileForm.markAllAsTouched();
        return;
      }
    }

    // Si se incluye dirección, validar según si es nueva o existente
    if (this.includeAddress()) {
      // Si está creando una nueva dirección, validar campos
      if (this.showNewAddressForm() || this.existingAddresses().length === 0) {
        if (this.completeProfileForm.get('addressLabel')?.invalid ||
            this.completeProfileForm.get('addressFullName')?.invalid ||
            this.completeProfileForm.get('addressPhone')?.invalid ||
            this.completeProfileForm.get('addressAddress')?.invalid ||
            this.completeProfileForm.get('addressDistrict')?.invalid ||
            this.completeProfileForm.get('addressCity')?.invalid ||
            this.completeProfileForm.get('addressRegion')?.invalid) {
          this.completeProfileForm.markAllAsTouched();
          return;
        }
      } else if (!this.selectedAddressId()) {
        // Si hay direcciones pero no se seleccionó ninguna
        this.errorMessage.set('Por favor selecciona una dirección o crea una nueva');
        return;
      }
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const profileData: any = {
      firstName: this.completeProfileForm.get('firstName')?.value,
      lastName: this.completeProfileForm.get('lastName')?.value,
      dni: this.completeProfileForm.get('dni')?.value,
      phone: this.completeProfileForm.get('phone')?.value
    };

    // Agregar método de pago si está incluido
    if (this.includePaymentMethod()) {
      const cardNumber = this.completeProfileForm.get('cardNumber')?.value.replace(/\s/g, '').replace(/-/g, '');
      profileData.paymentMethod = {
        cardHolderName: this.completeProfileForm.get('cardHolderName')?.value,
        cardNumber: cardNumber,
        expiryMonth: this.completeProfileForm.get('expiryMonth')?.value,
        expiryYear: this.completeProfileForm.get('expiryYear')?.value,
        isDefault: true
      };
    }

    // Agregar dirección si está incluida
    if (this.includeAddress()) {
      // Si hay direcciones existentes y se seleccionó una, no enviar datos (ya existe)
      // Solo enviar si es una nueva dirección
      if (this.showNewAddressForm() || this.existingAddresses().length === 0) {
        profileData.address = {
          label: this.completeProfileForm.get('addressLabel')?.value,
          fullName: this.completeProfileForm.get('addressFullName')?.value,
          phone: this.completeProfileForm.get('addressPhone')?.value,
          address: this.completeProfileForm.get('addressAddress')?.value,
          reference: this.completeProfileForm.get('addressReference')?.value || undefined,
          district: this.completeProfileForm.get('addressDistrict')?.value,
          city: this.completeProfileForm.get('addressCity')?.value,
          region: this.completeProfileForm.get('addressRegion')?.value,
          postalCode: this.completeProfileForm.get('addressPostalCode')?.value || undefined,
          isDefault: this.completeProfileForm.get('addressIsDefault')?.value || true
        };
      }
      // Si se seleccionó una dirección existente, no necesitamos enviar datos de dirección
      // porque ya está guardada en el perfil
    }

    this.authService.completeProfile(profileData).subscribe({
      next: () => {
        this.toastService.success('Perfil completado exitosamente');
        this.router.navigate(['/perfil']);
      },
      error: (error) => {
        this.isLoading.set(false);
        this.errorMessage.set(error.error?.message || error.error?.errors?.[0] || 'Error al completar el perfil');
      },
      complete: () => {
        this.isLoading.set(false);
      }
    });
  }

  togglePaymentMethod() {
    this.includePaymentMethod.update(value => !value);
    if (this.includePaymentMethod()) {
      // Agregar validadores cuando se activa
      this.completeProfileForm.get('cardHolderName')?.setValidators([Validators.required, Validators.maxLength(100)]);
      this.completeProfileForm.get('cardNumber')?.setValidators([Validators.required, Validators.pattern(/^\d{13,19}$/)]);
    } else {
      // Remover validadores cuando se desactiva
      this.completeProfileForm.get('cardHolderName')?.clearValidators();
      this.completeProfileForm.get('cardNumber')?.clearValidators();
    }
    this.completeProfileForm.get('cardHolderName')?.updateValueAndValidity();
    this.completeProfileForm.get('cardNumber')?.updateValueAndValidity();
  }

  toggleAddress() {
    this.includeAddress.update(value => !value);
    if (this.includeAddress()) {
      // Si no hay direcciones existentes, mostrar formulario nuevo
      if (this.existingAddresses().length === 0) {
        this.showNewAddressForm.set(true);
        this.addAddressValidators();
      }
    } else {
      this.showNewAddressForm.set(false);
      this.removeAddressValidators();
    }
  }

  selectExistingAddress(addressId: string) {
    this.selectedAddressId.set(addressId);
    this.showNewAddressForm.set(false);
    this.removeAddressValidators();
  }

  showNewAddress() {
    this.showNewAddressForm.set(true);
    this.selectedAddressId.set(null);
    this.addAddressValidators();
  }

  private addAddressValidators() {
    this.completeProfileForm.get('addressLabel')?.setValidators([Validators.required, Validators.maxLength(50)]);
    this.completeProfileForm.get('addressFullName')?.setValidators([Validators.required, Validators.maxLength(200)]);
    this.completeProfileForm.get('addressPhone')?.setValidators([Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]);
    this.completeProfileForm.get('addressAddress')?.setValidators([Validators.required, Validators.maxLength(500)]);
    this.completeProfileForm.get('addressDistrict')?.setValidators([Validators.required, Validators.maxLength(100)]);
    this.completeProfileForm.get('addressCity')?.setValidators([Validators.required, Validators.maxLength(100)]);
    this.completeProfileForm.get('addressRegion')?.setValidators([Validators.required, Validators.maxLength(100)]);
    this.updateAddressValidators();
  }

  private removeAddressValidators() {
    this.completeProfileForm.get('addressLabel')?.clearValidators();
    this.completeProfileForm.get('addressFullName')?.clearValidators();
    this.completeProfileForm.get('addressPhone')?.clearValidators();
    this.completeProfileForm.get('addressAddress')?.clearValidators();
    this.completeProfileForm.get('addressDistrict')?.clearValidators();
    this.completeProfileForm.get('addressCity')?.clearValidators();
    this.completeProfileForm.get('addressRegion')?.clearValidators();
    this.updateAddressValidators();
  }

  private updateAddressValidators() {
    this.completeProfileForm.get('addressLabel')?.updateValueAndValidity();
    this.completeProfileForm.get('addressFullName')?.updateValueAndValidity();
    this.completeProfileForm.get('addressPhone')?.updateValueAndValidity();
    this.completeProfileForm.get('addressAddress')?.updateValueAndValidity();
    this.completeProfileForm.get('addressDistrict')?.updateValueAndValidity();
    this.completeProfileForm.get('addressCity')?.updateValueAndValidity();
    this.completeProfileForm.get('addressRegion')?.updateValueAndValidity();
  }

  getYears(): number[] {
    const currentYear = new Date().getFullYear();
    const years: number[] = [];
    for (let i = 0; i < 20; i++) {
      years.push(currentYear + i);
    }
    return years;
  }

  formatMonth(month: number): string {
    return String(month).padStart(2, '0');
  }
}

