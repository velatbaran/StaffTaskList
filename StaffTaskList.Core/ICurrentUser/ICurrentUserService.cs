using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StaffTaskList.Core.ICurrentUser
{
    public interface ICurrentUserService
    {
        string? Username { get; }
    }
}
