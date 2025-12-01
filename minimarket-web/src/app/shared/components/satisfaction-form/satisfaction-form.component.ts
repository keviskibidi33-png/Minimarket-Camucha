import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-satisfaction-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './satisfaction-form.component.html',
  styleUrl: './satisfaction-form.component.css'
})
export class SatisfactionFormComponent {
  @Input() orderNumber: string = '';
  @Output() submitted = new EventEmitter<{ rating: number; comment: string; wouldRecommend: boolean }>();
  @Output() cancelled = new EventEmitter<void>();

  rating = signal(5);
  comment = signal('');
  wouldRecommend = signal(true);

  setRating(value: number) {
    this.rating.set(value);
  }

  submit() {
    this.submitted.emit({
      rating: this.rating(),
      comment: this.comment().trim(),
      wouldRecommend: this.wouldRecommend()
    });
  }

  cancel() {
    this.cancelled.emit();
  }
}

