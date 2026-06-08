import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: '',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/landing/landing').then(m => m.LandingComponent),
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login').then(m => m.LoginComponent),
  },
  {
    path: 'home',
    redirectTo: 'notes',
    pathMatch: 'full',
  },
  {
    path: 'notes',
    canActivate: [authGuard],
    loadComponent: () => import('./features/notes/note-list').then(m => m.NoteListComponent),
  },
  {
    path: 'notes/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/notes/note-editor').then(m => m.NoteEditorComponent),
  },
  {
    path: 'graph',
    canActivate: [authGuard],
    loadComponent: () => import('./features/graph/knowledge-graph').then(m => m.KnowledgeGraphComponent),
  },
  {
    path: 'share/:token',
    loadComponent: () => import('./features/share/shared-note').then(m => m.SharedNoteComponent),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
