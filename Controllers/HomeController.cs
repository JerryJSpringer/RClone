using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using RClone.Data;
using RClone.Models;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace RClone.Controllers
{
    public class HomeController : Controller
	{
        /* The database context associated with the website. */
        private readonly RCloneDbContext _context;

		/* The Authorization Service. */
		private readonly IAuthorizationService _authorizationService;

		/* The usermanager. */
		private readonly UserManager<ApplicationUser> _userManager;

		/**
		 * Creates a home controller with the given DataContext.
		 */
		public HomeController(RCloneDbContext context,
			IAuthorizationService authorizationService,
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_authorizationService = authorizationService;
			_userManager = userManager;
		}


		[Route("")]
		[AllowAnonymous]
		public async Task<IActionResult> Index() 
		{
			var userId = _userManager.GetUserId(User);

			if (userId == null)
			{
				// If user is not signed in include all posts
				IEnumerable<Post> posts = _context.Posts
					.Include(p => p.Community)
					.Include(p => p.UserInfo)
					.Include(p => p.Comments)
					.Include(p => p.UpvotedPosts)
					.Include(p => p.DownvotedPosts)
					.AsNoTracking();

				ViewBag.Posts = posts;
			}
			else
			{
				// If user is signed in then include all subscribed community posts
				ApplicationUser user = await _context.Users
						.Include(u => u.UserInfo)
						.ThenInclude(u => u.Subscriptions)
							.ThenInclude(s => s.Community)
								.ThenInclude(c => c.Posts)
						.AsNoTracking()
						.FirstOrDefaultAsync(u => u.Id == userId);

				if (user == null)
				{
					IEnumerable<Post> posts = _context.Posts
					.Include(p => p.Community)
					.Include(p => p.UserInfo)
					.Include(p => p.Comments)
					.Include(p => p.UpvotedPosts)
					.Include(p => p.DownvotedPosts);

					ViewBag.Posts = posts;
				}
				else
				{
					UserInfo userInfo = user.UserInfo;
					IEnumerable<Community> communities = user.UserInfo.Subscriptions
						.Select(s => s.Community);

					IEnumerable<Post> posts = _context.Posts
						.Include(p => p.UserInfo)
						.Include(p => p.Community)
						.Include(p => p.Comments)
						.Include(p => p.UpvotedPosts)
						.Include(p => p.DownvotedPosts)
						.AsNoTracking()
						.Where(p => communities.Contains(p.Community));

					ViewBag.Posts = posts;
				}
			}

			return View();
		}


        [Route("Explore")]
		public IActionResult Explore()
		{
			IEnumerable<Post> posts = _context.Posts
					.Include(p => p.Community)
					.Include(p => p.UserInfo)
					.Include(p => p.Comments)
					.Include(p => p.UpvotedPosts)
					.Include(p => p.DownvotedPosts);

			ViewBag.Posts = posts;

			return View();
		}
	}
}
