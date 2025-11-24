import { Component } from '@angular/core';
import { CustomerDTO, SwaggerClient, UserDTO, ViewerPaginationOfCustomerDTO, ViewerPaginationOfUserDTO } from '../../../../shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { DefaultListComponent } from '../../../../shared/helpers/default-list.component';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent extends DefaultListComponent<ViewerPaginationOfUserDTO, UserDTO> {
  breadcrumb: MenuItem[];
  direction: any = 'rtl'
  constructor(
    translate: TranslateService,
    router: Router,
    private route: ActivatedRoute,
    toast: MessageService,
    auth: EncryptDecryptService,
    private swagger: SwaggerClient,
    // confirmationService: ConfirmationService
  ) {
    super(translate, router, toast, auth);
    this.direction = localStorage.getItem('direction')
  }

  returnDataFn(): Observable<ViewerPaginationOfUserDTO> {
    return this.swagger.apiUserGetWithPaginatePost(this.searchTermControl.value, this.page, this.pageSize);
  }

  returnDeleteFn(id: number): Observable<any> {
    return this.swagger.apiUserDeletePost(id);
  }
  clearSearch() {
    this.searchTermControl.setValue('');
  }
}

