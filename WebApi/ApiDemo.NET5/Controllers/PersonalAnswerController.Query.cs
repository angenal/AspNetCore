using ApiDemo.NET5.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApiDemo.NET5.Controllers
{
    public partial class PersonalAnswerController
    {
        /// <summary>
        /// 查询
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            var now = DateTime.Now;

            //if (env.IsDevelopment()) db.Debug();

            var id = 1;
            var first1 = await db.Queryable<PersonalAnswer>().FirstAsync(q => q.Id == id);
            if (first1 == null)
            {
                first1 = new PersonalAnswer()
                {
                    Ip = ip,
                    Year = now.Year,
                    Sex = "A",
                    Age = "A",
                    City = "A",
                    Province = "A",
                    Education = "A",
                    University = "毕业院校",
                    Title = "A",
                    Political = "A",
                    WorkAge = "A",
                    WorkNature = "A",
                    WorkCategory = "A",
                    HasCertificate = "A",
                    WorkTraining = "A",
                    PersonTraining = "A",
                    MonthTrainingTime = "A",
                    YearTrainingTime = "A",
                    YearTrainingMoney = "A",
                    YearSalary = "A",
                    LikeSalary = "A",
                    KnowPoliciesMeasures = "A",
                    LikePoliciesMeasures = "A",
                    FollowAreas = "A",
                    FollowAreaOther = "",
                    HistoryRecords = 0,
                };
                await db.Insertable(first1).ExecuteCommandIdentityIntoEntityAsync();
            }

            id = first1.Id;

            var entity = await db.Queryable("PersonalAnswer", "t1")
                .Where("t1.Id=@id", new { id })
                .Select<PersonalAnswerModel1>("t1.Id,t1.Ip,t1.Title")
                .FirstAsync();

            entity = await db.Queryable<PersonalAnswer>()
                .Where(q => q.Id == id)
                .Select(q => new PersonalAnswerModel1 { Id = q.Id, Ip = q.Ip, Title = q.Title })
                .FirstAsync();

            entity = await db.Queryable<PersonalAnswer>()
                .Where(q => q.Id == id)
                .Select<PersonalAnswerModel1>("Id,Ip,Title")
                .FirstAsync();

            first1.Ip = ip;
            first1.Title = "B";
            var i = await db.Updateable(first1).ExecuteCommandAsync();

            await db.Updateable(first1).UpdateColumns(q => new { q.Ip, q.Title }).ExecuteCommandAsync();

            await db.Updateable(new PersonalAnswer { Id = id, Ip = ip, Title = "C" }).UpdateColumns(q => new { q.Ip, q.Title }).ExecuteCommandAsync();

            await db.Updateable(new PersonalAnswer { Id = id, Ip = ip, Title = "D" }).UpdateColumns(q => new { q.Ip, q.Title }).WhereColumns(q => new { q.Id }).ExecuteCommandAsync();

            await db.Updateable<PersonalAnswer>().SetColumns(q => new PersonalAnswer { Ip = first1.Ip, Title = first1.Title }).Where(q => q.Id == id).ExecuteCommandAsync();

            await db.Deleteable<PersonalAnswer>().In(id).ExecuteCommandAsync();

            await db.Deleteable<PersonalAnswer>().Where(q => q.Id == id).ExecuteCommandAsync();

            return Ok();
        }
    }
}
