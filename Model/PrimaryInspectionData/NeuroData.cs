using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class NeuroData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string Consciousness { get; set; } = "";
        public bool Hallucinations { get; set; }
        public string HallucinationsDesc { get; set; } = "";
        public string NeuroSymptoms { get; set; }
        public string NeuroDesc { get; set; } = "";
        public string MentalStatusChanged { get; set; }
        public string MentalDesc { get; set; } = "";
        public string MeningealSymptoms { get; set; }
        public string MeningealDesc { get; set; } = "";
    }
}
