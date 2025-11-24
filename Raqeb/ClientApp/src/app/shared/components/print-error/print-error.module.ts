import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PrintErrorComponent } from './print-error.component';
import { TranslateModule } from '@ngx-translate/core';



@NgModule({
  imports: [CommonModule, TranslateModule],
  declarations: [PrintErrorComponent],
  exports: [PrintErrorComponent],
})
export class PrintErrorModule { }
