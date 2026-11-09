using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class EMC : AuditableEntity

    {
        [Key]
        public int Id { get; set; }
        public int HospitalizationId { get; set; }
        [ForeignKey(nameof(HospitalizationId))]
        public virtual Hospitalization Hospitalization { get; set; }
        public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Surgery> Surgeries { get; set; } = new List<Surgery>();
        public virtual ICollection<LaboratoryResult> LaboratoryResults { get; set; } = new List<LaboratoryResult>();
        public virtual DischargeSummary DischargeSummary { get; set; }
    }
}
