import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateTaskDto, TaskDto, UpdateTaskDto } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly apiUrl = `${environment.apiUrl}/tasks`;
  readonly tasks = signal<TaskDto[]>([]);

  constructor(private readonly http: HttpClient) {}

  loadAll(): Observable<TaskDto[]> {
    return this.http
      .get<TaskDto[]>(this.apiUrl)
      .pipe(tap((tasks) => this.tasks.set(tasks)));
  }

  create(dto: CreateTaskDto): Observable<TaskDto> {
    return this.http
      .post<TaskDto>(this.apiUrl, dto)
      .pipe(tap((task) => this.tasks.update((current) => [task, ...current])));
  }

  update(id: string, dto: UpdateTaskDto): Observable<TaskDto> {
    return this.http
      .put<TaskDto>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        tap((task) =>
          this.tasks.update((current) => current.map((t) => (t.id === id ? task : t)))
        )
      );
  }

  delete(id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.apiUrl}/${id}`)
      .pipe(tap(() => this.tasks.update((current) => current.filter((t) => t.id !== id))));
  }
}
