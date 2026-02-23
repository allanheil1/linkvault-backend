using LinkVault.Application.Common.Models;
using LinkVault.Domain.Entities;

namespace LinkVault.Application.Common.Interfaces;

public interface ITokenService
{
    AccessToken CreateAccessToken(User user);
    RefreshTokenResult CreateRefreshToken();
}
