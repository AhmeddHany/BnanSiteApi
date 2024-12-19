using BnanApi.DTOS;

namespace BnanApi.Services.Whatsup
{
    public interface IWhatsupService
    {
        Task<string> SendMessageAsync(WhatsupDTO model);

    }
}
