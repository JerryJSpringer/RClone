using Microsoft.AspNetCore.Identity;
using RClone.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RClone.Data
{
	public class DbInitializer
	{
		public static void Initialize(RCloneDbContext context, UserManager<ApplicationUser> userManager)
		{
			// Check for existing data
			if (context.Users.Any())
			{
				return; // Database is already seeded
			}

			// Make new users
			var Users = new ApplicationUser[]
			{
				new ApplicationUser
				{
					UserName = "UserName1",
					Email = "Email1@Email.com",
				},

				new ApplicationUser
				{
					UserName = "UserName2",
					Email = "Email2@Email.com",
				},

				new ApplicationUser
				{
					UserName = "UserName3",
					Email = "Email3@Email.com",
				},

				new ApplicationUser
				{
					UserName = "UserName4",
					Email = "Email4@Email.com",
				},

				new ApplicationUser
				{
					UserName = "UserName5",
					Email = "Email5@Email.com",
				},

				new ApplicationUser
				{
					UserName = "UserName6",
					Email = "Email6@Email.com"
				}
			};

			// Add the new users
			foreach (ApplicationUser user in Users)
			{
				// Creates the user info and connects the ApplicationUser to there info
				user.UserInfo = new UserInfo { Username = user.UserName };

				// Creates the user with default password
				IdentityResult result = userManager.CreateAsync
					(user, "password").Result;
			}

			// Save the changes
			context.SaveChanges();


			// Make new communities
			var Communities = new Community[]
			{
				new Community
				{
					Name = "Community1",
					Time = new DateTime(1920, 12, 10),
					UserInfo = Users[0].UserInfo
				},

				new Community
				{
					Name = "Community2",
					Time = new DateTime(1940, 3, 24),
					UserInfo = Users[1].UserInfo
				},

				new Community
				{
					Name = "Community3",
					Time = new DateTime(1938, 5, 2),
					UserInfo = Users[2].UserInfo
				}
			};

			// Add the communities
			foreach (Community community in Communities)
			{
				// Connects communities owner to the user
				UserInfo owner = community.UserInfo;
				owner.OwnedCommunities = new Community[] { community };

				context.Communities.Add(community);
			}

			// Save the changes
			context.SaveChanges();


			// Make new Posts
			var Posts = new Post[]
			{
				new Post
				{
					UpvotedPosts = new List<UpvotedPost>(),
					DownvotedPosts = new List<DownvotedPost>(),
					Title = "Test Case: User who does not own community",
					Text = "Text for test post number 1",
					Time = new DateTime(1998, 10, 29),
					UserInfo = Users[3].UserInfo,
					Community = Communities[0]
				},

				new Post
				{
					UpvotedPosts = new List<UpvotedPost>(),
					DownvotedPosts = new List<DownvotedPost>(),
					Title = "Test Case: User who does own the community",
					Text = "Text for test post number 2",
					Time = new DateTime(1999, 2, 10),
					UserInfo = Users[0].UserInfo,
					Community = Communities[0]
                },

				new Post
				{
					UpvotedPosts = new List<UpvotedPost>(),
					DownvotedPosts = new List<DownvotedPost>(),
					Title = "Test Case: No downvotes",
					Text = "Text of test post number 3",
					Time = new DateTime(2000, 3, 11),
					UserInfo = Users[4].UserInfo,
					Community = Communities[2]
                },

				new Post
				{
					UpvotedPosts = new List<UpvotedPost>(),
					DownvotedPosts = new List<DownvotedPost>(),
					Title = "Test Case: No upvotes",
					Text = "Text of test post number 4",
					Time = new DateTime(2002, 1, 12),
					UserInfo = Users[0].UserInfo,
					Community = Communities[0]
                }
			};

			// Add the posts
			foreach (Post post in Posts)
			{
				context.Posts.Add(post);
			}

			// Save changes
			context.SaveChanges();


			// Make new comments
			var comments = new Comment[]
			{
				new Comment
				{
					UpvotedComments = new List<UpvotedComment>(),
					DownvotedComments = new List<DownvotedComment>(),
					Text = "Test Case: No downvotes",
					Time = new DateTime(1998, 10, 30),
					UserInfo = Users[0].UserInfo,
					Post = Posts[0]
				},

				new Comment
				{
					UpvotedComments = new List<UpvotedComment>(),
					DownvotedComments = new List<DownvotedComment>(),
					Text = "Test Case: No upvotes",
					Time = new DateTime(1998, 10, 31),
					UserInfo = Users[1].UserInfo,
					Post = Posts[0]
				},

				new Comment
				{
					UpvotedComments = new List<UpvotedComment>(),
					DownvotedComments = new List<DownvotedComment>(),
					Text = "Test Case: Equal upvotes and downvotes",
					Time = new DateTime(1998, 11, 1),
					UserInfo = Users[2].UserInfo,
					Post = Posts[0]
				},

				new Comment
				{
					UpvotedComments = new List<UpvotedComment>(),
					DownvotedComments = new List<DownvotedComment>(),
					Text = "Test Case: User made post",
					Time = new DateTime(1998, 11, 2),
					UserInfo = Users[3].UserInfo,
					Post = Posts[1]
				},

				new Comment
				{
					UpvotedComments = new List<UpvotedComment>(),
					DownvotedComments = new List<DownvotedComment>(),
					Text = "Test Case: User made post and owns community",
					Time = new DateTime(1999, 2, 12),
					UserInfo = Users[0].UserInfo,
					Post = Posts[1]
				}
			};

			// Add the comments
			foreach (Comment comment in comments)
			{
				context.Comments.Add(comment);
			}

			// Save changes
			context.SaveChanges();


			// Make new subscriptions
			var Subscriptions = new Subscription[]
			{
				new Subscription
				{
					UserInfo = Users[0].UserInfo,
					Community = Communities[0]
				},

				new Subscription
				{
					UserInfo = Users[0].UserInfo,
					Community = Communities[1]
				},

				new Subscription
				{
					UserInfo = Users[0].UserInfo,
					Community = Communities[2]
				},

				new Subscription
				{
					UserInfo = Users[1].UserInfo,
					Community = Communities[0]
				},

				new Subscription
				{
					UserInfo = Users[1].UserInfo,
					Community = Communities[1]
				},

				new Subscription
				{
					UserInfo = Users[2].UserInfo,
					Community = Communities[0]
				},

				new Subscription
				{
					UserInfo = Users[3].UserInfo,
					Community = Communities[1]
				},

				new Subscription
				{
					UserInfo = Users[4].UserInfo,
					Community = Communities[2]
				}
			};

			// Add the subscriptions
			foreach (Subscription subscription in Subscriptions)
			{
				context.Subscriptions.Add(subscription);
			}

			// Save changes
			context.SaveChanges();
		}
	}
}
