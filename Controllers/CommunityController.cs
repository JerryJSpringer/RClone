using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RClone.Data;
using RClone.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RClone.Controllers
{
    public class CommunityController : Controller
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
		public CommunityController(RCloneDbContext context,
			IAuthorizationService authorizationService, 
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_authorizationService = authorizationService;
			_userManager = userManager;
		}


		// GET: Community/Create
		[Route("Create")]
		public IActionResult Create()
		{
			return View();
		}


		// POST: Community/Create
		[Route("Create")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name")] Community community)
		{
			// Tries to create a new community otherwise error
			try
			{
				if (ModelState.IsValid)
				{
					var userId = _userManager.GetUserId(User);
					ApplicationUser user = await _context.Users
						.Include(u => u.UserInfo)
						.AsNoTracking()
						.FirstOrDefaultAsync(u => u.Id == userId);

					community.Time = DateTime.Now;
					community.UserInfoId = user.UserInfoId;
					community.Posts = new List<Post>();
					community.Subscriptions = new List<Subscription>();

					Subscription sub = new Subscription
					{
						UserInfoId = user.UserInfoId,
						CommunityId = community.CommunityId
					};

					community.Subscriptions.Add(sub);

					_context.Communities.Add(community);
					_context.Subscriptions.Add(sub);

					await _context.SaveChangesAsync();
					RedirectToAction("Index", "Home", new { area = "" });
				}
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on community create.");
			}

			return RedirectToAction("DisplayCommunity", "Community", new
			{
				area = "",
				communityName = community.Name
			});
		}


		// GET for DisplayCommunity
		[Route("c/{communityName}")]
		[AllowAnonymous]
		public async Task<IActionResult> DisplayCommunity(string communityName)
		{
			// Load community by the community name with associated Posts, Comments, and UserInfo
			var community = await _context.Communities
				.Include(c => c.Posts)
					.ThenInclude(p => p.UserInfo)
				.Include(c => c.Posts)
					.ThenInclude(p => p.Comments)
				.Include(c => c.Posts)
					.ThenInclude(p => p.Community)
				.Include(c => c.Posts)
					.ThenInclude(p => p.UpvotedPosts)
				.Include(c => c.Posts)
					.ThenInclude(p => p.DownvotedPosts)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => string.Equals(c.Name, communityName));

			// Community cannot be found return home page
			if (community == null) {
				RedirectToAction("Index", "Home", new { area = "" });
			}

			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			Subscription subscription = null;

			if (user != null)
			{
				subscription = await _context.Subscriptions
					.Include(s => s.Community)
					.Include(s => s.UserInfo)
					.AsNoTracking()
					.FirstOrDefaultAsync(s => s.UserInfoId == user.UserInfoId && s.CommunityId == community.CommunityId);
			}
			
			
			ViewBag.Community = community;
			ViewBag.Posts = community.Posts;
			ViewBag.Subscribed = (subscription != null);
			return View();
		}


		// GET for Subscribe
		[Route("c/{communityName}/subscribe")]
		public async Task<IActionResult> Subscribe(string communityName)
		{
			Subscription sub = await getSubscriptionAsync(communityName);

			try
			{
				_context.Subscriptions.Add(sub);

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on subscribe.");
			}

			return RedirectToAction("DisplayCommunity", "Community", new
			{
				area = "",
				communityName
			});
		}

		// GET for Unsubscribe
		[Route("c/{communityName}/unsubscribe")]
		public async Task<IActionResult> Unsubscribe(string communityName)
		{
			Subscription sub = await getSubscriptionAsync(communityName);

			try
			{
				_context.Subscriptions.Remove(sub);

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on subscribe.");
			}

			return RedirectToAction("DisplayCommunity", "Community", new
			{
				area = "",
				communityName
			});
		}

		private async Task<Subscription> getSubscriptionAsync(string communityName)
		{
			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.Include(u => u.UserInfo)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			Community community = await _context.Communities
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.Name == communityName);

			return new Subscription
			{
				UserInfoId = user.UserInfoId,
				CommunityId = community.CommunityId
			};
		}
	}
}