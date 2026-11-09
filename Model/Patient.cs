using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;

namespace ЦРБ_и_все_все_все.Классы
{
    [Index(nameof(FullName), nameof(BirthDate), nameof(PhoneNumber))]
    public class Patient : AuditableEntity
    {
      //  public int Id { get; set; }
        [Key]
        public string PatientId { get; set; } = GeneratePatientId();

        [Required, MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public DateOnly BirthDate { get; set; }

        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? PassportNumber { get; set; }

        [MaxLength(100)]
        public string? AlternativeDocumentType { get; set; }

        [MaxLength(50)]
        public string? AlternativeDocumentNumber { get; set; }
        
        [MaxLength(500)]
        public string? InsuranceHistory { get; set; }        

        [MaxLength(30)]
        public string PatientStatus { get; set; } = "Работник";   

        [MaxLength(200)]
        public string? WorkPlace { get; set; }

        [MaxLength(100)]
        public string? Position { get; set; }
        [MaxLength(500)]
        public string? DisabilityReason { get; set; }
        [MaxLength(100)]
        public string? ApproximateAge { get; set; }
        [MaxLength(100)]
        public string? CurrentDiagnosis { get; set; }
       

        [NotMapped]
        public bool IsCritical { get; set; } = false;

        [NotMapped]
        public string CurrentBed { get; set; } = "—";

        [NotMapped]
        public string DepartmentName { get; set; } = "";

        public bool IsUnknown { get; set; } = false;

        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public virtual ICollection<PatientContact> Contacts { get; set; } = new List<PatientContact>();
        public virtual ICollection<PrimaryInspection> PrimaryInspections { get; set; } = new List<PrimaryInspection>();
        public virtual ICollection<Hospitalization> Hospitalizations { get; set; } = new List<Hospitalization>();
        public virtual ICollection<Admission> Admissions { get; set; } = new List<Admission>();

        [NotMapped]       
        public string AgeDisplay => CalculateAge(BirthDate);

        private string CalculateAge(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
                age--;                      

            return $"Возраст: {age} ";
        }
        public Patient()
        {
            PatientId = GeneratePatientId();
        }

        private static string GeneratePatientId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ23456789";
            var random = new Random();

            char first = chars[random.Next(26)];                 // случайная буква A-Z
            char last = chars[random.Next(26)];                  // случайная буква A-Z

            var middle = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray()); // 10 символов

            return $"{first}{middle}{last}";
        }
    }
}