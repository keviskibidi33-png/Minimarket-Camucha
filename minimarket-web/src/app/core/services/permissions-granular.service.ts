import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface Module {
  id: string;
  nombre: string;
  descripcion: string;
  slug: string;
  isActive: boolean;
}

export interface RolePermission {
  id: string;
  roleId: string;
  roleName: string;
  moduleId: string;
  moduleName: string;
  moduleSlug: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

export interface UpdateRolePermissions {
  roleId: string;
  modulePermissions: ModulePermission[];
}

export interface ModulePermission {
  moduleId: string;
  canView: boolean;
  canCreate: boolean;
  canEdit: boolean;
  canDelete: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class PermissionsGranularService {
  private readonly apiUrl = `${environment.apiUrl}/permissions`;

  constructor(private http: HttpClient) {}

  getAllModules(): Observable<Module[]> {
    return this.http.get<Module[]>(`${this.apiUrl}/modules`);
  }

  getRolePermissions(roleId?: string): Observable<RolePermission[]> {
    let params = new HttpParams();
    if (roleId) {
      params = params.set('roleId', roleId);
    }
    return this.http.get<RolePermission[]>(`${this.apiUrl}/role-permissions`, { params });
  }

  updateRolePermissions(permissions: UpdateRolePermissions): Observable<RolePermission[]> {
    return this.http.put<RolePermission[]>(`${this.apiUrl}/role-permissions`, permissions);
  }
}

