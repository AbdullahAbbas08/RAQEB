import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ListComponent } from './list/list.component';
import { FormComponent } from './form/form.component';
import { EclCustomersComponent } from './ecl-customers/ecl-customers.component';

const routes: Routes = [
    {
          path: "list",
          component: ListComponent,
        },
        {
          path: "form",
          component: FormComponent,
        },
        {
          path: "customers",
          component: EclCustomersComponent,
        },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ECLRoutingModule { }
