using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class AbdomenData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string Soft { get; set; } = "";
        public string Painful { get; set; }= "";
        public string PainLocation { get; set; } = "";
        public string PeritonealIrritation { get; set; } = "";
        public string Notes { get; set; } = "";
        public string AbdomenEnlarged { get; set; } = "";
        public string AbdomenCause { get; set; } = "";
        public string LiverProtruding { get; set; } = "";
        public string LiverProtrusion { get; set; } = "";
        public string LiverPainful { get; set; } = "";
        public string SpleenPalpable { get; set; } = "";
        public string SpleenDescription { get; set; } = "";
        public string UrinationAbnormal { get; set; } = "";
        public string UrinationDescription { get; set; } = "";
        public string StoolAbnormal { get; set; } = "";
        public string StoolDescription { get; set; } = "";
    }
}
