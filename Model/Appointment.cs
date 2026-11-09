using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Appointment : AuditableEntity
    {
        [Key]
        public int Id { get; set; }  
        public int EmcId { get; set; }  
        [ForeignKey(nameof(EmcId))]
        public virtual EMC Emc { get; set; }  
        public string Type { get; set; }
        public string Name { get; set; }
        public string? Dosage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public string? ExecutedById { get; set; }
        [ForeignKey(nameof(ExecutedById))]
        public virtual Employee? ExecutedBy { get; set; }
        public DateTime? ExecutionDate { get; set; }
    }
}
