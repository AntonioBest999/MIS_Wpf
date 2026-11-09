using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Department : AuditableEntity
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // === Заведующий отделением ===
        public string? DepAheadId { get; set; }                    // string — правильно

        [ForeignKey(nameof(DepAheadId))]                           // ← ОБЯЗАТЕЛЬНО!
        public virtual Employee? DepAhead { get; set; }            // ← nullable

        public DepartmentType Type { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<Hospitalization> Hospitalizations { get; set; } = new List<Hospitalization>();

        public enum DepartmentType
        {
            MixedTrauma,
            Surgical,
            PurulentSeptic,
            HeadNeckReconstructiveSurgery,
            Oncology,
            Urology,
            OtolaryngologyOphthalmology,
            Pulmonology,
            Cardiology,
            Infectious,
            Psychiatric,
            Gastroenterology,
            Chemotherapy,
            Hematology,
            Admissions,
            TraumaPoint,
            OperatingBlock,
            PathologicalAnatomical,
            DiagnosticUnits,
            ClinicalDiagnosticLaboratory,
            CytologicalLaboratory,
            HistologicalLaboratory,
            XRay,
            Endoscopic,
            Endocrinology,
            ResuscitationAnesthesiology,
            BloodTransfusion,
            Administration
        }
    }
}