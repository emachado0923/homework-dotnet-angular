namespace TaskManager.Application.DTOs;

public record LoginDto(string Email, string Password);

public record RegisterDto(string Email, string Password, string FullName);

public record AuthResponseDto(string Token, Guid UserId, string Email, string FullName, DateTime ExpiresAt);
