// Helpers/TestAuthHelper.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public static class TestAuthHelper
{
    public static string GenerateToken(string userId, string role = "Customer")
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("80h9odGdWoRmwpaxGU605nd63YjEf0/mJKzfMQj8Zuw="));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: "Online_Shopping_System",
            audience: "Online_Shopping_System-client",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}