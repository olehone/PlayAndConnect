using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayAndConnect.Models;

namespace PlayAndConnect.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(p=> p.Id).ValueGeneratedOnAdd();
            builder.Property(p=> p.Login).HasMaxLength(15).IsRequired();
            builder.HasOne(u=> u.Info).WithOne(i=> i.User).HasForeignKey<UserInfo>(x=> x.UserId).HasPrincipalKey<User>(u=> u.Id);
        }
    }
}