import { CustomerDTO, NameIdForDropDown, SwaggerClient, UserDto } from './../../../../shared/services/Swagger/SwaggerClient.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { DefaultFormComponent } from '../../../../shared/helpers/default-form.component';
import { CustomApiService } from '../../../../shared/services/custom-api.service';
import { HttpEventType, HttpRequest } from '@angular/common/http';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-user-form',
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent extends DefaultFormComponent<CustomerDTO> {
  url: any = this.route.snapshot.paramMap.get('id')
    ? this.auth.decryptUsingAES256(this.route.snapshot.paramMap.get('id')?.replace(/__/g, "/")) : 0

  Users: UserDto[] = []
  direction: any = 'rtl'
  Customers: NameIdForDropDown[] = []
  constructor(
    route: ActivatedRoute,
    fb: FormBuilder,
    router: Router,
    toastr: MessageService,
    location: Location,
    toast: MessageService,
    private CustomApiService: CustomApiService,
    private swagger: SwaggerClient,
    private auth: EncryptDecryptService,
  ) {
    super(route, fb, router, toastr, location);
    this.direction = localStorage.getItem('direction')
    // forkJoin(this.swagger.apiLookupGetUsersGet(),
    //   this.swagger.apiLookupGetSegmentsGet(),
    //   this.swagger.apiLookupGetCountriesGet()).subscribe(res => {
    //   })
    this.getCustomers()
  }


  initForm(): void {
    this.form = this.fb.group({
      iD: [this.route.snapshot.paramMap.get('id') ? this.url : 0, [Validators.required]],
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      mobile: ['', Validators.required],
      password: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      customerId: ['', Validators.required],
      logoForm: ['', Validators.nullValidator],
      image: ['', Validators.nullValidator],
    });
    // this.files = this.form.get('logoForm').value
  }

  getCustomers() {
    this.swagger.apiCustomerGetAllCustomerForDropdownGet().subscribe(
      res => {
        this.Customers = res
      }
    )
  }

  // files: File = null;
  // onSelect(event) {
  //   this.files = event.currentFiles[0];
  //   const reader = new FileReader();
  //   reader.onload = () => {
  //     this.form.get('logoForm').patchValue(event.currentFiles[0]);
  //   };
  //   reader.readAsDataURL(event.currentFiles[0]);
  // }

  files: any; // Ensure that files property is declared

  onSelect(event) {
    const files = event;
    if (files && files.length > 0) {
      const reader = new FileReader();
      const file = files[0];
      reader.onload = () => {
        this.form.get('logoForm').patchValue(reader.result);
        this.form.get('image').patchValue(reader.result);

      };
      reader.readAsDataURL(file);
    }
    this.getImageSrc()
  }


  getImageSrc() {
    const logoFormValue = this.form.get('iD').value == 0 ? this.form.get('logoForm').value : this.form.get('image').value;
    return logoFormValue ? logoFormValue : 'assets/images/dashboard/avatarperson.png';
  }

  RemoveIamge() {
    if (this.form.get('iD').value == 0) {
      this.form.get('logoForm').patchValue('')
    } else {
      this.form.get('image').patchValue('')
    }

    this.getImageSrc()
  }

  onRemove(event) {
    // this.files.splice(this.files.indexOf(event), 1);
    this.files = null
  }


  returnGetModelByIdFn(): Observable<any> {
    return this.swagger.apiUserGetByIdGet(this.url)
  }

  returnAddFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('nameAr', this.form.get('nameAr').value);
    formData.append('nameEn', this.form.get('nameEn').value);
    formData.append('mobile', this.form.get('mobile').value);
    formData.append('email', this.form.get('email').value);
    formData.append('password', this.form.get('password').value);
    formData.append('customerId', this.form.get('customerId').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    formData.append('image', this.form.get('image').value);
    return this.CustomApiService.UserSaveData(formData);
  }

  returnEditFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('nameAr', this.form.get('nameAr').value);
    formData.append('nameEn', this.form.get('nameEn').value);
    formData.append('mobile', this.form.get('mobile').value);
    formData.append('email', this.form.get('email').value);
    formData.append('password', this.form.get('password').value);
    formData.append('customerId', this.form.get('customerId').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    formData.append('image', this.form.get('image').value);
    return this.CustomApiService.UserSaveData(formData);
  }

  onAdd(): void { }
  onEdit(): void { }
  onSave(response: any): void {
    if (response) {
      this.router.navigateByUrl('/user')
    } else {
      !response &&
        this.toast.add({
          severity: 'error',
          detail: 'KeyExist',
        });
    }

  }
  postSubscribtion(entity: any): void {
    this.form.patchValue(entity[0])
  }

  uploadedFiles: any[] = [];

  onUpload(event) {
    for (let file of event.files) {
      this.uploadedFiles.push(file);
    }
  }

}