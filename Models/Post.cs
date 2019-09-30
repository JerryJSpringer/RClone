using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RClone.Models
{
	public class Post
	{
        public int PostId { get; set; }

		[Required(ErrorMessage = "Please enter a title")]
		public string Title { get; set; }

		[Required(ErrorMessage = "Please enter a body")]
		public string Text { get; set; }

		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime Time { get; set; }

		public int? UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int CommunityId { get; set; }
		public Community Community { get; set; }

		public ICollection<Comment> Comments { get; set; }

		public ICollection<UpvotedPost> UpvotedPosts { get; set; }
		public ICollection<DownvotedPost> DownvotedPosts { get; set; }
	}
}
