using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class LabOrder : AuditableEntity
    {
        [Key]
        public int LabOrderId { get; set; }

        [Required]
        public string PatientId { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;
        public int? PrimaryInspectionId { get; set; }
        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; }

        public int? HospitalizationId { get; set; }

        public DateTime OrderedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string OrderedById { get; set; } = string.Empty;     // EmployeeId врача

        public LabOrderStatus Status { get; set; } = LabOrderStatus.Ordered;

        public virtual ICollection<LabOrderItem> Items { get; set; } = new List<LabOrderItem>();
    }

    public enum LabOrderStatus
    {
        Ordered = 1,      // Назначен
        InProgress = 2,   // В работе
        Completed = 3,    // Выполнен
        Cancelled = 4     // Отменён
    }
}