using Chat.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chat.Web.Data.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");

            builder.Property(s => s.Name).IsRequired().HasMaxLength(50);

            builder.HasOne(s => s.Admin)
                .WithMany(u => u.Rooms)
                .IsRequired();
        }
    }
}
