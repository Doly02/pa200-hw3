using System.ComponentModel.DataAnnotations;

namespace aspnet_get_started.Models
{
    public class QueueRequestViewModel
    {
        [Required(ErrorMessage = "Subject je povinny.")]
        [StringLength(120, ErrorMessage = "Subject muze mit maximalne 120 znaku.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Message je povinna.")]
        [StringLength(4000, ErrorMessage = "Message muze mit maximalne 4000 znaku.")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Priority je povinna.")]
        public string Priority { get; set; } = "normal";
    }
}
