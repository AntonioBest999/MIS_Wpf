using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.RegularInspectionData
{
    public class GeneralStatusDataRegular
    {
        [Key]
        public int InspectionId { get; set; }
        public virtual Inspection? Inspection { get; set; } = null!;

        public string ConditionSeverity { get; set; } = "";          
        public string Stability { get; set; } = "";                  
        public string Consciousness { get; set; } = "";              
        public string Substance { get; set; } = "";
        public string HasHallucinations { get; set; } = "";
        public string HallucinationsDesc { get; set; } = "";         
        public string StatusDynamics { get; set; } = "";             
        public string Notes { get; set; } = "";
    }
}
