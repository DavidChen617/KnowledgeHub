export interface ApiResponse<T> {
  isSuccess: boolean;
  data?: T;
  problem?: {
    status: number;
    detail?: string;
  };
}

export interface Note {
  noteId: string;
  title: string;
  content: string;
  categoryId?: string;
  linkedNoteIds: string[];
  sharedToken?: string;
  updatedAt: string;
}

export interface NoteSummary {
  noteId: string;
  title: string;
  updatedAt: string;
  categoryId?: string;
}

export interface Category {
  id: string;
  name: string;
}

export interface Comment {
  commentId: string;
  content: string;
  username: string;
  avatarUrl?: string;
  parentCommentId?: string;
  createdAt: string;
}

export interface GraphNode {
  id: string;
  type: 'note' | 'category';
  label: string;
}

export interface GraphEdge {
  source: string;
  target: string;
  type: 'link' | 'category';
}

export interface NoteGraph {
  nodes: GraphNode[];
  edges: GraphEdge[];
}

export interface SearchResult {
  noteId: string;
  title: string;
  snippet: string;
}

export interface StructureResult {
  structureId: string;
  description: string;
  content: string;
}

export interface NoteStructureSummary {
  structureId: string;
  description: string;
  content: string;
  createdAt: string;
}

export interface UploadedImage {
  originalName: string;
  url: string;
}
