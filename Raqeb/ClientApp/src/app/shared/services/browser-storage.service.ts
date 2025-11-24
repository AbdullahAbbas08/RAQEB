import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import * as CryptoJS from 'crypto-js';
import { EncryptDecryptService } from './encrypt-decrypt.service';
import { JwtHelperService } from '@auth0/angular-jwt';
import { SwaggerClient } from './Swagger/SwaggerClient.service';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class BrowserStorageService {

  helper = new JwtHelperService();
  private encryptKey = CryptoJS.enc.Utf8.parse('4512631236589784');

  constructor(private route: Router, private auth: EncryptDecryptService, private swagger: SwaggerClient) { }
  private getLocalStorage(key: string): string | null {
    return localStorage.getItem(key);
  }




  private setLocalStorage(key: string, value: string) {
    localStorage.setItem(key, value);
  }

  private getSessionStorage(key: string): string | null {
    return sessionStorage.getItem(key);
  }

  private setSessionStorage(key: string, value: string) {
    sessionStorage.setItem(key, value);
  }

  getToken(): string | null {
    return this.getLocalStorage('NBPC5W930aW12EZb2mICjhacFgbCBs');
  }



  setToken(value: string, rememberMe: boolean) {
    rememberMe
      ? this.setLocalStorage('NBPC5W930aW12EZb2mICjhacFgbCBs', value)
      : this.setSessionStorage('NBPC5W930aW12EZb2mICjhacFgbCBs', value);
  }


  getRefreshToken(): string | null {
    return (
      this.getSessionStorage('refreshToken') ||
      this.getLocalStorage('refreshToken')
    );
  }

  setRefreshToken(value: string, rememberMe: boolean) {
    rememberMe
      ? this.setLocalStorage('refreshToken', value)
      : this.setSessionStorage('refreshToken', value);
  }

  getLang(): string | null {
    return this.getLocalStorage('lang');
  }

  setLang(value: string) {
    this.setLocalStorage('lang', value);
  }

  setDir(value: string) {
    this.setLocalStorage('direction', value);
  }

  getDir() {
    this.getLocalStorage('direction');
  }

  getLocalization(): string {
    return localStorage.getItem('localization');
  }

  setLocalization(lang: any): void {    
    this.swagger.apiLocalizationJsonGet(lang).subscribe(
      (res) => {
        localStorage.removeItem('localization')
        localStorage.setItem('localization', JSON.stringify(res));
        setTimeout(() => {
          window.location.reload()
        }, 1000);
      },
      (err: HttpErrorResponse) => {
      }
    );
  }

  changeLanguage(lang) {
    this.setLocalization(lang['code']);
    // this.selectedLanguage = lang;
    this.setDir(lang['direction']);
    this.setLang(lang['code']);
    document.getElementsByTagName("html")[0].setAttribute("dir", lang['direction']);
    document.body.className = lang['direction'];
    // this.selectedLanguage = this.languages.find(x => x.code == this.lang)
  }



  removeToken() {
    localStorage.removeItem('NBPC5W930aW12EZb2mICjhacFgbCBs');
    sessionStorage.removeItem('NBPC5W930aW12EZb2mICjhacFgbCBs');
  }


  decrypteString(string: any) {
    if (!string) return;
    var decrypted = CryptoJS.AES.decrypt(string, this.encryptKey, {
      keySize: 128 / 8,
      iv: this.encryptKey,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });

    try {
      return decrypted.toString(CryptoJS.enc.Utf8);
    } catch (err) {
      return false;
    }
  }


  encrypteString(string = null) {
    if (!string) return
    var encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(string), this.encryptKey,
      {
        keySize: 128 / 8,
        iv: this.encryptKey,
        mode: CryptoJS.mode.CBC,
        padding: CryptoJS.pad.Pkcs7
      });

    return encrypted.toString();
  }
}
