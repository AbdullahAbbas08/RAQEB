import { CustomerDTO, LanguageCrudDto, LocalizationCrudDto, NameIdForDropDown } from './../../../../shared/services/Swagger/SwaggerClient.service';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { Observable } from 'rxjs';
import { SwaggerClient, UserDto } from 'src/app/shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { EncryptDecryptService } from 'src/app/shared/services/encrypt-decrypt.service';
import { DefaultFormComponent } from '../../../../shared/helpers/default-form.component';
import { CustomApiService } from '../../../../shared/services/custom-api.service';

@Component({
  selector: 'app-localization-form',
  templateUrl: './localization-form.component.html',
  styleUrl: './localization-form.component.scss'
})
export class LocalizationFormComponent extends DefaultFormComponent<LocalizationCrudDto> {
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
  }


  initForm(): void {
    this.form = this.fb.group({
      iD: [this.route.snapshot.paramMap.get('id') ? this.url : 0, [Validators.required]],
      code: ['', Validators.required],
    });
  }


  returnGetModelByIdFn(): Observable<any> {
    return this.swagger.apiLocalizationGetByIdGet(this.url)
  }

  returnAddFn(): Observable<any> {
    return this.swagger.apiLocalizationSaveDataPost(this.form.value);
  }

  returnEditFn(): Observable<any> {
    return this.swagger.apiLocalizationSaveDataPost(this.form.value);
  }

  onAdd(): void { }
  onEdit(): void { }
  onSave(response: any): void {
    if (response) {
      this.router.navigateByUrl('/localization')
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