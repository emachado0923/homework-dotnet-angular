using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (await userRepository.ExistsByEmailAsync(dto.Email, cancellationToken))
        {
            return Result<AuthResponseDto>.Failure("A user with this email already exists.", ResultError.Conflict);
        }

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = passwordHasher.Hash(dto.Password)
        };

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        var token = jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(
            new AuthResponseDto(token.Value, user.Id, user.Email, user.FullName, token.ExpiresAt));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(dto.Email, cancellationToken);
        if (user is null || !passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Failure("Invalid email or password.", ResultError.Unauthorized);
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return Result<AuthResponseDto>.Success(
            new AuthResponseDto(token.Value, user.Id, user.Email, user.FullName, token.ExpiresAt));
    }
}
