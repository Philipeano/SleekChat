using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SleekChat.Data.Helpers
{

    public interface ICurrentUser
    {
        public Guid GetUserId();
    }


    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor context;


        public CurrentUser(IHttpContextAccessor context)
        {
            this.context = context;
        }


        public Guid GetUserId()
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(context.HttpContext.User.Claims
                       .First(i => i.Type == ClaimTypes.NameIdentifier).Value);
            }
            catch
            {
                userId = Guid.Empty;
            }
            return userId;
        }

    }
}
