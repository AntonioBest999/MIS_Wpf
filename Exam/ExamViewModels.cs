using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using MisWpf.Infrastructure;

namespace MisWpf.Exam
{
    /// <summary>Вариант ответа в критерии (кнопка).</summary>
    public class OptionVm : ObservableObject
    {
        private bool _isSelected;
        public string Text { get; }
        public int Index { get; }
        public bool IsNormal => Index == 0;
        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public ICommand PickCommand { get; }

        public OptionVm(string text, int index, Action<int> pick)
        {
            Text = text;
            Index = index;
            PickCommand = new RelayCommand(() => pick(index));
        }
    }

    /// <summary>Маленький переключатель в уточнении (сторона или вариант).</summary>
    public class SmallOptionVm : ObservableObject
    {
        private bool _isSelected;
        public string Text { get; }
        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public ICommand PickCommand { get; }

        public SmallOptionVm(string text, Action<string> pick)
        {
            Text = text;
            PickCommand = new RelayCommand(() => pick(text));
        }
    }

    public class CriterionVm : ObservableObject
    {
        private int _selectedIndex = -1;
        private string _side = "";
        private string _chip = "";
        private string _detailText = "";
        private string _numberValue = "";

        public CriterionDef Def { get; }
        public string Label => Def.Label;
        public bool IsNumber => Def.Number != null;
        public bool IsOptions => Def.Number == null;
        public string Unit => Def.Number?.Unit ?? "";
        public string NumberHint => Def.Number?.Hint ?? "";

        public ObservableCollection<OptionVm> Options { get; } = new();
        public ObservableCollection<SmallOptionVm> Sides { get; } = new();
        public ObservableCollection<SmallOptionVm> Chips { get; } = new();

        /// <summary>Вызывается при любом изменении — раздел пересчитывает итог.</summary>
        public event Action? Changed;

        public CriterionVm(CriterionDef def)
        {
            Def = def;
            for (int i = 0; i < def.Options.Length; i++)
                Options.Add(new OptionVm(def.Options[i], i, Pick));
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (!Set(ref _selectedIndex, value)) return;
                foreach (var o in Options) o.IsSelected = o.Index == value;
                _side = ""; _chip = ""; _detailText = "";
                RebuildDetails();
                RaiseMany(nameof(Side), nameof(Chip), nameof(DetailText), nameof(HasDetails), nameof(HasDetailText), nameof(DetailHint), nameof(IsAbnormal));
                Changed?.Invoke();
            }
        }

        public string Side
        {
            get => _side;
            set { if (Set(ref _side, value)) { foreach (var s in Sides) s.IsSelected = s.Text == value; Changed?.Invoke(); } }
        }

        public string Chip
        {
            get => _chip;
            set { if (Set(ref _chip, value)) { foreach (var c in Chips) c.IsSelected = c.Text == value; Changed?.Invoke(); } }
        }

        public string DetailText
        {
            get => _detailText;
            set { if (Set(ref _detailText, value)) Changed?.Invoke(); }
        }

        public string NumberValue
        {
            get => _numberValue;
            set
            {
                if (!Set(ref _numberValue, value)) return;
                RaiseMany(nameof(NumberOutOfRange), nameof(IsAbnormal));
                Changed?.Invoke();
            }
        }

        private DetailDef? CurrentDetail =>
            _selectedIndex >= 0 && Def.Details.TryGetValue(_selectedIndex, out var d) ? d : null;

        public bool HasDetails => CurrentDetail != null;
        public bool HasDetailText => !string.IsNullOrEmpty(CurrentDetail?.TextHint);
        public string DetailHint => CurrentDetail?.TextHint ?? "";

        public double? NumericValue =>
            double.TryParse((_numberValue ?? "").Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : null;

        public bool NumberOutOfRange
        {
            get
            {
                var n = Def.Number;
                if (n == null || n.FreeText) return false;
                var v = NumericValue;
                if (v == null) return false;
                return (n.Low.HasValue && v < n.Low) || (n.High.HasValue && v > n.High);
            }
        }

        public string NormText
        {
            get
            {
                var n = Def.Number;
                if (n == null || !n.Low.HasValue || !n.High.HasValue) return "";
                return $"норма {n.Low.Value.ToString(CultureInfo.GetCultureInfo("ru-RU"))}–{n.High.Value.ToString(CultureInfo.GetCultureInfo("ru-RU"))}";
            }
        }

        public bool IsFilled => IsNumber ? !string.IsNullOrWhiteSpace(_numberValue) : _selectedIndex >= 0;
        public bool IsAbnormal => IsNumber ? NumberOutOfRange : _selectedIndex > 0;

        /// <summary>Выбранный вариант текстом ("" если не выбран).</summary>
        public string Value => IsNumber ? (_numberValue ?? "").Trim() : (_selectedIndex >= 0 ? Def.Options[_selectedIndex] : "");

        /// <summary>Значение вместе с уточнениями: "влажные, мелкопузырчатые, справа".</summary>
        public string ValueWithDetails
        {
            get
            {
                if (!IsFilled) return "";
                if (IsNumber) return Value + (string.IsNullOrEmpty(Unit) ? "" : " " + Unit);
                var parts = new List<string> { Value };
                if (!string.IsNullOrWhiteSpace(_chip)) parts.Add(_chip);
                if (!string.IsNullOrWhiteSpace(_side)) parts.Add(_side);
                if (!string.IsNullOrWhiteSpace(_detailText)) parts.Add(_detailText.Trim());
                return string.Join(", ", parts);
            }
        }

        /// <summary>Фраза для протокола: "Хрипы: влажные, мелкопузырчатые, справа".</summary>
        public string Phrase => IsFilled ? (IsNumber ? $"{Label} {ValueWithDetails}" : $"{Label}: {ValueWithDetails}") : "";

        public void Pick(int index) => SelectedIndex = SelectedIndex == index ? -1 : index;

        public void SetNormal()
        {
            if (IsFilled) return; // уже отмеченное не трогаем
            if (IsNumber) NumberValue = Def.Number!.Default;
            else SelectedIndex = 0;
        }

        public void Clear()
        {
            if (IsNumber) NumberValue = "";
            else SelectedIndex = -1;
        }

        private void RebuildDetails()
        {
            Sides.Clear();
            Chips.Clear();
            var d = CurrentDetail;
            if (d == null) return;
            foreach (var s in d.Sides ?? Array.Empty<string>())
                Sides.Add(new SmallOptionVm(s, t => Side = Side == t ? "" : t));
            foreach (var c in d.Chips ?? Array.Empty<string>())
                Chips.Add(new SmallOptionVm(c, t => Chip = Chip == t ? "" : t));
        }
    }

    public class SectionVm : ObservableObject
    {
        private bool _isOpen;
        public SectionDef Def { get; }
        public string Id => Def.Id;
        public string Title => Def.Title;
        public bool Collapsible { get; }
        public ObservableCollection<CriterionVm> Criteria { get; } = new();

        public ICommand ToggleCommand { get; }
        public ICommand NormCommand { get; }
        public ICommand ClearCommand { get; }

        public event Action? Changed;

        public SectionVm(SectionDef def, bool collapsible, bool open = false)
        {
            Def = def;
            Collapsible = collapsible;
            _isOpen = !collapsible || open;
            foreach (var c in def.Criteria)
            {
                var vm = new CriterionVm(c);
                vm.Changed += Recalc;
                Criteria.Add(vm);
            }
            ToggleCommand = new RelayCommand(() => { if (Collapsible) IsOpen = !IsOpen; });
            NormCommand = new RelayCommand(SetNormal);
            ClearCommand = new RelayCommand(() => { foreach (var c in Criteria) c.Clear(); });
        }

        public bool IsOpen
        {
            get => _isOpen;
            set { if (Set(ref _isOpen, value)) RaiseMany(nameof(ShowSummary), nameof(BadgeKind)); }
        }

        public int Filled => Criteria.Count(c => c.IsFilled);
        public int Abnormal => Criteria.Count(c => c.IsFilled && c.IsAbnormal);

        /// <summary>empty / ok / warn — для цвета значка.</summary>
        public string BadgeKind => Filled == 0 ? "empty" : Abnormal > 0 ? "warn" : "ok";

        public string Badge => Filled == 0
            ? "не заполнено"
            : Abnormal > 0
                ? $"отклонений: {Abnormal} · {Filled} из {Criteria.Count}"
                : $"норма · {Filled} из {Criteria.Count}";

        public string Summary => string.Join("; ", Criteria.Where(c => c.IsFilled).Select(c => c.Phrase)) + (Filled > 0 ? "." : "");
        public bool ShowSummary => !IsOpen && Filled > 0;

        public void SetNormal()
        {
            foreach (var c in Criteria) c.SetNormal();
        }

        public CriterionVm Get(string id) => Criteria.First(c => c.Def.Id == id);

        private void Recalc()
        {
            RaiseMany(nameof(Filled), nameof(Abnormal), nameof(Badge), nameof(BadgeKind), nameof(Summary), nameof(ShowSummary));
            Changed?.Invoke();
        }
    }
}
