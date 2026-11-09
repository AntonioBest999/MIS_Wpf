using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class DiagnosisHistory : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int InspectionId { get; set; }
        [ForeignKey(nameof(InspectionId))]
        public virtual Inspection Inspection { get; set; }
        public string PreviousDiagnosis { get; set; }
        public string NewDiagnosis { get; set; }
        public string ChangeReason { get; set; }
        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    }
}
