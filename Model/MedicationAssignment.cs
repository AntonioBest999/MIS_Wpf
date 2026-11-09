using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class MedicationAssignment : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = null!;
        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;

        [Required]
        public int HospitalizationId { get; set; }
        [ForeignKey(nameof(HospitalizationId))]
        public virtual Hospitalization Hospitalization { get; set; } = null!;

        [Required, MaxLength(200)]
        public string DrugName { get; set; } = string.Empty;     
        [MaxLength(100)]
        public string Dose { get; set; } = string.Empty;
        public string ScheduledTimes { get; set; } = string.Empty; 

        [MaxLength(50)]
        public string Route { get; set; } = string.Empty;        

        [MaxLength(100)]
        public string Frequency { get; set; } = string.Empty;    

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public string? Notes { get; set; }

        // Навигационное свойство для фактов приёма
        public virtual ICollection<MedicationIntake> Intakes { get; set; } = new List<MedicationIntake>();
    }
}