using System.Collections.Generic;

namespace RClone.Models
{
	/**
	 * This class helps separate public and private user data
	 */
	public class UserInfo
	{
		public int UserInfoId { get; set; }

		/**
		 * This is a temporary username that can be changed by the user,
		 * the permanent user name is held within identity user and is
		 * used for login.
		 */
		public string Username { get; set; }

		public ICollection<Post> Posts { get; set; }

		public ICollection<Comment> Comments { get; set; }

		public ICollection<Community> OwnedCommunities { get; set; }

		public ICollection<Subscription> Subscriptions { get; set; }

		public ICollection<UpvotedPost> VotedPosts { get; set; }

		public ICollection<UpvotedComment> VotedComments { get; set; }
	}
}
