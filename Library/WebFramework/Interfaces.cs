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
        string Generate(IEnumerable<Claim> claims);
        string Generate(Func<IEnumerable<Claim>> generator);
    }
}
