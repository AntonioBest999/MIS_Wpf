using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class HospitalizationHistory : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public int HospitalizationId { get; set; }
        [ForeignKey(nameof(HospitalizationId))]
        public virtual Hospitalization Hospitalization { get; set; } = null!;
        public int? PreviousDoctorId { get; set; }
        public int? NewDoctorId { get; set; }
        public int? PreviousDepartmentId { get; set; }
        public int? NewDepartmentId { get; set; }
        public string ChangeReason { get; set; } = string.Empty;  
    }
}
