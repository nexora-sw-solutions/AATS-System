using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AATS.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
