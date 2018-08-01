using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDotnetCore.db.sqlservr.yz;
using WebDotnetCore.db.sqlservr.yz.Models;
using WebFramework.Extensions;
using WebFramework.WebHost;

namespace WebApplication1.Pages.Movies
{
    public class ScheduleModel : PageModel
    {
        private readonly YzDbContext _context;

        public ScheduleModel([FromServices]YzDbContext context)
        {
            _context = context;
        }

        [TempData]
        public int MovieID { get; set; }
        [BindProperty]
        public FileUpload FileUpload { get; set; }
        public IList<MovieSchedule> MovieSchedules { get; set; }


        const string FileUploadDir = "/wwwroot/upload/movie/";


        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="ID">当前为路由ID，如果传参数可用指令 @page "{id:int}" </param>
        /// <returns></returns>
        public async Task OnGetAsync(int ID)
        {
            MovieID = ID;
            await OnDataLoad();
        }
        internal async Task OnDataLoad()
        {
            var q = from ms in _context.MovieSchedules where ms.MovieID.Equals(MovieID) select ms;
            //q = q.Where(ms => ms.MovieID.Equals(MovieID));
            MovieSchedules = await q.AsNoTracking().ToListAsync();
        }

        public async Task<IActionResult> OnPostDelAsync(int id)
        {
            var o = await _context.MovieSchedules.FindAsync(id);
            if (o != null)
            {
                //删除数据
                _context.MovieSchedules.Remove(o);
                await _context.SaveChangesAsync();
                //删除图片
                var fImage = FileUploadDir + o.UploadImage.Substring(o.UploadImage.LastIndexOf('/'));
                var _fImage = System.IO.Path.Combine(Environment.CurrentDirectory, fImage.TrimStart('/'));
                System.IO.File.Delete(_fImage);
            }
            return Redirect($"?id={MovieID}");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string fContent = "";
                bool requiredUploadSchedule = Attr<MovieSchedule>.RequiredFor(o => o.UploadSchedule) != null;
                if (requiredUploadSchedule || FileUpload.TxtFile.Length > 0)
                    fContent = await PostFormFile<FileUpload>.GetUploadedTextFile(ModelState, FileUpload.TxtFile, Attr<MovieSchedule>.StringLengthFor(o => o.UploadSchedule)?.MaximumLength ?? 0);

                if (ModelState.IsValid)
                {
                    string fImage = "";
                    bool requiredUploadImage = Attr<MovieSchedule>.RequiredFor(o => o.UploadImage) != null;
                    if (requiredUploadImage || FileUpload.ImgFile.Length > 0)
                    {
                        fImage = $"{FileUploadDir}{Guid.NewGuid().ToString("N")}.{FileUpload.ImgFile.FileName.Split('.').LastOrDefault()}";
                        var _fImage = System.IO.Path.Combine(Environment.CurrentDirectory, fImage.TrimStart('/'));
                        var _fDir = new System.IO.DirectoryInfo(System.IO.Path.GetDirectoryName(_fImage));
                        if (!_fDir.Exists) _fDir.Create();//需要配置权限
                        await PostFormFile<FileUpload>.SaveUploadedImageFile(ModelState, FileUpload.ImgFile, _fImage, Attr<MovieSchedule>.FileSizeFor(o => o.UploadImage)?.Sizes());
                        fImage = fImage.Substring(8);
                    }
                    if (ModelState.IsValid)
                    {
                        var entity = new MovieSchedule()
                        {
                            MovieID = MovieID,
                            Title = FileUpload.Title,
                            UploadSchedule = fContent,
                            UploadImage = fImage,
                            UploadDT = DateTime.Now.ToLocalTime()
                        };
                        _context.MovieSchedules.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            await OnDataLoad();
            return Page();
        }

    }
}