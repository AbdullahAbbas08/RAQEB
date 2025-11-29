import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ListComponent } from './list/list.component';
import { FormComponent } from './form/form.component';
import { ECLRoutingModule } from './ecl-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { EclCustomersComponent } from './ecl-customers/ecl-customers.component';



@NgModule({
  declarations: [ListComponent,FormComponent,EclCustomersComponent],
  imports: [
    CommonModule,
    ECLRoutingModule,
    SharedModule
  ]
})
export class EclModule { }
