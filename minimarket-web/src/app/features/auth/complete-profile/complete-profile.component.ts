import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/services/toast.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';
import { firstValueFrom } from 'rxjs';
import { getDepartments, getProvincesByDepartment, getDistrictsByProvince } from '../../../shared/data/peru-locations.data';

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
  includeAddress = signal(true); // Mostrar formulario de dirección por defecto
  existingAddresses = signal<any[]>([]);
  selectedAddressId = signal<string | null>(null);
  showNewAddressForm = signal(true); // Mostrar formulario de nueva dirección por defecto
  // Signals para ubicaciones de Perú
  departments = signal<string[]>([]);
  provinces = signal<string[]>([]);
  districts = signal<string[]>([]);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private authService: AuthService,
    private toastService: ToastService
  ) {
    this.completeProfileForm = this.fb.group({
      // Teléfono ya no es requerido aquí, se usa el de la dirección
      // Dirección de envío (ahora es el formulario principal)
      addressLabel: ['', [Validators.required, Validators.maxLength(50)]],
      addressFullName: ['', [Validators.required, Validators.maxLength(200)]],
      addressPhone: ['+51 ', [Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/), Validators.maxLength(20)]],
      addressAddress: ['', [Validators.required, Validators.maxLength(500)]],
      addressReference: [''],
      addressDistrict: ['', [Validators.required, Validators.maxLength(100)]],
      addressCity: ['', [Validators.required, Validators.maxLength(100)]],
      addressRegion: ['', [Validators.required, Validators.maxLength(100)]],
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
      // Obtener nombre del perfil si está disponible
      this.authService.getProfile().subscribe({
        next: (profile) => {
          if (profile?.firstName && profile?.lastName) {
            this.userName.set(`${profile.firstName} ${profile.lastName}`);
            // Pre-llenar nombre completo en la dirección automáticamente
            this.completeProfileForm.patchValue({ 
              addressFullName: `${profile.firstName} ${profile.lastName}`.trim()
            });
          } else if (profile?.firstName) {
            this.userName.set(profile.firstName);
            this.completeProfileForm.patchValue({ 
              addressFullName: profile.firstName
            });
          } else {
            this.userName.set('Usuario');
          }
          // Pre-llenar teléfono de la dirección si está disponible
          if (profile?.phone) {
            this.completeProfileForm.patchValue({ 
              addressPhone: profile.phone
            });
          }
        },
        error: () => {
          this.userName.set('Usuario');
        }
      });
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
        // Si hay direcciones existentes, mostrar selector en lugar del formulario
        this.showNewAddressForm.set(false);
        // Seleccionar la dirección predeterminada si existe
        const defaultAddress = addresses.find(a => a.isDefault);
        if (defaultAddress) {
          this.selectedAddressId.set(defaultAddress.id);
        } else {
          // Si no hay predeterminada, seleccionar la primera
          this.selectedAddressId.set(addresses[0].id);
        }
      } else {
        // Si no hay direcciones, mostrar formulario de nueva dirección
        this.showNewAddressForm.set(true);
        this.addAddressValidators();
      }
    } catch (error) {
      // Si no hay direcciones o hay un error, mostrar formulario de nueva dirección
      console.log('No se encontraron direcciones existentes o error al cargar:', error);
      this.existingAddresses.set([]);
      this.showNewAddressForm.set(true);
      this.addAddressValidators();
    }

    // Cargar departamentos de Perú
    this.departments.set(getDepartments());
  }

  async onSubmit(): Promise<void> {
    // Validar dirección (ahora es el formulario principal)
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

    this.isLoading.set(true);
    this.errorMessage.set(null);

    // Enviar teléfono desde la dirección (nombre, apellido y DNI ya están en el perfil desde el registro)
    // Si no hay teléfono en la dirección, usar el teléfono del perfil si está disponible
    const addressPhone = this.completeProfileForm.get('addressPhone')?.value;
    let phoneToSend = addressPhone || '';
    
    // Si el teléfono está vacío, intentar obtenerlo del perfil
    if (!phoneToSend) {
      try {
        const profile = await firstValueFrom(this.authService.getProfile());
        phoneToSend = profile?.phone || '';
      } catch (error) {
        console.error('Error al obtener teléfono del perfil:', error);
      }
    }
    
    const profileData: any = {
      phone: phoneToSend
    };

    // Agregar dirección (siempre se incluye ahora)
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

  // Métodos para manejar cambios en los dropdowns de ubicación
  onDepartmentChange(): void {
    const department = this.completeProfileForm.get('addressRegion')?.value;
    if (department) {
      this.provinces.set(getProvincesByDepartment(department));
      this.completeProfileForm.patchValue({ addressCity: '', addressDistrict: '' });
      this.districts.set([]);
    } else {
      this.provinces.set([]);
      this.districts.set([]);
    }
  }

  onProvinceChange(): void {
    const department = this.completeProfileForm.get('addressRegion')?.value;
    const province = this.completeProfileForm.get('addressCity')?.value;
    if (department && province) {
      this.districts.set(getDistrictsByProvince(department, province));
      this.completeProfileForm.patchValue({ addressDistrict: '' });
    } else {
      this.districts.set([]);
    }
  }

  skipProfile(): void {
    // Permitir al usuario omitir el completado del perfil y redirigir
    this.toastService.info('Puedes completar tu perfil más tarde desde tu cuenta');
    
    // Verificar si es administrador para redirigir correctamente
    const currentUser = this.authService.currentUser();
    if (currentUser?.roles?.includes('Administrador')) {
      // Si es admin, redirigir al dashboard (aunque puede que lo redirija al setup si no está completo)
      this.router.navigate(['/admin']);
    } else {
      // Si no es admin, redirigir a la página principal
      this.router.navigate(['/']);
    }
  }

}

