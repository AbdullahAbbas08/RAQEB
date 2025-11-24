import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LocalizationLanguageRoutingModule } from './localization-lang-routing.module';
import { LocalizationCodeListComponent } from './localization-code-list/localization-code-list.component';
import { LocalizationCodeFormComponent } from './localization-code-form/localization-code-form.component';
import { SharedModule } from '../../../../app/shared/shared.module';

@NgModule({
  declarations: [LocalizationCodeListComponent, LocalizationCodeFormComponent],
  imports: [
    CommonModule,
    LocalizationLanguageRoutingModule,
    SharedModule

  ]
})
export class LocalizationLangModule { }
