using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЦРБ_и_все_все_все.Классы.PrimaryInspectionData
{
    public class SkinData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string Color { get; set; } = "";
        public string Moisture { get; set; } = "";
        public string HasRash { get; set; } = "";
        public string RashLocation { get; set; } = "";
        public string HasCyanosis { get; set; } = "";
        public string CyanosisLocation { get; set; } = "";
        public string Temperature { get; set; } = "";
        public string HasEdema { get; set; } = "";
        public string EdemaLocation { get; set; } = "";
        public string LymphNodesPalpable  { get;set; } = "";
        public string LymphNodesLocation { get; set; } = "";
    }
}
