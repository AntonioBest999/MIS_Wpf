using System.Collections.ObjectModel;
using System.Windows.Input;
using MisWpf.Infrastructure;
using MisWpf.Services;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.ViewModels
{
    /// <summary>Карточка пациента в очереди приёмного отделения.</summary>
    public class QueueCardVm : ObservableObject
    {
        private bool _isSelected;
        private string _timer = "";
        private string _timerKind = "normal";
        private string _stripeKind = "neutral";

        public QueueItem Item { get; }
        public QueueStage Stage { get; }
        public int AdmissionId => Item.Admission.Id;
        public ICommand SelectCommand { get; }

        public QueueCardVm(QueueItem item, QueueStage stage, Action<QueueCardVm> select)
        {
            Item = item;
            Stage = stage;
            SelectCommand = new RelayCommand(() => select(this));
            Tags = BuildTags(item);
            Update(DateTime.UtcNow);
        }

        public string Fio => Item.Patient.FullName;
        public string Sub => $"{Format.Age(Item.Patient)} · {Format.Sex(Item.Patient.Gender)} · {Format.AdmissionNumber(AdmissionId)}";
        public string SourceText => Format.Source(Item.Admission.SourceType);
        public bool IsSmp => Item.Admission.SourceType == AdmissionSourceType.SMP;
        public List<TagVm> Tags { get; }
        public bool NoTags => Tags.Count == 0;
        public bool IsReady => Stage == QueueStage.ReadyForDecision;
        public bool IsReturned => Stage == QueueStage.Returned;
        public string ReturnText => Item.RejectedHospitalization == null ? ""
            : "Отказ: " + QueueService.ShortDept(Item.RejectedHospitalization.Department?.Name);

        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public string Timer { get => _timer; private set => Set(ref _timer, value); }
        /// <summary>normal / warn / danger</summary>
        public string TimerKind { get => _timerKind; private set => Set(ref _timerKind, value); }
        /// <summary>neutral / warn / danger / ok — цвет полосы слева</summary>
        public string StripeKind { get => _stripeKind; private set => Set(ref _stripeKind, value); }

        public double Minutes(DateTime nowUtc) => (nowUtc - Format.AsUtc(Item.Admission.AdmissionDateTime)).TotalMinutes;

        public void Update(DateTime nowUtc)
        {
            var elapsed = nowUtc - Format.AsUtc(Item.Admission.AdmissionDateTime);
            Timer = Format.Elapsed(elapsed);
            double m = elapsed.TotalMinutes;
            TimerKind = m >= AppConfig.CriticalMinutes ? "danger" : m >= AppConfig.WarnMinutes ? "warn" : "normal";
            StripeKind = IsReturned ? "danger"
                       : IsReady ? "ok"
                       : TimerKind == "danger" ? "danger"
                       : TimerKind == "warn" ? "warn" : "neutral";
        }

        private static List<TagVm> BuildTags(QueueItem item)
        {
            var tags = new List<TagVm>();
            bool female = Format.IsFemale(item.Patient.Gender);
            foreach (var o in item.LabOrders)
            {
                bool done = o.Status == LabOrderStatus.Completed;
                bool flag = done && o.Items.Any(i => i.Parameter != null && LabWorkService.Flag(i.Parameter, female, i.Result) != "");
                foreach (var cat in o.Items.Where(i => i.Parameter != null).Select(i => i.Parameter.Category).Distinct())
                    tags.Add(new TagVm { Text = cat + (done ? " ✓" : " ⏳"), Kind = done ? (flag ? "flag" : "ok") : "wait" });
            }
            foreach (var a in item.Instrumental)
            {
                bool done = OrderService.IsInstrumentalDone(a);
                tags.Add(new TagVm { Text = OrderService.InstrumentalName(a.DocumentType) + (done ? " ✓" : " ⏳"), Kind = done ? "ok" : "wait" });
            }
            return tags;
        }
    }

    public class QueueGroupVm : ObservableObject
    {
        public string Title { get; }
        public string Kind { get; }
        public QueueStage Stage { get; }
        public ObservableCollection<QueueCardVm> Cards { get; } = new();
        public int Count => Cards.Count;
        public bool IsVisible => Cards.Count > 0;

        public QueueGroupVm(string title, string kind, QueueStage stage)
        {
            Title = title;
            Kind = kind;
            Stage = stage;
        }

        public void Replace(IEnumerable<QueueCardVm> cards)
        {
            Cards.Clear();
            foreach (var c in cards) Cards.Add(c);
            RaiseMany(nameof(Count), nameof(IsVisible));
        }
    }

    /// <summary>Строка назначенного исследования (центр и правая колонка).</summary>
    public class OrderRowVm
    {
        public string Name { get; init; } = "";
        public string KindText { get; init; } = "";
        public string StatusText { get; init; } = "";
        /// <summary>ok / wait / flag / ordered</summary>
        public string StatusKind { get; init; } = "ordered";
        public LabOrder? Lab { get; init; }
        public Attachment? Instrumental { get; init; }
        public ICommand OpenCommand { get; init; } = null!;
    }

    public class ResultRowVm
    {
        public string Name { get; init; } = "";
        public string Value { get; init; } = "";
        public string Ref { get; init; } = "";
        public string Flag { get; init; } = "";
        public bool IsFlagged => Flag != "";
    }

    public class StepVm : ObservableObject
    {
        private bool _isCurrent;
        private bool _isDone;
        public int Number { get; }
        public string Label { get; }
        public ICommand GoCommand { get; }

        public StepVm(int number, string label, Action<int> go)
        {
            Number = number;
            Label = label;
            GoCommand = new RelayCommand(() => go(number));
        }

        public bool IsCurrent { get => _isCurrent; set { if (Set(ref _isCurrent, value)) Raise(nameof(Mark)); } }
        public bool IsDone { get => _isDone; set { if (Set(ref _isDone, value)) Raise(nameof(Mark)); } }
        public string Mark => IsDone && !IsCurrent ? "✓" : Number.ToString();
    }

    public class DeptVm : ObservableObject
    {
        private bool _isSelected;
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string PatientsText { get; init; } = "";
        public bool IsBlocked { get; init; }
        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public ICommand PickCommand { get; init; } = null!;
    }
}
