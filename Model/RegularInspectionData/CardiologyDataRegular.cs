using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class CardiologyDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string HeartTones { get; set; } = "";            
        public string PulseFilling { get; set; } = "";
        public string PulseSymmetric { get; set; } = "";        
        public string PulseRhythm { get; set; } = "";        
        public string HeartRhythm { get; set; } = "";               
        public string BloodPressureNormal { get; set; } = "";   
        public string CardiologyNotes { get; set; } = "";                 
    }
}
