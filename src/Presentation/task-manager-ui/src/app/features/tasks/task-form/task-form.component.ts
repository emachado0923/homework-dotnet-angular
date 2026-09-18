import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { TaskService } from '../../../core/services/task.service';
import { TaskDto, TaskItemStatus } from '../../../core/models/task.model';
import { extractErrorMessage } from '../../../core/utils/http-error.util';

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly taskService = inject(TaskService);

  readonly task = input<TaskDto | null>(null);
  readonly closed = output<void>();
  readonly saved = output<void>();

  readonly errorMessage = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly statuses = [
    { value: TaskItemStatus.Pending, label: 'Pending' },
    { value: TaskItemStatus.InProgress, label: 'In Progress' },
    { value: TaskItemStatus.Completed, label: 'Completed' }
  ];

  readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.maxLength(2000)]],
    dueDate: ['', [Validators.required]],
    status: [TaskItemStatus.Pending, [Validators.required]]
  });

  constructor() {
    effect(() => {
      const current = this.task();
      if (current) {
        this.form.patchValue({
          title: current.title,
          description: current.description,
          dueDate: current.dueDate.substring(0, 10),
          status: current.status
        });
      } else {
        this.form.reset({ title: '', description: '', dueDate: '', status: TaskItemStatus.Pending });
      }
    });
  }

  get isEditMode(): boolean {
    return !!this.task();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.isSubmitting.set(true);

    const { title, description, dueDate, status } = this.form.getRawValue();
    const dueDateIso = new Date(dueDate!).toISOString();
    const current = this.task();

    const request$ = current
      ? this.taskService.update(current.id, {
          title: title!,
          description: description ?? '',
          dueDate: dueDateIso,
          status: status!
        })
      : this.taskService.create({ title: title!, description: description ?? '', dueDate: dueDateIso });

    request$.subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.saved.emit();
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Could not save the task.'));
      }
    });
  }

  cancel(): void {
    this.closed.emit();
  }
}
