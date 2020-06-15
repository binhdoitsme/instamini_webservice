using System;
using InstaminiWebService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace InstaminiWebService.Database
{
    public partial class InstaminiContext : DbContext
    {
        public InstaminiContext()
        {
        }

        public InstaminiContext(DbContextOptions<InstaminiContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AvatarPhoto> AvatarPhotos { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Follow> Follows { get; set; }
        public virtual DbSet<Like> Likes { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AvatarPhoto>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId })
                    .HasName("PRIMARY");

                entity.ToTable("avatar_photos");

                entity.HasIndex(e => e.UserId)
                    .HasName("FKAvatarPhotos892102_idx");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasColumnName("file_name")
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.AvatarPhoto)
                    .HasForeignKey<AvatarPhoto>(p => p.UserId)
                    .HasConstraintName("FKAvatarPhotos892102");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId, e.PostId })
                    .HasName("PRIMARY");

                entity.ToTable("comments");

                entity.HasIndex(e => e.PostId)
                    .HasName("FKComments678039");

                entity.HasIndex(e => e.UserId)
                    .HasName("FKComments875700");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.Property(e => e.Content).HasColumnName("content");

                entity.Property(e => e.Timestamp).HasColumnName("timestamp");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKComments678039");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FKComments875700");
            });

            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.FollowerId })
                    .HasName("PRIMARY");

                entity.ToTable("follows");

                entity.HasIndex(e => e.FollowerId)
                    .HasName("FKFollows197244");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.FollowerId).HasColumnName("follower_id");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'1'");

                entity.HasOne(d => d.Follower)
                    .WithMany(p => p.Followings)
                    .HasForeignKey(d => d.FollowerId)
                    .HasConstraintName("FKFollows197244");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Followers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FKFollows549480");
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LikedPost })
                    .HasName("PRIMARY");

                entity.ToTable("likes");

                entity.HasIndex(e => e.LikedPost)
                    .HasName("FKLikes252442");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.LikedPost).HasColumnName("liked_post");

                entity.Property(e => e.IsActive)
                    .HasColumnName("is_active")
                    .HasColumnType("bit(1)")
                    .HasDefaultValueSql("b'1'");

                entity.HasOne(d => d.LikedPostNavigation)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(d => d.LikedPost)
                    .HasConstraintName("FKLikes252442");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FKLikes318134");
            });

            modelBuilder.Entity<Photo>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.PostId })
                    .HasName("PRIMARY");

                entity.ToTable("photos");

                entity.HasIndex(e => e.PostId)
                    .HasName("FKPhotos896484");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.PostId).HasColumnName("post_id");

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasColumnName("file_name")
                    .HasMaxLength(512)
                    .IsUnicode(false);

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Photos)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FKPhotos896484");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");

                entity.HasIndex(e => e.UserId)
                    .HasName("FKPosts199121");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Caption)
                    .IsRequired()
                    .HasColumnName("caption")
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Created).HasColumnName("created");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FKPosts199121");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Created).HasColumnName("created");

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasColumnName("display_name")
                    .HasMaxLength(127)
                    .IsUnicode(false);

                entity.Property(e => e.LastLogin).HasColumnName("last_login");

                entity.Property(e => e.LastUpdate).HasColumnName("last_update");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasColumnName("password")
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasColumnName("salt")
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasColumnName("username")
                    .HasMaxLength(64)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
