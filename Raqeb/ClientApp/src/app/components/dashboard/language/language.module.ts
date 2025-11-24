import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from '../../../shared/shared.module';
import { LanguageRoutingModule } from './language-routing.module';
import { LanguageFormComponent } from './language-form/language-form.component';
import { LanguageListComponent } from './language-list/language-list.component';


@NgModule({
  declarations: [LanguageFormComponent, LanguageListComponent],
  imports: [
    CommonModule,
    LanguageRoutingModule,
    SharedModule,
  ]
})
export class LanguageModule { }
