using TaskFlow.Api.Entities;

namespace TaskFlow.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
