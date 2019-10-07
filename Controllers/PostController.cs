using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RClone.Data;
using RClone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RClone.Authorization;

namespace RClone.Controllers
{
    public class PostController : Controller
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
		public PostController(RCloneDbContext context,
			IAuthorizationService authorizationService, 
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_authorizationService = authorizationService;
			_userManager = userManager;
		}


		// GET for DisplayPost
		[Route("c/{communityName}/comments/{postId}")]
		[AllowAnonymous]
		public async Task<IActionResult> DisplayPost(string communityName, int? postId = null)
		{
			var post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.Community)
				.Include(p => p.Comments)
					.ThenInclude(c => c.UserInfo)
				.Include(p => p.Comments)
					.ThenInclude(c => c.UpvotedComments)
				.Include(p => p.Comments)
					.ThenInclude(c => c.DownvotedComments)
				.Include(p => p.UpvotedPosts)
				.Include(p => p.DownvotedPosts)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == postId);

			if (post == null)
			{
				return RedirectToAction("Create", "Community", new { area = "" });
			}

			ViewBag.Post = post;
			ViewBag.UpvotedPosts = post.UpvotedPosts;
			ViewBag.DownvotedPosts = post.DownvotedPosts;
			return View();
		}


		// GET for posts
		[Route("Post")]
		public IActionResult Post()
		{
			ViewBag.Communities = PopulateCommunitiesDropDownList();
			return View();
		}

		// POST for posts
		[Route("Post")]
		[HttpPost]
		public async Task<IActionResult> Post([Bind("Title, Text, CommunityId")] Post post)
		{

			if (ModelState.IsValid)
			{
				var userId = _userManager.GetUserId(User);
				ApplicationUser user = await _context.Users
					.Include(u => u.UserInfo)
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.Id == userId);

				Community community = await _context.Communities
					.AsNoTracking()
					.FirstOrDefaultAsync(c => c.CommunityId == post.CommunityId);

				post.UpvotedPosts = new List<UpvotedPost>();
				post.DownvotedPosts = new List<DownvotedPost>();
				post.Time = DateTime.Now;
				post.UserInfoId = user.UserInfoId;
				post.Comments = new List<Comment>();

				_context.Posts.Add(post);
				await _context.SaveChangesAsync();

				// change to redirect to new post
				return RedirectToAction("DisplayPost", "Post", new
				{
					area = "",
					communityName = community.Name,
					postId = post.PostId
				});
			}

			return RedirectToAction("Index", "Home", new { area = "" });
		}

		private IOrderedQueryable PopulateCommunitiesDropDownList()
		{
			var communitiesQuery = from c in _context.Communities
								   orderby c.Name
								   select c;
			// creates a select list based on what it retrieves from the DB
			return communitiesQuery;

		}


		// GET for EditPost
		[Route("EditPost/{id}")]
		public async Task<IActionResult> EditPost(int? id)
		{

			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.Comments)
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == id);

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, post, RCloneOperations.Create);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			if (post == null)
			{
				return NotFound();
			}

			ViewBag.Post = post;

			return View();
		}

		// POST for EditPost
		[Route("EditPost/{id}")]
		[HttpPost, ActionName("EditPost")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPostConfirmed(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.Comments)
				.Include(p => p.Community)
				.FirstOrDefaultAsync(p => p.PostId == id);

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, post, RCloneOperations.Create);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			if (await TryUpdateModelAsync<Post>(
					post,
					"",
					p => p.Title, p => p.Text))
			{
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction("Index", "Home", new { area = "" });
				}
				catch (DbUpdateException /* ex */)
				{
					// Log the update error
					ModelState.AddModelError("", "Unable to save changes.");
				}
			}

			return View();
		}


		// GET for DeletePost
		[Route("DeletePost/{id}")]
		public async Task<IActionResult> DeletePost(int? id, bool? saveChangesError = false)
		{
			if (id == null)
			{
				return NotFound();
			}

			var post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.Comments)
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == id);

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, post, RCloneOperations.Create);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			if (post == null)
			{
				return NotFound();
			}

			if (saveChangesError.GetValueOrDefault())
			{
				ViewData["ErrorMessage"] = "Delete failed.";
			}

			ViewBag.Post = post;

			return View();
		}

		// POST for DeletePost
		[Route("DeletePost/{id}")]
		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var post = await _context.Posts
				.Include(p => p.UpvotedPosts)
				.Include(p => p.DownvotedPosts)
				.Include(p => p.Comments)
					.ThenInclude(c => c.UpvotedComments)
				.Include(p => p.Comments)
					.ThenInclude(c => c.DownvotedComments)
				.FirstOrDefaultAsync(p => p.PostId == id);

			if (post == null)
			{
				RedirectToAction("Index", "Home", new { area = "" });
			}

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, post, RCloneOperations.Create);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			try
			{
				foreach (Comment c in post.Comments)
				{
					foreach (UpvotedComment uc in c.UpvotedComments)
					{
						_context.UpvotedComments.Remove(uc);
					}

					foreach (DownvotedComment dc in c.DownvotedComments)
					{
						_context.DownvotedComments.Remove(dc);
					}

					_context.Comments.Remove(c);
				}

				foreach (UpvotedPost up in post.UpvotedPosts)
				{
					_context.Remove(up);
				}

				foreach (DownvotedPost dp in post.DownvotedPosts)
				{
					_context.Remove(dp);
				}

				_context.Posts.Remove(post);

				await _context.SaveChangesAsync();

				RedirectToAction("Index", "Home", new { area = "" });
			}
			catch (DbUpdateException /* ex */)
			{
				return RedirectToAction(nameof(DeletePost), new { id = id, saveChangesError = true });
			}

			return RedirectToAction(nameof(DeletePost), new { id = id, saveChangesError = true });
		}


		// Post for Comment
		[Route("Comment/{postId?}")]
		[HttpPost]
		public async Task<IActionResult> Comment([Bind("Text, PostId, CommunityId")] Comment Comment)
		{

			var commentCommunityName = _context.Communities.FirstOrDefault(c => c.CommunityId == c.CommunityId).Name;

			if (ModelState.IsValid)
			{
				var userId = _userManager.GetUserId(User);
				ApplicationUser user = await _context.Users
					.Include(u => u.UserInfo)
					.AsNoTracking()
					.FirstOrDefaultAsync(u => u.Id == userId);

				Comment.UpvotedComments = new List<UpvotedComment>();
				Comment.DownvotedComments = new List<DownvotedComment>();
				Comment.Time = DateTime.Now;
				Comment.UserInfoId = user.UserInfoId;

				_context.Comments.Add(Comment);
				await _context.SaveChangesAsync();
			}

			return RedirectToAction("DisplayPost", "Post", new
			{
				area = "",
				communityName = commentCommunityName,
				postId = Comment.PostId
			});
		}


		// POST for UpvotePost
		[Route("UpvotePost")]
		[HttpPost]
		public async Task UpvotePost(int postId)
		{
			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.Include(u => u.UserInfo)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			UpvotedPost upvotedPost = await _context.UpvotedPosts
				.AsNoTracking()
				.FirstOrDefaultAsync(up => up.PostId == postId && up.UserInfoId == user.UserInfoId);

			DownvotedPost downvotedPost = await _context.DownvotedPosts
				.AsNoTracking()
				.FirstOrDefaultAsync(dp => dp.PostId == postId && dp.UserInfoId == user.UserInfoId);

			try
			{
				// If not upvoted upvote
				// Else remove upvote
				if (upvotedPost == null)
					_context.UpvotedPosts.Add(
						new UpvotedPost
						{
							PostId = postId,
							UserInfoId = user.UserInfoId
						});
				else
					_context.UpvotedPosts.Remove(upvotedPost);

				// If downvote exists remove it
				if (downvotedPost != null)
					_context.DownvotedPosts.Remove(
						new DownvotedPost
						{
							PostId = postId,
							UserInfoId = user.UserInfoId
						});

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on upvote post.");
			}
		}

		// POST for DownvotePost
		[Route("DownvotePost")]
		[HttpPost]
		public async Task DownvotePost(int postId)
		{
			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.Include(u => u.UserInfo)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			UpvotedPost upvotedPost = await _context.UpvotedPosts
				.AsNoTracking()
				.FirstOrDefaultAsync(up => up.PostId == postId && up.UserInfoId == user.UserInfoId);

			DownvotedPost downvotedPost = await _context.DownvotedPosts
				.AsNoTracking()
				.FirstOrDefaultAsync(dp => dp.PostId == postId && dp.UserInfoId == user.UserInfoId);

			try
			{
				// If not downvoted downvote
				// Else remove downvote
				if (downvotedPost == null)
					_context.DownvotedPosts.Add(
						new DownvotedPost
						{
							PostId = postId,
							UserInfoId = user.UserInfoId
						});
				else
					_context.DownvotedPosts.Remove(downvotedPost);

				// If upvote exists remove it
				if (upvotedPost != null)
					_context.UpvotedPosts.Remove(
						new UpvotedPost
						{
							PostId = postId,
							UserInfoId = user.UserInfoId
						});

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on upvote post.");
			}
		}


		// POST for UpvoteComment
		[Route("UpvoteComment")]
		[HttpPost]
		public async Task UpvoteComment(int postId, int commentId)
		{
			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.Include(u => u.UserInfo)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			UpvotedComment upvotedComment = await _context.UpvotedComments
				.AsNoTracking()
				.FirstOrDefaultAsync(uc => uc.CommentId == commentId && uc.UserInfoId == user.UserInfoId);

			DownvotedComment downvotedComment = await _context.DownvotedComments
				.AsNoTracking()
				.FirstOrDefaultAsync(dc => dc.CommentId == commentId && dc.UserInfoId == user.UserInfoId);

			try
			{
				// If not upvoted upvote
				// Else remove upvote
				if (upvotedComment == null)
					_context.UpvotedComments.Add(
						new UpvotedComment
						{
							CommentId = commentId,
							UserInfoId = user.UserInfoId
						});
				else
					_context.UpvotedComments.Remove(upvotedComment);

				// If downvote exists remove it
				if (downvotedComment != null)
					_context.DownvotedComments.Remove(
						new DownvotedComment
						{
							CommentId = commentId,
							UserInfoId = user.UserInfoId
						});

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on upvote post.");
			}
		}

		// POST for DownvoteComment
		[Route("DownvoteComment")]
		public async Task DownvoteComment(int postId, int commentId)
		{
			var userId = _userManager.GetUserId(User);
			ApplicationUser user = await _context.Users
				.Include(u => u.UserInfo)
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == userId);

			UpvotedComment upvotedComment = await _context.UpvotedComments
				.AsNoTracking()
				.FirstOrDefaultAsync(uc => uc.CommentId == commentId && uc.UserInfoId == user.UserInfoId);

			DownvotedComment downvotedComment = await _context.DownvotedComments
				.AsNoTracking()
				.FirstOrDefaultAsync(dc => dc.CommentId == commentId && dc.UserInfoId == user.UserInfoId);

			try
			{
				// If not downvoted downvote
				// Else remove downvote
				if (downvotedComment == null)
					_context.DownvotedComments.Add(
						new DownvotedComment
						{
							CommentId = commentId,
							UserInfoId = user.UserInfoId
						});
				else
					_context.DownvotedComments.Remove(downvotedComment);

				// If upvote exists remove it
				if (upvotedComment != null)
					_context.UpvotedComments.Remove(
						new UpvotedComment
						{
							CommentId = commentId,
							UserInfoId = user.UserInfoId
						});

				await _context.SaveChangesAsync();
			}
			catch (DbUpdateException /* ex */)
			{
				// Logs the database error
				ModelState.AddModelError("", "Database error on upvote post.");
			}
		}
	}
}