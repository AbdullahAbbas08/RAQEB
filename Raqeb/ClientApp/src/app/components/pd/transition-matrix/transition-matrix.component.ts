import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { SwaggerClient, PDMatrixFilterDto, PagedResultOfPDTransitionMatrixDto } from '../../../shared/services/Swagger/SwaggerClient.service';
import { MessageService } from 'primeng/api';

@Component({
  templateUrl: './transition-matrix.component.html',
  styleUrl: './transition-matrix.component.scss',
  providers: [MessageService]
})
export class TransitionMatrixComponent implements OnInit {
  matrixData!: PagedResultOfPDTransitionMatrixDto;
  filterForm: FormGroup;
  loading = false;
  years: number[] = [];
  months: { label: string; value: number }[] = [];
  currentMatrix: any = null;

  constructor(
    private swaggerClient: SwaggerClient,
    private fb: FormBuilder,
    private messageService: MessageService
  ) {
    // Set default year to 2015 and January as default month
    this.filterForm = this.fb.group({
      year: [2015],
      month: [1], // Set to January (1)
      pageSize: [12],
      page: [1],
      version: [1],
      poolId : [1]
    });

    // Initialize months array
    this.months = [
      { label: 'January', value: 1 },
      { label: 'February', value: 2 },
      { label: 'March', value: 3 },
      { label: 'April', value: 4 },
      { label: 'May', value: 5 },
      { label: 'June', value: 6 },
      { label: 'July', value: 7 },
      { label: 'August', value: 8 },
      { label: 'September', value: 9 },
      { label: 'October', value: 10 },
      { label: 'November', value: 11 },
      { label: 'December', value: 12 }
    ];
  }

  ngOnInit() {
    // Generate years from 2015 to current year
    const currentYear = new Date().getFullYear();
    for (let year = 2015; year <= 2021; year++) {
      this.years.push(year);
    }
    this.loadData();
  }

  hasData(): boolean {
    return !!this.currentMatrix?.cells && this.currentMatrix.cells.length > 0;
  }

  getMonthName(month: number): string {
    return this.months.find(m => m.value === month)?.label || '';
  }

  loadData() {
    this.loading = true;
    const filter: PDMatrixFilterDto = this.filterForm.value;
    
    this.swaggerClient.apiPDTransitionMatricesPost(filter)
      .subscribe({
        next: (data) => {
          this.matrixData = data;
          if (data.items && data.items.length > 0) {
            this.currentMatrix = data.items[0];
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
            detail: 'Failed to load transition matrix data'
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
    const baseColor = '0, 61, 125';  // RGB values
    return `rgba(${baseColor}, ${intensity})`;
  }

  getMaxValue(): number {
    if (!this.currentMatrix?.cells) return 0;
    return Math.max(...this.currentMatrix.cells.map(c => c.count));
  }

  exportPdf() {
    if (!this.hasData()) {
      this.messageService.add({
        severity: 'warn',
        summary: 'No Data',
        detail: 'No data available to export'
      });
      return;
    }

    const filter: PDMatrixFilterDto = this.filterForm.value;
    
    this.swaggerClient.apiPDTransitionMatricesExportPost(filter)
      .subscribe({
        next: (response) => {
     if (response && response.data) {
        // Create blob and download
            const blob = new Blob([response.data], { type: 'application/pdf' });
    const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
     link.href = url;
         link.download = `transition-matrix-${filter.year}-${filter.month}.pdf`;
      link.click();
            window.URL.revokeObjectURL(url);

   this.messageService.add({
        severity: 'success',
     summary: 'Success',
            detail: 'PDF exported successfully'
       });
     }
        },
  error: (error) => {
          console.error('Error exporting PDF:', error);
    this.messageService.add({
         severity: 'error',
   summary: 'Error',
            detail: 'Failed to export PDF'
          });
        }
    });
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
    
    this.swaggerClient.apiPDTransitionMatricesExportPost(filter)
      .subscribe({
    next: (response) => {
          if (response && response.data) {
     // Create blob and download
   const blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            const url = window.URL.createObjectURL(blob);
 const link = document.createElement('a');
 link.href = url;
      link.download = `transition-matrix-${filter.year}-${filter.month}.xlsx`;
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