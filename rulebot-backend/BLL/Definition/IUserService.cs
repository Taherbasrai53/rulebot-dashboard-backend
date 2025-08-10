using rulebot_backend.Entities;

namespace rulebot_backend.BLL.Definition
{
    public interface IUserService
    {
        public LoginResponseDTO login(LoginDto req);
    }
}
