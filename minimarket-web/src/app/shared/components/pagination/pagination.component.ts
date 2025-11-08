import { Component, Input, Output, EventEmitter, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PageButtonComponent } from './page-button.component';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule, PageButtonComponent],
  templateUrl: './pagination.component.html',
  styleUrl: './pagination.component.css'
})
export class PaginationComponent {
  @Input() currentPage: number = 1;
  @Input() totalItems: number = 0;
  @Input() itemsPerPage: number = 4;
  @Input() maxVisiblePages: number = 5;
  
  @Output() pageChange = new EventEmitter<number>();

  totalPages = computed(() => Math.ceil(this.totalItems / this.itemsPerPage));
  
  startIndex = computed(() => (this.currentPage - 1) * this.itemsPerPage + 1);
  endIndex = computed(() => Math.min(this.currentPage * this.itemsPerPage, this.totalItems));

  pageNumbers = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage;
    const numbers: number[] = [];
    
    if (total <= this.maxVisiblePages) {
      for (let i = 1; i <= total; i++) {
        numbers.push(i);
      }
    } else {
      if (current <= 2) {
        numbers.push(1, 2, 3, total);
      } else if (current >= total - 1) {
        numbers.push(1, total - 2, total - 1, total);
      } else {
        numbers.push(1, current - 1, current, current + 1, total);
      }
    }
    
    return numbers;
  });

  hasEllipsisBefore = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage;
    return total > this.maxVisiblePages && current > 3;
  });

  hasEllipsisAfter = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage;
    return total > this.maxVisiblePages && current < total - 2;
  });

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.pageChange.emit(page);
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.pageChange.emit(this.currentPage - 1);
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages()) {
      this.pageChange.emit(this.currentPage + 1);
    }
  }

  // TrackBy function para *ngFor
  trackByIndex(index: number): number {
    return index;
  }
}

