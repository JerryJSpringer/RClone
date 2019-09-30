using RClone.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RClone.Data
{
	public class RCloneDbContext : IdentityDbContext<ApplicationUser>
	{
		public RCloneDbContext(DbContextOptions<RCloneDbContext> options) : base(options)
		{
		}

		public DbSet<UserInfo> UserInfos { get; set; }
		public DbSet<Subscription> Subscriptions { get; set; }
		public DbSet<Community> Communities { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<UpvotedPost> UpvotedPosts { get; set; }
		public DbSet<DownvotedPost> DownvotedPosts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<UpvotedComment> UpvotedComments { get; set; }
		public DbSet<DownvotedComment> DownvotedComments { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserInfo>()
				.HasMany(u => u.Posts)
				.WithOne(p => p.UserInfo)
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<UserInfo>()
				.HasMany(u => u.Subscriptions)
				.WithOne(s => s.UserInfo)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<UserInfo>().ToTable("UserInfo");


			modelBuilder.Entity<Community>()
				.HasMany(c => c.Posts)
				.WithOne(p => p.Community)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Community>()
				.HasMany(c => c.Subscriptions)
				.WithOne(s => s.Community)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Community>().ToTable("Community");

			modelBuilder.Entity<Subscription>().ToTable("Subscription");
			modelBuilder.Entity<Subscription>()
				.HasKey(s => new { s.UserInfoId, s.CommunityId });


			modelBuilder.Entity<Post>()
				.HasMany(p => p.Comments)
				.WithOne(c => c.Post)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Post>()
				.HasMany(p => p.UpvotedPosts)
				.WithOne(up => up.Post)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Post>()
				.HasMany(p => p.DownvotedPosts)
				.WithOne(dp => dp.Post)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Post>().ToTable("Post");

			modelBuilder.Entity<UpvotedPost>()
				.HasKey(p => new { p.UserInfoId, p.PostId });
			modelBuilder.Entity<UpvotedPost>().ToTable("UpvotedPost");

			modelBuilder.Entity<DownvotedPost>()
				.HasKey(p => new { p.UserInfoId, p.PostId });
			modelBuilder.Entity<DownvotedPost>().ToTable("DownvotedPost");


			modelBuilder.Entity<Comment>()
				.HasMany(c => c.UpvotedComments)
				.WithOne(uc => uc.Comment)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Comment>()
				.HasMany(c => c.DownvotedComments)
				.WithOne(dc => dc.Comment)
				.IsRequired()
				.OnDelete(DeleteBehavior.Restrict);
			modelBuilder.Entity<Comment>().ToTable("Comment");

			modelBuilder.Entity<UpvotedComment>()
				.HasKey(uc => new { uc.UserInfoId, uc.CommentId });
			modelBuilder.Entity<UpvotedComment>().ToTable("UpvotedComment");

			modelBuilder.Entity<DownvotedComment>()
				.HasKey(dc => new { dc.UserInfoId, dc.CommentId });
			modelBuilder.Entity<DownvotedComment>().ToTable("DownvotedComment");


			base.OnModelCreating(modelBuilder);
		}
	}
}
