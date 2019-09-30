using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RClone.Models
{
	public class Community
	{

        public int CommunityId { get; set; }

		[Required(ErrorMessage = "Please enter a community name")]
		public string Name { get; set; }

		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime Time { get; set; }

		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public ICollection<Post> Posts { get; set; }

		public ICollection<Subscription> Subscriptions { get; set; }
	}
}
