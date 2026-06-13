import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { demoModeGuard } from './core/guards/demo-mode.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.page').then(m => m.LoginPage),
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
    ],
  },
  {
    path: '**',
    redirectTo: '/login',
  },
];