import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, LoginDto, RegisterDto } from '../models/auth.model';

const STORAGE_KEY = 'task-manager-auth';

interface StoredAuth {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  expiresAt: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/auth`;
  private readonly authState = signal<StoredAuth | null>(this.readFromStorage());

  readonly currentUser = computed(() => this.authState());
  readonly isAuthenticated = computed(() => {
    const auth = this.authState();
    return !!auth && new Date(auth.expiresAt) > new Date();
  });

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router
  ) {}

  register(dto: RegisterDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.apiUrl}/register`, dto)
      .pipe(tap((response) => this.setSession(response)));
  }

  login(dto: LoginDto): Observable<AuthResponseDto> {
    return this.http
      .post<AuthResponseDto>(`${this.apiUrl}/login`, dto)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.authState.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return this.authState()?.token ?? null;
  }

  private setSession(response: AuthResponseDto): void {
    const stored: StoredAuth = response;
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    this.authState.set(stored);
  }

  private readFromStorage(): StoredAuth | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredAuth) : null;
    } catch {
      return null;
    }
  }
}
