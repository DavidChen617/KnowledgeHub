import { marked } from 'marked';
import DOMPurify from 'dompurify';
import { createHighlighterCore } from 'shiki/core';
import { createOnigurumaEngine } from 'shiki/engine/oniguruma';

type Highlighter = Awaited<ReturnType<typeof createHighlighterCore>>;
let highlighterPromise: Promise<Highlighter> | null = null;

function getHighlighter(): Promise<Highlighter> {
  if (!highlighterPromise) {
    highlighterPromise = createHighlighterCore({
      themes: [import('shiki/themes/one-dark-pro.mjs')],
      langs: [
        import('shiki/langs/typescript.mjs'),
        import('shiki/langs/javascript.mjs'),
        import('shiki/langs/csharp.mjs'),
        import('shiki/langs/python.mjs'),
        import('shiki/langs/bash.mjs'),
        import('shiki/langs/json.mjs'),
        import('shiki/langs/markdown.mjs'),
        import('shiki/langs/html.mjs'),
        import('shiki/langs/css.mjs'),
      ],
      engine: createOnigurumaEngine(import('shiki/wasm')),
    });
  }
  return highlighterPromise;
}

const SUPPORTED_LANGS = new Set(['typescript', 'ts', 'javascript', 'js', 'csharp', 'cs', 'python', 'bash', 'sh', 'json', 'markdown', 'md', 'html', 'css']);

function preprocessWikilinks(content: string, noteMap?: Map<string, string>): string {
  return content.replace(/\[\[([^\[\]]+)\]\]/g, (_, id: string) => {
    const trimmed = id.trim();
    const title = noteMap?.get(trimmed) ?? (trimmed.slice(0, 8) + '…');
    return `<a href="/notes/${trimmed}" data-wiki-link="${trimmed}">${title}</a>`;
  });
}

export async function renderMarkdown(content: string, noteMap?: Map<string, string>): Promise<string> {
  const highlighter = await getHighlighter();
  const processedContent = preprocessWikilinks(content, noteMap);

  const renderer = new marked.Renderer();
  renderer.code = ({ text, lang }) => {
    const language = lang && SUPPORTED_LANGS.has(lang) ? lang : 'text';
    try {
      return highlighter.codeToHtml(text, { lang: language, theme: 'one-dark-pro' });
    } catch {
      return `<pre><code>${text}</code></pre>`;
    }
  };

  // Pass renderer directly to avoid mutating global marked state
  const html = await marked.parse(processedContent, { renderer, gfm: true, breaks: false });
  return DOMPurify.sanitize(html, { ADD_TAGS: ['style'], ADD_ATTR: ['style', 'data-wiki-link'] });
}
