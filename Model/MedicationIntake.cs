using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class MedicationIntake : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MedicationAssignmentId { get; set; }
        [ForeignKey(nameof(MedicationAssignmentId))]
        public virtual MedicationAssignment MedicationAssignment { get; set; } = null!;

        [Required]
        public DateTime IntakeDate { get; set; }          // дата, за которую ставится галочка

        public bool Taken { get; set; } = false;          // принято ли

        [MaxLength(200)]
        public string? Notes { get; set; }                // примечание медсестры
    }
}