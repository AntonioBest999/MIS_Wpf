using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class GeneralData
    {

        [Key]
        public int PrimaryInspectionId { get; set; }
        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string GeneralConditionSatisfactory { get; set; } = "";
        public string Position { get; set; } = "";
        public string Stability { get; set; } = "";
        public string Constitution { get; set; } = "";
        public string Consciousness { get; set; } = "";
        public string Hallucinations { get; set; } = "";
        public string BodyBuild { get; set; } = "";
    }
}
