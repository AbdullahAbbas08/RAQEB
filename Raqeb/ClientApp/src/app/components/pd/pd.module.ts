import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PDRoutingModule } from './pd-routing.module';
import { SharedModule } from '../../shared/shared.module';
import { PdFormComponent } from './pd-form/pd-form.component';
import { PdListComponent } from './pd-list/pd-list.component';
import { TransitionMatrixComponent } from './transition-matrix/transition-matrix.component';
import { YearlyAvgTransitionMatrixComponent } from './yearly-avg-transition-matrix/yearly-avg-transition-matrix.component';
import { LongRunMatrixComponent } from './long-run-matrix/long-run-matrix.component';
import { ODRComponent } from './odr/odr.component';
import { CalibrationSummariesComponent } from './calibration-summaries/calibration-summaries.component';
import { MarginalPDComponent } from './marginal-pd/marginal-pd.component';


@NgModule({
  declarations: [
    PdFormComponent,
    PdListComponent,
    TransitionMatrixComponent,
    YearlyAvgTransitionMatrixComponent,
    LongRunMatrixComponent,
    ODRComponent,
    CalibrationSummariesComponent,
    MarginalPDComponent
  ],
  imports: [
    CommonModule,
    PDRoutingModule,
    SharedModule
  ]
})
export class PDModule { }
