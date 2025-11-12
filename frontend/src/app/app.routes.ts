import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'blogs' },
  { path: 'blogs', loadComponent: () => import('./pages/blog-list/blog-list.component').then(m => m.BlogListComponent) },
  { path: 'blogs/new', loadComponent: () => import('./pages/blog-form/blog-form.component').then(m => m.BlogFormComponent) },
  { path: 'blogs/:id', loadComponent: () => import('./pages/blog-detail/blog-detail.component').then(m => m.BlogDetailComponent) },
  { path: 'blogs/:id/edit', loadComponent: () => import('./pages/blog-form/blog-form.component').then(m => m.BlogFormComponent) },
  { path: '**', redirectTo: 'blogs' }
];
