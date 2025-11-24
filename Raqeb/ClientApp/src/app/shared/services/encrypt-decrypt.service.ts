import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment.prod';
import * as CryptoJS from 'crypto-js';

@Injectable({
  providedIn: 'root'
})
export class EncryptDecryptService {

  private key = CryptoJS.enc.Utf8.parse(environment.EncryptKey);
  private iv = CryptoJS.enc.Utf8.parse(environment.EncryptIV);

  constructor() { }

  encryptUsingAES256(text: any): any {
    var encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(text), this.key, {
      keySize: 128 / 8,
      iv: this.iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });
    return encrypted.toString();
  }

  decryptUsingAES256(decString: any) {
    var decrypted = CryptoJS.AES.decrypt(decString, this.key, {
      keySize: 128 / 8,
      iv: this.iv,
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7
    });
    return decrypted.toString(CryptoJS.enc.Utf8);
  }

  // EncryptId(id: any): any {
  //   return this.encryptUsingAES256(id?.toString());
  // }

  EncryptId(id: any): any {
    let val = this.encryptUsingAES256(id?.toString());
    let NewVal = val.replace(/\//g, "__")
    return NewVal;
  }


  unshiftString(input: string, shift: number): string {
    let unshiftedString = '';
    for (let i = 0; i < input.length; i++) {
      const c = input[i];
      const unshiftedChar = String.fromCharCode(c.charCodeAt(0) - shift);
      unshiftedString += unshiftedChar;
    }
    return unshiftedString;
  }





}
