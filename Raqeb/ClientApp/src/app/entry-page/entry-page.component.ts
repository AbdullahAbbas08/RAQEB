import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-entry-page',
  templateUrl: './entry-page.component.html',
  styleUrls: ['./entry-page.component.scss']
})
export class EntryPageComponent {
  showEclSimpleOptions = false;
  showNormalEclOptions = false;
  showNormalEclSubOptions: { [key: string]: boolean } = {
    lgd: false,
    pd: false,
    ecl: false
  };

  constructor(private router: Router) {}

  toggleEclSimple(): void {
    this.showEclSimpleOptions = !this.showEclSimpleOptions;
    this.showNormalEclOptions = false;
    this.resetNormalEclSubOptions();
  }

  toggleNormalEcl(): void {
    this.showNormalEclOptions = !this.showNormalEclOptions;
    this.showEclSimpleOptions = false;
    this.resetNormalEclSubOptions();
  }

  toggleNormalEclSubOption(option: string): void {
    // Reset all sub-options
    Object.keys(this.showNormalEclSubOptions).forEach(key => {
      this.showNormalEclSubOptions[key] = false;
    });
    // Toggle the selected option
    this.showNormalEclSubOptions[option] = true;
  }

  resetNormalEclSubOptions(): void {
    Object.keys(this.showNormalEclSubOptions).forEach(key => {
      this.showNormalEclSubOptions[key] = false;
    });
  }

  // Navigation methods for ECL Simple
  navigateToEclSimpleUpload(): void {
    this.router.navigate(['/ecl-simplify/form']);
  }

  navigateToEclSimpleDisplay(): void {
    this.router.navigate(['/ecl-simplify/list']);
  }

  // Navigation methods for Normal ECL - LGD
  navigateToLgdUpload(): void {
     this.router.navigate(['/LGD/form']);
  }

  navigateToLgdDisplay(): void {
    this.router.navigate(['/LGD/list']);
  }

  // Navigation methods for Normal ECL - PD
  navigateToPdUpload(): void {
    this.router.navigate(['/PD/form']);
  }

  navigateToPdDisplay(): void {
     this.router.navigate(['/PD/marginal-pd']);
  }

  // Navigation methods for Normal ECL - ECL
  navigateToEclUpload(): void {
    this.router.navigate(['/ecl/form']);
  }

  navigateToEclDisplay(): void {
    this.router.navigate(['/ecl/list']);
  }
}