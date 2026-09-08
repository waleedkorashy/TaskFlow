export interface Label {
  id: string;
  projectId: string;
  name: string;
  colorHex: string;
}

export interface CreateLabelRequest {
  name: string;
  colorHex: string;
}