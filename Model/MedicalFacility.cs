using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class MedicalFacility : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Code { get; set; }
        public MedicalFacilityType Type { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public virtual ICollection<Department> Departments { get; set; } = new List<Department>();
        public virtual ICollection<PatientVisit> Visits { get; set; } = new List<PatientVisit>();
    }

    public enum MedicalFacilityType
    {
        Hospital,
        Polyclinic,
        PrimaryCare,
        WomensClinic,
        Other
    }

    public class PatientVisit : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        public string PatientId { get; set; }
        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }
        public int FacilityId { get; set; }  
        [ForeignKey(nameof(FacilityId))]
        public virtual MedicalFacility Facility { get; set; }
        public VisitType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; }
        public string BedNumber { get; set; }
        public virtual EMC? Emc { get; set; }  
    }

    public enum VisitType
    {
        Outpatient,
        Inpatient,
        Emergency,
        DayHospital
    }
}
