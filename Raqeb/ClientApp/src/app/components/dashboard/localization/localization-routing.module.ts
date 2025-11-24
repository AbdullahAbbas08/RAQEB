import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LocalizationFormComponent } from './localization-form/localization-form.component';
import { LocalizationListComponent } from './localization-list/localization-list.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        component: LocalizationListComponent
      },
      {
        path: 'create',
        component: LocalizationFormComponent
      },
      {
        path: 'edit/:id',
        component: LocalizationFormComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LocalizationRoutingModule { }
