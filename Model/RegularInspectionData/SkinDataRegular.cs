using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class SkinDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string Color { get; set; } = "";               
        public string Moisture { get; set; } = "";            
        public string HasRash { get; set; } = "";             
        public string RashLocation { get; set; } = "";        
        public string HasCyanosis { get; set; } = "";         
        public string CyanosisLocation { get; set; } = "";    
        public string HasEdema { get; set; } = "";            
        public string EdemaLocation { get; set; } = "";       
        public string LymphNodesPalpable { get; set; } = "";  
        public string LymphNodesLocation { get; set; } = "";  
        public string SkinNotes { get; set; } = "";
    }
}
