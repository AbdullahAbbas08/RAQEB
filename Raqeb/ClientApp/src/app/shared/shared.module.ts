import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgbModule } from "@ng-bootstrap/ng-bootstrap";
import { PrintErrorModule } from "../shared/components/print-error/print-error.module";
// Components
import { BreadcrumbComponent } from "./components/breadcrumb/breadcrumb.component";
import { FeatherIconsComponent } from "./components/feather-icons/feather-icons.component";
import { FooterComponent } from "./components/footer/footer.component";
import { HeaderComponent } from "./components/header/header.component";
import { ContentComponent } from "./components/layout/content/content.component";
import { CustomizerComponent } from "./components/customizer/customizer.component";
import { FullComponent } from "./components/layout/full/full.component";
import { LoaderComponent } from "./components/loader/loader.component";
import { SidebarComponent } from "./components/sidebar/sidebar.component";
import { TapToTopComponent } from "./components/tap-to-top/tap-to-top.component";
// Header Elements Components
import { SearchComponent } from "./components/header/elements/search/search.component";
import { LanguagesComponent } from "./components/header/elements/languages/languages.component";
import { NotificationComponent } from "./components/header/elements/notification/notification.component";
import { BookmarkComponent } from "./components/header/elements/bookmark/bookmark.component";
import { CartComponent } from "./components/header/elements/cart/cart.component";
import { MessageBoxComponent } from "./components/header/elements/message-box/message-box.component";
import { MyAccountComponent } from "./components/header/elements/my-account/my-account.component";

// Services
import { LayoutService } from "./services/layout.service";
import { NavService } from "./services/nav.service";
import { DecimalPipe } from "@angular/common";
import { SvgIconComponent } from "./components/svg-icon/svg-icon.component";
import { SwiperComponent } from "./components/header/elements/swiper/swiper.component";
// @ts-ignore: swiper's package.json "exports" prevents TypeScript from resolving its .d.ts; ignore this import for now
import { SwiperModule } from "swiper/angular";

//primeng
import { ToastModule } from 'primeng/toast';
import { DropdownModule } from 'primeng/dropdown';
import { RadioButtonModule } from 'primeng/radiobutton';
import { CalendarModule } from 'primeng/calendar';
import { MultiSelectModule } from 'primeng/multiselect';
import { InputTextModule } from 'primeng/inputtext';
import { PaginatorModule } from 'primeng/paginator';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TranslateModule } from "@ngx-translate/core";
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { SelectButtonModule } from 'primeng/selectbutton';
import { DialogModule } from 'primeng/dialog';
import { FieldsetModule } from 'primeng/fieldset';
import { PasswordModule } from 'primeng/password';
import { FileUploadModule } from 'primeng/fileupload';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageModule } from 'primeng/message';
import { UploadComponent } from "./components/upload/upload.component";

const importss = [
  DropdownModule,
  RadioButtonModule,
  CalendarModule,
  MultiSelectModule,
  InputTextModule,
  PaginatorModule,
  InputSwitchModule,
  TranslateModule,
  TooltipModule,
  TableModule,
  SelectButtonModule,
  DialogModule,
  FieldsetModule,
  PasswordModule,
  FileUploadModule,
  CardModule,
  ButtonModule,
  ProgressSpinnerModule,
  MessageModule,
  ToastModule
];


@NgModule({
  declarations: [
    HeaderComponent,
    FooterComponent,
    SidebarComponent,
    ContentComponent,
    BreadcrumbComponent,
    FeatherIconsComponent,
    FullComponent,
    LoaderComponent,
    TapToTopComponent,
    SearchComponent,
    LanguagesComponent,
    NotificationComponent,
    BookmarkComponent,
    CartComponent,
    MessageBoxComponent,
    MyAccountComponent,
    SvgIconComponent,
    SwiperComponent,
    CustomizerComponent,
    UploadComponent
  ],
  imports: [CommonModule,
    RouterModule,
    ReactiveFormsModule,
    FormsModule,
    NgbModule,
    SwiperModule,
    PrintErrorModule,
    ...importss
  ],
  providers: [NavService, LayoutService, DecimalPipe],
  exports: [NgbModule,
    ReactiveFormsModule,
    FormsModule,
    LoaderComponent,
    BreadcrumbComponent,
    FeatherIconsComponent,
    TapToTopComponent,
    SvgIconComponent,
    UploadComponent,
    SwiperModule,
    PrintErrorModule,
    ...importss],
})
export class SharedModule { }
