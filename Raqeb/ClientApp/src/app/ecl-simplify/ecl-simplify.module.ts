import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ListComponent } from './list/list.component';
import { FormComponent } from './form/form.component';
import { ECLSimplifyRoutingModule } from './ecl-simplify-routing.module';
import { SharedModule } from '../shared/shared.module';



@NgModule({
  declarations: [ListComponent,
    FormComponent
  ],
  imports: [
     CommonModule,
    ECLSimplifyRoutingModule,
    SharedModule
  ]
})
export class EclSimplifyModule { }
