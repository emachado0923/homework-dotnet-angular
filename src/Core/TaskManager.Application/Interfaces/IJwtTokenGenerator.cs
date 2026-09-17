using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public record JwtToken(string Value, DateTime ExpiresAt);

public interface IJwtTokenGenerator
{
    JwtToken GenerateToken(User user);
}
