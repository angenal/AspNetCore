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
    public class IndexModel : PageModel
    {
        private readonly YzDbContext _context;

        public IndexModel(YzDbContext context)
        {
            _context = context;
        }

        public IList<Movie> Movie { get;set; }

        //public void OnGet()
        //{
        //    Movie = _context.Movies.AsNoTracking().ToList();
        //}
        public async Task OnGetAsync()
        {
            Movie = await _context.Movies.ToListAsync();
        }
    }
}
