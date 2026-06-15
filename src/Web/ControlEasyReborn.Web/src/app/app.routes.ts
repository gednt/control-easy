import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { changePasswordGuard } from './core/guards/change-password.guard';
import { demoModeGuard } from './core/guards/demo-mode.guard';
import { platformAdminGuard } from './core/guards/platform-admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.page').then(m => m.LoginPage),
  },
  {
    path: 'change-password',
    canActivate: [changePasswordGuard],
    loadComponent: () => import('./features/auth/change-password.page').then(m => m.ChangePasswordPage),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/app-shell.component').then(m => m.AppShellComponent),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard.page').then(m => m.DashboardPage),
      },
      {
        path: 'help/demo',
        canActivate: [demoModeGuard],
        loadComponent: () => import('./features/help/demo-help.page').then(m => m.DemoHelpPageComponent),
      },
      {
        path: 'residents',
        loadComponent: () => import('./features/residents/residents.page').then(m => m.ResidentsPage),
      },
      {
        path: 'apartments',
        loadComponent: () => import('./features/apartments/apartments.page').then(m => m.ApartmentsPage),
      },
      {
        path: 'visits',
        loadComponent: () => import('./features/visits/visits.page').then(m => m.VisitsPage),
      },
      {
        path: 'vehicles',
        loadComponent: () => import('./features/vehicles/vehicles.page').then(m => m.VehiclesPage),
      },
      {
        path: 'service-providers',
        loadComponent: () => import('./features/service-providers/service-providers.page').then(m => m.ServiceProvidersPage),
      },
      {
        path: 'administration',
        loadComponent: () => import('./features/administration/administration.page').then(m => m.AdministrationPage),
      },
      {
        path: 'platform/condominiums',
        canActivate: [platformAdminGuard],
        loadComponent: () => import('./features/platform/condominiums.page').then(m => m.CondominiumsPage),
      },
      {
        path: 'design-system/showcase',
        loadComponent: () => import('./design-system/showcase/showcase.page').then(m => m.ShowcasePageComponent),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '/login',
  },
];