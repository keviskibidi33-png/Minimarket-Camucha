import { Component, signal, OnInit, computed, OnDestroy, HostListener, effect, afterNextRender, DestroyRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormArray, FormControl } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/services/toast.service';
import { SetupStatusService } from '../../../core/services/setup-status.service';
import { fadeSlideAnimation } from '../../../shared/animations/route-animations';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

interface SetupStep {
  id: number;
  name: string;
  title: string;
  completed: boolean;
}

@Component({
  selector: 'app-admin-setup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ConfirmDialogComponent],
  templateUrl: './admin-setup.component.html',
  styleUrl: './admin-setup.component.css',
  animations: [fadeSlideAnimation]
})
export class AdminSetupComponent implements OnInit, OnDestroy {
  setupForm: FormGroup;
  currentStep = signal<number>(1);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showExitWarning = signal(false);
  shouldClearData = signal(false); // Flag para indicar si se deben borrar datos existentes
  private readonly FORM_STORAGE_KEY = 'admin_setup_form_data';
  private readonly STEP_STORAGE_KEY = 'admin_setup_current_step';
  private stepEffectCleanup?: ReturnType<typeof effect>;

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
    private route: ActivatedRoute,
    private toastService: ToastService,
    private setupStatusService: SetupStatusService,
    private destroyRef: DestroyRef
  ) {
    this.setupForm = this.fb.group({
      // Paso 1: Información Básica
      storeName: ['', [Validators.required, Validators.maxLength(200)]],
      businessType: ['', Validators.required],
      phone: ['+51 ', [Validators.required, Validators.maxLength(20)]], // Movido aquí, requerido
      whatsAppPhone: ['+51 ', [Validators.maxLength(20)]], // Número de WhatsApp para notificaciones
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
      yapePlinPhone: ['+51 ', Validators.maxLength(20)],
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
    // Esto debe hacerse ANTES de cargar los datos guardados
    this.predefinedCategories.forEach(cat => {
      this.categoriesArray.push(this.fb.control(true));
    });

    // Verificar si se viene desde "Personalizar por Completo" (con parámetro reset)
    const resetParam = this.route.snapshot.queryParams['reset'];
    if (resetParam === 'true') {
      // Si viene con reset=true, limpiar datos y empezar desde el paso 1
      this.clearSavedFormData();
      this.currentStep.set(1);
      // Marcar que se deben borrar datos existentes al completar
      this.shouldClearData.set(true);
      // Remover el parámetro de la URL
      this.router.navigate(['/auth/admin-setup'], { replaceUrl: true });
    } else {
      // Verificar si hay datos guardados válidos antes de cargarlos
      // Si los datos están incompletos o el paso guardado no es válido, limpiar y empezar desde el paso 1
      if (!this.hasValidSavedData()) {
        this.clearSavedFormData();
        this.currentStep.set(1);
      } else {
        // Cargar datos guardados del localStorage DESPUÉS de inicializar categorías
        this.loadSavedFormData();
      }
    }

    // Suscribirse a cambios del formulario para guardar automáticamente
    this.setupForm.valueChanges.subscribe(() => {
      this.saveFormData();
    });

    // Guardar paso actual cuando cambie (usando afterNextRender para contexto de inyección)
    afterNextRender(() => {
      this.stepEffectCleanup = effect(() => {
        const step = this.currentStep();
        localStorage.setItem(this.STEP_STORAGE_KEY, step.toString());
      });

      // Limpiar el effect cuando el componente se destruya
      this.destroyRef.onDestroy(() => {
        this.stepEffectCleanup?.destroy();
      });
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

  ngOnDestroy(): void {
    // Guardar datos antes de destruir el componente
    this.saveFormData();
  }

  @HostListener('window:beforeunload', ['$event'])
  onBeforeUnload(event: BeforeUnloadEvent): void {
    // Si el formulario tiene datos y no está completo, mostrar advertencia
    if (this.hasFormData() && !this.isFormComplete()) {
      event.preventDefault();
      event.returnValue = '¿Estás seguro de salir? Los datos del formulario no se han guardado completamente.';
    }
  }

  private hasFormData(): boolean {
    const formValue = this.setupForm.value;
    return !!(formValue.storeName || formValue.phone || formValue.primaryColor);
  }

  private isFormComplete(): boolean {
    // Verificar si el formulario está completo (todos los pasos validados)
    return this.setupForm.valid && this.currentStep() === 5;
  }

  private hasValidSavedData(): boolean {
    try {
      const savedData = localStorage.getItem(this.FORM_STORAGE_KEY);
      const savedStep = localStorage.getItem(this.STEP_STORAGE_KEY);
      
      // Si no hay datos guardados, no es válido
      if (!savedData || !savedStep) {
        return false;
      }
      
      const formData = JSON.parse(savedData);
      const step = parseInt(savedStep, 10);
      
      // Verificar que el paso sea válido
      if (step < 1 || step > 5) {
        return false;
      }
      
      // Verificar que los datos mínimos requeridos estén presentes
      // Si está en el paso 1, debe tener al menos storeName
      if (step === 1) {
        return !!(formData.storeName && formData.storeName.trim());
      }
      
      // Si está en pasos posteriores, debe tener los datos básicos del paso 1
      if (!formData.storeName || !formData.storeName.trim()) {
        return false;
      }
      
      // Si está en el paso 2 o posterior, debe tener colores
      if (step >= 2 && (!formData.primaryColor || !formData.secondaryColor)) {
        return false;
      }
      
      return true;
    } catch (error) {
      console.error('Error verificando datos guardados:', error);
      return false;
    }
  }

  private saveFormData(): void {
    try {
      const formValue = this.setupForm.value;
      // No guardar archivos en localStorage
      const dataToSave = {
        ...formValue,
        logoFile: null,
        faviconFile: null,
        yapePlinQRFile: null,
        homeBannerImage: null,
        // Guardar categorías personalizadas
        customCategories: this.customCategories()
      };
      localStorage.setItem(this.FORM_STORAGE_KEY, JSON.stringify(dataToSave));
    } catch (error) {
      console.error('Error guardando formulario:', error);
    }
  }

  private loadSavedFormData(): void {
    try {
      const savedData = localStorage.getItem(this.FORM_STORAGE_KEY);
      const savedStep = localStorage.getItem(this.STEP_STORAGE_KEY);
      
      if (savedData) {
        const formData = JSON.parse(savedData);
        
        // Manejar categorías guardadas si existen
        if (formData.categories && Array.isArray(formData.categories)) {
          // Asegurarse de que el FormArray tenga suficientes controles
          while (this.categoriesArray.length < formData.categories.length) {
            this.categoriesArray.push(this.fb.control(false));
          }
          // Actualizar valores de categorías
          formData.categories.forEach((value: boolean, index: number) => {
            if (this.categoriesArray.at(index)) {
              this.categoriesArray.at(index).setValue(value);
            }
          });
          // Remover categorías del objeto para evitar conflictos al hacer patchValue
          delete formData.categories;
        }
        
        // Cargar categorías personalizadas si existen
        if (formData.customCategories && Array.isArray(formData.customCategories)) {
          this.customCategories.set(formData.customCategories);
          delete formData.customCategories;
        }
        
        // Aplicar el resto de los datos al formulario
        this.setupForm.patchValue(formData);
      }
      
      if (savedStep) {
        const step = parseInt(savedStep, 10);
        if (step >= 1 && step <= 5) {
          this.currentStep.set(step);
        } else {
          // Si el paso guardado no es válido, resetear a paso 1
          this.currentStep.set(1);
        }
      }
    } catch (error) {
      console.error('Error cargando formulario guardado:', error);
      // En caso de error, asegurarse de que el paso sea válido
      this.currentStep.set(1);
    }
  }

  clearSavedFormData(): void {
    localStorage.removeItem(this.FORM_STORAGE_KEY);
    localStorage.removeItem(this.STEP_STORAGE_KEY);
  }

  attemptExit(): void {
    // Si el formulario tiene datos, mostrar advertencia
    if (this.hasFormData() && !this.isFormComplete()) {
      this.showExitWarning.set(true);
    } else {
      // Si no hay datos o está completo, permitir salir
      this.handleExit();
    }
  }

  handleExit(): void {
    // Limpiar datos guardados si el usuario decide salir
    this.clearSavedFormData();
    this.showExitWarning.set(false);
    // Redirigir al dashboard
    this.router.navigate(['/admin']);
  }

  onExitWarningConfirmed(): void {
    // Usuario decidió continuar con la configuración
    this.showExitWarning.set(false);
  }

  onExitWarningCancelled(): void {
    // Usuario decidió omitir por ahora
    this.handleExit();
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
      formData.append('whatsAppPhone', this.setupForm.get('whatsAppPhone')?.value || '');
      
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
      
      // Flag para borrar datos existentes (si viene desde "Personalizar por Completo")
      formData.append('clearExistingData', this.shouldClearData().toString());
      
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
            // Limpiar datos guardados
            this.clearSavedFormData();
            // Marcar setup como completo
            this.setupStatusService.markSetupComplete();
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

