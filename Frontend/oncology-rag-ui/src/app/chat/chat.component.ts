import { Component, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, SourceItem } from '../services/api.service';

interface Message {
  role: 'user' | 'assistant';
  text: string;
  sources?: SourceItem[];
  outOfScope?: boolean;
  outOfScopeReason?: string;
  streaming?: boolean;
}

interface TokenUsage {
  inputTokens: number;
  outputTokens: number;
}

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="chat-container">
      <!-- Messages -->
      <div class="messages" #messageList>
        <div class="welcome" *ngIf="messages.length === 0">
          <div class="welcome-icon">🧬</div>
          <h3>Oncology Knowledge Assistant</h3>
          <p>Ask questions about the indexed cancer treatment guidelines.</p>
          <div class="suggestions">
            <button *ngFor="let s of suggestions" (click)="sendSuggestion(s)">{{ s }}</button>
          </div>
        </div>

        <div *ngFor="let msg of messages" class="message" [class.user]="msg.role === 'user'" [class.assistant]="msg.role === 'assistant'">

          <div class="bubble" [class.out-of-scope]="msg.outOfScope">
            <!-- Out-of-scope warning -->
            <div class="scope-warning" *ngIf="msg.outOfScope">
              <span class="scope-icon">⚠️</span>
              <span>{{ msg.outOfScopeReason }}</span>
            </div>

            <div class="text" *ngIf="!msg.outOfScope">{{ msg.text }}<span class="cursor" *ngIf="msg.streaming">▌</span></div>

            <!-- Sources -->
            <div class="sources" *ngIf="msg.sources && msg.sources.length > 0">
              <div class="sources-label">Sources</div>
              <div class="source-chips">
                <a *ngFor="let s of msg.sources" [href]="s.url" target="_blank" rel="noopener" class="chip">
                  <span class="chip-file">{{ s.file }}</span>
                  <span class="chip-page">p.{{ s.page }}</span>
                  <span class="chip-score">{{ (s.score * 100).toFixed(0) }}%</span>
                </a>
              </div>
            </div>
          </div>

        </div>
      </div>

      <!-- Input -->
      <div class="input-area">
        <div class="input-row">
          <textarea
            #inputArea
            [(ngModel)]="inputText"
            placeholder="Ask an oncology question…"
            rows="1"
            [disabled]="streaming"
            (keydown.enter)="onEnter($event)"
            (input)="autoResize($event)"
          ></textarea>
          <button class="send-btn" (click)="send()" [disabled]="!inputText.trim() || streaming">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor"><path d="M2 21l21-9L2 3v7l15 2-15 2v7z"/></svg>
          </button>
        </div>

        <!-- Token usage bar -->
        <div class="token-bar">
          <span class="token-label">Tokens</span>
          <span class="token-item">Input: <strong>{{ usage?.inputTokens ?? '—' }}</strong></span>
          <span class="token-sep">·</span>
          <span class="token-item">Output: <strong>{{ usage?.outputTokens ?? '—' }}</strong></span>
          <span class="token-sep">·</span>
          <span class="token-item">Total: <strong>{{ usage ? (usage.inputTokens + usage.outputTokens) : '—' }}</strong></span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .chat-container {
      display: flex; flex-direction: column; height: 100%;
      background: #0f1117;
    }
    .messages {
      flex: 1; overflow-y: auto; padding: 24px 20px;
      display: flex; flex-direction: column; gap: 16px;
    }
    .welcome {
      display: flex; flex-direction: column; align-items: center;
      justify-content: center; height: 100%; text-align: center; gap: 12px;
      .welcome-icon { font-size: 48px; }
      h3 { font-size: 18px; font-weight: 600; color: #c8cfe8; }
      p { color: #6b7280; font-size: 14px; }
    }
    .suggestions {
      display: flex; flex-wrap: wrap; gap: 8px; justify-content: center; margin-top: 8px;
      button {
        background: #1a1d27; border: 1px solid #2d3145;
        border-radius: 20px; padding: 8px 14px;
        color: #9ca3c1; font-size: 12px; cursor: pointer;
        transition: all 0.2s;
        &:hover { border-color: #4f6ef7; color: #c8cfe8; }
      }
    }
    .message { display: flex; &.user { justify-content: flex-end; } &.assistant { justify-content: flex-start; } }
    .bubble {
      max-width: 75%; padding: 14px 16px; border-radius: 12px;
      font-size: 14px; line-height: 1.6;
      .user & { background: #4f6ef7; color: #fff; border-bottom-right-radius: 3px; }
      .assistant & { background: #1a1d27; border: 1px solid #2d3145; border-bottom-left-radius: 3px; }
      &.out-of-scope { background: #1f1510; border-color: #5c3317; }
    }
    .scope-warning {
      display: flex; gap: 8px; align-items: flex-start;
      color: #e0925a; font-size: 13px;
      .scope-icon { flex-shrink: 0; }
    }
    .cursor { animation: blink 1s step-start infinite; }
    @keyframes blink { 50% { opacity: 0; } }
    .sources { margin-top: 12px; padding-top: 12px; border-top: 1px solid #2d3145; }
    .sources-label { font-size: 11px; color: #5a6280; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px; }
    .source-chips { display: flex; flex-wrap: wrap; gap: 6px; }
    .chip {
      display: flex; align-items: center; gap: 5px;
      background: #0f1117; border: 1px solid #2d3145; border-radius: 6px;
      padding: 4px 8px; text-decoration: none; transition: border-color 0.2s;
      &:hover { border-color: #4f6ef7; }
      .chip-file { font-size: 11px; color: #9ca3c1; }
      .chip-page { font-size: 10px; color: #5a6280; }
      .chip-score { font-size: 10px; color: #4f8a4f; font-weight: 500; }
    }
    .input-area {
      border-top: 1px solid #1e2235; padding: 12px 20px 8px;
      background: #13151f;
    }
    .input-row {
      display: flex; align-items: flex-end; gap: 8px;
      background: #1a1d27; border: 1px solid #2d3145; border-radius: 10px; padding: 10px 12px;
      &:focus-within { border-color: #4f6ef7; }
      textarea {
        flex: 1; background: transparent; border: none; outline: none;
        color: #e8eaf0; font-size: 14px; font-family: inherit;
        resize: none; max-height: 120px; line-height: 1.5;
        &::placeholder { color: #3e4459; }
        &:disabled { opacity: 0.5; }
      }
    }
    .send-btn {
      background: #4f6ef7; border: none; border-radius: 6px;
      padding: 6px 8px; color: #fff; cursor: pointer;
      display: flex; align-items: center; transition: background 0.2s;
      flex-shrink: 0;
      &:hover:not(:disabled) { background: #3a58e3; }
      &:disabled { opacity: 0.4; cursor: not-allowed; }
    }
    .token-bar {
      display: flex; align-items: center; gap: 8px;
      padding: 6px 4px 2px; font-size: 11px; color: #3e4459;
      .token-label { color: #2d3145; font-size: 10px; text-transform: uppercase; letter-spacing: 0.5px; margin-right: 2px; }
      .token-item { color: #5a6280; strong { color: #7b8099; } }
      .token-sep { color: #2d3145; }
    }
  `]
})
export class ChatComponent implements AfterViewChecked {
  @ViewChild('messageList') messageList!: ElementRef<HTMLDivElement>;

  messages: Message[] = [];
  inputText = '';
  streaming = false;
  usage: TokenUsage | null = null;

  suggestions = [
    'What are the treatment options for stage 2 breast cancer?',
    'What are the lung cancer screening recommendations?',
    'What side effects are associated with chemotherapy?'
  ];

  private shouldScroll = false;

  constructor(private api: ApiService) {}

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      const el = this.messageList.nativeElement;
      el.scrollTop = el.scrollHeight;
      this.shouldScroll = false;
    }
  }

  sendSuggestion(text: string): void {
    this.inputText = text;
    this.send();
  }

  onEnter(event: KeyboardEvent): void {
    if (!event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  autoResize(event: Event): void {
    const el = event.target as HTMLTextAreaElement;
    el.style.height = 'auto';
    el.style.height = Math.min(el.scrollHeight, 120) + 'px';
  }

  send(): void {
    const text = this.inputText.trim();
    if (!text || this.streaming) return;

    this.messages.push({ role: 'user', text });
    this.inputText = '';
    this.usage = null;
    this.streaming = true;
    this.shouldScroll = true;

    const assistantMsg: Message = { role: 'assistant', text: '', streaming: true };
    this.messages.push(assistantMsg);

    const stream$ = this.api.streamChat(text) as unknown as AsyncIterable<import('../services/api.service').ChatEvent>;

    (async () => {
      for await (const event of stream$) {
        switch (event.type) {
          case 'bounds_check':
            if (!event.inScope) {
              assistantMsg.outOfScope = true;
              assistantMsg.outOfScopeReason = event.reason ?? 'Out of scope.';
            }
            break;
          case 'sources':
            assistantMsg.sources = event.items;
            break;
          case 'token':
            assistantMsg.text += event.text ?? '';
            this.shouldScroll = true;
            break;
          case 'usage':
            this.usage = { inputTokens: event.inputTokens ?? 0, outputTokens: event.outputTokens ?? 0 };
            break;
          case 'done':
            assistantMsg.streaming = false;
            this.streaming = false;
            break;
          case 'error':
            assistantMsg.text = event.message ?? 'An error occurred.';
            assistantMsg.streaming = false;
            this.streaming = false;
            break;
        }
      }
      assistantMsg.streaming = false;
      this.streaming = false;
    })();
  }
}
