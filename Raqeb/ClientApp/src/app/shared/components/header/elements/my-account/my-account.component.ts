import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "src/app/shared/services/auth.service";
import { SwaggerClient } from "../../../../services/Swagger/SwaggerClient.service";
import { EncryptDecryptService } from "../../../../services/encrypt-decrypt.service";

@Component({
  selector: "app-my-account",
  templateUrl: "./my-account.component.html",
  styleUrls: ["./my-account.component.scss"],
})
export class MyAccountComponent implements OnInit {
  public profileImg: "assets/images/dashboard/profile.jpg";
  UserName: any = ''
  profile: any = ''
  userId: any
  constructor(public router: Router,
    private auth: AuthService,
    private Encryption: EncryptDecryptService,
    private swagger: SwaggerClient) {
    this.userId = this.auth.User$.getValue()?.ID
  }

  ngOnInit() {
    this.UserName = this.auth.userFullName
    this.getUserImage()
  }

  EncryptId(id: any): any {
    let val = this.Encryption.encryptUsingAES256(id?.toString());
    let NewVal = val.replace(/\//g, "__")
    return NewVal;
  }

  logoutFunc() {
    this.auth.logout()
  }

  getUserImage() {
    this.swagger.apiUserGetUserImagePost(this.auth.User$.getValue()?.ID).subscribe(
      res => {
        this.profile = res
      }
    )
  }
}
