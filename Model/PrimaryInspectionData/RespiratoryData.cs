using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class RespiratoryData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public int Rate { get; set; }
        public string Rhythm { get; set; } = "";
        public string RespiratoryRate { get; set; } = "";
        public string PathologicalBreathing { get; set; }
        public string PathologicalType { get; set; } = "";
        public string Percussion { get; set; } = "";
        public string PercussionDesc { get; set; } = "";
        public string Auscultation { get; set; } = "";
        public string AuscultationDesc { get; set; } = "";
      
        public string Cough { get; set; }
   //     public string CoughDescription { get; set; } = "";
        public string Wheezes { get; set; }
       
        public string Weakening { get; set; }
        public string WeakeningLocation { get; set; } = "";
        public string BreathingParticipation { get; set; }
        public string ParticipationDescription { get; set; } = "";
        public string PathologicalNoises { get; set; }
        public string NoisesType { get; set; } = "";
    //    public string NoisesDescription { get; set; } = "";
        public string Chest { get; set; } = "";
        public string ChestDescription { get; set; } = "";
        public string PalpationPainful { get; set; } = "";
        public string PalpationLocation { get; set; } = "";
        public string Notes { get; set; } = "";
    }
}
