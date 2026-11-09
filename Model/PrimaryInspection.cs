using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;

namespace ЦРБ_и_все_все_все.Классы
{
    [Index(nameof(PatientId))]
    [Index(nameof(DoctorId))]
    [Index(nameof(InspectionDateTime))]
    public class PrimaryInspection : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = null!;
       
        [Required]
        public string DoctorId { get; set; } = null!;
        public virtual Employee? Doctor { get; set; }

        [MaxLength(1000)]
        public string Complaint { get; set; } = string.Empty;
        public string DiagnosisOnAdmission { get; set; } = string.Empty;
        public string SmpDiagnosis { get; set; } = string.Empty;
      //  public string? SmpDiagnosis { get; set; }          // ← добавьте
        public string? ProvidedCare { get; set; }
        public string ProvidedCareByPO { get; set; } = string.Empty;
        public DateTime InspectionDateTime { get; set; } = DateTime.UtcNow;

        public virtual Patient Patient { get; set; } = null!;
      //  public virtual Employee Doctor { get; set; } = null!;

        [MaxLength(500)]
        public string PreliminaryDiagnosis { get; set; } = string.Empty;

        // Все детальные данные храним в JSON
        //[Column(TypeName = "jsonb")]
        //public string DetailedDataJson { get; set; } = "{}";
        [MaxLength(1500)]
        public string AnamnesisMorbi { get; set; } = string.Empty;

        [MaxLength(100)]
        public string GeneralCondition { get; set; } = "Удовлетворительное";

        [MaxLength(100)]
        public string Consciousness { get; set; } = "Ясное";

        //[MaxLength(300)]
        //public string RespiratorySyndrome { get; set; } = string.Empty;

        //[MaxLength(300)]
        //public string CardiovascularSyndrome { get; set; } = string.Empty;

        //[MaxLength(300)]
        //public string AbdominalSyndrome { get; set; } = string.Empty;

   //   public string NeurologicalSyndrome { get; set; } = string.Empty;

        public bool HasFever { get; set; } = false;
    //    public decimal? Temperature { get; set; }

    //    [MaxLength(20)]
        //public string? BloodPressure { get; set; }

      //  public int? HeartRate { get; set; }
    //    public int? RespiratoryRate { get; set; }
        // public virtual Employee Doctor { get; set; } = null!;
        [NotMapped]
        public PrimaryInspectionDetailedData DetailedData { get; set; } = new();
    }
}
