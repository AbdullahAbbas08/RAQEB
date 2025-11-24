import { ProfileImageComponent } from './profile-image/profile-image.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserFormComponent } from './user-form/user-form.component';
import { UserListComponent } from './user-list/user-list.component';

import { UserRoutingModule } from './user-routing.module';
import { SharedModule } from '../../../shared/shared.module';
import { NgxDropzoneModule } from 'ngx-dropzone';

@NgModule({
  imports: [
    CommonModule,
    UserRoutingModule,
    SharedModule,
    NgxDropzoneModule
  ],
  declarations: [UserFormComponent, UserListComponent,ProfileImageComponent]
})
export class UserModule { }
