namespace RClone.Models
{
	/**
	 * Used for the many to many relationship between
	 * Users and there upvoted comments.
	 */
	public class UpvotedComment
	{
		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int CommentId { get; set; }
		public Comment Comment { get; set; }
	}
}
