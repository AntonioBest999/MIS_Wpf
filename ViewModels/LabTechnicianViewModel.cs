using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using MisWpf.Infrastructure;
using MisWpf.Services;
using ЦРБ_и_все_все_все.Классы;

namespace MisWpf.ViewModels
{
    public class LabCardVm : ObservableObject
    {
        private bool _isSelected;
        private string _timer = "";
        private string _timerKind = "normal";

        public LabWorkItem Item { get; }
        public ICommand SelectCommand { get; }

        public LabCardVm(LabWorkItem item, Action<LabCardVm> select)
        {
            Item = item;
            SelectCommand = new RelayCommand(() => select(this));
            Categories = item.Order.Items.Where(i => i.Parameter != null).Select(i => i.Parameter.Category).Distinct()
                .Select(c => new TagVm { Text = c, Kind = "neutral" }).ToList();
            Update(DateTime.UtcNow);
        }

        public int OrderId => Item.Order.LabOrderId;
        public string Fio => Item.Fio;
        public string Sub => $"{Item.Age} · {Item.Number}";
        public List<TagVm> Categories { get; }
        public string CountText => $"{Item.Order.Items.Count} показ.";
        public bool IsInWork => Item.Order.Status == LabOrderStatus.InProgress;
        public bool IsDone => Item.Order.Status == LabOrderStatus.Completed;
        public string StatusText => IsDone ? "выполнен " + (Item.CompletedAt.HasValue ? Format.Local(Item.CompletedAt.Value) : "")
                                  : IsInWork ? "в работе" : "новый";
        public string StatusKind => IsDone ? "ok" : IsInWork ? "accent" : "neutral";

        public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
        public string Timer { get => _timer; private set => Set(ref _timer, value); }
        public string TimerKind { get => _timerKind; private set => Set(ref _timerKind, value); }

        public void Update(DateTime nowUtc)
        {
            if (IsDone) { Timer = "заказ в " + Format.Local(Item.Order.OrderedAt); TimerKind = "normal"; return; }
            var t = nowUtc - Format.AsUtc(Item.Order.OrderedAt);
            Timer = Format.Elapsed(t);
            TimerKind = t.TotalMinutes >= 60 ? "danger" : t.TotalMinutes >= 30 ? "warn" : "normal";
        }
    }

    public class ParamRowVm : ObservableObject
    {
        private string _result;
        private string _comment;
        private readonly LabParameter _p;
        private readonly bool _female;

        public int ItemId { get; }
        public string Name => _p.Name;
        public string Units => _p.Units;
        public string RefText { get; }
        public bool IsReadOnly { get; }

        public ParamRowVm(LabOrderItem item, bool female, bool readOnly)
        {
            _p = item.Parameter;
            _female = female;
            ItemId = item.LabOrderItemId;
            _result = item.Result ?? "";
            _comment = item.Comment ?? "";
            RefText = LabWorkService.RangeText(_p, female);
            IsReadOnly = readOnly;
        }

        public string Result
        {
            get => _result;
            set { if (Set(ref _result, value)) RaiseMany(nameof(Flag), nameof(IsFlagged), nameof(IsFilled)); }
        }

        public string Comment { get => _comment; set => Set(ref _comment, value); }
        public string Flag => LabWorkService.Flag(_p, _female, _result);
        public bool IsFlagged => Flag != "";
        public bool IsFilled => !string.IsNullOrWhiteSpace(_result);
    }

    public class ParamGroupVm
    {
        public string Category { get; init; } = "";
        public List<ParamRowVm> Rows { get; init; } = new();
    }

    /// <summary>Рабочий стол лаборанта.</summary>
    public class LabTechnicianViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private int _secondsToRefresh;
        private bool _reloading;
        private List<LabCardVm> _all = new();
        private LabCardVm? _selected;
        private string _filter = "active";
        private string _clock = "";

        public ObservableCollection<LabCardVm> Cards { get; } = new();
        public ObservableCollection<ParamGroupVm> Groups { get; } = new();
        public ObservableCollection<ChoiceVm> Filters { get; } = new();

        public ICommand TakeCommand { get; }
        public ICommand SaveDraftCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public LabTechnicianViewModel()
        {
            Filters.Add(new ChoiceVm("В очереди", "active", PickFilter) { IsSelected = true });
            Filters.Add(new ChoiceVm("Выполнено за смену", "done", PickFilter));

            TakeCommand = new AsyncRelayCommand(TakeAsync, () => Selected != null && !Selected.IsInWork && !Selected.IsDone);
            SaveDraftCommand = new AsyncRelayCommand(() => SaveAsync(false), () => Selected != null && !Selected.IsDone);
            CompleteCommand = new AsyncRelayCommand(() => SaveAsync(true), () => Selected != null && !Selected.IsDone);
            RefreshCommand = new AsyncRelayCommand(ReloadAsync);

            _timer.Tick += async (_, _) =>
            {
                Clock = DateTime.Now.ToString("HH:mm");
                var now = DateTime.UtcNow;
                foreach (var c in _all) c.Update(now);
                if (++_secondsToRefresh >= AppConfig.RefreshSeconds)
                {
                    _secondsToRefresh = 0;
                    try { await ReloadAsync(); } catch { }
                }
            };
        }

        public async Task InitAsync()
        {
            await ReloadAsync();
            _timer.Start();
        }

        public void Stop() => _timer.Stop();

        public string Clock { get => _clock; set => Set(ref _clock, value); }
        public int NewCount => _filter == "active" ? _all.Count(c => !c.IsInWork) : 0;
        public int InWorkCount => _filter == "active" ? _all.Count(c => c.IsInWork) : 0;
        public bool IsEmpty => Cards.Count == 0;
        public string EmptyText => _filter == "active" ? "Новых заказов нет" : "За смену ещё ничего не выполнено";

        private void PickFilter(ChoiceVm c)
        {
            ChoiceVm.SelectSingle(Filters, c);
            _filter = (string)c.Value!;
            Selected = null;
            _ = ReloadAsync();
        }

        public async Task ReloadAsync()
        {
            if (_reloading) return;
            _reloading = true;
            try
            {
                int? selectedId = Selected?.OrderId;
                var items = await LabWorkService.LoadAsync(completedThisShift: _filter == "done");
                int before = _all.Count(c => !c.IsInWork && !c.IsDone);
                _all = items.Select(i => new LabCardVm(i, SelectCard)).ToList();
                Cards.Clear();
                foreach (var c in _all) Cards.Add(c);

                int now = _all.Count(c => !c.IsInWork && !c.IsDone);
                if (_filter == "active" && now > before && before >= 0 && _initialized)
                    Ui.Toast($"Новых заказов: {now - before}");
                _initialized = true;

                RaiseMany(nameof(NewCount), nameof(InWorkCount), nameof(IsEmpty), nameof(EmptyText));

                var again = selectedId == null ? null : _all.FirstOrDefault(c => c.OrderId == selectedId);
                if (again != null)
                {
                    // Не затираем то, что лаборант сейчас вводит: только обновляем выделение
                    foreach (var c in _all) c.IsSelected = c.OrderId == selectedId;
                    _selected = again;
                    RaiseMany(nameof(Selected), nameof(HasSelected), nameof(NoSelection));
                }
                else if (selectedId != null)
                {
                    Selected = null;
                }
            }
            finally
            {
                _reloading = false;
            }
        }

        private bool _initialized;

        private void SelectCard(LabCardVm card)
        {
            foreach (var c in _all) c.IsSelected = ReferenceEquals(c, card);
            Selected = card;
        }

        public LabCardVm? Selected
        {
            get => _selected;
            private set
            {
                _selected = value;
                Groups.Clear();
                if (value != null)
                {
                    bool ro = value.IsDone;
                    foreach (var g in value.Item.Order.Items.Where(i => i.Parameter != null)
                                 .OrderBy(i => i.LabParameterId)
                                 .GroupBy(i => i.Parameter.Category))
                    {
                        Groups.Add(new ParamGroupVm
                        {
                            Category = g.Key,
                            Rows = g.Select(i => new ParamRowVm(i, value.Item.IsFemale, ro)).ToList()
                        });
                    }
                }
                RaiseMany(nameof(Selected), nameof(HasSelected), nameof(NoSelection), nameof(SelectedFio), nameof(SelectedSub),
                          nameof(SelectedStatus), nameof(IsReadOnly), nameof(ProgressText));
            }
        }

        public bool HasSelected => Selected != null;
        public bool NoSelection => Selected == null;
        public string SelectedFio => Selected?.Fio ?? "";
        public string SelectedSub => Selected?.Sub ?? "";
        public string SelectedStatus => Selected?.StatusText ?? "";
        public bool IsReadOnly => Selected?.IsDone == true;
        public string ProgressText
        {
            get
            {
                var rows = Groups.SelectMany(g => g.Rows).ToList();
                return rows.Count == 0 ? "" : $"Заполнено {rows.Count(r => r.IsFilled)} из {rows.Count}";
            }
        }

        private async Task TakeAsync()
        {
            if (Selected == null) return;
            await LabWorkService.TakeInWorkAsync(Selected.OrderId);
            Ui.Toast($"{Selected.Fio}: заказ взят в работу.");
            await ReloadAsync();
            var again = _all.FirstOrDefault(c => c.OrderId == Selected?.OrderId);
            if (again != null) SelectCard(again);
        }

        private async Task SaveAsync(bool complete)
        {
            if (Selected == null) return;
            var rows = Groups.SelectMany(g => g.Rows).ToList();
            if (complete)
            {
                int empty = rows.Count(r => !r.IsFilled);
                if (empty > 0) { Ui.Info($"Не заполнено показателей: {empty}. Заполните все, чтобы завершить заказ."); return; }
                if (!Ui.Confirm($"Завершить заказ для {Selected.Fio}?\nПосле этого изменить результаты будет нельзя.")) return;
            }

            var values = rows.ToDictionary(r => r.ItemId, r => (r.Result, (string?)r.Comment));
            await LabWorkService.SaveAsync(Selected.OrderId, values, complete);
            Ui.Toast(complete ? $"{Selected.Fio}: результаты отправлены врачу." : "Черновик сохранён.");

            if (complete)
            {
                Selected = null;
                await ReloadAsync();
            }
            else
            {
                Raise(nameof(ProgressText));
            }
        }
    }
}
