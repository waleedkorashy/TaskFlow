using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Api.Entities;

/// <summary>
/// Represents an application user authenticated via ASP.NET Core Identity.
/// </summary>
public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}
