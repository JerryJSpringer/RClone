namespace RClone.Models
{
	/**
	 * Used for the many to many relationship between
	 * Users and Communities.
	 */
	public class Subscription
	{
		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int CommunityId { get; set; }
		public Community Community { get; set; }
	}
}
