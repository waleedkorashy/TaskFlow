export interface ProjectSummary {
  id: string;
  name: string;
  createdAt: string;
  boardCount: number;
  taskCount: number;
  memberCount: number;
}

export interface DashboardStats {
  projectCount: number;
  boardCount: number;
  taskCount: number;
  recentProjects: ProjectSummary[];
  projects: ProjectSummary[];
}