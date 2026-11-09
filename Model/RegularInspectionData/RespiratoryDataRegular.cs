using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class RespiratoryDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string BreathingRhythm { get; set; } = "";     
        public string PathologicalBreathing { get; set; } = ""; 
        public string RespiratoryRate { get; set; } = "";      
        public string HasWheezes { get; set; } = "";           
        public string Percussion { get; set; } = "";
        public string PercussionDesc { get; set; } = "";
        public string Cough { get; set; } = "";
          
        public string HasWeakening { get; set; } = "";         
        public string WeakeningLocation { get; set; } = "";    
        public string AuscultationType { get; set; } = "";     
      
        public string AuscultationDescription { get; set; } = ""; 
        public string RespiratoryNotes { get; set; } = "";                
    }
}
