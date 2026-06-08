import {
  Component, ElementRef, ViewChild, input, output,
  OnInit, OnDestroy, OnChanges, SimpleChanges, ChangeDetectionStrategy,
} from '@angular/core';
import { EditorView, keymap, lineNumbers } from '@codemirror/view';
import { EditorState, EditorSelection } from '@codemirror/state';
import { defaultKeymap, history, historyKeymap } from '@codemirror/commands';
import { markdown } from '@codemirror/lang-markdown';
import { autocompletion, CompletionContext, CompletionResult } from '@codemirror/autocomplete';

const khTheme = EditorView.theme({
  '&': {
    height: '100%',
    backgroundColor: 'transparent',
    fontSize: '14px',
    fontFamily: '"JetBrains Mono", monospace',
  },
  '.cm-content': {
    padding: '20px 24px',
    caretColor: 'var(--color-accent)',
    color: 'var(--color-text)',
    lineHeight: '1.75',
  },
  '.cm-line': { padding: '0' },
  '.cm-cursor': { borderLeftColor: 'var(--color-accent)' },
  '.cm-selectionBackground, ::selection': {
    backgroundColor: 'rgba(83, 155, 245, 0.2) !important',
  },
  '.cm-gutters': {
    backgroundColor: 'var(--color-surface)',
    borderRight: '1px solid var(--color-border)',
    color: 'var(--color-muted)',
    minWidth: '44px',
    paddingRight: '8px',
    fontSize: '12px',
  },
  '.cm-activeLineGutter': { backgroundColor: 'var(--color-surface-2)' },
  '.cm-activeLine': { backgroundColor: 'rgba(255,255,255,0.03)' },
  '&.cm-focused .cm-selectionBackground': { backgroundColor: 'rgba(83, 155, 245, 0.15) !important' },
  '.cm-scroller': { fontFamily: '"JetBrains Mono", monospace' },
}, { dark: true });

@Component({
  selector: 'md-editor',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div #host class="h-full w-full overflow-hidden"></div>`,
})
export class MdEditorComponent implements OnInit, OnDestroy, OnChanges {
  @ViewChild('host', { static: true }) hostRef!: ElementRef<HTMLDivElement>;

  readonly value = input('');
  readonly valueChange = output<string>();
  readonly imageDropped = output<File>();
  readonly notes = input<{ noteId: string; title: string }[]>([]);

  private view?: EditorView;

  ngOnInit(): void {
    const wikiLinkSource = (context: CompletionContext): CompletionResult | null => {
      const match = context.matchBefore(/\[\[[^\]]*$/);
      if (!match) return null;

      const search = match.text.slice(2).toLowerCase();
      const options = this.notes()
        .filter(n => n.title.toLowerCase().includes(search))
        .map(n => ({
          label: n.title,
          apply: (view: EditorView, _: unknown, from: number, to: number) => {
            view.dispatch({
              changes: { from: match.from, to, insert: `[[${n.noteId}]]` },
              selection: { anchor: match.from + n.noteId.length + 4 },
            });
          },
        }));

      return { from: match.from + 2, options, validFor: /^[^\]]*$/ };
    };

    this.view = new EditorView({
      state: EditorState.create({
        doc: this.value(),
        extensions: [
          history(),
          keymap.of([...defaultKeymap, ...historyKeymap]),
          markdown(),
          lineNumbers(),
          khTheme,
          EditorView.lineWrapping,
          autocompletion({ override: [wikiLinkSource], closeOnBlur: true }),
          EditorView.updateListener.of(update => {
            if (update.docChanged) {
              this.valueChange.emit(update.state.doc.toString());
            }
          }),
        ],
      }),
      parent: this.hostRef.nativeElement,
    });

    this.view.dom.addEventListener('drop', (e: DragEvent) => {
      const images = Array.from(e.dataTransfer?.files ?? []).filter(f => f.type.startsWith('image/'));
      if (images.length === 0) return;
      e.preventDefault();
      e.stopPropagation();
      images.forEach(f => this.imageDropped.emit(f));
    }, { capture: true });

    this.view.dom.addEventListener('paste', (e: ClipboardEvent) => {
      const images = Array.from(e.clipboardData?.files ?? []).filter(f => f.type.startsWith('image/'));
      if (images.length === 0) return;
      e.preventDefault();
      images.forEach(f => this.imageDropped.emit(f));
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.view || !changes['value']) return;
    const incoming = changes['value'].currentValue as string;
    const current = this.view.state.doc.toString();
    if (incoming !== current) {
      this.view.dispatch({
        changes: { from: 0, to: current.length, insert: incoming },
      });
    }
  }

  ngOnDestroy(): void {
    this.view?.destroy();
  }

  insertText(text: string) {
    if (!this.view) return;
    const { head } = this.view.state.selection.main;
    this.view.dispatch({
      changes: { from: head, insert: text },
      selection: { anchor: head + text.length },
    });
    this.view.focus();
  }

  wrapSelection(before: string, after: string, placeholder: string) {
    if (!this.view) return;
    this.view.dispatch(this.view.state.changeByRange(range => {
      if (range.empty) {
        const text = before + placeholder + after;
        return {
          changes: { from: range.from, insert: text },
          range: EditorSelection.range(range.from + before.length, range.from + before.length + placeholder.length),
        };
      }
      return {
        changes: [
          { from: range.from, insert: before },
          { from: range.to, insert: after },
        ],
        range: EditorSelection.range(range.from + before.length, range.to + before.length),
      };
    }));
    this.view.focus();
  }

  insertLinePrefix(prefix: string) {
    if (!this.view) return;
    const state = this.view.state;
    this.view.dispatch(state.changeByRange(range => {
      const line = state.doc.lineAt(range.head);
      const lineText = state.doc.sliceString(line.from, line.to);
      if (lineText.startsWith(prefix)) {
        return {
          changes: { from: line.from, to: line.from + prefix.length, insert: '' },
          range: EditorSelection.cursor(Math.max(line.from, range.head - prefix.length)),
        };
      }
      return {
        changes: { from: line.from, insert: prefix },
        range: EditorSelection.cursor(range.head + prefix.length),
      };
    }));
    this.view.focus();
  }

  insertBlock(text: string, innerOffset?: number) {
    if (!this.view) return;
    const { head } = this.view.state.selection.main;
    const line = this.view.state.doc.lineAt(head);
    const needsNewline = head > line.from;
    const prefix = needsNewline ? '\n' : '';
    const insert = prefix + text;
    const cursor = head + prefix.length + (innerOffset ?? text.length);
    this.view.dispatch({
      changes: { from: head, insert },
      selection: EditorSelection.cursor(cursor),
    });
    this.view.focus();
  }
}
