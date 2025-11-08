import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './page-button.component.html'
})
export class PageButtonComponent {
  @Input() pageNumber: number = 1;
  @Input() isActive: boolean = false;
  @Output() pageClick = new EventEmitter<number>();

  onClick(): void {
    this.pageClick.emit(this.pageNumber);
  }
}

