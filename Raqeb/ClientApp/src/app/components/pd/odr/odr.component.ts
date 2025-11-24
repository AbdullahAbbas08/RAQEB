import { Component, OnInit } from '@angular/core';
import { SwaggerClient, PDObservedRateDto } from '../../../shared/services/Swagger/SwaggerClient.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-odr',
  templateUrl: './odr.component.html',
  styleUrls: ['./odr.component.scss'],
  providers: [MessageService],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class ODRComponent implements OnInit {
  loading: boolean = false;
  exporting: boolean = false;
  observedRates: PDObservedRateDto[] = [];
  error: string | null = null;

  constructor(private swaggerClient: SwaggerClient, private messageService: MessageService) {}

  ngOnInit() {
    this.loadObservedRates();
  }

  async loadObservedRates() {
    try {
      this.loading = true;
      this.error = null;
      const response = await this.swaggerClient.apiPDObservedDefaultRatesGet().toPromise();
      
      if (response && response.success) {
        this.observedRates = response.data || [];
      } else {
        //this.error = response?.message || 'Failed to load data';
        //this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      }
    } catch (err: any) {
      //this.error = err?.message || 'Error loading observed default rates';
      //this.messageService.add({ severity: 'error', summary: 'Error', detail: this.error });
      console.error('Error:', err);
    } finally {
      this.loading = false;
    }
  }

  async exportToExcel() {
    if (this.exporting || !this.observedRates.length) return;
    
    try {
      this.exporting = true;
      this.error = null;
      
      const response = await this.swaggerClient.apiPDObservedDefaultRatesExportGet().toPromise();
      
      if (response) {
        // Create a blob from the response data
        const blob = new Blob([response.data], { 
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
        });
        
        // Create a download link and trigger the download
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = response.fileName || 'ObservedDefaultRates.xlsx';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      }
    } catch (err: any) {
      this.error = 'Error exporting to Excel';
      this.messageService.add({ severity: 'error', summary: 'Export Error', detail: this.error });
      console.error('Export error:', err);
    } finally {
      this.exporting = false;
    }
  }

  // Helper method to format percentage
  formatPercentage(value: number): string {
    return (value ).toFixed(4) + '%';
  }

  // Check if rate is considered high risk
  isHighRisk(rate: number): boolean {
    return rate > 0.8; // 80%
  }
}
