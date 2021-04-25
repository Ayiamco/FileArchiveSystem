using archivesystemDomain.Entities;
using System.Threading.Tasks;

namespace archivesystemDomain.Services
{
    public interface IAccessCodeGenerator
    {
        Task<string> GenerateCode(AppUser user, string method);
        string HashCode(string code);
        string NewOTP();
        bool VerifyCode(string code, string hashedcode);
    }
}