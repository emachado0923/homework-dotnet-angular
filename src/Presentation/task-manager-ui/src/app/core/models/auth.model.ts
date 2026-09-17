export interface LoginDto {
  email: string;
  password: string;
}

export interface RegisterDto {
  email: string;
  password: string;
  fullName: string;
}

export interface AuthResponseDto {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  expiresAt: string;
}
