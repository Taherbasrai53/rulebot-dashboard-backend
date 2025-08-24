using Microsoft.IdentityModel.Tokens;
using rulebot_backend.BLL.Definition;
using rulebot_backend.DAL.Definition;
using rulebot_backend.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace rulebot_backend.BLL.Implementation
{
    public class UserService:IUserService
    {
        IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public LoginResponseDTO login(LoginDto req)
        {
            var connectionString = _userRepo.getClientConnectionString(req.RID);
            var user = _userRepo.ValidateLogin(req, connectionString);

            if (user == null)
            {
                throw new Exception("Invalid User");
            }
            var token = GenerateToken(user, req.RID);
            return new LoginResponseDTO
            {
                token = token,
                user = user,
                connectionString = connectionString
            };
        }

        private string GenerateToken(User user, string RID)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                 {
                    new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("userId", user.Id.ToString()),
                    new Claim("rid", RID)
                 };

                var token = new JwtSecurityToken
                    (
                    _config["Jwt:Issuer"],
                    _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(120),
                    signingCredentials: credentials

                    );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
