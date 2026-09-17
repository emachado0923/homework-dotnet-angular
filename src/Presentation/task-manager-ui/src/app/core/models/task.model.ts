export enum TaskItemStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2
}

export interface TaskDto {
  id: string;
  title: string;
  description: string;
  status: TaskItemStatus;
  dueDate: string;
  createdAt: string;
  isOverdue: boolean;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  status: TaskItemStatus;
  dueDate: string;
}

export type TaskFilter = 'All' | 'Pending' | 'InProgress' | 'Completed';
