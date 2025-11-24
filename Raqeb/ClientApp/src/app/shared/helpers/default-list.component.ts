import { HttpErrorResponse } from '@angular/common/http';
import { Component, Directive, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
// import { TranslateService } from '@ngx-translate/core';
import { MenuItem, MessageService } from 'primeng/api';
import { Observable, debounceTime, distinctUntilChanged, switchMap, tap } from 'rxjs';
import { EncryptDecryptService } from '../../shared/services/encrypt-decrypt.service';
import Swal from 'sweetalert2';
import { TranslateService } from '@ngx-translate/core';
import { log } from 'console';

@Directive()
export abstract class DefaultListComponent<
  T extends {
    paginationList?: K[] | undefined;
    originalListListCount?: number;
  },
  K
> implements OnInit {
  entities: K[] | undefined = [];
  isLoading: boolean = false;
  page: number = 1;
  pageSize: number = 9;
  count: number = 0;
  alternatepageSize: number = 9;
  alternatepage: number = 1;
  alternateCount: number = 0;
  pageSizeOptions: number[] = [9,12,15,18,21,42,84,168,504,1008];

  searchTermControl: FormControl = new FormControl('');

  abstract breadcrumb: MenuItem[];
  constructor(
    protected translate: TranslateService,
    protected router: Router,
    protected toast: MessageService,
    protected auth: EncryptDecryptService,
    // protected confirmationService: ConfirmationService,
  ) { }

  ngOnInit(): void {
    this.getData();
    this.trackSearchTerm();
  }

  EncryptId(id: any): any {
    let val = this.auth.encryptUsingAES256(id?.toString());
    let NewVal = val.replace(/\//g, "__")
    return NewVal;
  }


  getData($event?: any) {
    this.isLoading = true;
    if ($event) {
      this.page = $event.page + 1;
      this.pageSize = $event.rows;
    }
    this.returnDataFn().subscribe(
      (res) => {
        this.entities = res.paginationList;
        this.count = res.originalListListCount ? res.originalListListCount : 0;
        this.isLoading = false;     
        console.log(this.entities);           
      },
      (err: HttpErrorResponse) => {
        this.isLoading = false;
      }
    );
  }

  trackSearchTerm() {
    this.searchTermControl.valueChanges
      .pipe(
        tap(() => {
          this.isLoading = true;
        }),
        debounceTime(1000),
        distinctUntilChanged(),
        switchMap(() => {
          return this.returnDataFn();
        })
      )
      .subscribe(
        (res) => {
          this.entities = res.paginationList;
          this.count = res.originalListListCount
            ? res.originalListListCount
            : 0;
          this.isLoading = false;
        },
        (err: HttpErrorResponse) => {
          this.isLoading = false;
        }
      );
  }

  delete(id: number) {
    Swal.fire({
      title:this.translate.instant('Areyousure'),
      text:this.translate.instant('Youwontbeabletorevertthis'),
      icon: "warning",
      showCancelButton: true,
      confirmButtonColor: "#3085d6",
      cancelButtonColor: "#d33",
      confirmButtonText: this.translate.instant('Yesdeleteit'),
      cancelButtonText: this.translate.instant('Cancel'),
    }).then((result) => {
      if (result.isConfirmed) {
        this.returnDeleteFn(id).subscribe(
          (res) => {
            this.toast.add({
              severity: 'success',
              detail: 'ProcessSuccess',
            });
            this.getData();
            this.isLoading = false;
          },
          (err: HttpErrorResponse) => {
            this.isLoading = false;
          }
        );
        // Swal.fire({
        //   title: "Deleted!",
        //   text: "Your file has been deleted.",
        //   icon: "success"
        // });
      }
    });

    // this.confirmationService.confirm({
    //   message: 'AreYouSure',
    //   key: 'deleteConfirm',
    //   accept: () => {
    //     this.isLoading = true;
    //     this.returnDeleteFn(id).subscribe(
    //       (res) => {
    //         this.toast.add({
    //           severity: 'success',
    //           detail: 'ProcessSuccess',
    //         });
    //         this.getData();
    //         this.isLoading = false;
    //       },
    //       (err: HttpErrorResponse) => {
    //         this.isLoading = false;
    //       }
    //     );
    //   },
    // });
  }

  add() {
    this.router.navigateByUrl(this.router.url + '/form/')
  }

  edit(id: any) {

    this.router.navigate([this.router.url + '/form/' + id]);
  }

  entityType(entity: K): K {
    return entity;
  }

  abstract returnDataFn(): Observable<T>;

  abstract returnDeleteFn(id: number): Observable<boolean>;
}
