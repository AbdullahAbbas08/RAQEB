import { CustomerDTO, SwaggerClient, UserDto } from './../../../../shared/services/Swagger/SwaggerClient.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { DefaultFormComponent } from '../../../../shared/helpers/default-form.component';
import { CustomApiService } from '../../../../shared/services/custom-api.service';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-customer-form',
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss'
})
export class CustomerFormComponent extends DefaultFormComponent<CustomerDTO> {
  url: any = this.route.snapshot.paramMap.get('id')
    ? this.auth.decryptUsingAES256(this.route.snapshot.paramMap.get('id')?.replace(/__/g, "/")) : 0

  Users: UserDto[] = []
  direction: any = 'rtl'

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
  }


  initForm(): void {
    this.form = this.fb.group({
      iD: [this.route.snapshot.paramMap.get('id') ? this.url : 0, [Validators.required]],
      nameAr: ['', Validators.required],
      nameEn: ['', Validators.required],
      phone: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      logoForm: ['', Validators.nullValidator],
      maxNumberOfSites: ['', Validators.required],
      logoBase64: ['', Validators.nullValidator],
    });
    // this.files = this.form.get('logoForm').value
  }

  files: any; // Ensure that files property is declared

  onSelect(event) {
    const files = event;
    if (files && files.length > 0) {
      const reader = new FileReader();
      const file = files[0];
      reader.onload = () => {
        this.form.get('logoForm').patchValue(reader.result);
        this.form.get('logoBase64').patchValue(reader.result);

      };
      reader.readAsDataURL(file);
    }
    this.getImageSrc()
  }


  getImageSrc() {
    const logoFormValue = this.form.get('iD').value == 0 ? this.form.get('logoForm').value : this.form.get('logoBase64').value;
    return logoFormValue ? logoFormValue : 'assets/images/dashboard/avatarperson.png';
  }

  RemoveIamge() {
    if (this.form.get('iD').value == 0) {
      this.form.get('logoForm').patchValue('')
    } else {
      this.form.get('logoBase64').patchValue('')
    }

    this.getImageSrc()
  }


  onRemove(event) {
    // this.files.splice(this.files.indexOf(event), 1);
    this.files = null
  }


  returnGetModelByIdFn(): Observable<any> {
    return this.swagger.apiCustomerGetByIdGet(this.url)
  }

  returnAddFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('nameAr', this.form.get('nameAr').value);
    formData.append('nameEn', this.form.get('nameEn').value);
    formData.append('phone', this.form.get('phone').value);
    formData.append('email', this.form.get('email').value);
    formData.append('maxNumberOfSites', this.form.get('maxNumberOfSites').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    formData.append('logoBase64', this.form.get('logoBase64').value);
    return this.CustomApiService.CustomerSaveData(formData);
  }

  returnEditFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('nameAr', this.form.get('nameAr').value);
    formData.append('nameEn', this.form.get('nameEn').value);
    formData.append('phone', this.form.get('phone').value);
    formData.append('email', this.form.get('email').value);
    formData.append('maxNumberOfSites', this.form.get('maxNumberOfSites').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    formData.append('logoBase64', this.form.get('logoBase64').value);
    return this.CustomApiService.CustomerSaveData(formData);
  }

  onAdd(): void { }
  onEdit(): void { }
  onSave(response: any): void {
    if (response) {
      this.router.navigateByUrl('/customer')
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


}