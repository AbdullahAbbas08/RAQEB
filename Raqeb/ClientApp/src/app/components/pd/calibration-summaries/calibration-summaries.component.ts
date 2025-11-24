import { Component, OnInit } from '@angular/core';
import { SwaggerClient, CalibrationSummaryDto } from '../../../shared/services/Swagger/SwaggerClient.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-calibration-summaries',
  templateUrl: './calibration-summaries.component.html',
  styleUrls: ['./calibration-summaries.component.scss'],
  providers: [MessageService],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(-20px)', opacity: 0 }),
        animate('400ms ease-out', style({ transform: 'translateX(0)', opacity: 1 }))
      ])
    ])
  ]
})
export class CalibrationSummariesComponent implements OnInit {
  loading: boolean = false;
  summaries: CalibrationSummaryDto[] | undefined = [];
  selectedSummary: CalibrationSummaryDto | null = null;
  error: string | null = null;
  hasData: boolean = false;
  chartData: any;
  chartOptions: any;

  constructor(
    private swaggerClient: SwaggerClient,
    private messageService: MessageService
  ) {
    this.initChartOptions();
  }

  ngOnInit() {
    this.loadSummaries();
  }

  private initChartOptions() {
    this.chartOptions = {
      plugins: {
        legend: {
          labels: {
            color: '#495057',
            usePointStyle: true,
            font: {
              weight: 500
            }
          }
        },
        tooltip: {
          mode: 'index',
          intersect: false,
          backgroundColor: 'rgba(255, 255, 255, 0.9)',
          titleColor: '#1e40af',
          bodyColor: '#334155',
          borderColor: '#e2e8f0',
          borderWidth: 1,
          padding: 10,
          boxPadding: 4,
          callbacks: {
            label: (context: any) => {
              return `${context.dataset.label}: ${context.parsed.y.toFixed(2)}%`;
            }
          }
        }
      },
      scales: {
        x: {
          ticks: {
            color: '#495057',
            font: {
              size: 12
            }
          },
          grid: {
            color: '#ebedef'
          },
          title: {
            display: true,
            text: 'Grades',
            color: '#1e40af',
            font: {
              weight: 'bold'
            }
          }
        },
        y: {
          ticks: {
            color: '#495057',
            font: {
              size: 12
            },
            callback: (value: any) => value + '%'
          },
          grid: {
            color: '#ebedef'
          },
          title: {
            display: true,
            text: 'Probability (%)',
            color: '#1e40af',
            font: {
              weight: 'bold'
            }
          }
        }
      },
      interaction: {
        intersect: false,
        mode: 'index'
      },
      responsive: true,
      maintainAspectRatio: false
    };
  }

  loadSummaries() {
    this.loading = true;
    this.error = null;
    this.hasData = false;
    
    this.swaggerClient.apiPDCalibrationSummariesGet().subscribe({
      next: (response) => {
        console.log('API Response:', response);
        
        this.summaries = response;
        
        // Check if we have valid data
        if (this.summaries && this.summaries.length > 0) {
          this.hasData = true;
          this.selectSummary(this.summaries[0]);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `Loaded ${this.summaries.length} calibration ${this.summaries.length === 1 ? 'summary' : 'summaries'}`,
            life: 3000
          });
        } else {
          this.hasData = false;
          this.messageService.add({
            severity: 'info',
            summary: 'No Data Available',
            detail: 'No calibration summaries found in the system',
            life: 5000
          });
        }
        
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading summaries:', err);
        this.loading = false;
        this.hasData = false;
        this.error = err.message || 'Failed to load calibration summaries';
        
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: this.error,
          life: 5000
        });
      }
    });
  }

  selectSummary(summary: CalibrationSummaryDto) {
    this.selectedSummary = summary;
    this.updateChartData();
  }

  private updateChartData() {
    if (!this.selectedSummary?.grades || this.selectedSummary.grades.length === 0) {
      this.chartData = null;
      return;
    }

    const grades = this.selectedSummary.grades.sort((a, b) => a.grade - b.grade);
    
    this.chartData = {
      labels: grades.map(g => `Grade ${g.grade}`),
      datasets: [
        {
          label: 'Observed Default Rate',
          data: grades.map(g => g.odr),
          fill: false,
          borderColor: '#3b82f6',
          backgroundColor: '#3b82f6',
          tension: 0.4,
          pointBackgroundColor: '#3b82f6',
          pointBorderColor: '#ffffff',
          pointHoverBackgroundColor: '#ffffff',
          pointHoverBorderColor: '#3b82f6',
          pointRadius: 4,
          pointHoverRadius: 6
        },
        {
          label: 'Fitted PD',
          data: grades.map(g => g.fittedPD),
          fill: false,
          borderColor: '#10b981',
          backgroundColor: '#10b981',
          tension: 0.4,
          pointBackgroundColor: '#10b981',
          pointBorderColor: '#ffffff',
          pointHoverBackgroundColor: '#ffffff',
          pointHoverBorderColor: '#10b981',
          pointRadius: 4,
          pointHoverRadius: 6
        },
        {
          label: 'Calibrated PD',
          data: grades.map(g => g.cFittedPD),
          fill: false,
          borderColor: '#f59e0b',
          backgroundColor: '#f59e0b',
          tension: 0.4,
          pointBackgroundColor: '#f59e0b',
          pointBorderColor: '#ffffff',
          pointHoverBackgroundColor: '#ffffff',
          pointHoverBorderColor: '#f59e0b',
          pointRadius: 4,
          pointHoverRadius: 6
        }
      ]
    };
  }

  formatPercent(value: number): string {
    return (value || 0).toFixed(2) + '%';
  }

  formatLargeNumber(value: number): string {
    return new Intl.NumberFormat().format(value || 0);
  }

  retryLoad() {
    this.loadSummaries();
  }
}