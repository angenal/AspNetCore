using System.ComponentModel.DataAnnotations;

namespace Chat.Web.ViewModels
{
    public class UserViewModel
    {
        [StringLength(30, MinimumLength = 2, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        public string Username { get; set; }
        [StringLength(30, MinimumLength = 2, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.")]
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }
    }
}
