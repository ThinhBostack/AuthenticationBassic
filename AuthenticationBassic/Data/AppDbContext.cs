using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationBassic.Data
{
    //This class gives all the functionalities to communicate with the Db 
    //IdentityDbContext contains all user tables
    public class AppDbContext : IdentityDbContext //Play with Db
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options)
        {

        }
    }
}
