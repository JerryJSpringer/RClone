namespace RClone.Models
{
	/**
	 * Used for the many to many relationship between
	 * Users and there upvoted posts.
	 */
	public class UpvotedPost
	{
		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int PostId { get; set; }
		public Post Post { get; set; }
	}
}
