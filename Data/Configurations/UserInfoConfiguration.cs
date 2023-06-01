using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlayAndConnect.Models;

namespace PlayAndConnect.Data.Configurations
{
    public class UserInfoConfiguration
    {
        public void Configure(EntityTypeBuilder<UserInfo> builder)
        {
            builder.Property(p=> p.Id).ValueGeneratedOnAdd();
            builder.Property(p=>p.ImagePath).IsRequired(false);
            builder.Property(p=> p.Age).IsRequired();
            builder.Property(p=> p.Name).IsRequired();
            
        }
    }
}