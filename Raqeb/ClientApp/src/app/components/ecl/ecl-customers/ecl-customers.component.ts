import { Component, OnInit } from '@angular/core';
import { debounceTime, Subject } from 'rxjs';
import { SwaggerClient } from '../../../shared/services/Swagger/SwaggerClient.service';



@Component({
  selector: 'app-ecl-customers',
  templateUrl: './ecl-customers.component.html',
  styleUrls: ['./ecl-customers.component.scss']
})
export class EclCustomersComponent implements OnInit {
  customers: any[] = [];
  loading = false;
  error: string | null = null;
  endRow = 0;


  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalRows = 0;
  totalPages = 0;

  // Search filters
  customerNumber: number | null = null;
  customerName: string | null = null;

  // Search debounce
  private searchSubject = new Subject<void>();

  constructor(
    private apiService: SwaggerClient // Inject your API service here
  ) {}

  ngOnInit(): void {
    this.loadCustomers();

    // Setup search debounce
    this.searchSubject.pipe(
      debounceTime(500)
    ).subscribe(() => {
      this.currentPage = 1;
      this.loadCustomers();
    });
  }

  loadCustomers(): void {
    // this.loading = true;
    this.error = null;

    // Replace this with your actual API call
   this.apiService.apiEclCustomersStageGet(
                    this.customerNumber,
                    this.customerName,
                    this.currentPage,
                    this.pageSize
                  ).subscribe(x=>{
                      this.customers = x.data || [];
                      this.totalRows = x.totalRows || 0;
                      this.totalPages = Math.ceil(this.totalRows / this.pageSize);
                 this.endRow = Math.min(this.currentPage * this.pageSize, this.totalRows);

                    this.loading = false;
                  });


  
  }

  onSearchChange(): void {
    this.searchSubject.next();
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCustomers();
    }
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadCustomers();
  }

  clearFilters(): void {
    this.customerNumber = null;
    this.customerName = null;
    this.currentPage = 1;
    this.loadCustomers();
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxVisible = 5;
    let start = Math.max(1, this.currentPage - Math.floor(maxVisible / 2));
    let end = Math.min(this.totalPages, start + maxVisible - 1);

    if (end - start < maxVisible - 1) {
      start = Math.max(1, end - maxVisible + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }

    return pages;
  }
}