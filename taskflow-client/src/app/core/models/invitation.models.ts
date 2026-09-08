export interface Invitation {
  id: string;
  projectId: string;
  projectName: string;
  email: string;
  role: string;
  status: string;
  createdAt: string;
  expiresAt: string;
}

export interface CreateInvitationRequest {
  email: string;
  role?: string;
}

export interface InvitationPreview {
  projectName: string;
  inviterName: string;
  email: string;
  isValid: boolean;
}