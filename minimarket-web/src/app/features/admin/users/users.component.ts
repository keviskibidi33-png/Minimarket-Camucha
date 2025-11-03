import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

// TODO: Crear servicio de usuarios cuando esté disponible en el backend
export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  createdAt: string;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit {
  users = signal<User[]>([]);
  filteredUsers = signal<User[]>([]);
  isLoading = signal(true);
  searchTerm = signal('');
  selectedRole = signal<string>('all'); // 'all', 'Gerente', 'Vendedor'

  // Mock data por ahora - TODO: Reemplazar con servicio real
  mockUsers: User[] = [
    {
      id: '1',
      username: 'Ana Torres',
      email: 'ana.torres@example.com',
      role: 'Gerente',
      createdAt: '15/08/2023'
    },
    {
      id: '2',
      username: 'Carlos Gomez',
      email: 'carlos.gomez@example.com',
      role: 'Vendedor',
      createdAt: '12/07/2023'
    },
    {
      id: '3',
      username: 'Luisa Fernandez',
      email: 'luisa.fernandez@example.com',
      role: 'Vendedor',
      createdAt: '10/06/2023'
    },
    {
      id: '4',
      username: 'Miguel Soto',
      email: 'miguel.soto@example.com',
      role: 'Vendedor',
      createdAt: '05/05/2023'
    }
  ];

  constructor() {}

  ngOnInit() {
    // TODO: Cargar usuarios desde el servicio
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    // Simular carga
    setTimeout(() => {
      this.users.set(this.mockUsers);
      this.filteredUsers.set(this.mockUsers);
      this.isLoading.set(false);
    }, 500);
  }

  onSearch() {
    const search = this.searchTerm().toLowerCase();
    const role = this.selectedRole();
    
    let filtered = this.users();
    
    if (search) {
      filtered = filtered.filter(user => 
        user.username.toLowerCase().includes(search) ||
        user.email.toLowerCase().includes(search)
      );
    }
    
    if (role !== 'all') {
      filtered = filtered.filter(user => user.role === role);
    }
    
    this.filteredUsers.set(filtered);
  }

  onRoleFilterChange(role: string) {
    this.selectedRole.set(role);
    this.onSearch();
  }

  deleteUser(userId: string) {
    // TODO: Implementar eliminación de usuario
    if (confirm('¿Está seguro de eliminar este usuario?')) {
      const updated = this.users().filter(u => u.id !== userId);
      this.users.set(updated);
      this.onSearch();
    }
  }

  getRoleBadgeClass(role: string): string {
    if (role === 'Gerente') {
      return 'inline-flex items-center rounded-full bg-primary-admin/20 px-2.5 py-0.5 text-xs font-medium text-primary-admin';
    }
    return 'inline-flex items-center rounded-full bg-green-100 dark:bg-green-900/50 px-2.5 py-0.5 text-xs font-medium text-success dark:text-green-300';
  }
}

