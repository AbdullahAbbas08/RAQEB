import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { BrowserStorageService } from '../services/browser-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(private authSvr: AuthService, private router: Router, private browser: BrowserStorageService) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ):
    | Observable<boolean | UrlTree>
    | Promise<boolean | UrlTree>
    | boolean
    | UrlTree {
    if (this.authSvr.isAuthenticated$.getValue() &&
      this.authSvr.userPermissions$.getValue() &&
      this.authSvr.userPermissions$.getValue().length > 0) {
      return true;
    } else {
      this.router.navigate(['/auth/login']);
      return false;
    }
  }

}
