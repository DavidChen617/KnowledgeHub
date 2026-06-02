import {
  Component, ElementRef, ViewChild, inject, signal,
  OnInit, OnDestroy, AfterViewInit, ChangeDetectionStrategy,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import ForceGraph, { ForceGraphGeneric } from 'force-graph';
import { forceX, forceY } from 'd3-force';
import { NoteService } from '../../core/api/note.service';
import { SpinnerComponent } from '../../shared/components/spinner';

@Component({
  selector: 'knowledge-graph',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, SpinnerComponent],
  template: `
    <div class="h-screen bg-[var(--color-bg)] flex flex-col overflow-hidden">

      <header class="border-b border-[var(--color-border)] px-6 h-12 flex items-center justify-between shrink-0">
        <a routerLink="/home" class="font-display text-xs font-bold tracking-widest text-[var(--color-accent)] uppercase">
          KnowledgeHub
        </a>
        <div class="flex items-center gap-4">
          <span class="font-mono text-xs text-[var(--color-muted)]">{{ nodeCount() }} 節點 · {{ edgeCount() }} 連結</span>
          <a routerLink="/notes" class="flex items-center gap-1.5 text-xs text-[var(--color-muted)] hover:text-[var(--color-text)] transition-colors">
            ← 返回筆記
          </a>
        </div>
      </header>

      <div #container class="flex-1 relative overflow-hidden">
        @if (loading()) {
          <div class="absolute inset-0 flex items-center justify-center">
            <spinner size="lg" />
          </div>
        }
      </div>

    </div>
  `,
})
export class KnowledgeGraphComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('container', { static: true }) containerRef!: ElementRef<HTMLDivElement>;

  private readonly noteService = inject(NoteService);
  private readonly router = inject(Router);

  readonly loading = signal(true);
  readonly nodeCount = signal(0);
  readonly edgeCount = signal(0);

  private graph?: InstanceType<typeof ForceGraph>;
  private viewReady = false;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private pendingData?: { nodes: any[]; links: any[] };

  async ngOnInit() {
    const data = await this.noteService.graph();
    const nodeIds = new Set(data.nodes.map(n => n.id));

    const nodes = data.nodes.map(n => ({ id: n.id, label: n.label, type: n.type }));
    const links = data.edges
      .filter(e => nodeIds.has(e.source) && nodeIds.has(e.target))
      .map(e => ({ source: e.source, target: e.target }));

    this.nodeCount.set(nodes.length);
    this.edgeCount.set(links.length);
    this.loading.set(false);

    const graphData = { nodes, links };
    if (this.viewReady) {
      this.initGraph(graphData);
    } else {
      this.pendingData = graphData;
    }
  }

  ngAfterViewInit() {
    this.viewReady = true;
    if (this.pendingData) {
      // Wait one frame so the container has its real dimensions
      requestAnimationFrame(() => this.initGraph(this.pendingData!));
    }
  }

  ngOnDestroy() {
    this.graph?._destructor();
  }

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  private initGraph(data: { nodes: any[]; links: any[] }) {
    const el = this.containerRef.nativeElement;

    this.graph = new ForceGraph(el)
      .graphData(data)
      .width(el.clientWidth)
      .height(el.clientHeight)
      .backgroundColor('transparent')
      .nodeId('id')
      .linkSource('source')
      .linkTarget('target')
      .nodeVal((n: any) => n.type === 'category' ? 8 : 3)
      .nodeColor((n: any) => n.type === 'category' ? '#6ee7b7' : '#64748b')
      .nodeLabel((n: any) => n.label)
      .nodeCanvasObjectMode(() => 'after')
      .nodeCanvasObject((n: any, ctx: CanvasRenderingContext2D, scale: number) => {
        const label = n.label?.length > 18 ? n.label.slice(0, 17) + '…' : n.label;
        const fontSize = 10 / scale;
        const nodeR = (n.type === 'category' ? 11 : 7);
        ctx.font = `${fontSize}px "JetBrains Mono", monospace`;
        ctx.fillStyle = '#94a3b8';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'top';
        ctx.fillText(label, n.x, n.y + nodeR / scale + 2 / scale);
      })
      .linkColor(() => '#2d2d3a')
      .linkWidth(1)
      .linkDirectionalParticles(2)
      .linkDirectionalParticleSpeed(0.004)
      .linkDirectionalParticleWidth(2)
      .linkDirectionalParticleColor(() => '#539bf5')
      .onNodeClick((n: any) => {
        if (n.type === 'note') {
          this.router.navigate(['/notes', n.id.replace('note-', '')]);
        }
      })
      // keep simulation warm so nodes keep gently floating
      .d3AlphaDecay(0.01)
      .d3VelocityDecay(0.3)
      .d3Force('x', forceX(0).strength(0.05))
      .d3Force('y', forceY(0).strength(0.05));

    // Resize on container size change
    new ResizeObserver(() => {
      this.graph?.width(el.clientWidth).height(el.clientHeight);
    }).observe(el);
  }
}
