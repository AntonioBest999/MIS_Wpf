using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ЦРБ_и_все_все_все.Классы
{
    public class Attachment : AuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = null!;

        [ForeignKey(nameof(PatientId))]
        public virtual Patient Patient { get; set; } = null!;

        public int? AdmissionId { get; set; }
        public int? HospitalizationId { get; set; }

        [Required, MaxLength(50)]
        public AttachmentType DocumentType { get; set; }

        [Required, MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string RelativePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        [MaxLength(100)]
        public string? ContentHash { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

       
        [MaxLength(50)]
        public string? InterpretingDoctorId { get; set; }

        [ForeignKey(nameof(InterpretingDoctorId))]
        public virtual Employee? InterpretingDoctor { get; set; }

        public DateTime? InterpretationDate { get; set; }

        [MaxLength(4000)]
        public string? InterpretationText { get; set; }

        public AttachmentStatus Status { get; set; } = AttachmentStatus.Draft;

        
        public virtual Admission? Admission { get; set; }
        public virtual Hospitalization? Hospitalization { get; set; }

        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AttachmentStatus
    {
        Draft = 0,
        InProgress = 1,
        Finalized = 2,
        NeedsRevision = 3,
        Archived = 4
    }

    public enum AttachmentType
    {
        ECG = 0,
        ChestXRay = 1,
        Ultrasound = 2,
        CT = 3,
        MRI = 4,
        LabScan = 5,
        ConsultationNote = 6,
        Other = 99
    }
}