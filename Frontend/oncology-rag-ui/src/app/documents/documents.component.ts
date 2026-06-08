import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, Document } from '../services/api.service';

@Component({
  selector: 'app-documents',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="sidebar">
      <div class="sidebar-header">
        <span class="icon">📄</span>
        <h3>Indexed Documents</h3>
      </div>

      <div class="loading" *ngIf="loading">
        <div class="spinner"></div>
        <span>Loading…</span>
      </div>

      <div class="empty" *ngIf="!loading && documents.length === 0">
        <p>No documents indexed yet.</p>
        <p class="hint">Start the backend to begin indexing.</p>
      </div>

      <div class="doc-list" *ngIf="!loading && documents.length > 0">
        <a
          *ngFor="let doc of documents"
          [href]="doc.url"
          target="_blank"
          rel="noopener"
          class="doc-card"
        >
          <div class="doc-name">{{ doc.fileName }}</div>
          <div class="doc-meta">
            <span>{{ doc.pageCount }} pages</span>
            <span class="dot">·</span>
            <span>{{ doc.chunkCount }} chunks</span>
          </div>
          <div class="doc-date">Indexed {{ formatDate(doc.indexedAt) }}</div>
        </a>
      </div>

      <button class="refresh-btn" (click)="load()" [disabled]="loading">
        ↻ Refresh
      </button>
    </div>
  `,
  styles: [`
    .sidebar {
      display: flex; flex-direction: column;
      height: 100%; padding: 20px 16px;
      border-right: 1px solid #1e2235;
      background: #13151f;
    }
    .sidebar-header {
      display: flex; align-items: center; gap: 8px;
      margin-bottom: 20px;
      .icon { font-size: 18px; }
      h3 { font-size: 13px; font-weight: 600; color: #9ca3c1; letter-spacing: 0.5px; text-transform: uppercase; }
    }
    .loading {
      display: flex; align-items: center; gap: 8px; color: #6b7280; font-size: 13px;
    }
    .spinner {
      width: 14px; height: 14px;
      border: 2px solid #2d3145; border-top-color: #4f6ef7;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    .empty { color: #4a5068; font-size: 13px; p { margin-bottom: 4px; } .hint { font-size: 11px; } }
    .doc-list { flex: 1; overflow-y: auto; display: flex; flex-direction: column; gap: 8px; }
    .doc-card {
      display: block;
      background: #1a1d27; border: 1px solid #2d3145; border-radius: 8px;
      padding: 12px; text-decoration: none; color: inherit;
      transition: border-color 0.2s, background 0.2s;
      &:hover { border-color: #4f6ef7; background: #1e2235; }
    }
    .doc-name { font-size: 13px; font-weight: 500; color: #c8cfe8; margin-bottom: 5px; line-height: 1.4; }
    .doc-meta { display: flex; align-items: center; gap: 4px; font-size: 11px; color: #5a6280; .dot { color: #3a3f4e; } }
    .doc-date { font-size: 10px; color: #3e4459; margin-top: 4px; }
    .refresh-btn {
      margin-top: 12px; width: 100%;
      background: transparent; border: 1px solid #2d3145;
      border-radius: 6px; padding: 8px; color: #6b7280;
      font-size: 12px; cursor: pointer; transition: all 0.2s;
      &:hover:not(:disabled) { border-color: #4f6ef7; color: #a0aec0; }
      &:disabled { opacity: 0.4; cursor: not-allowed; }
    }
  `]
})
export class DocumentsComponent implements OnInit {
  documents: Document[] = [];
  loading = false;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.api.getDocuments().subscribe({
      next: docs => { this.documents = docs; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  formatDate(iso: string): string {
    return new Date(iso).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  }
}
