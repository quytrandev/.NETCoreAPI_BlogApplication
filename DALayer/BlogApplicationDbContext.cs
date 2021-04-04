
using DALayer.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DALayer
{
    public class BlogApplicationDbContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        //private readonly string connectionString = "Server=(local)\\SQLEXPRESS;Database=BlogApplication;Trusted_Connection=True;MultipleActiveResultSets=true";
        public BlogApplicationDbContext(DbContextOptions<BlogApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PostComment>(x => x.HasKey(pc => new { pc.PostId, pc.CommentId }));
            modelBuilder.Entity<PostComment>()
            .HasOne(u => u.Post)
            .WithMany(p => p.PostComments)
            .HasForeignKey(u => u.PostId);
        }
    }
}
