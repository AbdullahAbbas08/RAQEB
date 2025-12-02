// dashboard.component.ts
import { Component } from '@angular/core';
import { Router } from '@angular/router';

interface Module {
  id: string;
  name: string;
  icon: string;
  gradient: string;
  shadowColor: string;
}

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  selectedModule: string | null = null;

  modules: Module[] = [
    {
      id: 'lgd',
      name: 'LGD',
      icon: 'paid',
      gradient: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      shadowColor: '102, 126, 234'
    },
    {
      id: 'pd',
      name: 'PD',
      icon: 'trending_up',
      gradient: 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      shadowColor: '240, 147, 251'
    },
    {
      id: 'ecl',
      name: 'ECL',
      icon: 'account_balance',
      gradient: 'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      shadowColor: '79, 172, 254'
    }
  ];

  constructor(private router: Router) {}

  selectModule(moduleId: string): void {
    this.selectedModule = this.selectedModule === moduleId ? null : moduleId;
  }

  navigateToUpload(moduleId: string): void {
    console.log(`Upload ${moduleId.toUpperCase()}`);
  }

  navigateToResults(moduleId: string): void {
    console.log(`Results ${moduleId.toUpperCase()}`);
  }
}