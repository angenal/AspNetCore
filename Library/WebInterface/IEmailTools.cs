using System.Threading.Tasks;

namespace WebInterface
{
    public interface IEmailTools
    {
        Task SendEmailAsync(string email, string subject, string content, bool html = true, params string[] cc);
    }
}
