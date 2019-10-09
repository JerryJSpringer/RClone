using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RClone.Authorization;
using RClone.Data;
using RClone.Models;

namespace RClone.Controllers
{
    public class CommentController : Controller
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
		public CommentController(RCloneDbContext context,
			IAuthorizationService authorizationService,
			UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_authorizationService = authorizationService;
			_userManager = userManager;
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


		// GET for DeleteComment
		[Route("DeleteComment/{postId}/{commentId}")]
		public async Task<IActionResult> DeleteComment(int postId, int commentId)
		{
			Comment comment = await _context.Comments
				.Include(c => c.UserInfo)
				.Include(c => c.UpvotedComments)
				.Include(c => c.DownvotedComments)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CommentId == commentId);

			Post post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.UpvotedPosts)
				.Include(p => p.DownvotedPosts)
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == postId);

			ViewBag.Comment = comment;
			ViewBag.Post = post;

			return View();
		}

		// POST for DeleteComment
		[Route("DeleteComment/{postId}/{commentId}")]
		[HttpPost, ActionName("DeleteComment")]
		public async Task<IActionResult> DeleteCommentConfirmed(int? postId, int? commentId, bool? saveChangesError)
		{
			var comment = await _context.Comments
				.Include(c => c.UpvotedComments)
				.Include(c => c.DownvotedComments)
				.FirstOrDefaultAsync(c => c.CommentId == commentId);

			var post = await _context.Posts
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == postId);

			if (comment == null)
			{
				RedirectToAction("Index", "Home", new { area = "" });
			}

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, comment, RCloneOperations.Delete);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			try
			{
				foreach (UpvotedComment uc in comment.UpvotedComments)
				{
					_context.UpvotedComments.Remove(uc);
				}

				foreach (DownvotedComment dc in comment.DownvotedComments)
				{
					_context.DownvotedComments.Remove(dc);
				}

				_context.Comments.Remove(comment);

				await _context.SaveChangesAsync();

				return RedirectToAction("DisplayPost", "Post",
						new
						{
							communityName = post.Community.Name,
							postId = postId
						});
			}
			catch (DbUpdateException /* ex */)
			{
				return RedirectToAction(nameof(DeleteComment), new { postId, commentId, saveChangesError = true });
			}
		}


		// GET for EditComment
		[Route("EditComment/{postId}/{commentId}")]
		public async Task<IActionResult> EditComment(int? postId, int? commentId)
		{
			Comment comment = await _context.Comments
				.Include(c => c.UserInfo)
				.Include(c => c.UpvotedComments)
				.Include(c => c.DownvotedComments)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CommentId == commentId);

			Post post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.UpvotedPosts)
				.Include(p => p.DownvotedPosts)
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == postId);

			ViewBag.Comment = comment;
			ViewBag.Post = post;

			return View();
		}

		// POST for EditComment
		[Route("EditComment/{postId}/{commentId}")]
		[HttpPost, ActionName("EditComment")]
		public async Task<IActionResult> EditCommentConfirmed(int? postId, int? commentId)
		{
			if (commentId == null || postId == null)
			{
				return NotFound();
			}

			var comment = await _context.Comments
				.FirstOrDefaultAsync(c => c.CommentId == commentId);

			var post = await _context.Posts
				.Include(p => p.Community)
				.FirstOrDefaultAsync(p => p.PostId == postId);

			var isAuthorized = await _authorizationService.AuthorizeAsync(
				User, comment, RCloneOperations.Update);

			if (!isAuthorized.Succeeded)
			{
				return new ChallengeResult();
			}

			if (await TryUpdateModelAsync<Comment>(
					comment,
					"",
					c => c.Text))
			{
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction("DisplayPost", "Post",
						new
						{
							communityName = post.Community.Name,
							postId = post.PostId
						});
				}
				catch (DbUpdateException /* ex */)
				{
					// Log the update error
					ModelState.AddModelError("", "Unable to save changes.");
				}
			}

			ViewBag.Comment = await _context.Comments
				.Include(c => c.UserInfo)
				.Include(c => c.UpvotedComments)
				.Include(c => c.DownvotedComments)
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CommentId == commentId);

			ViewBag.Post = await _context.Posts
				.Include(p => p.UserInfo)
				.Include(p => p.UpvotedPosts)
				.Include(p => p.DownvotedPosts)
				.Include(p => p.Community)
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PostId == postId);

			return View();
		}
	}
}