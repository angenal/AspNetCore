using System.ComponentModel.DataAnnotations;

namespace Chat.Web.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        public string Name { get; set; }
    }
}
