using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class VitalSignsDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string Temperature { get; set; } = "";    
        public string PulseRate { get; set; } = "";        
        public string HeartRate { get; set; } = "";       
        public string BloodPressure { get; set; } = "";    
        public string NormalBloodPressure { get; set; } = "";            
    }
}
