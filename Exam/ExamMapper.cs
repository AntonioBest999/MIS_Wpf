using ЦРБ_и_все_все_все.Классы;
using ЦРБ_и_все_все_все.Классы.PrimaryInspectionData;

namespace MisWpf.Exam
{
    /// <summary>
    /// Переносит заполненный осмотр в существующие таблицы БД (те же, что пишет WinForms-приложение).
    /// В эти поля WinForms записывает готовые фрагменты текста протокола, поэтому и здесь пишем фразы.
    /// Критериев, для которых в таблицах нет отдельного поля (например, фотореакция или поза Ромберга),
    /// фразы складываются в поля-описания (NeuroDesc, MentalDesc, Notes).
    /// </summary>
    public static class ExamMapper
    {
        private static string P(SectionVm s, string id)
        {
            var c = s.Get(id);
            return c.IsFilled ? c.Phrase + ". " : "";
        }

        private static string Join(SectionVm s, params string[] ids) => string.Concat(ids.Select(id => P(s, id)));

        private static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s[1..];

        public static PrimaryInspection BuildInspection(ExamState e, Admission admission, string doctorId)
        {
            var g = e.Section("general");
            var cond = g.Get("cond");
            var cons = g.Get("cons");
            return new PrimaryInspection
            {
                PatientId = admission.PatientId,
                DoctorId = doctorId,
                InspectionDateTime = DateTime.UtcNow,
                Complaint = Trunc(e.Complaint, 1000),
                AnamnesisMorbi = Trunc(e.AnamnesisMorbi, 1500),
                SmpDiagnosis = admission.DiagnosisOnAdmission ?? "",
                ProvidedCare = admission.ProvidedCare,
                ProvidedCareByPO = BuildCareText(e),
                PreliminaryDiagnosis = Trunc(e.Diagnosis, 500),
                DiagnosisOnAdmission = e.Diagnosis ?? "",
                GeneralCondition = Trunc(cond.IsFilled ? Cap(cond.Value) : "Удовлетворительное", 100),
                Consciousness = Trunc(cons.IsFilled ? Cap(cons.ValueWithDetails) : "Ясное", 100),
                HasFever = e.HasFever
            };
        }

        /// <summary>Помощь в ПО + исход (для отказа/перевода — причина и рекомендации, отдельных полей для них в БД пока нет).</summary>
        private static string BuildCareText(ExamState e)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(e.CareInReception)) parts.Add(e.CareInReception.Trim());
            switch (e.Decision)
            {
                case DecisionKind.NotNeeded:
                    parts.Add("В госпитализации не нуждается." + (string.IsNullOrWhiteSpace(e.RefusalReason) ? "" : " Причина: " + e.RefusalReason + "."));
                    if (!string.IsNullOrWhiteSpace(e.Recommendations)) parts.Add("Рекомендации: " + e.Recommendations.Trim());
                    break;
                case DecisionKind.TransferOut:
                    parts.Add("Переводится в " + e.TransferTo.Trim() + ".");
                    break;
            }
            return string.Join("\n", parts);
        }

        public static GeneralData General(ExamState e)
        {
            var s = e.Section("general");
            return new GeneralData
            {
                GeneralConditionSatisfactory = P(s, "cond"),
                Stability = P(s, "stab"),
                Consciousness = P(s, "cons"),
                Position = P(s, "pos"),
                BodyBuild = P(s, "build"),
                Constitution = "",
                Hallucinations = ""
            };
        }

        public static SkinData Skin(ExamState e)
        {
            var s = e.Section("skin");
            var temp = e.Section("general").Get("temp");
            return new SkinData
            {
                Color = P(s, "col"),
                Moisture = P(s, "hum"),
                Temperature = temp.IsFilled ? $"t° = {temp.Value} °C. " : "",
                HasRash = P(s, "rash"),
                RashLocation = "",
                HasCyanosis = P(s, "cyan"),
                CyanosisLocation = "",
                HasEdema = P(s, "edema"),
                EdemaLocation = "",
                LymphNodesPalpable = P(s, "ln"),
                LymphNodesLocation = ""
            };
        }

        public static RespiratoryData Respiratory(ExamState e)
        {
            var s = e.Section("resp");
            var rr = s.Get("rr");
            return new RespiratoryData
            {
                Chest = P(s, "chest"),
                ChestDescription = "",
                PalpationPainful = P(s, "palp"),
                PalpationLocation = "",
                BreathingParticipation = P(s, "part"),
                ParticipationDescription = "",
                Rhythm = P(s, "rhythm"),
                PathologicalBreathing = "",
                PathologicalType = "",
                Rate = rr.NumericValue.HasValue ? (int)Math.Round(rr.NumericValue.Value) : 0,
                RespiratoryRate = P(s, "rr"),
                Percussion = P(s, "perc"),
                PercussionDesc = "",
                Auscultation = P(s, "ausc"),
                AuscultationDesc = "",
                Cough = P(s, "cough"),
                Wheezes = P(s, "wh"),
                Weakening = P(s, "weak"),
                WeakeningLocation = "",
                PathologicalNoises = P(s, "noise"),
                NoisesType = "",
                Notes = Join(s, "spo2", "dysp")
            };
        }

        public static CardiologyData Cardiology(ExamState e)
        {
            var s = e.Section("cardio");
            return new CardiologyData
            {
                Tones = Join(s, "tones", "mur"),
                HeartRhythm = P(s, "rh"),
                HeartRate = P(s, "hr"),
                BloodPressure = P(s, "bp"),
                PulseFilling = P(s, "pulse"),
                PulsRate = "",
                PulseRhythmic = "",
                PulseForm = "",
                NormalBloodPressure = "",
                BordersDescription = ""
            };
        }

        public static AbdomenData Abdomen(ExamState e)
        {
            var s = e.Section("gi");
            return new AbdomenData
            {
                Soft = Join(s, "abd", "apalp"),
                Painful = "",
                PainLocation = "",
                PeritonealIrritation = P(s, "periton"),
                Notes = Join(s, "tongue", "perist"),
                AbdomenEnlarged = "",
                AbdomenCause = "",
                LiverProtruding = P(s, "liver"),
                LiverProtrusion = "",
                LiverPainful = "",
                SpleenPalpable = P(s, "spleen"),
                SpleenDescription = "",
                UrinationAbnormal = Join(s, "uri", "past"),
                UrinationDescription = "",
                StoolAbnormal = P(s, "stool"),
                StoolDescription = ""
            };
        }

        public static NeuroData Neuro(ExamState e)
        {
            var s = e.Section("neuro");
            var cons = e.Section("general").Get("cons");
            return new NeuroData
            {
                Consciousness = cons.IsFilled ? cons.ValueWithDetails : "",
                Hallucinations = false,
                HallucinationsDesc = "",
                MeningealSymptoms = P(s, "mening"),
                MeningealDesc = "",
                NeuroSymptoms = P(s, "focal"),
                NeuroDesc = Join(s, "pupil", "pupw", "photo", "speech", "power", "tone", "coord", "romberg"),
                MentalStatusChanged = P(s, "mental"),
                MentalDesc = Join(s, "gait", "smot", "alco")
            };
        }

        public static AnamnesisVitaeData AnamnesisVitae(ExamState e)
        {
            var s = e.Anamnesis;
            string V(string id) => s.Get(id).IsFilled ? s.Get(id).ValueWithDetails : "";
            var all = s.Get("all");
            return new AnamnesisVitaeData
            {
                Allergy = all.IsFilled ? all.Value : "",
                AllergyDescription = all.IsFilled ? all.DetailText.Trim() : "",
                HIV = V("hiv"),
                Tuberculosis = V("tb"),
                Hepatitis = V("hep"),
                MPC = V("mpc")
            };
        }

        private static string Trunc(string? s, int max)
        {
            s ??= "";
            s = s.Trim();
            return s.Length <= max ? s : s[..max];
        }
    }
}
