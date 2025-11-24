import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { LocalizationRoutingModule } from './localization-routing.module';
import { LocalizationFormComponent } from './localization-form/localization-form.component';
import { LocalizationListComponent } from './localization-list/localization-list.component';
import { SharedModule } from '../../../shared/shared.module';


@NgModule({
  declarations: [LocalizationFormComponent, LocalizationListComponent],
  imports: [
    CommonModule,
    LocalizationRoutingModule,
    SharedModule
  ]
})
export class LocalizationModule { }
