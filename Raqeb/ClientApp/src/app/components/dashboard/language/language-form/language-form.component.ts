import { CustomerDTO, LanguageCrudDto, NameIdForDropDown } from './../../../../shared/services/Swagger/SwaggerClient.service';
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
  selector: 'app-language-form',
  templateUrl: './language-form.component.html',
  styleUrl: './language-form.component.scss'
})
export class LanguageFormComponent extends DefaultFormComponent<LanguageCrudDto> {
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
      name: ['', Validators.required],
      code: ['', Validators.required],
      direction: ['', Validators.required],
      icon: ['', Validators.nullValidator],
      logoForm: ['', Validators.nullValidator],
    });
  }

  files: File = null;

  onSelect(event) {
    const files = event;
    if (files && files.length > 0) {
      const reader = new FileReader();
      const file = files[0];
      reader.onload = () => {
        this.form.get('logoForm').patchValue(reader.result);
        this.form.get('icon').patchValue(reader.result);

      };
      reader.readAsDataURL(file);
    }
    this.getImageSrc()
  }

  getImageSrc() {
    const logoFormValue = this.form.get('iD').value == 0 ? this.form.get('logoForm').value : this.form.get('icon').value;
    return logoFormValue ? logoFormValue : 'assets/images/dashboard/avatarperson.png';
  }

  RemoveIamge() {
    if (this.form.get('iD').value == 0) {
      this.form.get('logoForm').patchValue('')
    } else {
      this.form.get('icon').patchValue('')
    }

    this.getImageSrc()
  }

  

  onRemove(event) {
    // this.files.splice(this.files.indexOf(event), 1);
    this.files = null
  }


  returnGetModelByIdFn(): Observable<any> {
    return this.swagger.apiLanguageGetByIdGet(this.url)
  }

  returnAddFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('name', this.form.get('name').value);
    formData.append('direction', this.form.get('direction').value);
    formData.append('code', this.form.get('code').value);
    formData.append('icon', this.form.get('icon').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    return this.CustomApiService.LangaugeSaveData(formData);
  }

  returnEditFn(): Observable<any> {
    const formData = new FormData();
    formData.append('iD', this.form.get('iD').value);
    formData.append('name', this.form.get('name').value);
    formData.append('direction', this.form.get('direction').value);
    formData.append('code', this.form.get('code').value);
    formData.append('icon', this.form.get('icon').value);
    formData.append('logoForm', this.form.get('logoForm').value);
    return this.CustomApiService.LangaugeSaveData(formData);
  }

  onAdd(): void { }
  onEdit(): void { }
  onSave(response: any): void {
    if (response) {
      this.router.navigateByUrl('/language')
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