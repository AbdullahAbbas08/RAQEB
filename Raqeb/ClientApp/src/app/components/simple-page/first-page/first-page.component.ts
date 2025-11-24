import { Component, OnInit } from '@angular/core';
import { ApiResponseOfPoolLGDCalculationResultDTO, PoolLGDCalculationResultDTO, PoolLGDDTO, SwaggerClient } from '../../../shared/services/Swagger/SwaggerClient.service';

interface LgdItem {
  poolId: number;
  poolName: string;
  ead: number;
  recoveryRate: number;
  unsecuredLGD: number;
}

@Component({
  selector: 'app-first-page',
  templateUrl: './first-page.component.html',
  styleUrls: ['./first-page.component.scss']
})
export class FirstPageComponent implements OnInit {
  lgdData: PoolLGDDTO[] = [];
  loading = false;
  error: string | null = null;
  message: string | null = null;
  noResults = false;
  versions: number[] = [];
  selectedVersion: number | null = null;
  currentVersion: number | null = null;

  constructor(private swaggerClient: SwaggerClient) {}

  ngOnInit(): void {
    this.loadVersions();
  }

   loadVersions(): void {
    this.swaggerClient.apiLGDGetAllVersionsGet().subscribe(
      (res) => {
        try {
          this.versions = Array.isArray(res) ? res : [];
        } catch {
          this.versions = [];
        }
        if (this.versions.length) {
          // assume versions are numeric; pick latest (max)
          this.versions.sort((a, b) => b - a);
          // this.selectedVersion = this.versions[0];
          this.loadLatestLgd(this.selectedVersion);
        } else {
          this.loadLatestLgd();
        }
      },
      (err) => {
        console.error('Failed to load LGD versions', err);
        // still try to load latest if versions endpoint fails
        this.loadLatestLgd();
      }
    );
  }

  private loadLatestLgd(version?: number | null): void {
    this.loading = true;
    this.error = null;
    this.noResults = false;
    this.message = null;
    this.swaggerClient.apiLGDLatestLgdResultsGet(version ?? null).subscribe(
      (resp :ApiResponseOfPoolLGDCalculationResultDTO) => {
        try {
          // debugger
          const data  = resp.data  ;
          console.log(resp);
          
          if (!resp.success) {
            this.error = '   No LGD results found.';
            this.loading = false;
            return;
          }

          // Two possible shapes depending on generated client:
          // 1) resp is already a parsed ApiResponseOfPoolLGDCalculationResultDTO (resp.data is object)
          // 2) resp.data is a Blob (older client) — fall back to reading blob text

            // If server responded with success:false -> show no results message
            if (!resp.success) {
              this.noResults = true;
              this.message = 'No LGD results found.';
              this.lgdData = [];
              this.loading = false;
              return;
            }

            this.message = resp.message || null;
            this.currentVersion =  data!.version ?? (version ?? null);
            this.lgdData = Array.isArray(data!.pools) ?  data!.pools : [];
            this.loading = false;
            return;
          

          // // If data is a Blob, read it and parse JSON
          // const blob = (resp as any).data as Blob;
          // if (blob && (blob as any).text) {
          //   (blob as any).text().then((text: string) => this.processResponseText(text));
          // } else if (blob) {
          //   const reader = new FileReader();
          //   reader.onload = () => this.processResponseText(reader.result as string);
          //   reader.onerror = () => {
          //     this.error = 'Failed to read response blob';
          //     this.loading = false;
          //   };
          //   reader.readAsText(blob);
          // } else {
          //   this.error = 'Unexpected response format from server.';
          //   this.loading = false;
          // }
        } catch (e: any) {
          this.error = e?.message || 'Unexpected response processing error';
          this.loading = false;
        }
      },
      (err) => {
        console.error(err);
        this.error = (err && err.message) ? err.message : 'Failed to load latest LGD results.';
        this.loading = false;
      }
    );
  }

  private processResponseText(text: string) {
    try {
      const parsed = text ? JSON.parse(text) : null;
      if (!parsed) {
        this.error = 'Empty JSON response';
        this.loading = false;
        return;
      }
      // If server indicates no results
      if (parsed.success === false) {
        this.noResults = true;
        this.message = 'No LGD results found.';
        this.lgdData = [];
        this.loading = false;
        return;
      }

      this.message = parsed.message || null;
      // New API shape: parsed.data = { version: number, pools: [] }
      if (parsed.data && parsed.data.pools) {
        this.currentVersion = parsed.data.version ?? null;
        this.lgdData = Array.isArray(parsed.data.pools) ? parsed.data.pools : [];
      } else if (Array.isArray(parsed.data)) {
        // backward-compat
        this.lgdData = parsed.data;
      } else {
        this.lgdData = [];
      }
    } catch (e: any) {
      this.error = 'Failed to parse response JSON: ' + (e?.message || e);
    } finally {
      this.loading = false;
    }
  }

  onVersionChange(value: string) {
    const v = value ? Number(value) : null;
    this.selectedVersion = v;
    this.loadLatestLgd(v);
  }

  reloadLatest() {
    this.loadLatestLgd(this.selectedVersion);
  }

  trackByPoolId(index: number, item: LgdItem) {
    return item.poolId;
  }
}
