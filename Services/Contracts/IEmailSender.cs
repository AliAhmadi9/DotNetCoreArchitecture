using Common;
using System.Threading.Tasks;

namespace Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(MailRequest mailRequest);
    }
}