import { Component, PLATFORM_ID, Inject } from '@angular/core';
// import { isPlatformBrowser } from '@angular/common';
import { LoadingBarService } from '@ngx-loading-bar/core';
import { map, delay, withLatestFrom } from 'rxjs/operators';
// import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {

  // For Progressbar
  loaders = this.loader.progress$.pipe(
    delay(1000),
    withLatestFrom(this.loader.progress$),
    map(v => v[1]),
  );

  direction: any
  constructor(@Inject(PLATFORM_ID) private platformId: Object,
    private loader: LoadingBarService) {

    this.direction = localStorage.getItem('direction')
    if (this.direction) {
      document.getElementsByTagName("html")[0].setAttribute("dir", this.direction);
      document.body.className = this.direction;

      const multiSelectElement = document.querySelector('.p-multiselect') as HTMLElement;
      if (multiSelectElement) {
        multiSelectElement.style.direction = this.direction;
      }

    } else {
      document.getElementsByTagName("html")[0].removeAttribute("dir");
      document.body.className = "";
    }

  }

}
