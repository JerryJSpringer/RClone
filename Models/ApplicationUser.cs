using Microsoft.AspNetCore.Identity;
namespace RClone.Models
{
	public class ApplicationUser : IdentityUser
	{
		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }
	}
}
