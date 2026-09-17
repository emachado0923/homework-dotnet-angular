import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { TaskService } from '../../../core/services/task.service';
import { TaskDto, TaskFilter, TaskItemStatus } from '../../../core/models/task.model';
import { TaskFormComponent } from '../task-form/task-form.component';

@Component({
  selector: 'app-task-list',
  imports: [TaskFormComponent, DatePipe],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit {
  readonly TaskItemStatus = TaskItemStatus;
  readonly filter = signal<TaskFilter>('All');
  readonly isFormOpen = signal(false);
  readonly editingTask = signal<TaskDto | null>(null);

  readonly filteredTasks = computed(() => {
    const tasks = this.taskService.tasks();
    const current = this.filter();
    if (current === 'All') {
      return tasks;
    }
    const statusMap: Record<Exclude<TaskFilter, 'All'>, TaskItemStatus> = {
      Pending: TaskItemStatus.Pending,
      InProgress: TaskItemStatus.InProgress,
      Completed: TaskItemStatus.Completed
    };
    return tasks.filter((t) => t.status === statusMap[current]);
  });

  constructor(private readonly taskService: TaskService) {}

  ngOnInit(): void {
    this.taskService.loadAll().subscribe();
  }

  setFilter(filter: TaskFilter): void {
    this.filter.set(filter);
  }

  openCreateForm(): void {
    this.editingTask.set(null);
    this.isFormOpen.set(true);
  }

  openEditForm(task: TaskDto): void {
    this.editingTask.set(task);
    this.isFormOpen.set(true);
  }

  closeForm(): void {
    this.isFormOpen.set(false);
    this.editingTask.set(null);
  }

  onSaved(): void {
    this.closeForm();
  }

  deleteTask(task: TaskDto): void {
    if (confirm(`Delete "${task.title}"?`)) {
      this.taskService.delete(task.id).subscribe();
    }
  }

  statusLabel(status: TaskItemStatus): string {
    return TaskItemStatus[status];
  }
}
