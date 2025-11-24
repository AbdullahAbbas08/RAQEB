import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserFormComponent } from './user-form/user-form.component';
import { UserListComponent } from './user-list/user-list.component';
import { ProfileImageComponent } from './profile-image/profile-image.component';

const routes: Routes = [
  {
    path: '',
    children: [
      {
        path: '',
        component: UserListComponent
      },
      {
        path: 'create',
        component: UserFormComponent
      },
      {
        path: 'edit/:id',
        component: UserFormComponent
      },
      {
        path: 'profile-data/:id',
        component: ProfileImageComponent
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserRoutingModule { }
