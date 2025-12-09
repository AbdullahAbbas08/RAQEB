import { Component, OnInit } from '@angular/core';
import { SwaggerClient } from '../../shared/services/Swagger/SwaggerClient.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss']
})
export class ListComponent implements OnInit {
  data: any = null;
  rows: any[] = [];
  loading: boolean = false;
  error: string | null = null;
  asOfDate: Date | null = null;
  year: number | null = null;
  month: number | null = null;

  displayedColumns: string[] = [
    'bucket',
    'receivableBalance',
    'eclBase',
    'eclBest',
    'eclWorst',
    'eclWeightedAverage',
    'lossRatio'
  ];

  constructor(private swaggerClient: SwaggerClient) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.error = null;

    this.swaggerClient.apiECLSEMPGet().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.data = response.data;
          this.rows = response.data.rows || [];
          this.asOfDate = response.data.asOfDate ? new Date(response.data.asOfDate) : null;
          this.year = response.data.year;
          this.month = response.data.month;
        } else {
          this.error = response.message || 'Failed to load data';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'An error occurred while fetching data';
        console.error('API Error:', err);
        this.loading = false;
      }
    });
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  formatDate(date: Date | null): string {
    if (!date) return '';
    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    }).format(date);
  }

  isTotal(bucket: string): boolean {
    return bucket.toLowerCase() === 'total';
  }
}