import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PermissionsGranularService, Module, RolePermission, UpdateRolePermissions, ModulePermission } from '../../../core/services/permissions-granular.service';
import { ToastService } from '../../../shared/services/toast.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface Role {
  id: string;
  name: string;
}

@Component({
  selector: 'app-permissions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './permissions.component.html',
  styleUrl: './permissions.component.css'
})
export class PermissionsComponent implements OnInit {
  roles = signal<Role[]>([]);
  modules = signal<Module[]>([]);
  selectedRoleId = signal<string | null>(null);
  rolePermissions = signal<Map<string, RolePermission>>(new Map());
  isLoading = signal(false);
  isSaving = signal(false);

  // Matriz de permisos: [roleId][moduleId] = { canView, canCreate, canEdit, canDelete }
  permissionsMatrix = signal<Map<string, Map<string, ModulePermission>>>(new Map());

  constructor(
    private permissionsService: PermissionsGranularService,
    private toastService: ToastService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadModules();
  }

  loadRoles(): void {
    this.http.get<Role[]>(`${environment.apiUrl}/permissions/roles`).subscribe({
      next: (roles) => {
        this.roles.set(roles);
        if (roles.length > 0 && !this.selectedRoleId()) {
          this.selectedRoleId.set(roles[0].id);
          this.loadRolePermissions(roles[0].id);
        }
      },
      error: (error) => {
        console.error('Error loading roles:', error);
        this.toastService.error('Error al cargar roles');
      }
    });
  }

  loadModules(): void {
    this.isLoading.set(true);
    this.permissionsService.getAllModules().subscribe({
      next: (modules) => {
        this.modules.set(modules);
        this.initializePermissionsMatrix();
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading modules:', error);
        this.toastService.error('Error al cargar módulos');
        this.isLoading.set(false);
      }
    });
  }

  loadRolePermissions(roleId: string): void {
    this.isLoading.set(true);
    this.permissionsService.getRolePermissions(roleId).subscribe({
      next: (permissions) => {
        // Convertir a Map para acceso rápido
        const permissionsMap = new Map<string, RolePermission>();
        permissions.forEach(p => {
          permissionsMap.set(p.moduleId, p);
        });
        this.rolePermissions.set(permissionsMap);

        // Actualizar matriz de permisos
        this.updatePermissionsMatrix(roleId, permissions);

        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error loading role permissions:', error);
        this.toastService.error('Error al cargar permisos');
        this.isLoading.set(false);
      }
    });
  }

  initializePermissionsMatrix(): void {
    const matrix = new Map<string, Map<string, ModulePermission>>();
    this.roles().forEach(role => {
      const rolePermissions = new Map<string, ModulePermission>();
      this.modules().forEach(module => {
        rolePermissions.set(module.id, {
          moduleId: module.id,
          canView: false,
          canCreate: false,
          canEdit: false,
          canDelete: false
        });
      });
      matrix.set(role.id, rolePermissions);
    });
    this.permissionsMatrix.set(matrix);
  }

  updatePermissionsMatrix(roleId: string, permissions: RolePermission[]): void {
    const matrix = this.permissionsMatrix();
    const roleMatrix = matrix.get(roleId) || new Map<string, ModulePermission>();

    permissions.forEach(perm => {
      roleMatrix.set(perm.moduleId, {
        moduleId: perm.moduleId,
        canView: perm.canView,
        canCreate: perm.canCreate,
        canEdit: perm.canEdit,
        canDelete: perm.canDelete
      });
    });

    matrix.set(roleId, roleMatrix);
    this.permissionsMatrix.set(new Map(matrix));
  }

  onRoleSelected(roleId: string): void {
    this.selectedRoleId.set(roleId);
    this.loadRolePermissions(roleId);
  }

  getPermission(roleId: string, moduleId: string): ModulePermission {
    const matrix = this.permissionsMatrix();
    const roleMatrix = matrix.get(roleId);
    if (!roleMatrix) {
      return {
        moduleId,
        canView: false,
        canCreate: false,
        canEdit: false,
        canDelete: false
      };
    }
    return roleMatrix.get(moduleId) || {
      moduleId,
      canView: false,
      canCreate: false,
      canEdit: false,
      canDelete: false
    };
  }

  updatePermission(roleId: string, moduleId: string, action: 'view' | 'create' | 'edit' | 'delete', value: boolean): void {
    const matrix = this.permissionsMatrix();
    const roleMatrix = matrix.get(roleId) || new Map<string, ModulePermission>();
    const permission = roleMatrix.get(moduleId) || {
      moduleId,
      canView: false,
      canCreate: false,
      canEdit: false,
      canDelete: false
    };

    switch (action) {
      case 'view':
        permission.canView = value;
        break;
      case 'create':
        permission.canCreate = value;
        break;
      case 'edit':
        permission.canEdit = value;
        break;
      case 'delete':
        permission.canDelete = value;
        break;
    }

    roleMatrix.set(moduleId, permission);
    matrix.set(roleId, roleMatrix);
    this.permissionsMatrix.set(new Map(matrix));
  }

  savePermissions(): void {
    const roleId = this.selectedRoleId();
    if (!roleId) {
      this.toastService.error('Selecciona un rol');
      return;
    }

    this.isSaving.set(true);

    const matrix = this.permissionsMatrix();
    const roleMatrix = matrix.get(roleId);
    if (!roleMatrix) {
      this.toastService.error('No hay permisos para guardar');
      this.isSaving.set(false);
      return;
    }

    const modulePermissions: ModulePermission[] = Array.from(roleMatrix.values());

    const updateData: UpdateRolePermissions = {
      roleId,
      modulePermissions
    };

    this.permissionsService.updateRolePermissions(updateData).subscribe({
      next: () => {
        this.toastService.success('Permisos guardados correctamente');
        this.loadRolePermissions(roleId);
        this.isSaving.set(false);
      },
      error: (error) => {
        console.error('Error saving permissions:', error);
        this.toastService.error('Error al guardar permisos');
        this.isSaving.set(false);
      }
    });
  }
}

