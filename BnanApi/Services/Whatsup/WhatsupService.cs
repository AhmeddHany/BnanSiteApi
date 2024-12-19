using BnanApi.DTOS;
using BnanApi.Helper;
using Newtonsoft.Json;

namespace BnanApi.Services.Whatsup
{
    public class WhatsupService : IWhatsupService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string api = "http://207.180.229.2:3000";
        public WhatsupService()
        {

        }

        public async Task<string> SendMessageAsync(WhatsupDTO model)
        {
            var url = $"{api}/api/sendMessage_text";

            // إعداد البيانات بتنسيق x-www-form-urlencoded
            var formData = new Dictionary<string, string>
    {
        { "phone", model.Phone },
        { "message", model.Message },
        { "apiToken", "Bnan_fgfghgfhnbbbmhhjhgmghhgghhgj" }, // استبدل الـ Token بقيمة مناسبة
        { "id", "0000" }
    };

            var data = new FormUrlEncodedContent(formData);

            try
            {
                var response = await _httpClient.PostAsync(url, data);
                if (!response.IsSuccessStatusCode)
                    return ApiResponseStatus.Failure;

                var content = await response.Content.ReadAsStringAsync();
                var jsonResult = JsonConvert.DeserializeObject<dynamic>(content);

                // التحقق من الحالة
                if (jsonResult != null && (jsonResult.status == true || jsonResult.status.ToString().ToLower() == "true"))
                    return ApiResponseStatus.Success;

                return ApiResponseStatus.Failure;
            }
            catch (HttpRequestException)
            {
                return ApiResponseStatus.ServerError;
            }
            catch (Exception)
            {
                return ApiResponseStatus.ServerError;
            }
        }
    }
}
