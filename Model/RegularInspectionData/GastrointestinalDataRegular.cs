using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class GastrointestinalDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        // Живот
        public string AbdomenTense { get; set; } = "";
        public string AbdomenPainful { get; set; } = "";     
        public string PainLocation { get; set; } = "";       
        public string AbdomenEnlarged { get; set; } = "";  
        public string AbdomenEnlargementCause { get; set; } = ""; 
        public string PeritonealIrritation { get; set; } = "";    
        public string PeritonealNotes { get; set; } = "";          
    //    public string AbdomenLocation { get; set; } = "";         
                                                         
      //  public string LiverPalpable { get; set; } = "";       
        public string LiverProtrusion { get; set; } = "";     
        public string LiverPainful { get; set; } = "";        
                                                        
        public string SpleenPalpable { get; set; } = "";      
        public string SpleenDescription { get; set; } = "";

        public string Stool { get; set; } = "";               
        public string StoolDescription { get; set; } = "";
        public string Urination { get; set; } = "";           
        public string UrinationDescription { get; set; } = "";
        public string Notes { get; set; } = "";               
    }
}
