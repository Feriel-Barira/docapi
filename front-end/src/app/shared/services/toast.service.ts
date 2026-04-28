import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  showSuccess(message: string): void {
    alert('✅ ' + message);
  }

  showError(message: string): void {
    alert('❌ ' + message);
  }

  showInfo(message: string): void {
    alert('ℹ️ ' + message);
  }
}