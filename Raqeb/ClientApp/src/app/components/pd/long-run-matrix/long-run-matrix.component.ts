import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FormBuilder, FormGroup } from '@angular/forms';
import { SwaggerClient, TransitionMatrixDto } from '../../../shared/services/Swagger/SwaggerClient.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-long-run-matrix',
  templateUrl: './long-run-matrix.component.html',
  styleUrl: './long-run-matrix.component.scss',
  providers: [MessageService],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
      style({ opacity: 0 }),
    animate('300ms', style({ opacity: 1 }))
 ])
    ])
  ]
})
export class LongRunMatrixComponent implements OnInit {
  matrixData: TransitionMatrixDto | null = null;
  loading = false;

  // Color themes for the matrix
  colorScheme = {
    header: '#673AB7', // Deep Purple
    headerText: '#ffffff',
  cell: {
      base: '103, 58, 183', // RGB values for Deep Purple
      highlight: '156, 39, 176' // Purple
    },
    totalCell: '#EDE7F6', // Very Light Purple
    pdCell: '#9C27B0', // Purple
    border: '#E1BEE7' // Light Purple
  };

  constructor(
    private swaggerClient: SwaggerClient,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadData();
  }

  hasData(): boolean {
    return !!this.matrixData?.cells && this.matrixData.cells.length > 0;
  }

  loadData() {
    this.loading = true;
    
    this.swaggerClient.apiPDTransitionMatrixLongrunPost()
      .subscribe({
        next: (data) => {
      this.matrixData = data;
          this.loading = false;
          if (this.hasData()) {
     this.messageService.add({
     severity: 'success',
   summary: 'Success',
              detail: 'Long-run matrix data loaded successfully'
    });
     }
     },
        error: (error) => {
          console.error('Error loading matrix data:', error);
          this.messageService.add({
     severity: 'error',
       summary: 'Error',
  detail: 'Failed to load long-run matrix data'
          });
          this.loading = false;
        }
  });
  }

  getMatrixData(fromGrade: number, toGrade: number): number {
    if (!this.matrixData?.cells) return 0;
    const cell = this.matrixData.cells.find(
      c => c.fromGrade === fromGrade && c.toGrade === toGrade
  );
    return cell?.count || 0;
  }

  getRowStats(fromGrade: number) {
    if (!this.matrixData?.rowStats) return null;
    return this.matrixData.rowStats.find(
      r => r.fromGrade === fromGrade
    );
  }

  getCellColor(value: number, max: number): string {
    if (value === 0) return '#ffffff';
    const intensity = Math.log(value + 1) / Math.log(max + 1);
    return `rgba(${this.colorScheme.cell.base}, ${intensity})`;
  }

  getCellTextColor(value: number, max: number): string {
    const intensity = Math.log(value + 1) / Math.log(max + 1);
    return intensity > 0.5 ? '#ffffff' : '#000000';
  }

  getMaxValue(): number {
  if (!this.matrixData?.cells) return 0;
    return Math.max(...this.matrixData.cells.map(c => c.count));
  }

  getTotalRowData(): { totals: number[], grandTotal: number, avgPD: number } {
    const totals = [0, 0, 0, 0];
    let grandTotal = 0;
    let totalPD = 0;

    if (this.matrixData?.rowStats) {
      this.matrixData.rowStats.forEach(stat => {
        grandTotal += stat.totalCount;
        totalPD += stat.pdPercent;
      });
    }

    if (this.matrixData?.cells) {
  for (let toGrade = 1; toGrade <= 4; toGrade++) {
        totals[toGrade - 1] = this.matrixData.cells
          .filter(c => c.toGrade === toGrade)
     .reduce((sum, cell) => sum + cell.count, 0);
  }
    }

    const avgPD = this.matrixData?.rowStats?.length 
    ? totalPD / this.matrixData.rowStats.length 
    : 0;

    return { totals, grandTotal, avgPD };
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

    this.swaggerClient.apiPDTransitionMatrixLongrunExportPost()
      .subscribe({
        next: (response) => {
     if (response && response.data) {
const blob = new Blob([response.data], { 
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
        });
        const url = window.URL.createObjectURL(blob);
         const link = document.createElement('a');
            link.href = url;
          link.download = `long-run-matrix-${new Date().toISOString().split('T')[0]}.xlsx`;
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
