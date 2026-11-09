using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class NeurologicalDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string MentalStatus { get; set; } = "";     
        public string MentalDescription { get; set; } = "";   
        public string NeurologicalSymptoms { get; set; } = "";
        // public string NeurologicalDescription { get; set; } = "";
        public string MeningealSigns { get; set; } = "";      
        //public string MeningealDescription { get; set; } = ""; 
        public string Notes { get; set; } = "";               
    }
}
