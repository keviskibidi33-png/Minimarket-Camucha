import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SalesService } from '../../../core/services/sales.service';
import { ToastService } from '../../../shared/services/toast.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-cancel-sale',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './cancel-sale.component.html',
  styleUrl: './cancel-sale.component.css'
})
export class CancelSaleComponent implements OnInit {
  cancelForm: FormGroup;
  saleId = signal<string | null>(null);
  isLoading = signal(false);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private toastService: ToastService
  ) {
    this.cancelForm = this.fb.group({
      reason: ['', [Validators.required, Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.saleId.set(id);
    } else {
      this.router.navigate(['/ventas']);
    }
  }

  onSubmit(): void {
    if (this.cancelForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    const reason = this.cancelForm.get('reason')?.value;

    this.http.post(`${environment.apiUrl}/sales/${this.saleId()}/cancel`, { reason }).subscribe({
      next: () => {
        this.toastService.success('Venta anulada exitosamente');
        this.router.navigate(['/ventas']);
      },
      error: (error) => {
        console.error('Error cancelling sale:', error);
        this.toastService.error(error.error?.errors?.[0] || 'Error al anular la venta');
        this.isLoading.set(false);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/ventas']);
  }
}

