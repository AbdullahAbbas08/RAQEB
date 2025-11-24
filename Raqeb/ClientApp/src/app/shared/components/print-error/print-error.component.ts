import { Component, Input, OnInit } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-print-error',
  templateUrl: './print-error.component.html',
  styleUrls: ['./print-error.component.scss']
})
export class PrintErrorComponent implements OnInit {

  @Input('control') formControl!: AbstractControl | null;
  @Input('minlength') minlength?: number;
  @Input('maxlength') maxlength?: number;
  @Input('max') max?: number;
  @Input('min') min?: number;
  @Input('patternKey') patternKey?: string = 'error.pattern';
  constructor() {}

  ngOnInit(): void {}

}
