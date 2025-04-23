using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ContactsManager.Core.Domain.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid> // type of Id
    {
        // by default it has id, username, passwordhash, email

        public string? FirstName { get; set; }

    }
}
