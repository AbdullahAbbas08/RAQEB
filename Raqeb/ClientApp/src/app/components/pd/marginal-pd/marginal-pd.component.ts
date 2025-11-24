import { Component, OnInit } from '@angular/core';
import { MarginalPdRowDto, MarginalPdTablesResponse, SwaggerClient } from '../../../shared/services/Swagger/SwaggerClient.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { MessageService } from 'primeng/api';


@Component({
  selector: 'app-marginal-pd',
  templateUrl: './marginal-pd.component.html',
  styleUrls: ['./marginal-pd.component.scss'],
  providers: [MessageService],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate('400ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ])
  ]
})
export class MarginalPDComponent implements OnInit {
  loading: boolean = false;
  exporting: boolean = false;
  marginalData: MarginalPdTablesResponse | null = null;
  error: string | null = null;
  activeTab: 'base' | 'best' | 'worst' = 'base';

  constructor(
    private swaggerClient: SwaggerClient,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadMarginalPD();
  }

  async loadMarginalPD() {
    try {
      this.loading = true;
      this.error = null;
      const response = await this.swaggerClient.apiPDMarginalMarginalTablesGet().toPromise();
      
      if (response && response.success) {
        this.marginalData = response.data || null;
      } else {
        // this.error = response?.message || 'Failed to load data';
        // this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    } catch (err: any) {
      // this.error = err?.message || 'Error loading Marginal PD data';
      // this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      console.error('Error:', err);
    } finally {
      this.loading = false;
    }
  }

  async exportToExcel() {
    if (this.exporting || !this.marginalData) return;
    
    try {
      this.exporting = true;
      this.error = null;
      
      const response = await this.swaggerClient.apiPDMarginalExportMarginalGet().toPromise();
      
      if (response) {
        // Create a blob from the response data
        const blob = new Blob([response.data], { 
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
        });
        
        // Create a download link and trigger the download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = response.fileName || 'MarginalPD.xlsx';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        this.messageService.add({ 
          severity: 'success', 
          summary: 'Success', 
          detail: 'Marginal PD exported successfully' 
        });
      }
    } catch (err: any) {
      this.error = 'Error exporting to Excel';
      this.messageService.add({ severity: 'error', summary: 'Export Error', detail: this.error });
      console.error('Export error:', err);
    } finally {
      this.exporting = false;
    }
  }

  setActiveTab(tab: 'base' | 'best' | 'worst') {
    this.activeTab = tab;
  }

  getActiveData(): MarginalPdRowDto[] {
    if (!this.marginalData) return [];
    return this.marginalData[this.activeTab] || [];
  }

  getTabLabel(tab: 'base' | 'best' | 'worst'): string {
    const labels = {
      base: 'Base Scenario',
      best: 'Best Scenario',
      worst: 'Worst Scenario'
    };
    return labels[tab];
  }

  getTabIcon(tab: 'base' | 'best' | 'worst'): string {
    const icons = {
      base: 'pi-chart-line',
      best: 'pi-arrow-up',
      worst: 'pi-arrow-down'
    };
    return icons[tab];
  }

  // Helper method to check if a value is high risk
  isHighRisk(value: string, columnName: string): boolean {
    if (!value) return false;
    const numValue = parseFloat(value.replace('%', ''));
    
    // High risk if PiT values are very high (> 80%)
    if (columnName.startsWith('piT_T') && numValue > 80) {
      return true;
    }
    
    return false;
  }

  hasData(): boolean {
    return !!(this.marginalData && 
             (this.marginalData.base?.length > 0 || 
              this.marginalData.best?.length > 0 || 
              this.marginalData.worst?.length > 0));
  }
}