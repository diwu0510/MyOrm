using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Account { get; set; }

        [Required]
        public string Pw { get; set; }
    }
}
