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
            builder.Property(p=> p.Login).HasMaxLength(15);
            builder.Property(p=>p.ImgURL).IsRequired(false);
        }
    }
}