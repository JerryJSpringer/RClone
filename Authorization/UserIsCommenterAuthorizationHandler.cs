using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RClone.Data;
using RClone.Models;

namespace RClone.Authorization
{
	public class UserIsCommenterAuthorizationHandler :
		AuthorizationHandler<OperationAuthorizationRequirement, Comment>
	{
		private readonly RCloneDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public UserIsCommenterAuthorizationHandler(RCloneDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		protected override Task
			HandleRequirementAsync(AuthorizationHandlerContext authContext,
				OperationAuthorizationRequirement requirement,
				Comment resource)
		{
			// Checks it the user or resource is null
			if (authContext.User == null || resource == null)
			{
				return Task.CompletedTask;
			}

			// Checks if the operation is a CRUD operation
			if (requirement.Name != Constants.CreateOperationName &&
				requirement.Name != Constants.ReadOperationName &&
				requirement.Name != Constants.UpdateOperationName &&
				requirement.Name != Constants.DeleteOperationName)
			{
				return Task.CompletedTask;
			}

			// Gets the application user
			ApplicationUser applicationUser = (_context.Users
				.Include(u => u.UserInfo)
				.FirstOrDefault(u => u.Id == _userManager.GetUserId(authContext.User)));

			// If the user does not exist (not signed in) then return
			if (applicationUser == null)
			{
				return Task.CompletedTask;
			}

			// If the application user exists and is the owner than authentication succeeds
			if (resource.UserInfoId == applicationUser.UserInfoId)
			{
				authContext.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}
}
