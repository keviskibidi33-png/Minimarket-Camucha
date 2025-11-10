import { Component, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/services/toast.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';

interface SetupStep {
  id: number;
  name: string;
  title: string;
  completed: boolean;
}

@Component({
  selector: 'app-admin-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-setup.component.html',
  styleUrl: './admin-setup.component.css',
  animations: [fadeSlideAnimation]
})
export class AdminSetupComponent implements OnInit {
  setupForm: FormGroup;
  currentStep = signal<number>(1);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  // Línea de tiempo de pasos
  steps: SetupStep[] = [
    { id: 1, name: 'Creación', title: 'Información Básica', completed: false },
    { id: 2, name: 'Personalización', title: 'Branding y Diseño', completed: false },
    { id: 3, name: 'Gestión', title: 'Configuración del Sistema', completed: false },
    { id: 4, name: 'Pago y Envío', title: 'Información de Pago y Envío', completed: false },
    { id: 5, name: 'Finalización', title: 'Crear Usuarios', completed: false }
  ];

  // Rubros de negocio comunes
  businessTypes = [
    'Minimarket / Bodega',
    'Supermercado',
    'Farmacia',
    'Tienda de Conveniencia',
    'Restaurante / Cafetería',
    'Tienda de Ropa',
    'Electrónica',
    'Otro'
  ];

  // Categorías predefinidas
  predefinedCategories = [
    'Lácteos',
    'Abarrotes',
    'Bebidas',
    'Golosinas',
    'Conservas',
    'Limpieza',
    'Carnes',
    'Frutas y Verduras',
    'Panadería',
    'Congelados',
    'Cuidado Personal',
    'Bebé'
  ];
  
  // Categorías personalizadas creadas por el usuario
  customCategories = signal<string[]>([]);
  newCategoryName = signal('');
  showNewCategoryForm = signal(false);

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private toastService: ToastService
  ) {
    this.setupForm = this.fb.group({
      // Paso 1: Información Básica
      storeName: ['', [Validators.required, Validators.maxLength(200)]],
      businessType: ['', Validators.required],
      phone: ['', [Validators.required, Validators.maxLength(20)]], // Movido aquí, requerido
      description: ['', Validators.maxLength(1000)],
      whatSells: ['', Validators.maxLength(500)],
      isVirtual: [false],
      sedeAddress: [''],
      sedeCity: [''],
      sedeRegion: [''],
      
      // Paso 2: Branding
      logoFile: [null],
      faviconFile: [null],
      primaryColor: ['#4CAF50', Validators.required],
      secondaryColor: ['#0d7ff2', Validators.required],
      email: ['', [Validators.email, Validators.maxLength(100)]],
      ruc: ['', Validators.maxLength(20)],
      slogan: ['', Validators.maxLength(500)],
      
      // Paso 3: Configuración
      categories: this.fb.array([]),
      systemUsers: ['1-5', Validators.required], // 1-5, 6-10, 11-20, 21+
      // Personalización de página (Paso 3)
      homeTitle: ['', Validators.maxLength(200)],
      homeSubtitle: ['', Validators.maxLength(500)],
      homeDescription: ['', Validators.maxLength(1000)],
      homeBannerImage: [null],
      homeBannerImageUrl: [''],
      
      // Paso 4: Información de Pago y Envío
      // Información de pago (unificado Yape/Plin)
      yapePlinPhone: ['', Validators.maxLength(20)],
      yapePlinQRFile: [null],
      // Cuenta bancaria
      bankName: ['', Validators.maxLength(100)],
      bankAccountType: [''], // 'Ahorros' | 'Corriente'
      bankAccountNumber: ['', Validators.maxLength(50)],
      bankCCI: ['', Validators.maxLength(50)],
      // Opciones de envío
      deliveryType: ['Ambos', Validators.required], // 'SoloRecogida' | 'SoloEnvio' | 'Ambos'
      deliveryCost: [0],
      deliveryZones: [''],
      
      // Paso 5: Usuarios
      createCashier: [true],
      cashierEmail: ['', [Validators.email]],
      cashierPassword: ['', [Validators.minLength(6)]],
      cashierFirstName: [''],
      cashierLastName: [''],
      cashierDni: ['', [Validators.pattern(/^\d{8}$/)]]
    });
  }

  ngOnInit(): void {
    // Inicializar categorías con las predefinidas seleccionadas primero
    this.predefinedCategories.forEach(cat => {
      this.categoriesArray.push(this.fb.control(true));
    });

    // Verificar que el usuario sea admin
    try {
      const user = this.authService.currentUser();
      if (!user || !user.roles?.includes('Administrador')) {
        this.router.navigate(['/']);
        return;
      }
    } catch (error) {
      // Si hay error al obtener el usuario, redirigir al login
      console.error('Error verificando usuario:', error);
      this.router.navigate(['/auth/login']);
    }
  }

  get categoriesArray(): FormArray {
    return this.setupForm.get('categories') as FormArray;
  }

  hasNoCategoriesSelected(): boolean {
    return this.categoriesArray.controls.every(control => !control.value);
  }

  getCategoryControl(index: number): FormControl {
    return this.categoriesArray.at(index) as FormControl;
  }

  addCustomCategory(): void {
    const categoryName = this.newCategoryName().trim();
    if (!categoryName) {
      this.toastService.error('Ingresa un nombre para la categoría');
      return;
    }
    
    if (this.predefinedCategories.includes(categoryName) || this.customCategories().includes(categoryName)) {
      this.toastService.error('Esta categoría ya existe');
      return;
    }
    
    this.customCategories.update(cats => [...cats, categoryName]);
    this.newCategoryName.set('');
    this.showNewCategoryForm.set(false);
  }

  removeCustomCategory(category: string): void {
    this.customCategories.update(cats => cats.filter(c => c !== category));
  }

  get currentStepData(): SetupStep {
    return this.steps[this.currentStep() - 1];
  }

  get progressPercentage(): number {
    return (this.currentStep() / this.steps.length) * 100;
  }

  nextStep(): void {
    if (this.currentStep() < this.steps.length) {
      // Validar el paso actual antes de avanzar
      if (this.validateCurrentStep()) {
        this.steps[this.currentStep() - 1].completed = true;
        this.currentStep.update(step => step + 1);
      }
    }
  }

  previousStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(step => step - 1);
    }
  }

  goToStep(stepNumber: number): void {
    // Solo permitir ir a pasos ya completados o el siguiente
    if (stepNumber <= this.currentStep() || this.steps[stepNumber - 2]?.completed) {
      this.currentStep.set(stepNumber);
    }
  }

  validateCurrentStep(): boolean {
    const step = this.currentStep();
    
    switch (step) {
      case 1:
        const isVirtual = this.setupForm.get('isVirtual')?.value ?? false;
        const storeNameValid = this.setupForm.get('storeName')?.valid ?? false;
        const businessTypeValid = this.setupForm.get('businessType')?.valid ?? false;
        const phoneValid = this.setupForm.get('phone')?.valid ?? false;
        
        // Validaciones básicas siempre requeridas
        if (!storeNameValid || !businessTypeValid || !phoneValid) {
          return false;
        }
        
        // Si NO es virtual, requiere dirección y ciudad
        if (!isVirtual) {
          const hasAddress = !!this.setupForm.get('sedeAddress')?.value?.trim();
          const hasCity = !!this.setupForm.get('sedeCity')?.value?.trim();
          return hasAddress && hasCity;
        }
        
        // Si es virtual, no requiere dirección/ciudad
        return true;
      case 2:
        return (this.setupForm.get('primaryColor')?.valid ?? false) && 
               (this.setupForm.get('secondaryColor')?.valid ?? false);
      case 3:
        const hasPredefinedCategories = this.categoriesArray.controls.some(control => control.value === true);
        const hasCustomCategories = this.customCategories().length > 0;
        return (this.setupForm.get('systemUsers')?.valid ?? false) &&
               (hasPredefinedCategories || hasCustomCategories);
      case 4:
        // Paso 4: Información de Pago y Envío - todos los campos son opcionales
        return true;
      case 5:
        if (this.setupForm.get('createCashier')?.value === true) {
          return this.setupForm.get('cashierEmail')?.valid &&
                 this.setupForm.get('cashierPassword')?.valid &&
                 this.setupForm.get('cashierFirstName')?.value &&
                 this.setupForm.get('cashierLastName')?.value &&
                 this.setupForm.get('cashierDni')?.valid;
        }
        return true;
      default:
        return true;
    }
  }

  onLogoSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.setupForm.patchValue({ logoFile: file });
    }
  }

  onFaviconSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.setupForm.patchValue({ faviconFile: file });
    }
  }

  onYapePlinQRSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.setupForm.patchValue({ yapePlinQRFile: file });
    }
  }

  onHomeBannerSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.setupForm.patchValue({ homeBannerImage: file });
      // Crear preview URL
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.setupForm.patchValue({ homeBannerImageUrl: e.target.result });
      };
      reader.readAsDataURL(file);
    }
  }

  async onSubmit(): Promise<void> {
    if (!this.validateCurrentStep()) {
      this.errorMessage.set('Por favor, completa todos los campos requeridos');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    try {
      // Preparar datos para enviar
      const formData = new FormData();
      
      // Información básica
      formData.append('storeName', this.setupForm.get('storeName')?.value);
      formData.append('businessType', this.setupForm.get('businessType')?.value);
      formData.append('description', this.setupForm.get('description')?.value || '');
      formData.append('whatSells', this.setupForm.get('whatSells')?.value || '');
      formData.append('isVirtual', (this.setupForm.get('isVirtual')?.value ?? false).toString());
      
      // Teléfono (movido al paso 1, siempre se envía)
      formData.append('phone', this.setupForm.get('phone')?.value || '');
      
      // Sede (si no es virtual)
      if (!this.setupForm.get('isVirtual')?.value) {
        formData.append('sedeAddress', this.setupForm.get('sedeAddress')?.value);
        formData.append('sedeCity', this.setupForm.get('sedeCity')?.value);
        formData.append('sedeRegion', this.setupForm.get('sedeRegion')?.value || '');
      }
      
      // Branding
      if (this.setupForm.get('logoFile')?.value) {
        formData.append('logoFile', this.setupForm.get('logoFile')?.value);
      }
      if (this.setupForm.get('faviconFile')?.value) {
        formData.append('faviconFile', this.setupForm.get('faviconFile')?.value);
      }
      formData.append('primaryColor', this.setupForm.get('primaryColor')?.value);
      formData.append('secondaryColor', this.setupForm.get('secondaryColor')?.value);
      formData.append('email', this.setupForm.get('email')?.value || '');
      formData.append('ruc', this.setupForm.get('ruc')?.value || '');
      formData.append('slogan', this.setupForm.get('slogan')?.value || '');
      
      // Categorías seleccionadas (predefinidas + personalizadas)
      const selectedPredefined = this.predefinedCategories.filter((_, index) => 
        this.categoriesArray.at(index)?.value
      );
      const allCategories = [...selectedPredefined, ...this.customCategories()];
      formData.append('categories', JSON.stringify(allCategories));
      formData.append('systemUsers', this.setupForm.get('systemUsers')?.value);
      
      // Personalización de página
      formData.append('homeTitle', this.setupForm.get('homeTitle')?.value || '');
      formData.append('homeSubtitle', this.setupForm.get('homeSubtitle')?.value || '');
      formData.append('homeDescription', this.setupForm.get('homeDescription')?.value || '');
      if (this.setupForm.get('homeBannerImage')?.value) {
        formData.append('homeBannerImage', this.setupForm.get('homeBannerImage')?.value);
      }
      
      // Información de Pago y Envío (Paso 4) - Yape/Plin unificado
      const yapePlinPhone = this.setupForm.get('yapePlinPhone')?.value || '';
      formData.append('yapePhone', yapePlinPhone); // Backend espera yapePhone
      formData.append('plinPhone', yapePlinPhone); // Mismo valor para ambos
      if (this.setupForm.get('yapePlinQRFile')?.value) {
        formData.append('yapeQRFile', this.setupForm.get('yapePlinQRFile')?.value);
        formData.append('plinQRFile', this.setupForm.get('yapePlinQRFile')?.value); // Mismo archivo para ambos
      }
      formData.append('bankName', this.setupForm.get('bankName')?.value || '');
      formData.append('bankAccountType', this.setupForm.get('bankAccountType')?.value || '');
      formData.append('bankAccountNumber', this.setupForm.get('bankAccountNumber')?.value || '');
      formData.append('bankCCI', this.setupForm.get('bankCCI')?.value || '');
      formData.append('deliveryType', this.setupForm.get('deliveryType')?.value || 'Ambos');
      formData.append('deliveryCost', (this.setupForm.get('deliveryCost')?.value || 0).toString());
      formData.append('deliveryZones', this.setupForm.get('deliveryZones')?.value || '');
      
      // Usuario cajero (si se crea)
      if (this.setupForm.get('createCashier')?.value === true) {
        formData.append('createCashier', 'true');
        formData.append('cashierEmail', this.setupForm.get('cashierEmail')?.value);
        formData.append('cashierPassword', this.setupForm.get('cashierPassword')?.value);
        formData.append('cashierFirstName', this.setupForm.get('cashierFirstName')?.value);
        formData.append('cashierLastName', this.setupForm.get('cashierLastName')?.value);
        formData.append('cashierDni', this.setupForm.get('cashierDni')?.value);
      }

      // Enviar al backend
      this.http.post(`${environment.apiUrl}/auth/admin-setup`, formData).subscribe({
        next: (response: any) => {
          this.toastService.success('Configuración inicial completada exitosamente');
          // Marcar perfil como completo
          this.authService.loadUserProfile().subscribe(() => {
            this.router.navigate(['/admin']);
          });
        },
        error: (error) => {
          this.isLoading.set(false);
          this.errorMessage.set(error.error?.message || 'Error al guardar la configuración');
        },
        complete: () => {
          this.isLoading.set(false);
        }
      });
    } catch (error: any) {
      this.isLoading.set(false);
      this.errorMessage.set('Error al procesar la configuración');
    }
  }
}

