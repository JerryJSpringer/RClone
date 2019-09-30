namespace RClone.Models
{
	/**
	 * Used for the many to many relationship between
	 * Users and there downvoted comments.
	 */
	public class DownvotedComment
	{
		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int CommentId { get; set; }
		public Comment Comment { get; set; }
	}
}
