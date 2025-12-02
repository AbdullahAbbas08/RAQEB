import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { ContentComponent } from "./shared/components/layout/content/content.component";
import { FullComponent } from "./shared/components/layout/full/full.component";
import { full } from "./shared/routes/full.routes";
import { content } from "./shared/routes/routes";
import { LoginComponent } from "./auth/login/login.component";
import { AuthGuard } from './shared/guards/auth.guard';
import { NoAuthGuard } from './shared/guards/no-auth.guard';

const routes: Routes = [
  {
    canActivate: [NoAuthGuard],
    path: 'auth/login',
    component: LoginComponent
  },
  // {
  //   canActivate: [AuthGuard],
  //   path: "",
  //   redirectTo: "LGD/list",
  //   pathMatch: "full",
  // },
    {
    canActivate: [AuthGuard],
    path: "",
    redirectTo: "main/dashboard",
    pathMatch: "full",
  },
  {
    canActivate: [AuthGuard],
    path: "",
    component: ContentComponent,
    children: content

  },
  {
    path: "",
    component: FullComponent,
    children: full


  },
  {
    path: "**",
    redirectTo: "main/dashboard",
  },
];

@NgModule({
  imports: [
    [
      RouterModule.forRoot(routes, {
        anchorScrolling: "enabled",
        scrollPositionRestoration: "enabled",
      }),
    ],
  ],
  exports: [RouterModule],
})
export class AppRoutingModule { }
