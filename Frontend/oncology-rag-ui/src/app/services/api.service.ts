import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, from, map } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Document {
  id: number;
  fileName: string;
  url: string;
  pageCount: number;
  chunkCount: number;
  indexedAt: string;
}

export interface ChatEvent {
  type: 'bounds_check' | 'sources' | 'token' | 'usage' | 'done' | 'error';
  inScope?: boolean;
  reason?: string;
  items?: SourceItem[];
  text?: string;
  inputTokens?: number;
  outputTokens?: number;
  message?: string;
}

export interface SourceItem {
  file: string;
  url: string;
  page: number;
  score: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private apiKey = '';

  constructor(private http: HttpClient) {}

  setApiKey(key: string): void {
    this.apiKey = key;
  }

  hasApiKey(): boolean {
    return this.apiKey.length > 0;
  }

  validateKey(apiKey: string): Observable<{ valid: boolean; error?: string }> {
    return this.http.post<{ valid: boolean; error?: string }>(
      `${environment.apiUrl}/api/auth/validate`,
      { apiKey }
    );
  }

  getDocuments(): Observable<Document[]> {
    return this.http.get<Document[]>(`${environment.apiUrl}/api/documents`, {
      headers: new HttpHeaders({ 'X-Api-Key': this.apiKey })
    });
  }

  streamChat(message: string): Observable<ChatEvent> {
    // SSE via fetch — EventSource doesn't support custom headers or POST body
    return from(this.fetchStream(message));
  }

  private async *fetchStream(message: string): AsyncIterable<ChatEvent> {
    const response = await fetch(`${environment.apiUrl}/api/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'X-Api-Key': this.apiKey
      },
      body: JSON.stringify({ message, apiKey: this.apiKey })
    });

    const reader = response.body!.getReader();
    const decoder = new TextDecoder();
    let buffer = '';

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split('\n');
      buffer = lines.pop() ?? '';

      for (const line of lines) {
        if (line.startsWith('data: ')) {
          try {
            yield JSON.parse(line.slice(6)) as ChatEvent;
          } catch {
            // skip malformed lines
          }
        }
      }
    }
  }
}
