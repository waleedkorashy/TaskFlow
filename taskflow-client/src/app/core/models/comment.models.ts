export interface Comment {
  id: string;
  taskItemId: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
}

export interface CreateCommentRequest {
  content: string;
}