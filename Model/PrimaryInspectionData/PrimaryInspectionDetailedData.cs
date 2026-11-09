using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class PrimaryInspectionDetailedData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public GeneralData General { get; set; } = new();
        public SkinData Skin { get; set; } = new();
        public RespiratoryData Respiratory { get; set; } = new();
        public CardiologyData Cardiology { get; set; } = new();
        public AbdomenData Abdomen { get; set; } = new();
        public NeuroData Neuro { get; set; } = new();
        public AnamnesisVitaeData AnamnesisVitae { get; set; } = new();
    }
}
