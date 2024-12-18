using BnanApi.DTOS;

namespace BnanApi.Services.Email
{
    public interface IMailingService
    {
        Task<bool> SendEmailToBnan(EmailDTO request);
        Task<bool> SendEmailForCustomer(string EmailCustomer, string userName);
        bool IsValidEmail(string email);

    }
}
