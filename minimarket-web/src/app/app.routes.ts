import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';
import { setupGuard } from './core/guards/setup.guard';

export const routes: Routes = [
  // Rutas públicas de la TIENDA
  {
    path: '',
    loadComponent: () => import('./features/store/home/home.component').then(m => m.HomeComponent)
  },
  {
    path: 'tienda/productos',
    loadComponent: () => import('./features/store/products/products.component').then(m => m.StoreProductsComponent)
  },
  {
    path: 'tienda/ofertas',
    loadComponent: () => import('./features/store/ofertas/ofertas.component').then(m => m.StoreOfertasComponent)
  },
  {
    path: 'tienda/ofertas/:id',
    loadComponent: () => import('./features/store/ofertas/oferta-detail/oferta-detail.component').then(m => m.OfertaDetailComponent)
  },
  {
    path: 'tienda/contacto',
    loadComponent: () => import('./features/store/contact/contact.component').then(m => m.StoreContactComponent)
  },
  {
    path: 'tienda/producto/:id',
    loadComponent: () => import('./features/store/product-detail/product-detail.component').then(m => m.ProductDetailComponent)
  },
  {
    path: 'carrito',
    loadComponent: () => import('./features/store/cart/cart.component').then(m => m.CartComponent)
  },
  {
    path: 'checkout/envio',
    loadComponent: () => import('./features/store/checkout/shipping/shipping.component').then(m => m.ShippingComponent)
  },
  {
    path: 'checkout/pago',
    loadComponent: () => import('./features/store/checkout/payment/payment.component').then(m => m.PaymentComponent)
  },
  {
    path: 'checkout/confirmacion',
    loadComponent: () => import('./features/store/checkout/confirmation/confirmation.component').then(m => m.ConfirmationComponent)
  },
  {
    path: 'checkout/exito',
    loadComponent: () => import('./features/store/checkout/success/success.component').then(m => m.SuccessComponent)
  },
  // Rutas de AUTENTICACIÓN (públicas)
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'auth/reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },
  {
    path: 'auth/success-reset-password',
    loadComponent: () => import('./features/auth/success-reset-password/success-reset-password.component').then(m => m.SuccessResetPasswordComponent)
  },
  {
    path: 'auth/complete-profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/auth/complete-profile/complete-profile.component').then(m => m.CompleteProfileComponent)
  },
  {
    path: 'auth/admin-setup',
    canActivate: [authGuard, roleGuard(['Administrador'])],
    loadComponent: () => import('./features/auth/admin-setup/admin-setup.component').then(m => m.AdminSetupComponent)
  },
  // Rutas LEGALES (públicas)
  {
    path: 'legal/terms',
    loadComponent: () => import('./features/legal/terms/terms.component').then(m => m.TermsComponent)
  },
  {
    path: 'legal/privacy',
    loadComponent: () => import('./features/legal/privacy/privacy.component').then(m => m.PrivacyComponent)
  },
  {
    path: 'legal/additional-purposes',
    loadComponent: () => import('./features/legal/additional-purposes/additional-purposes.component').then(m => m.AdditionalPurposesComponent)
  },
  // Ruta legacy para login (redirigir a /auth/login)
  {
    path: 'login',
    redirectTo: '/auth/login',
    pathMatch: 'full'
  },
  // Ruta de PERFIL DE USUARIO (protegida)
  {
    path: 'perfil',
    canActivate: [authGuard],
    loadComponent: () => import('./features/store/profile/profile.component').then(m => m.ProfileComponent)
  },
  {
    path: 'admin',
    canActivate: [authGuard, setupGuard],
    loadComponent: () => import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'pos',
        canActivate: [roleGuard(['Administrador', 'Cajero'])],
        loadComponent: () => import('./features/pos/pos.component').then(m => m.PosComponent)
      },
      {
        path: 'dashboard',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'productos',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/products/products.component').then(m => m.ProductsComponent)
      },
      {
        path: 'productos/nuevo',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/products/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'productos/editar/:id',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/products/product-form/product-form.component').then(m => m.ProductFormComponent)
      },
      {
        path: 'clientes',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'clientes/nuevo',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/customers/customer-form/customer-form.component').then(m => m.CustomerFormComponent)
      },
      {
        path: 'clientes/editar/:id',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/customers/customer-form/customer-form.component').then(m => m.CustomerFormComponent)
      },
      {
        path: 'ventas',
        canActivate: [roleGuard(['Administrador', 'Cajero'])],
        loadComponent: () => import('./features/sales/sales.component').then(m => m.SalesComponent)
      },
      {
        path: 'ventas/:id',
        canActivate: [roleGuard(['Administrador', 'Cajero'])],
        loadComponent: () => import('./features/sales/sale-detail/sale-detail.component').then(m => m.SaleDetailComponent)
      },
      {
        path: 'ventas/:id/anular',
        canActivate: [roleGuard(['Administrador', 'Cajero'])],
        loadComponent: () => import('./features/sales/cancel-sale/cancel-sale.component').then(m => m.CancelSaleComponent)
      },
      {
        path: 'categorias',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/categories/categories.component').then(m => m.CategoriesComponent)
      },
      {
        path: 'categorias/nuevo',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'categorias/:id',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/categories/category-detail/category-detail.component').then(m => m.CategoryDetailComponent)
      },
      {
        path: 'categorias/editar/:id',
        canActivate: [roleGuard(['Administrador', 'Almacenero'])],
        loadComponent: () => import('./features/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'usuarios',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'configuraciones',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'configuraciones/marca',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/brand-settings/brand-settings.component').then(m => m.BrandSettingsComponent)
      },
      {
        path: 'configuraciones/permisos',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/permissions/permissions.component').then(m => m.PermissionsComponent)
      },
      {
        path: 'sedes',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/sedes/sedes.component').then(m => m.SedesComponent)
      },
      {
        path: 'ofertas',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/ofertas/ofertas.component').then(m => m.OfertasComponent)
      },
      {
        path: 'page-builder',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/page-builder/page-builder.component').then(m => m.PageBuilderComponent)
      },
      {
        path: 'analytics',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/analytics/analytics.component').then(m => m.AnalyticsComponent)
      },
      {
        path: 'pedidos',
        canActivate: [roleGuard(['Administrador'])],
        loadComponent: () => import('./features/admin/orders/orders.component').then(m => m.OrdersComponent)
      }
    ]
  },
  // Redirección legacy para rutas admin sin /admin
  {
    path: 'dashboard',
    redirectTo: '/admin/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'productos',
    redirectTo: '/admin/productos',
    pathMatch: 'prefix'
  },
  {
    path: 'clientes',
    redirectTo: '/admin/clientes',
    pathMatch: 'prefix'
  },
  {
    path: 'ventas',
    redirectTo: '/admin/ventas',
    pathMatch: 'prefix'
  },
  {
    path: 'categorias',
    redirectTo: '/admin/categorias',
    pathMatch: 'prefix'
  },
  {
    path: '404',
    loadComponent: () => import('./features/errors/not-found/not-found.component').then(m => m.NotFoundComponent)
  },
  {
    path: '500',
    loadComponent: () => import('./features/errors/server-error/server-error.component').then(m => m.ServerErrorComponent)
  },
  {
    path: '**',
    redirectTo: '/404'
  }
];

