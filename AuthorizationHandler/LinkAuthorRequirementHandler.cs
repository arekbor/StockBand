using Microsoft.AspNetCore.Authorization;
using StockBand.Authorization;
using StockBand.Data;
using StockBand.Interfaces;
using StockBand.Models;

namespace StockBand.AuthorizationHandler
{
    public class LinkAuthorRequirementHandler : AuthorizationHandler<LinkAuthorRequirement, UniqueLink>
    {
        private IUserContextService _userContextService;
        public LinkAuthorRequirementHandler(IUserContextService userContextService)
        {
            _userContextService = userContextService;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, LinkAuthorRequirement requirement, UniqueLink resource)
        {
            if(resource.User.Id == _userContextService.GetUserId() || _userContextService.GetUser().IsInRole(UserRoles.Roles[1]))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }
    }
}
