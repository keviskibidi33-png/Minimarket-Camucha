import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UsersService, User, CreateUserRequest, UpdateUserRequest } from '../../../core/services/users.service';
import { ToastService } from '../../../shared/services/toast.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  users = signal<User[]>([]);
  filteredUsers = signal<User[]>([]);
  isLoading = signal(true);
  searchTerm = signal('');
  selectedRole = signal<string>('all'); // 'all', 'Administrador', 'Cajero', 'Almacenero', 'Cliente'
  
  // Paginación
  currentPage = signal(1);
  pageSize = 10;
  totalCount = signal(0);
  totalPages = signal(0);
  pagesArray = computed(() => {
    return Array.from({ length: this.totalPages() }, (_, i) => i + 1);
  });

  // Modales
  showCreateModal = signal(false);
  showEditModal = signal(false);
  showDeleteModal = signal(false);
  showResetPasswordModal = signal(false);
  selectedUser = signal<User | null>(null);

  // Formularios
  createUserForm: FormGroup;
  editUserForm: FormGroup;
  resetPasswordForm: FormGroup;

  // Roles disponibles
  availableRoles = ['Administrador', 'Cajero', 'Almacenero', 'Cliente'];

  constructor(
    private usersService: UsersService,
    private toastService: ToastService,
    private fb: FormBuilder
  ) {
    this.createUserForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      dni: ['', [Validators.required, Validators.pattern(/^\d{8}$/)]],
      phone: ['', [Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      roles: [[], [Validators.required, Validators.minLength(1)]],
      emailConfirmed: [true]
    });

    this.editUserForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.maxLength(20)]],
      roles: [[], [Validators.required, Validators.minLength(1)]],
      emailConfirmed: [true]
    });

    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.usersService.getAll({
      searchTerm: this.searchTerm() || undefined,
      roleFilter: this.selectedRole() !== 'all' ? this.selectedRole() : undefined,
      pageNumber: this.currentPage(),
      pageSize: this.pageSize
    }).subscribe({
      next: (users) => {
        this.users.set(users);
        this.filteredUsers.set(users);
        this.totalCount.set(users.length);
        this.totalPages.set(Math.ceil(users.length / this.pageSize));
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading users:', error);
        this.toastService.error('Error al cargar usuarios');
        this.isLoading.set(false);
      }
    });
  }

  onSearch() {
    this.currentPage.set(1);
    this.loadUsers();
  }

  onRoleFilterChange(role: string) {
    this.selectedRole.set(role);
    this.currentPage.set(1);
    this.loadUsers();
  }

  openCreateModal() {
    this.createUserForm.reset({
      roles: [],
      emailConfirmed: true
    });
    this.showCreateModal.set(true);
  }

  closeCreateModal() {
    this.showCreateModal.set(false);
    this.createUserForm.reset();
  }

  onCreateUser() {
    if (this.createUserForm.invalid) {
      this.toastService.error('Por favor, completa todos los campos requeridos');
      return;
    }

    const formValue = this.createUserForm.value;
    const createRequest: CreateUserRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      dni: formValue.dni,
      phone: formValue.phone || undefined,
      password: formValue.password,
      roles: formValue.roles,
      emailConfirmed: formValue.emailConfirmed ?? true
    };

    this.usersService.create(createRequest).subscribe({
      next: () => {
        this.toastService.success('Usuario creado exitosamente');
        this.closeCreateModal();
        this.loadUsers();
      },
      error: (error) => {
        console.error('Error creating user:', error);
        this.toastService.error(error.error?.message || 'Error al crear usuario');
      }
    });
  }

  openEditModal(user: User) {
    this.selectedUser.set(user);
    this.editUserForm.patchValue({
      firstName: user.firstName || '',
      lastName: user.lastName || '',
      email: user.email,
      phone: user.phone || '',
      roles: user.roles || [],
      emailConfirmed: user.emailConfirmed
    });
    this.showEditModal.set(true);
  }

  closeEditModal() {
    this.showEditModal.set(false);
    this.selectedUser.set(null);
    this.editUserForm.reset();
  }

  onUpdateUser() {
    if (this.editUserForm.invalid || !this.selectedUser()) {
      this.toastService.error('Por favor, completa todos los campos requeridos');
      return;
    }

    const formValue = this.editUserForm.value;
    const updateRequest: UpdateUserRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone || undefined,
      roles: formValue.roles,
      emailConfirmed: formValue.emailConfirmed
    };

    this.usersService.update(this.selectedUser()!.id, updateRequest).subscribe({
      next: () => {
        this.toastService.success('Usuario actualizado exitosamente');
        this.closeEditModal();
        this.loadUsers();
      },
      error: (error) => {
        console.error('Error updating user:', error);
        this.toastService.error(error.error?.message || 'Error al actualizar usuario');
      }
    });
  }

  openDeleteModal(user: User) {
    this.selectedUser.set(user);
    this.showDeleteModal.set(true);
  }

  closeDeleteModal() {
    this.showDeleteModal.set(false);
    this.selectedUser.set(null);
  }

  onDeleteUser() {
    if (!this.selectedUser()) return;

    this.usersService.delete(this.selectedUser()!.id).subscribe({
      next: () => {
        this.toastService.success('Usuario eliminado exitosamente');
        this.closeDeleteModal();
        this.loadUsers();
      },
      error: (error) => {
        console.error('Error deleting user:', error);
        this.toastService.error(error.error?.message || 'Error al eliminar usuario');
      }
    });
  }

  openResetPasswordModal(user: User) {
    this.selectedUser.set(user);
    this.resetPasswordForm.reset();
    this.showResetPasswordModal.set(true);
  }

  closeResetPasswordModal() {
    this.showResetPasswordModal.set(false);
    this.selectedUser.set(null);
    this.resetPasswordForm.reset();
  }

  onResetPassword() {
    if (this.resetPasswordForm.invalid || !this.selectedUser()) {
      this.toastService.error('Por favor, completa todos los campos');
      return;
    }

    const formValue = this.resetPasswordForm.value;
    if (formValue.newPassword !== formValue.confirmPassword) {
      this.toastService.error('Las contraseñas no coinciden');
      return;
    }

    this.usersService.resetPassword(this.selectedUser()!.id, formValue.newPassword).subscribe({
      next: () => {
        this.toastService.success('Contraseña actualizada exitosamente');
        this.closeResetPasswordModal();
      },
      error: (error) => {
        console.error('Error resetting password:', error);
        this.toastService.error(error.error?.message || 'Error al actualizar contraseña');
      }
    });
  }

  onPageChange(page: number) {
    this.currentPage.set(page);
    this.loadUsers();
  }

  getRoleBadgeClass(roles: string[]): string {
    if (roles.includes('Administrador')) {
      return 'inline-flex items-center rounded-full bg-primary-admin/20 px-2.5 py-0.5 text-xs font-medium text-primary-admin';
    }
    if (roles.includes('Cajero')) {
      return 'inline-flex items-center rounded-full bg-blue-100 dark:bg-blue-900/50 px-2.5 py-0.5 text-xs font-medium text-blue-800 dark:text-blue-300';
    }
    if (roles.includes('Almacenero')) {
      return 'inline-flex items-center rounded-full bg-purple-100 dark:bg-purple-900/50 px-2.5 py-0.5 text-xs font-medium text-purple-800 dark:text-purple-300';
    }
    return 'inline-flex items-center rounded-full bg-green-100 dark:bg-green-900/50 px-2.5 py-0.5 text-xs font-medium text-success dark:text-green-300';
  }

  formatDate(date: string): string {
    if (!date) return 'N/A';
    try {
      const d = new Date(date);
      return d.toLocaleDateString('es-PE', { 
        year: 'numeric', 
        month: '2-digit', 
        day: '2-digit' 
      });
    } catch {
      return date;
    }
  }

  getRolesDisplay(roles: string[]): string {
    return roles.length > 0 ? roles.join(', ') : 'Sin rol';
  }

  isRoleSelected(role: string, formType: 'create' | 'edit'): boolean {
    const form = formType === 'create' ? this.createUserForm : this.editUserForm;
    const rolesControl = form.get('roles');
    return rolesControl?.value?.includes(role) || false;
  }

  toggleRole(role: string, formType: 'create' | 'edit'): void {
    const form = formType === 'create' ? this.createUserForm : this.editUserForm;
    const rolesControl = form.get('roles');
    if (!rolesControl) return;
    
    const currentRoles = rolesControl.value || [];
    if (currentRoles.includes(role)) {
      rolesControl.setValue(currentRoles.filter((r: string) => r !== role));
    } else {
      rolesControl.setValue([...currentRoles, role]);
    }
  }

  Math = Math;
}

