export interface Project {
  id: string;
  name: string;
  description: string | null;
  ownerId: string;
  ownerName: string;
  createdAt: string;
  memberCount: number;
}

export interface CreateProjectRequest {
  name: string;
  description: string | null;
}

export interface ProjectMember {
  userId: string;
  fullName: string;
  email: string;
  role: string;
}