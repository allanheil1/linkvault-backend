using LinkVault.Application.Common.Interfaces;
using LinkVault.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkVault.Application.Features.Auth.Commands;

public record RegisterCommand(string Name, string Email, string Password) : IRequest<AuthUserDto>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthUserDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _clock;

    public RegisterCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher, IDateTimeProvider clock)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<AuthUserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailLower = request.Email.Trim().ToLowerInvariant();
        var exists = await _context.Users.AnyAsync(u => u.Email == emailLower, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = emailLower,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = _clock.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthUserDto(user.Id, user.Name, user.Email);
    }
}
