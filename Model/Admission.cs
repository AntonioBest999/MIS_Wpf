using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Admission
    {
        public int Id { get; set; }

        [Required]
        //  public int PatientId { get; set; }
        public string PatientId { get; set; } = null!;
        public Patient Patient { get; set; } = null!;

        [Required]
        public DateTime AdmissionDateTime { get; set; } = DateTime.Now;

        [Required]
        public AdmissionSourceType SourceType { get; set; }

        [MaxLength(500)]
        public string? SourceDetails { get; set; } 

        [MaxLength(1000)]
        public string? Complaint { get; set; } 

        [MaxLength(300)]
        public string? DiagnosisOnAdmission { get; set; }

        [MaxLength(500)]
        public string? ProvidedCare { get; set; }

        public bool IsActive { get; set; } = true;
        [MaxLength(300)]
        public string? DeliveredBy { get; set; }       

        [MaxLength(50)]
        public string? AccompanyingSheetNumber { get; set; }
        [MaxLength(50)]
        public string Status { get; set; } = "Поступил";


    }
    public enum AdmissionSourceType
    {
        [Display(Name = "Доставлен СМП")]
        SMP,

        [Display(Name = "Самообращение")]
        SelfAppeal,

        [Display(Name = "Переведен из ...")]
        Transfer,

        [Display(Name = "Уточнение...")]
        Other
    }
}
