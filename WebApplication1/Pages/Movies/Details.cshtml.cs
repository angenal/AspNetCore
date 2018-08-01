using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDotnetCore.db.sqlservr.yz.Models;

namespace WebApplication1.Pages.Movies
{
    public class DetailsModel : PageModel
    {
        private readonly YzDbContext _context;

        public DetailsModel(YzDbContext context)
        {
            _context = context;
        }

        public Movie Movie { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Movie = await _context.Movies.FindAsync(id);
            Movie = await _context.Movies.SingleOrDefaultAsync(m => m.ID == id);

            if (Movie == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
