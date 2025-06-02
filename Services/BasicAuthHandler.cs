using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace CrmSystem.Api.Services;

public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) // ⚠️ да, устаревший, но обязателен
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

        var header = Request.Headers["Authorization"].ToString();
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));

        var encoded = header.Substring("Basic ".Length).Trim();
        string decoded;

        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Base64 Encoding"));
        }

        var parts = decoded.Split(':');
        if (parts.Length != 2)
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic Auth Format"));

        var username = parts[0];
        var password = parts[1];

        if (username != "admin" || password != "admin123")
            return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}