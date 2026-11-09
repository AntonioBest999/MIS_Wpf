using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class LabParameter
    {
        [Key]
        public int LabParameterId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;           // HGB, WBC, GLU, TROP, CREA и т.д.

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;           // "Гемоглобин", "Глюкоза крови", "Тропонин I"

        [MaxLength(30)]
        public string Units { get; set; } = string.Empty;          // г/л, ммоль/л, нг/мл и т.д.

        public decimal? RefLow { get; set; }
        public decimal? RefHigh { get; set; }

        public decimal? RefLowFemale { get; set; }
        public decimal? RefHighFemale { get; set; }

        [Required, MaxLength(50)]
        public string Category { get; set; } = string.Empty;       // "ОАК", "ОАМ", "БАК", "Коагулограмма"

        public bool IsActive { get; set; } = true;
    }
}