import { Component } from '@angular/core';
import { CustomerDTO, LanguageCrudDto, LanguageLocalizationListDto, LocalizationCrudDto, SwaggerClient, ViewerPaginationOfLanguageLocalizationListDto, ViewerPaginationOfLocalizationCrudDto, ViewerPaginationOfUserDTO } from '../../../../shared/services/Swagger/SwaggerClient.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Observable, forkJoin } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';
import { DefaultListComponent } from '../../../../shared/helpers/default-list.component';
import { EncryptDecryptService } from '../../../../shared/services/encrypt-decrypt.service';

@Component({
  selector: 'app-localization-code-list',
  templateUrl: './localization-code-list.component.html',
  styleUrl: './localization-code-list.component.scss'
})
export class LocalizationCodeListComponent extends DefaultListComponent<ViewerPaginationOfLanguageLocalizationListDto, LanguageLocalizationListDto> {
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

  returnDataFn(): Observable<ViewerPaginationOfLanguageLocalizationListDto> {
    return this.swagger.apiLocalizationGetLocalizationLanguageWithPaginateGet(this.searchTermControl.value, this.page, this.pageSize);
  }

  returnDeleteFn(id: number): Observable<any> {
    return this.swagger.apiLanguageDeletePost(0);
  }
  clearSearch() {
    this.searchTermControl.setValue('');
  }
}

