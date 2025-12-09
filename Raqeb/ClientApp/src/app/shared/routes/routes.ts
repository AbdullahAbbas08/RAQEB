import { Routes } from "@angular/router";

export const content: Routes = [
  {
    path: "main",
    loadChildren: () => import("../../dashboard-component/dashboard.module").then((m) => m.DashboardModule),
  },
  {
    path: "auth",
    loadChildren: () => import("../../auth/auth.module").then((m) => m.AuthModule),
  },
  {
    path: "LGD",
    loadChildren: () => import("../../components/simple-page/simple-page.module").then((m) => m.SimplePageModule),
  },
  {
    path: "PD",
    loadChildren: () => import("../../components/pd/pd.module").then((m) => m.PDModule),
  },
  {
    path: "ecl",
    loadChildren: () => import("../../components/ecl/ecl.module").then((m) => m.EclModule),
  },
  {
    path: "ecl-simplify",
    loadChildren: () => import("../../ecl-simplify/ecl-simplify.module").then((m) => m.EclSimplifyModule),
  },
  {
    path: "entry-page",
    loadChildren: () => import("../../entry-page/entry-page.module").then((m) => m.EntryPageModule),
  },
  {
    path: "customer",
    loadChildren: () => import("../../components/dashboard/customer/customer.module").then((m) => m.CustomerModule),
  },
  {
    path: "user",
    loadChildren: () => import("../../components/dashboard/user/user.module").then((m) => m.UserModule),
  },
  {
    path: "language",
    loadChildren: () => import("../../components/dashboard/language/language.module").then((m) => m.LanguageModule),
  },
  {
    path: "localization",
    loadChildren: () => import("../../components/dashboard/localization/localization.module").then((m) => m.LocalizationModule),
  },
  {
    path: "localization-language",
    loadChildren: () => import("../../components/dashboard/localization-language/localization-lang.module").then((m) => m.LocalizationLangModule),
  },
];
