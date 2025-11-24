import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { SwaggerClient, PDMatrixFilterDto, TransitionMatrixDto } from '../../../shared/services/Swagger/SwaggerClient.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-yearly-avg-transition-matrix',
  templateUrl: './yearly-avg-transition-matrix.component.html',
  styleUrl: './yearly-avg-transition-matrix.component.scss',
  providers: [MessageService]
})
export class YearlyAvgTransitionMatrixComponent implements OnInit {
  matrixData: TransitionMatrixDto[] = [];
  filterForm: FormGroup;
  loading = false;
  years: number[] = [];
  currentMatrix: TransitionMatrixDto | null = null;

  constructor(
    private swaggerClient: SwaggerClient,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    this.filterForm = this.fb.group({
      year: [2015],
       pageSize: [12],
      page: [1],
      version: [1],
      poolId : [7]
    });
  }

  ngOnInit() {
  const currentYear = new Date().getFullYear();
    for (let year = 2015; year <= 2021; year++) {
      this.years.push(year);
    }
    this.loadData();
  }

  hasData(): boolean {
    return !!this.currentMatrix?.cells && this.currentMatrix.cells.length > 0;
  }

  loadData() {
    this.loading = true;
    const filter: PDMatrixFilterDto = this.filterForm.value;
    
    this.swaggerClient.apiPDYearlyAveragesPost(filter)
    .subscribe({
        next: (data) => {
      this.matrixData = data || [];
          if (data && data.length > 0) {
          this.currentMatrix = data[0];
     } else {
   this.currentMatrix = null;
}
          this.loading = false;
        },
        error: (error) => {
  console.error('Error loading matrix data:', error);
 this.messageService.add({
          severity: 'error',
 summary: 'Error',
      detail: 'Failed to load yearly average matrix data'
          });
          this.loading = false;
        this.currentMatrix = null;
     }
      });
  }

  onFilterChange() {
    this.loadData();
  }

  getMatrixData(fromGrade: number, toGrade: number): number {
    if (!this.currentMatrix?.cells) return 0;
    const cell = this.currentMatrix.cells.find(
      c => c.fromGrade === fromGrade && c.toGrade === toGrade
    );
    return cell?.count || 0;
  }

  getRowStats(fromGrade: number) {
    if (!this.currentMatrix?.rowStats) return null;
    return this.currentMatrix.rowStats.find(
      r => r.fromGrade === fromGrade
    );
  }

  getCellColor(value: number, max: number): string {
    if (value === 0) return '#ffffff';
    const intensity = Math.log(value + 1) / Math.log(max + 1);
    const baseColor = '0, 61, 125';
    return `rgba(${baseColor}, ${intensity})`;
  }

  getMaxValue(): number {
if (!this.currentMatrix?.cells) return 0;
    return Math.max(...this.currentMatrix.cells.map(c => c.count));
  }

  exportExcel() {
    if (!this.hasData()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'No Data',
  detail: 'No data available to export'
      });
      return;
    }

    const filter: PDMatrixFilterDto = this.filterForm.value;
    
    this.swaggerClient.apiPDYearlyAveragesExportPost(filter)
      .subscribe({
next: (response) => {
          if (response && response.data) {
            const blob = new Blob([response.data], { 
  type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
       });
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `yearly-average-matrix-${filter.year}.xlsx`;
            link.click();
       window.URL.revokeObjectURL(url);

    this.messageService.add({
   severity: 'success',
     summary: 'Success',
   detail: 'Excel file exported successfully'
      });
        }
        },
        error: (error) => {
          console.error('Error exporting Excel:', error);
     this.messageService.add({
            severity: 'error',
   summary: 'Error',
            detail: 'Failed to export Excel file'
  });
        }
      });
  }
}
