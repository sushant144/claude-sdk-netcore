import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiKeyComponent } from './api-key/api-key.component';
import { DocumentsComponent } from './documents/documents.component';
import { ChatComponent } from './chat/chat.component';
import { ApiService } from './services/api.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, ApiKeyComponent, DocumentsComponent, ChatComponent],
  template: `
    <app-api-key *ngIf="!keyAccepted" (keyAccepted)="onKeyAccepted()"></app-api-key>

    <div class="toast" *ngIf="showToast">✓ API key accepted</div>

    <div class="layout" *ngIf="keyAccepted">
      <div class="sidebar-pane">
        <app-documents></app-documents>
      </div>
      <div class="main-pane">
        <app-chat></app-chat>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; height: 100vh; }
    .layout {
      display: grid; grid-template-columns: 280px 1fr; height: 100vh; overflow: hidden;
    }
    .sidebar-pane { overflow: hidden; }
    .main-pane { overflow: hidden; }
    .toast {
      position: fixed; top: 20px; left: 50%; transform: translateX(-50%);
      background: #1a3a1a; border: 1px solid #2d6b2d; border-radius: 8px;
      padding: 10px 20px; color: #6abf6a; font-size: 13px; font-weight: 500;
      z-index: 2000; animation: fadeOut 3s ease forwards;
    }
    @keyframes fadeOut {
      0%, 70% { opacity: 1; }
      100% { opacity: 0; pointer-events: none; }
    }
  `]
})
export class AppComponent {
  keyAccepted = false;
  showToast = false;

  constructor(private api: ApiService) {}

  onKeyAccepted(): void {
    this.keyAccepted = true;
    this.showToast = true;
    setTimeout(() => { this.showToast = false; }, 3200);
  }
}
