import { Component, OnInit } from '@angular/core';
import { EclGradeSummary, EclStageSummary, SwaggerClient } from '../../../shared/services/Swagger/SwaggerClient.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { MessageService } from 'primeng/api';

interface PoolOption {
  label: string;
  value: string;
  hasData: boolean;
}

@Component({
  selector: 'app-list',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
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
export class ListComponent implements OnInit {
  loading: boolean = false;
  stageSummary: EclStageSummary[] = [];
  gradeSummary: EclGradeSummary[] = [];
  error: string | null = null;
  activeTab: 'stage' | 'grade' = 'stage';
  selectedPool: string = 'pool1';
  
  pools: PoolOption[] = [
    { label: 'Pool 1', value: 'pool1', hasData: true },
    { label: 'Pool 2', value: 'pool2', hasData: false },
    { label: 'Pool 3', value: 'pool3', hasData: false },
    { label: 'Pool 4', value: 'pool4', hasData: false },
    { label: 'Pool 5', value: 'pool5', hasData: false },
    { label: 'Pool 6', value: 'pool6', hasData: false }
  ];

  // Store original Pool 1 data
  private pool1StageSummary: EclStageSummary[] = [];
  private pool1GradeSummary: EclGradeSummary[] = [];

  constructor(
    private swaggerClient: SwaggerClient,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadEclData();
  }

  async loadEclData() {
    try {
      this.loading = true;
      this.error = null;
      
      // Load both stage and grade summaries in parallel
      const [stageResponse, gradeResponse] = await Promise.all([
        this.swaggerClient.apiEclGetEclStageSummaryAsyncGet().toPromise(),
        this.swaggerClient.apiEclGetEclGradeSummaryAsyncGet().toPromise()
      ]);
      
      if (stageResponse && stageResponse.success) {
        this.pool1StageSummary = stageResponse.data || [];
        this.stageSummary = [...this.pool1StageSummary];
      }
      
      if (gradeResponse && gradeResponse.success) {
        this.pool1GradeSummary = gradeResponse.data || [];
        this.gradeSummary = [...this.pool1GradeSummary];
      }
      
      if (!this.hasData()) {
        this.messageService.add({ 
          severity: 'warn', 
          summary: 'No Data', 
          detail: 'No ECL data available for Pool 1' 
        });
      }
    } catch (err: any) {
      this.error = err?.message || 'Error loading ECL data';
      this.messageService.add({ 
        severity: 'error', 
        summary: 'Error', 
        detail: 'Failed to load ECL data'
      });
      console.error('Error:', err);
    } finally {
      this.loading = false;
    }
  }

  selectPool(poolValue: string) {
    this.selectedPool = poolValue;
    
    if (poolValue === 'pool1' || poolValue === 'total') {
      // Pool 1 or Total Pools = Pool 1 data
      this.stageSummary = [...this.pool1StageSummary];
      this.gradeSummary = [...this.pool1GradeSummary];
    } else {
      // Other pools have no data
      this.stageSummary = [];
      this.gradeSummary = [];
    }
  }

  getPoolLabel(): string {
    if (this.selectedPool === 'total') {
      return 'Total Pools';
    }
    const pool = this.pools.find(p => p.value === this.selectedPool);
    return pool ? pool.label : 'Pool 1';
  }

  setActiveTab(tab: 'stage' | 'grade') {
    this.activeTab = tab;
  }

  getActiveData(): EclStageSummary[] | EclGradeSummary[] {
    return this.activeTab === 'stage' ? this.stageSummary : this.gradeSummary;
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  }

  formatPercentage(value: number): string {
    return (value * 100).toFixed(2) + '%';
  }

  isHighRisk(value: number, type: 'lossRatio' | 'osContribution'): boolean {
    if (type === 'lossRatio') {
      return value > 0.35; // High risk if loss ratio > 35%
    }
    if (type === 'osContribution') {
      return value > 0.90; // High concentration if > 90%
    }
    return false;
  }

  hasData(): boolean {
    return this.stageSummary.length > 0 || this.gradeSummary.length > 0;
  }

  getTotalOutstanding(): number {
    const data = this.getActiveData();
    return data.reduce((sum, item) => sum + item.outstanding, 0);
  }

  getTotalEcl(): number {
    const data = this.getActiveData();
    return data.reduce((sum, item) => sum + item.ecl, 0);
  }

  getAverageLossRatio(): number {
    const data = this.getActiveData();
    if (data.length === 0) return 0;
    const totalLossRatio = data.reduce((sum, item) => sum + item.lossRatio, 0);
    return totalLossRatio / data.length;
  }
}