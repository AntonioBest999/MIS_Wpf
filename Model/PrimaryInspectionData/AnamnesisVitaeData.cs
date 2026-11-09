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
    public class AnamnesisVitaeData
    {
        [Key]
        public int PrimaryInspectionId { get; set; }

        [ForeignKey(nameof(PrimaryInspectionId))]
        public virtual PrimaryInspection PrimaryInspection { get; set; } = null!;
        public string Hepatitis { get; set; } = "отрицает";
        public string Allergy { get; set; } = "не отягощён";
        public string AllergyDescription { get; set; } = "";
        public string HIV { get; set; } = "отрицает";
        public string Tuberculosis { get; set; } = "отрицает";
        public string MPC { get; set; } = "отрицает";
    }
}
