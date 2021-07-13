using System.Threading.Tasks;
using WebInterface;

namespace WebCore
{
    public class EmailTools : IEmailTools
    {
        public Task SendEmailAsync(string email, string subject, string message) => Task.CompletedTask;
    }
}
