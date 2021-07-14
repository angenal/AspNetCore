using System.ComponentModel.DataAnnotations;

namespace Chat.Web.ViewModels
{
    public class MessageViewModel
    {
        [Required]
        [StringLength(500)]
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string From { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Room { get; set; }
        public string Avatar { get; set; }
    }
}
