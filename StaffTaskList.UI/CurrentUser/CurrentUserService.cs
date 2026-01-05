
using StaffTaskList.Core.ICurrentUser;

namespace StaffTaskList.UI.CurrentUser
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string? Username => _httpContextAccessor.HttpContext?.User?.FindFirst("Username")?.Value;
    }
}
