import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { TranslateService } from '@ngx-translate/core';
import { BrowserStorageService } from './browser-storage.service';


@Injectable({
  providedIn: 'root',
})
export class AppInitService {
  lang = 'en';
  localization: any = ''
  constructor(
    private translate: TranslateService,
    private authSvr: AuthService,
    private browser: BrowserStorageService
  ) {

    if (!this.browser.getLang()) {
      this.lang = 'en';
      this.browser.setLang(this.lang);
    } else {
      this.lang = this.browser.getLang();
    }
    if (this.browser.getLocalization() == null) {
      this.browser.setLocalization(this.lang)
    }
  }

  Init() {
    return new Promise((resolve, reject) => {
      this.authSvr.initAuth();
      const localizationData = JSON.parse(JSON.parse(this.browser.getLocalization()));
      this.translate.setTranslation(this.lang, localizationData);
      this.translate.use(this.lang);
      resolve(true);
    });
  }
}
