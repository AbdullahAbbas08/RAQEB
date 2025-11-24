import { HttpErrorResponse } from "@angular/common/http";
import { AfterViewInit, Component, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { MessageService } from "primeng/api";
import { Subject, takeUntil } from "rxjs";
import { LanguageDtoForHeader, SwaggerClient } from "../../shared/services/Swagger/SwaggerClient.service";
import { AuthService } from "../../shared/services/auth.service";
import { BrowserStorageService } from '../../shared/services/browser-storage.service';
import { EncryptDecryptService } from "../../shared/services/encrypt-decrypt.service";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";


@Component({
  selector: "app-login",
  templateUrl: "./login.component.html",
  styleUrls: ["./login.component.scss"],
})
export class LoginComponent implements OnInit, OnDestroy, AfterViewInit {
  // Form Groups
  public loginForm!: FormGroup;

  // UI States
  public isLoading: boolean = false;

  // Data
  public languages: LanguageDtoForHeader[] = [];
  public selectedLanguage?: LanguageDtoForHeader;
  public currentLang: string = 'en';
  private destroy$ = new Subject<void>();

  constructor(
    private formBuilder: FormBuilder,
    private router: Router,
    private authService: AuthService,
    private encryptService: EncryptDecryptService,
    private swaggerClient: SwaggerClient,
    private browserService: BrowserStorageService,
    private messageService: MessageService
  ) {
    // Initialize form in constructor

  }

  ngOnInit(): void {
    this.initLoginForm()
    this.setupFormDebug();
    this.loadLanguages();
  }

  ngAfterViewInit() {
  this.loginForm.valueChanges.subscribe(values => {
  });
}

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupFormDebug(): void {
    // Log form state for debugging
    this.loginForm.statusChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(status => {
      
      });
  }
 initLoginForm(){
      this.loginForm = this.formBuilder.group({
      email: ['', Validators.required],
      password: ['', Validators.required]
    });
 }
  private loadLanguages(): void {
    this.swaggerClient.apiLookupGetLanguagePost()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (languages: LanguageDtoForHeader[]) => {
          this.languages = languages;
          localStorage.setItem('languages', JSON.stringify(languages));
          this.selectedLanguage = this.languages.find(lang => lang.code === this.currentLang);
        },
        error: (error: unknown) => {
          console.error('Error loading languages:', error);
          this.showErrorMessage('ErrorLoadingLanguages');
        }
      });
  }

  public onLanguageChange(language: LanguageDtoForHeader): void {
    if (!language) return;
    this.browserService.changeLanguage(language);
  }

  private showErrorMessage(messageKey: string): void {
    this.messageService.add({
      severity: 'error',
      detail: messageKey
    });
  }

  private handleLoginSuccess(response: string): void {
    try {
      const decryptedResponse = this.decryptResponse(response);
      const parsedResponse = JSON.parse(decryptedResponse) ;
console.log(parsedResponse);

      if (this.isValidLoginResponse(parsedResponse)) {
        this.authService.login(
          parsedResponse.Token, 
          parsedResponse.refreshToken, 
          true
        );
      } else {
        this.handleLoginError(parsedResponse.Message);
      }
    } catch (error) {
      console.error('Error processing login response:', error);
      this.showErrorMessage('ErrorProcessingResponse');
    } finally {
      this.isLoading = false;
    }
  }

  private decryptResponse(response: string): string {
    return this.encryptService.decryptUsingAES256(
      this.encryptService.unshiftString(response, 6)
    );
  }

  private isValidLoginResponse(response: any): boolean {
    return !!(response.Token && response.refreshToken);
  }

  private handleLoginError(errorType?: string): void {
    const errorMessages: Record<string, string> = {
      'InvalidUsernameOrPassword': 'InvalidCredentials',
      'LockAccount': 'AccountLocked',
      'NoPermissions': 'InsufficientPermissions'
    };

    this.showErrorMessage(errorMessages[errorType || ''] || 'UnknownError');
  }

  public onSubmit(): void {
    if (!this.loginForm) {
      console.error('Form not initialized');
      return;
    }

    this.isLoading = true;
    const loginData: any = {
      email: this.loginForm.get('email')?.value || '',
      password: this.loginForm.get('password')?.value || ''
    };

    this.swaggerClient.apiUserLoginPost(loginData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          this.handleLoginSuccess(response);
        },
        error: (error: HttpErrorResponse) => {
          console.error('Login error:', error);
          this.showErrorMessage('LoginFailed');
          this.isLoading = false;
        }
      });
  }

  // Form Getters
  get emailControl() { return this.loginForm.get('email'); }
  get passwordControl() { return this.loginForm.get('password'); }
  get isFormValid() { return this.loginForm && this.loginForm.valid; }

}
