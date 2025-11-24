import { HttpErrorResponse } from '@angular/common/http';
import { Directive, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormGroupName } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { MenuItem, MessageService } from 'primeng/api';
import { Location } from '@angular/common';

@Directive()
export abstract class DefaultFormComponent<K> implements OnInit {
  isLoadingData: boolean = false;
  isLoadingBtn: boolean = false;
  form!: FormGroup;
  editMode: boolean = false;
  // abstract breadcrumb: MenuItem[];
  constructor(
    protected route: ActivatedRoute,
    protected fb: FormBuilder,
    protected router: Router,
    protected toast: MessageService,
    protected location: Location,

  ) { }

  ngOnInit(): void {
    this.checkAddEdit();
    this.initForm();
    this.getDataFromServer();
  }

  private checkAddEdit() {
    if (this.route.snapshot.paramMap.get('id')) {
      this.editMode = true;
    }
  }

   getDataFromServer() {
    this.isLoadingData = true;
    if (this.editMode) {
      // this.breadcrumb.push({
      //   label: 'edit',
      // });
      this.editMode = true;
      this.onEdit();
      this.returnGetModelByIdFn().subscribe(
        (entity) => {
          this.form.patchValue(entity as any);
          this.postSubscribtion(entity);
          this.isLoadingData = false;          
        },
        (err: HttpErrorResponse) => {
          this.isLoadingData = false;
        }

      );
    } else {
      // this.breadcrumb.push({
      //   label: 'add',
      // });
      this.onAdd();
      this.isLoadingData = false;
    }
  }

  submit() {
    if (this.form.invalid) {
      this.toast.add({
        severity: 'error',
        detail: 'invalidForm',
      });
      this.form.markAllAsTouched();
    }
    else {

      this.isLoadingBtn = true;
      if (this.editMode) {
        this.returnEditFn().subscribe(
          (response) => {
            if (response) {
              this.toast.add({
                severity: 'success',
                detail: 'ProcessSuccess',
              });
            }

            this.onSave(response);
            this.isLoadingBtn = false;
          },
          (err: HttpErrorResponse) => {
            this.isLoadingBtn = false;
          }
        );
      } else {

        this.isLoadingBtn = true;

        this.returnAddFn().subscribe(
          (response) => {

            if (response) {
              this.toast.add({
                severity: 'success',
                detail: 'ProcessSuccess',
              });
            }
            this.onSave(response);

            this.isLoadingBtn = false;
          },
          (err: HttpErrorResponse) => {
            this.isLoadingBtn = false;
          }
        );
      }
    }
  }

  onCancel(): void {
    // this.router.navigate([
    //   this.breadcrumb[this.breadcrumb.length - 2].routerLink,]);
  }

  validateEnglishChars(event: KeyboardEvent): boolean {
    const pattern = /^[A-Za-z0-9\s]*$/; // Regular expression to match English characters, numbers, and spaces
    const inputChar = String.fromCharCode(event.keyCode);
    if (!pattern.test(inputChar)) {
      event.preventDefault(); // Prevent the keypress event if the character is not English or a number
      return false;
    }

    return true;
  }

  validateArabicChars(event: KeyboardEvent): boolean {
    const pattern = /^[\u0600-\u06FF0-9\s]*$/; // Regular expression to match Arabic characters, numbers, and spaces
    const inputChar = String.fromCharCode(event.keyCode);

    if (!pattern.test(inputChar)) {
      event.preventDefault(); // Prevent the keypress event if the character is not Arabic or a number
      return false;
    }
    return true;
  }

  validateNumericInput(event: KeyboardEvent): boolean {
    const keyCode = event.keyCode || event.which;
    const charCode = String.fromCharCode(keyCode);

    if (!/^\d+$/.test(charCode)) {
      event.preventDefault(); // Prevent the keypress event if the character is not a number
      return false;
    }

    return true;
  }

  Back() {
    this.location.back()
  }



  abstract initForm(): void;
  abstract onAdd(): void;
  abstract onEdit(): void;
  abstract onSave(response: any): void;
  abstract postSubscribtion(entity?: K): void;
  abstract returnGetModelByIdFn(): Observable<K>;
  abstract returnAddFn(): Observable<any>;
  abstract returnEditFn(): Observable<boolean>;
}
