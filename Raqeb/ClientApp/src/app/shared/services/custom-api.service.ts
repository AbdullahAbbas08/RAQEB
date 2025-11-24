import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CustomApiService {
  baseUrl: any = window.location.origin;
  constructor(private http: HttpClient) {
  }

  CloseActionTracking(data: FormData): Observable<any> {
    return this.http.post(
      this.baseUrl + `/api/ActionTracking/CloseActionTrackingAsync`,
      data,
    );
  }

  CustomerSaveData(data: FormData): Observable<any> {
    return this.http.post(
      this.baseUrl + `/api/Customer/SaveData`,
      data,
    );
  }

  UserSaveData(data: FormData): Observable<any> {
    return this.http.post(
      this.baseUrl + `/api/User/SaveData`,
      data,
    );
  }

  LangaugeSaveData(data: FormData): Observable<any> {
    return this.http.post(
      this.baseUrl + `/api/Language/SaveData`,
      data,
    );
  }



}


export enum CustomFinalStrategy {
  Low = 'Low Risk Supervisor',
  Medium = 'Medium Risk Control strategies',
  // AboveMedium = 'Above Medium Risk Control strategies',
  High = 'High Risk Intervention',
}