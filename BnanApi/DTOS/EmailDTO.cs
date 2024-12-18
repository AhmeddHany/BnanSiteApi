using System.ComponentModel.DataAnnotations;

namespace BnanApi.DTOS
{
    public class EmailDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        public string? Body { get; set; }
    }
}
