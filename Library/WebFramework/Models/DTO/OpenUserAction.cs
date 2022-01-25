using System.Collections.Generic;

namespace WebFramework.Models.DTO
{
    public class OpenUserAction
    {
        public OpenUserAction() { }

        public string Key { get; set; }
        public string Prompt { get; set; }
        public string AssignedPrincipal { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
}
