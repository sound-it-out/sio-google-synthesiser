using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIO.Infrastructure.EntityFrameworkCore.EntityConfiguration;

namespace SIO.Domain.GoogleSynthesizes.Projections.Configurations
{
    internal sealed class GoogleSynthesizeQueueTypeConfiguration : IProjectionTypeConfiguration<GoogleSynthesizeQueue>
    {
        public void Configure(EntityTypeBuilder<GoogleSynthesizeQueue> builder)
        {
            builder.ToTable(nameof(GoogleSynthesizeQueue));
            builder.HasKey(epq => epq.Subject);
            builder.Property(epq => epq.Subject)
                   .ValueGeneratedNever();
            builder.HasIndex(epf => epf.DocumentSubject);
        }
    }
}
