import { Component } from '@angular/core';
import { CustomerDTO, LanguageCrudDto, SwaggerClient, UserDTO, ViewerPaginationOfCustomerDTO, ViewerPaginationOfLanguageCrudDto, ViewerPaginationOfUserDTO } from '../../../../shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { DefaultListComponent } from '../../../../shared/helpers/default-list.component';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-language-list',
  templateUrl: './language-list.component.html',
  styleUrl: './language-list.component.scss'
})
export class LanguageListComponent extends DefaultListComponent<ViewerPaginationOfLanguageCrudDto, LanguageCrudDto> {
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

  returnDataFn(): Observable<ViewerPaginationOfLanguageCrudDto> {
    return this.swagger.apiLanguageGetLanguageWithPaginateGet(this.searchTermControl.value, this.page, this.pageSize);
  }

  returnDeleteFn(id: number): Observable<any> {
    return this.swagger.apiLanguageDeletePost(id);
  }
  clearSearch() {
    this.searchTermControl.setValue('');
  }
}

