using BnanApi.DTOS.RenterInfoWithFiles;

namespace BnanApi.DTOS
{
    public class ApiRenterResponse<T>
    {
        public string Status { get; set; } // Success or Error
        public int Code { get; set; } // HTTP status code (e.g., 200, 400, 404)
        public string Message { get; set; } // Message explaining the response
        public RenterInfoDTO RenterInfo { get; set; } // The actual data being returned (generic)
        public T Data { get; set; } // The actual data being returned (generic)
        public object Errors { get; set; } // Optional: Details about any errors
    }
}
