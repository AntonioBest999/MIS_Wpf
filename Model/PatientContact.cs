using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class PatientContact : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        public string PatientId { get; set; } = null!;

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Relationship { get; set; }   
                
        public virtual Patient Patient { get; set; } = null!;
    }
}
