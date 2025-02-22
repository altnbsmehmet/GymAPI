using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


namespace Data 
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<Employee> Employee { get; set; }
        public DbSet<Member> Member { get; set; }
        public DbSet<Membership> Membership { get; set; }
        public DbSet<Subscription> Subscription { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = "1", Name = "Employee", NormalizedName = "EMPLOYEE" },
                new IdentityRole { Id = "2", Name = "Member", NormalizedName = "MEMBER" }
            );

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Employee>(e => e.UserId)
                .IsRequired();

            modelBuilder.Entity<Member>()
                .HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Member>(m => m.UserId)
                .IsRequired();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Id)
                .IsUnique();

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Member)
                .WithMany(m => m.Subscriptions)
                .HasForeignKey(s => s.MemberId);
            
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Membership)
                .WithOne(m => m.Subscription)
                .HasForeignKey<Subscription>(s => s.MembershipId);
        }

    }
}