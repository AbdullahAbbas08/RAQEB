import { Directive, ElementRef, Renderer2 } from '@angular/core';

@Directive({
  selector: '[appAsteriskColor]'
})
export class AsteriskColorDirective {

  constructor(private el: ElementRef, private renderer: Renderer2) { }

  ngOnInit() {
    const labelContent = this.el.nativeElement.textContent;
    if (labelContent.includes('*')) {
      this.renderer.setStyle(this.el.nativeElement, 'color', 'red');
    }
  }
}
