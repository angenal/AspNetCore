using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using Microsoft.Extensions.Options;
using System;
using System.Text;
using WebInterface.Settings;

namespace ApiDemo.NET5.Models
{
    public sealed class TestDbContext : DbContext
    {
        private readonly byte[] _encryptionIV;
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionSalt;

        private readonly ApiSettings _appSettings;

        private readonly IHttpContextAccessor _context;
        private readonly IEncryptionProvider _provider;
        private readonly IDataProtector _protector;

        public TestDbContext(
            DbContextOptions options,
            IOptions<AesSettings> aesSettings,
            IOptions<ApiSettings> appSettings,
            IHttpContextAccessor context,
            IDataProtectionProvider provider)
            : base(options)
        {
            _encryptionIV = Encoding.Unicode.GetBytes(Environment.GetEnvironmentVariable("AES_IV") ?? aesSettings.Value.IV);
            _encryptionKey = Encoding.Unicode.GetBytes(Environment.GetEnvironmentVariable("AES_Key") ?? aesSettings.Value.Key);
            _encryptionSalt = Encoding.Unicode.GetBytes(Environment.GetEnvironmentVariable("AES_Salt") ?? aesSettings.Value.Salt);

            _appSettings = appSettings?.Value;

            _context = context;
            _provider = new AesProvider(_encryptionKey, _encryptionIV);
            _protector = provider?.CreateProtector($"{nameof(ApiSettings)}.{nameof(Sid)}");

            if (_context.HttpContext != null)
            {
                string sid = null;
                if (_context.HttpContext.Request.Headers.TryGetValue("X-Request-Sid", out var s1)) sid = s1.ToString();
                else if (_context.HttpContext.Request.Query.TryGetValue("sid", out var s2)) sid = s2.ToString();
                Sid = string.IsNullOrEmpty(sid) ? Guid.Empty : Guid.Parse(_protector.Unprotect(sid));
            }

            // Disabling tracking behavior for LINQ queries with EF.
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        /// <summary>
        /// Api Client Identity Settings, Request Headers: X-Request-Sid
        /// </summary>
        public Guid Sid { get; set; }

        //public DbSet<Company> Company { get; set; }
        //public DbSet<Customer> Customer { get; private set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseEncryption(_provider);
            modelBuilder.HasDefaultSchema("public");

            #region Apply Configuration

            //modelBuilder.ApplyConfiguration(new CustomerConfiguration(_companyid));

            #endregion

            #region Query Filter

            //modelBuilder.Entity<Customer>().HasQueryFilter(q => q.companyid == _companyid);

            #endregion
        }
    }
}
