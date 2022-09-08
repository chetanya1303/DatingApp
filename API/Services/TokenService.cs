using API.Interfaces;
using API.Entities;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace API.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly UserManager<AppUser> _userManager;
        public TokenService(IConfiguration config,UserManager<AppUser> userManager)
        {
          _userManager = userManager;
          _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }

        public async Task<string> CreateToken(AppUser user)
        {
          var claims = new List<Claim>
          {
             new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
             new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName.ToString())
          };
          var roles = await _userManager.GetRolesAsync(user);
          claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role,role)));

          var creds = new SigningCredentials(_key,SecurityAlgorithms.HmacSha512Signature);
          var tokenDescriptor = new SecurityTokenDescriptor
          {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds
          };

          var tokenHandler = new JwtSecurityTokenHandler();
          var token = tokenHandler.CreateJwtSecurityToken(tokenDescriptor);
          return tokenHandler.WriteToken(token);
        }
    }
}