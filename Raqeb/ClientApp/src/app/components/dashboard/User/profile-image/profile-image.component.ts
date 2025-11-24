import { CustomerDTO, NameIdForDropDown } from './../../../../shared/services/Swagger/SwaggerClient.service';
import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { SwaggerClient, UserDto } from 'src/app/shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Location } from '@angular/common';
import { EncryptDecryptService } from 'src/app/shared/services/encrypt-decrypt.service';
import { DefaultFormComponent } from '../../../../shared/helpers/default-form.component';
import { CustomApiService } from '../../../../shared/services/custom-api.service';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-profile-image',
  templateUrl: './profile-image.component.html',
  styleUrl: './profile-image.component.scss'
})
export class ProfileImageComponent extends DefaultFormComponent<CustomerDTO> {
  url: any = this.route.snapshot.paramMap.get('id')
    ? this.auth.decryptUsingAES256(this.route.snapshot.paramMap.get('id')?.replace(/__/g, "/")) : 0

  Users: UserDto[] = []
  direction: any = 'rtl'
  Customers: NameIdForDropDown[] = []
  disabled: boolean = true;

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
      customerName: ['', Validators.required],
      mobile: ['', Validators.required],
      password: ['', Validators.nullValidator],
      oldPassword: ['', Validators.nullValidator],
      email: ['', [Validators.required, Validators.email]],
      customerId: ['', Validators.required],
      logoForm: ['', Validators.nullValidator],
      logoBase64: ['', Validators.nullValidator],
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
      // this.router.navigateByUrl('/user')
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

  openChangePasswordModal() {
    Swal.mixin({
      title: 'Change Password',
      html:
        '<input id="old-password" class="swal2-input" placeholder="Old Password" type="password">' +
        '<div class="password-input-container">' +
        '<input id="new-password" class="swal2-input" placeholder="New Password" type="password">' +
        '<i id="show-hide-new-password" class="fa fa-eye"></i>' +
        '</div>' +
        '<div class="password-input-container">' +
        '<input id="re-new-password" class="swal2-input" placeholder="Re New Password" type="password">' +
        '<i id="show-hide-re-new-password" class="fa fa-eye"></i>' +
        '</div>',
      focusConfirm: false,
      preConfirm: () => {
        const oldPassword = (<HTMLInputElement>document.getElementById('old-password')).value;
        const newPassword = (<HTMLInputElement>document.getElementById('new-password')).value;
        const reNewPassword = (<HTMLInputElement>document.getElementById('re-new-password')).value;

        if (oldPassword !== this.form.get('password').value) {
          Swal.showValidationMessage('Old password Incorrect');
          return false; // Prevent closing the modal
        }

        if (newPassword !== reNewPassword || newPassword == '') {
          Swal.showValidationMessage('New passwords do not match');
          return false; // Prevent closing the modal
        }


        const formData = new FormData();
        formData.append('iD', this.form.get('iD').value);
        formData.append('oldPassword', oldPassword);
        formData.append('password', newPassword);
        this.CustomApiService.UserSaveData(formData).subscribe(
          res => {
            if (res) {
              Swal.fire({
                icon: 'success',
                title: 'Success',
                text: 'Password changed successfully!',
              });
            } else {
              // Handle other response scenarios if needed
              Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to change the password. Please try again.',
              });
            }
          }
        )
      },
      didOpen: () => {
        // Add event listeners for show/hide password icons
        document.getElementById('show-hide-new-password').addEventListener('click', (ev) => this.togglePasswordVisibility('new-password', ev));
        document.getElementById('show-hide-re-new-password').addEventListener('click', (ev) => this.togglePasswordVisibility('re-new-password', ev));
      },
      willClose: () => {
        // Remove event listeners when modal is about to close
        document.getElementById('show-hide-new-password').removeEventListener('click', (ev) => this.togglePasswordVisibility('new-password', ev));
        document.getElementById('show-hide-re-new-password').removeEventListener('click', (ev) => this.togglePasswordVisibility('re-new-password', ev));
      }
    }).fire();
  }

  togglePasswordVisibility(inputId: string, event: MouseEvent) {
    const input = <HTMLInputElement>document.getElementById(inputId);
    if (input.type === 'password') {
      input.type = 'text';
    } else {
      input.type = 'password';
    }
    // Prevent the click event from being propagated
    event.stopPropagation();
  }





}