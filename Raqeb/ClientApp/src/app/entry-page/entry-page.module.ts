import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EntryPageComponent } from './entry-page.component';
import { EntryPageRoutingModule } from './entry-page-routing.module';
import { SharedModule } from '../shared/shared.module';



@NgModule({
  declarations: [EntryPageComponent],
  imports: [
    CommonModule,
     EntryPageRoutingModule,
        SharedModule
  ]
})
export class EntryPageModule { }
