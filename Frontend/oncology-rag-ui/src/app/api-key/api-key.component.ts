import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-api-key',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="overlay">
      <div class="modal">
        <div class="modal-header">
          <div class="logo">🧬</div>
          <h2>Oncology RAG</h2>
          <p>Powered by Claude AI &amp; pgvector</p>
        </div>

        <div class="modal-body">
          <label for="apiKey">Anthropic API Key</label>
          <div class="input-wrap">
            <input
              id="apiKey"
              type="password"
              [(ngModel)]="key"
              placeholder="sk-ant-..."
              (keydown.enter)="validate()"
              [disabled]="loading"
              autocomplete="off"
            />
          </div>

          <div class="error" *ngIf="error">{{ error }}</div>

          <button (click)="validate()" [disabled]="!key || loading">
            <span *ngIf="!loading">Validate Key</span>
            <span *ngIf="loading">Validating…</span>
          </button>

          <p class="note">Your key is held in memory only and never stored or transmitted beyond this session.</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .overlay {
      position: fixed; inset: 0;
      background: rgba(0,0,0,0.75);
      display: flex; align-items: center; justify-content: center;
      z-index: 1000;
    }
    .modal {
      background: #1a1d27;
      border: 1px solid #2d3145;
      border-radius: 12px;
      padding: 40px;
      width: 420px;
      max-width: 95vw;
    }
    .modal-header {
      text-align: center;
      margin-bottom: 28px;
    }
    .logo { font-size: 40px; margin-bottom: 8px; }
    h2 { font-size: 22px; font-weight: 600; color: #fff; margin-bottom: 4px; }
    p { color: #7b8099; font-size: 13px; }
    label { display: block; font-size: 12px; font-weight: 500; color: #9ca3c1; margin-bottom: 8px; }
    .input-wrap input {
      width: 100%;
      background: #0f1117;
      border: 1px solid #2d3145;
      border-radius: 8px;
      padding: 12px 14px;
      color: #e8eaf0;
      font-size: 14px;
      outline: none;
      font-family: monospace;
      transition: border-color 0.2s;
      &:focus { border-color: #4f6ef7; }
      &:disabled { opacity: 0.5; }
    }
    .error {
      background: #2d1a1a;
      border: 1px solid #7f2d2d;
      border-radius: 6px;
      padding: 10px 12px;
      color: #e57373;
      font-size: 13px;
      margin-top: 12px;
    }
    button {
      width: 100%; margin-top: 16px;
      background: #4f6ef7; color: #fff;
      border: none; border-radius: 8px;
      padding: 12px; font-size: 14px; font-weight: 500;
      cursor: pointer; transition: background 0.2s;
      &:hover:not(:disabled) { background: #3a58e3; }
      &:disabled { opacity: 0.5; cursor: not-allowed; }
    }
    .note { font-size: 11px; color: #4a5068; margin-top: 14px; text-align: center; }
  `]
})
export class ApiKeyComponent {
  @Output() keyAccepted = new EventEmitter<string>();

  key = '';
  loading = false;
  error = '';

  constructor(private api: ApiService) {}

  validate(): void {
    if (!this.key) return;
    this.loading = true;
    this.error = '';

    this.api.validateKey(this.key).subscribe({
      next: res => {
        this.loading = false;
        if (res.valid) {
          this.api.setApiKey(this.key);
          this.keyAccepted.emit(this.key);
        } else {
          this.error = res.error ?? 'Invalid API key. Please try again.';
        }
      },
      error: () => {
        this.loading = false;
        this.error = 'Could not reach the backend. Is the API server running?';
      }
    });
  }
}
