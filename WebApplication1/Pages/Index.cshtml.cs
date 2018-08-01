using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; private set; } = " Model.Message In C# ";

        public void OnGet()
        {
            this.Message += $"Now is {DateTime.Now.ToLocalTime()}";
        }
    }
}
