using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class LabOrderItem
    {
        [Key]
        public int LabOrderItemId { get; set; }

        public int LabOrderId { get; set; }
        [ForeignKey(nameof(LabOrderId))]
        public virtual LabOrder LabOrder { get; set; } = null!;

        public int LabParameterId { get; set; }
        [ForeignKey(nameof(LabParameterId))]
        public virtual LabParameter Parameter { get; set; } = null!;

        // Результаты заполняет лаборант
        [MaxLength(50)]
        public string? Result { get; set; }

        [MaxLength(200)]
        public string? Comment { get; set; }

        public DateTime? PerformedAt { get; set; }

        public string? PerformedById { get; set; }        // ID лаборанта
    }
}