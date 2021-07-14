using Microsoft.AspNetCore.Identity;

namespace WebInterface.Settings
{
    /// <summary>
    /// Identity Server Settings
    /// </summary>
    public class IdentitySettings
    {
        /// <summary>
        /// Default Instance.
        /// </summary>
        public static IdentitySettings Instance = new IdentitySettings();
        /// <summary>
        /// Configuration Section in appsettings.json
        /// </summary>
        public const string AppSettings = "Identity";
        /*
          "Identity": {
            "LiteDB": "Filename=App_Data/Identity.db;Password=HGJ766GR767FKJU0",
            "User": {
              "AllowedUserNameCharacters": "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_",
              "RequireUniqueEmail": false
            },
            "Password": {
              "RequireNonAlphanumeric": false,
              "RequireLowercase": false,
              "RequireUppercase": false,
              "RequireDigit": false,
              "RequiredUniqueChars": 0,
              "RequiredLength": 6
            },
            "SignIn": {
              "RequireConfirmedEmail": false,
              "RequireConfirmedPhoneNumber": false,
              "RequireConfirmedAccount": false
            }
          }
        */

        /// <summary>
        /// LiteDB ConnectionString.
        /// </summary>
        public string LiteDB { get; set; }

        /// <summary>
        /// Gets or sets the Microsoft.AspNetCore.Identity.UserOptions for the identity system.
        /// </summary>
        public UserOptions User { get; set; }
        /// <summary>
        /// Gets or sets the Microsoft.AspNetCore.Identity.PasswordOptions for the identity system.
        /// </summary>
        public PasswordOptions Password { get; set; }
        /// <summary>
        /// Gets or sets the Microsoft.AspNetCore.Identity.SignInOptions for the identity system.
        /// </summary>
        public SignInOptions SignIn { get; set; }
        ///// <summary>
        ///// Gets or sets the Microsoft.AspNetCore.Identity.LockoutOptions for the identity system.
        ///// </summary>
        //public LockoutOptions Lockout { get; set; }
    }
}
