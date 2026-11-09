using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Hospitalization : AuditableEntity
    {
        [Key]
        public int Id { get; set; }
        //   public int PatientId { get; set; }  
        public string PatientId { get; set; } = null!;
        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; }
        public DateTime AdmissionDate { get; set; }
      
        public DateTime? DischargeDate { get; set; }
        public int DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; }
        public string? CurrentBed { get; set; }
        public virtual EMC Emc { get; set; }
       
        [MaxLength(50)]                     
        public string MedicalRecordNumber { get; set; }
        [MaxLength(50)]
        public string SourceType { get; set; } = string.Empty;          

        [MaxLength(500)]
        public string? SourceDetails { get; set; }                      

        [MaxLength(200)]
        public string? DeliveredBy { get; set; }                        

        [MaxLength(50)]
        public string? AccompanyingSheetNumber { get; set; }
        public string? AttendingDoctorId { get; set; }  
        [ForeignKey(nameof(AttendingDoctorId))]
        public virtual Employee? AttendingDoctor { get; set; }
        public string Status { get; set; } = "";
        [MaxLength(250)]
        public string? AdmissionDiagnosis { get; set; }
        [MaxLength(250)]
        public string? CurrentDiagnosis { get; set; }
        [MaxLength(1000)]
        public string? SecondaryDiagnosis { get; set; }
        public int? PrimaryInspectionId { get; set; }
        public virtual PrimaryInspection? PrimaryInspection { get; set; }

        public int? FinalAttendingDoctorId { get; set; }

       
        public virtual ICollection<HospitalizationHistory> Histories { get; set; } = new List<HospitalizationHistory>();
    }
}
