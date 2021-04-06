using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server
{
    public static class Constants
    {
        public const string Issuer = Audience;
        public const string Audience = "https://localhost:5001/";
        public const string Secret = "not_too_short_dkmperrytheplatipus";
    }
}
