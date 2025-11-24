import { Component, OnInit } from '@angular/core';
import { NavService } from '../../../../services/nav.service';
import { LanguageDtoForHeader, SwaggerClient } from '../../../../services/Swagger/SwaggerClient.service';
import { BrowserStorageService } from '../../../../services/browser-storage.service';

@Component({
  selector: 'app-languages',
  templateUrl: './languages.component.html',
  styleUrls: ['./languages.component.scss']
})
export class LanguagesComponent implements OnInit {

  public language: boolean = false;

  public languages: LanguageDtoForHeader[] = []
  lang: any = 'en'

  public selectedLanguage: LanguageDtoForHeader

  constructor(public navServices: NavService, private swagger: SwaggerClient, private browser: BrowserStorageService) { }
  // 
  ngOnInit() {
    this.lang = localStorage.getItem('lang')
    this.getLangs()
  }

  changeLanguage(lang: LanguageDtoForHeader) {
    this.browser.changeLanguage(lang)
  }

  getLangs() {
    this.languages = JSON.parse(localStorage.getItem('languages'))
    this.selectedLanguage = this.languages?.find(x => x.code == this.lang)
    // this.swagger.apiLookupGetLanguagePost().subscribe(
    //   res => {
    //     this.languages = res
    //     this.selectedLanguage = this.languages.find(x => x.code == this.lang)
    //   }
    // )
  }
}
