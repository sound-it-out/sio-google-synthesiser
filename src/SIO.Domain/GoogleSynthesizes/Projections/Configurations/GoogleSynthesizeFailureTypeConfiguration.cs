using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIO.Infrastructure.EntityFrameworkCore.EntityConfiguration;

namespace SIO.Domain.GoogleSynthesizes.Projections.Configurations
{
    internal sealed class GoogleSynthesizeFailureTypeConfiguration : IProjectionTypeConfiguration<GoogleSynthesizeFailure>
    {
        public void Configure(EntityTypeBuilder<GoogleSynthesizeFailure> builder)
        {
            builder.ToTable(nameof(GoogleSynthesizeFailure));
            builder.HasKey(epf => epf.Subject);
            builder.Property(epf => epf.Subject)
                   .ValueGeneratedNever();
            builder.HasIndex(epf => epf.DocumentSubject);
        }
    }
}
