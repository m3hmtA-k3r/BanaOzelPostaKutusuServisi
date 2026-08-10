using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PostaKutusuServisi.Entities;

namespace PostaKutusuServisi.Context
{
    public class AppDbContext: IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions  options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>()
                .HasMany(message => message.SentMessages)
                .WithOne(s => s.Sender)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppUser>()
                .HasMany(message => message.ReceiverMessages)
                .WithOne(s => s.Receiver)
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                .HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserMessage>()
                .HasOne(m => m.Category)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<AppUser>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Entity<MessageReport>()
            .HasOne(r => r.Message)
            .WithMany()
            .HasForeignKey(r => r.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MessageReport>()
                .HasOne(r => r.ReportedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }


        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MessageReport> MessageReports { get; set; }



    }
}
