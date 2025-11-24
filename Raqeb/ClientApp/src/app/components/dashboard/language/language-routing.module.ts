import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LanguageListComponent } from './language-list/language-list.component';
import { LanguageFormComponent } from './language-form/language-form.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        component: LanguageListComponent
      },
      {
        path: 'create',
        component: LanguageFormComponent
      },
      {
        path: 'edit/:id',
        component: LanguageFormComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LanguageRoutingModule { }
