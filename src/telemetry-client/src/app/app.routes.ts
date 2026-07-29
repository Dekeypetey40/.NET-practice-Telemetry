import { Routes } from '@angular/router';
import { RunListComponent } from './components/run-list/run-list.component';
import { RunCreateComponent } from './components/run-create/run-create.component';
import { RunDetailComponent } from './components/run-detail/run-detail.component';
import { InstrumentListComponent } from './components/instrument-list/instrument-list.component';
import { SupportComponent } from './components/support/support.component';

export const routes: Routes = [
  { path: '', redirectTo: 'runs', pathMatch: 'full' },
  { path: 'runs', component: RunListComponent },
  { path: 'runs/new', component: RunCreateComponent },
  { path: 'runs/:id', component: RunDetailComponent },
  { path: 'instruments', component: InstrumentListComponent },
  { path: 'support', component: SupportComponent },
];
