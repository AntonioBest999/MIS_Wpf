using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class DischargeSummary : AuditableEntity //ВЫПИСНОЙ ЭПИКРИЗ
    {
        [Key]
        public int Id { get; set; }
        public int MedicalRecordId { get; set; }
        public DateTime DischargeDate { get; set; }
        public string? FinalDiagnosis { get; set; }
        public string? Recommendations { get; set; }
        public string? DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual Employee? Doctor { get; set; }
    }
}
