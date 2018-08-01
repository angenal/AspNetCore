using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDotnetCore.db.sqlservr.yz.Models;

namespace WebApplication1.Pages.Movies
{
    public class EditModel : PageModel
    {
        private readonly YzDbContext _context;

        public EditModel(YzDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Movie).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(Movie.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="id">主键</param>
        /// <param name="movie">Bind特性是防止过度的一种方法，在Bind特性中包含想要更改的属性。</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]//ValidateAntiForgeryToken 特性用于防止请求伪造，并与编辑视图文件
                                  //(Views/Movies/Edit.cshtml) 中生成的防伪标记相配对。有关详细信息，请参阅反请求伪造。
                                  //编辑视图文件使用表单标记帮助程序生成防伪标记<form asp-action="Edit">
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Genre,Price,Score")]Movie movie)
        {
            this.Movie = movie;
            return await OnPostAsync();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.ID == id);
        }
    }
}
