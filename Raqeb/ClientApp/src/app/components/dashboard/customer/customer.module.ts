import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CustomerFormComponent } from './customer-form/customer-form.component';
import { CustomerListComponent } from './customer-list/customer-list.component';
import { CustomerRoutingModule } from './customer-routing.module';
import { SharedModule } from '../../../shared/shared.module';
import { NgxDropzoneModule } from 'ngx-dropzone';

@NgModule({
  imports: [
    CommonModule,
    CustomerRoutingModule,
    SharedModule,
    NgxDropzoneModule
  ],
  declarations: [CustomerFormComponent, CustomerListComponent]
})
export class CustomerModule { }
