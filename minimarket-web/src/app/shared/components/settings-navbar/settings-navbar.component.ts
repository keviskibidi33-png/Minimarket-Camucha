import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-settings-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './settings-navbar.component.html',
  styleUrl: './settings-navbar.component.css'
})
export class SettingsNavbarComponent {
  currentTab: string = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) {
    // Detectar el tab activo desde la URL
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateActiveTab();
      });
    
    this.updateActiveTab();
  }

  private updateActiveTab(): void {
    const url = this.router.url;
    
    if (url.includes('/configuraciones/marca')) {
      this.currentTab = 'marca';
    } else if (url.includes('/configuraciones/permisos')) {
      this.currentTab = 'permisos';
    } else if (url.includes('/configuraciones')) {
      // Leer query param desde la URL
      const parsedUrl = this.router.parseUrl(url);
      const tabParam = parsedUrl.queryParams['tab'];
      this.currentTab = tabParam || 'cart';
    } else {
      this.currentTab = '';
    }
  }

  isActiveTab(tab: string): boolean {
    return this.currentTab === tab;
  }
}

