import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LocalizationCodeListComponent } from './localization-code-list/localization-code-list.component';
import { LocalizationCodeFormComponent } from './localization-code-form/localization-code-form.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        component: LocalizationCodeListComponent
      },
      {
        path: 'create',
        component: LocalizationCodeFormComponent
      },
      {
        path: 'edit/:id',
        component: LocalizationCodeFormComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LocalizationLanguageRoutingModule { }
