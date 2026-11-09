using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class CardiologyData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string HeartRhythm { get; set; } = "";
        public string Tones { get; set; } = "";
   //     public string Borders { get; set; } = "";
     //   public string PulseSymmetric { get; set; } = "";
        public string PulseRhythmic { get; set; } = "";
        public string PulseFilling { get; set; } = "";
        public string PulseForm { get; set; } = "";
        public string BordersDescription { get; set; } = "";
        public string HeartRate { get; set; } = "";
        public string PulsRate { get; set; } = "";
        public string BloodPressure { get; set; } = "";
        public string NormalBloodPressure { get; set; } = "";
    }
}
