using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace WebFramework
{
    /// <summary>
    /// JWT generator interface.
    /// </summary>
    public interface IJwtGenerator
    {
        /// <summary></summary>
        string Generate(IEnumerable<Claim> claims);
        /// <summary></summary>
        string Generate(Func<IEnumerable<Claim>> generator);
    }
}
