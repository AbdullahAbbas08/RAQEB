import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CustomerFormComponent } from './customer-form/customer-form.component';
import { CustomerListComponent } from './customer-list/customer-list.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        component: CustomerListComponent
      },
      {
        path: 'create',
        component: CustomerFormComponent
      },
      {
        path: 'edit/:id',
        component: CustomerFormComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class CustomerRoutingModule { }
