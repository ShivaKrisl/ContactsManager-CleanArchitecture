using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactsManager.Core.Domain.IdentityEntities
{
    public class ApplicationRole : IdentityRole<Guid> // type of Id
    {
        // by default it has id, name, normalizedname
        // you can add custom properties here if needed

    }
    
}
