import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS, HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SharedModule } from "./shared/shared.module";
import { AppRoutingModule } from './app-routing.module';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

// // for HttpClient import:
import { LoadingBarHttpClientModule } from '@ngx-loading-bar/http-client';
// // for Router import:
import { LoadingBarRouterModule } from '@ngx-loading-bar/router';
// // for Core import:
import { LoadingBarModule } from '@ngx-loading-bar/core';

import { CookieService } from 'ngx-cookie-service';

import { AppComponent } from './app.component';

import { OverlayModule } from '@angular/cdk/overlay';
import { SwaggerClient } from './shared/services/Swagger/SwaggerClient.service';
import { AppInitService } from './shared/services/app-init.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { JwtInterceptor } from './shared/interceptors/jwt.interceptor';
import { TranslateModule } from '@ngx-translate/core';
import { ConfirmDialogModule } from 'primeng/confirmdialog';

export function initializeApp(appInitService: AppInitService) {
  return async (): Promise<any> => {
    return await appInitService.Init();
  };
}

// PrintErrorComponent

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    TranslateModule.forRoot(),
    OverlayModule,
    SharedModule,
    AppRoutingModule,
    HttpClientModule,
    NgbModule,
    //     // for HttpClient use:
    LoadingBarHttpClientModule,
    //     // for Router use:
    LoadingBarRouterModule,
    //     // for Core use:
    LoadingBarModule,
    ToastModule,
    ConfirmDialogModule
  ],
  providers: [CookieService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppInitService],
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true,
    },
    SwaggerClient,
    ConfirmationService,
    MessageService,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
