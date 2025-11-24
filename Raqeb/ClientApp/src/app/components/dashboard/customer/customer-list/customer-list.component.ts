import { Component } from '@angular/core';
import { CustomerDTO, SwaggerClient, ViewerPaginationOfCustomerDTO } from '../../../../shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { DefaultListComponent } from '../../../../shared/helpers/default-list.component';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss',
})
export class CustomerListComponent extends DefaultListComponent<ViewerPaginationOfCustomerDTO, CustomerDTO> {
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
    super(translate,router, toast, auth);
    this.direction = localStorage.getItem('direction')
  }

  returnDataFn(): Observable<ViewerPaginationOfCustomerDTO> {
    return this.swagger.apiCustomerGetWithPaginateGet(this.searchTermControl.value, this.page, this.pageSize);
  }

  returnDeleteFn(id: number): Observable<any> {
    return this.swagger.apiCustomerDeletePost(id);
  }

  clearSearch() {
    this.searchTermControl.setValue('');
  }

}
