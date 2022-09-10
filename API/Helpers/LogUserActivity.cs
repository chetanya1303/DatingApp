using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using API.Extensions;
using Microsoft.Extensions.DependencyInjection;
using API.Interfaces;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

            var userId = resultContext.HttpContext.User.GetUserId();
            var uow = resultContext.HttpContext.RequestServices.GetService<IUnitOfWork>();

            var user = await uow.UserRepository.GetUserByIdAsync(userId);
            user.LastActive = DateTime.Now;
            await uow.Complete();
        }
    }
}