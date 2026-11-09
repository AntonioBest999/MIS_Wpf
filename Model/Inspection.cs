using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ЦРБ_и_все_все_все.Классы.RegularInspectionData;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Inspection : AuditableEntity 
    {
        [Key]
        public int Id { get; set; }
        public int MedicalRecordId { get; set; }
        public string DoctorId { get; set; }
        [ForeignKey(nameof(DoctorId))]
        public virtual Employee Doctor { get; set; }
        public DateTime Date { get; set; }
        public string Diagnosis { get; set; }
        [MaxLength(250)]
        public string? CurrentDiagnosis { get; set; }
        [MaxLength(1000)]
        public string? SecondaryDiagnosis { get; set; }
        public string Notes { get; set; }
        
        public virtual VitalSignsDataRegular? VitalSigns { get; set; }
        public virtual GeneralStatusDataRegular? GeneralStatus { get; set; }
        public virtual SkinDataRegular? Skin { get; set; }
        public virtual RespiratoryDataRegular? Respiratory { get; set; }
        public virtual CardiologyDataRegular? Cardiology { get; set; }
        public virtual GastrointestinalDataRegular? Gastrointestinal { get; set; }
        public virtual NeurologicalDataRegular? Neurological { get; set; }
        public enum InspectionType
        {
            Regular,      // Обычный осмотр
            Consultation,  // Консилиум
            SecondOpinion  // консультация другого специалиста
        }

        public string Type { get; set; } = InspectionType.Regular.ToString();       
        public bool IsApproved { get; set; } = false;               
        public virtual ICollection<Employee> ConsultingDoctors { get; set; } = new List<Employee>();
                
        public virtual ICollection<DiagnosisHistory> DiagnosisHistories { get; set; } = new List<DiagnosisHistory>();
    }
}
