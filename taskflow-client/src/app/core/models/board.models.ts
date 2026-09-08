export interface BoardColumn {
  id: string;
  name: string;
  sortOrder: number;
}

export interface Board {
  id: string;
  projectId: string;
  name: string;
  createdAt: string;
  columns: BoardColumn[];
}

export interface CreateBoardRequest {
  name: string;
}

export interface CreateColumnRequest {
  name: string;
}