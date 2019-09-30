using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RClone.Models
{
	public class Comment
	{
		public int CommentId { get; set; }

		[Required(ErrorMessage = "Please enter text")]
		public string Text { get; set; }

		[DataType(DataType.DateTime)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime Time { get; set; }

		public int UserInfoId { get; set; }
		public UserInfo UserInfo { get; set; }

		public int PostId { get; set; }
		public Post Post { get; set; }

		public ICollection<UpvotedComment> UpvotedComments { get; set; }
		public ICollection<DownvotedComment> DownvotedComments { get; set; }
	}
}
