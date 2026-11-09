namespace MisWpf.Exam
{
    /// <summary>Уточнение, которое появляется при выборе варианта с отклонением.</summary>
    public class DetailDef
    {
        public string[]? Sides { get; init; }
        public string[]? Chips { get; init; }
        public string? TextHint { get; init; }
    }

    public class NumberDef
    {
        public string Unit { get; init; } = "";
        public double? Low { get; init; }
        public double? High { get; init; }
        public string Default { get; init; } = "";
        public string Hint { get; init; } = "";
        /// <summary>Свободный ввод (например АД "120/80") без проверки на норму.</summary>
        public bool FreeText { get; init; }
    }

    /// <summary>Один критерий осмотра. Первый вариант в Options — всегда норма.</summary>
    public class CriterionDef
    {
        public string Id { get; init; } = "";
        public string Label { get; init; } = "";
        public string[] Options { get; init; } = Array.Empty<string>();
        public Dictionary<int, DetailDef> Details { get; init; } = new();
        public NumberDef? Number { get; init; }
    }

    public class SectionDef
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public CriterionDef[] Criteria { get; init; } = Array.Empty<CriterionDef>();
    }

    /// <summary>
    /// Описание всех разделов осмотра. Добавить критерий = добавить строку сюда
    /// (и, если нужно сохранить его в отдельное поле БД, — строчку в ExamMapper).
    /// </summary>
    public static class ExamCatalog
    {
        private static readonly string[] S3 = { "справа", "слева", "с обеих сторон" };

        private static Dictionary<int, DetailDef> D(params (int index, DetailDef def)[] items)
            => items.ToDictionary(i => i.index, i => i.def);

        public static readonly SectionDef Anamnesis = new()
        {
            Id = "anam",
            Title = "Анамнез жизни",
            Criteria = new[]
            {
                new CriterionDef { Id = "all", Label = "Аллергоанамнез", Options = new[] { "не отягощён", "отягощён" },
                    Details = D((1, new DetailDef { TextHint = "на что и как проявляется" })) },
                new CriterionDef { Id = "hiv", Label = "ВИЧ", Options = new[] { "отрицает", "подтверждает" } },
                new CriterionDef { Id = "tb", Label = "Туберкулёз", Options = new[] { "отрицает", "подтверждает" } },
                new CriterionDef { Id = "hep", Label = "Вирусные гепатиты", Options = new[] { "отрицает", "подтверждает" },
                    Details = D((1, new DetailDef { Chips = new[] { "B", "C", "B и C" } })) },
                new CriterionDef { Id = "mpc", Label = "МРС (сифилис)", Options = new[] { "отрицает", "подтверждает" } },
            }
        };

        public static readonly SectionDef[] Objective =
        {
            new SectionDef
            {
                Id = "general", Title = "Общий статус",
                Criteria = new[]
                {
                    new CriterionDef { Id = "cond", Label = "Общее состояние", Options = new[] { "удовлетворительное", "средней степени тяжести", "тяжёлое", "крайне тяжёлое" } },
                    new CriterionDef { Id = "stab", Label = "Стабильность", Options = new[] { "стабильное", "нестабильное" } },
                    new CriterionDef { Id = "cons", Label = "Сознание", Options = new[] { "ясное", "оглушение", "сопор", "кома" },
                        Details = D((1, new DetailDef { TextHint = "ШКГ, баллы" }), (2, new DetailDef { TextHint = "ШКГ, баллы" }), (3, new DetailDef { TextHint = "ШКГ, баллы" })) },
                    new CriterionDef { Id = "pos", Label = "Положение", Options = new[] { "активное", "пассивное", "вынужденное" },
                        Details = D((2, new DetailDef { TextHint = "какое" })) },
                    new CriterionDef { Id = "build", Label = "Телосложение", Options = new[] { "нормостеническое", "гиперстеническое", "астеническое" } },
                    new CriterionDef { Id = "temp", Label = "Температура тела", Number = new NumberDef { Unit = "°C", Low = 35.9, High = 37.0, Default = "36,6", Hint = "36,6" } },
                }
            },
            new SectionDef
            {
                Id = "skin", Title = "Кожные покровы",
                Criteria = new[]
                {
                    new CriterionDef { Id = "col", Label = "Окраска", Options = new[] { "обычной окраски", "бледные", "гиперемированы", "желтушные", "цианотичные" } },
                    new CriterionDef { Id = "hum", Label = "Влажность", Options = new[] { "нормальной влажности", "сухие", "влажные" } },
                    new CriterionDef { Id = "rash", Label = "Сыпь", Options = new[] { "нет", "есть" }, Details = D((1, new DetailDef { TextHint = "характер и локализация" })) },
                    new CriterionDef { Id = "cyan", Label = "Цианоз", Options = new[] { "нет", "акроцианоз", "диффузный" } },
                    new CriterionDef { Id = "edema", Label = "Отёки", Options = new[] { "нет", "есть" }, Details = D((1, new DetailDef { Sides = S3, TextHint = "локализация" })) },
                    new CriterionDef { Id = "ln", Label = "Лимфоузлы", Options = new[] { "не увеличены", "увеличены" }, Details = D((1, new DetailDef { TextHint = "группы, размер" })) },
                }
            },
            new SectionDef
            {
                Id = "resp", Title = "Дыхательная система",
                Criteria = new[]
                {
                    new CriterionDef { Id = "chest", Label = "Грудная клетка", Options = new[] { "правильной формы", "асимметричная" }, Details = D((1, new DetailDef { TextHint = "описание" })) },
                    new CriterionDef { Id = "palp", Label = "Пальпация", Options = new[] { "безболезненная", "болезненная" }, Details = D((1, new DetailDef { TextHint = "локализация" })) },
                    new CriterionDef { Id = "part", Label = "Участие в дыхании", Options = new[] { "обе половины симметрично", "отстаёт половина" }, Details = D((1, new DetailDef { Sides = new[] { "правая", "левая" } })) },
                    new CriterionDef { Id = "rhythm", Label = "Дыхание", Options = new[] { "ритмичное", "поверхностное", "аритмичное" },
                        Details = D((2, new DetailDef { Chips = new[] { "Куссмауля", "Чейна-Стокса", "Биота", "Грокка", "гаспинг" } })) },
                    new CriterionDef { Id = "rr", Label = "ЧДД", Number = new NumberDef { Unit = "в минуту", Low = 12, High = 20, Default = "16", Hint = "16" } },
                    new CriterionDef { Id = "spo2", Label = "SpO₂", Number = new NumberDef { Unit = "%", Low = 95, High = 100, Default = "98", Hint = "98" } },
                    new CriterionDef { Id = "dysp", Label = "Одышка", Options = new[] { "нет", "инспираторная", "экспираторная", "смешанная" } },
                    new CriterionDef { Id = "cough", Label = "Кашель", Options = new[] { "нет", "есть" },
                        Details = D((1, new DetailDef { Chips = new[] { "сухой", "влажный", "с гнойной мокротой", "с кровью" } })) },
                    new CriterionDef { Id = "perc", Label = "Перкуторный звук", Options = new[] { "ясный лёгочный", "притуплённый", "тупой", "тимпанит", "коробочный" },
                        Details = D((1, new DetailDef { Sides = S3, TextHint = "область" }), (2, new DetailDef { Sides = S3, TextHint = "область" }),
                                    (3, new DetailDef { Sides = S3, TextHint = "область" }), (4, new DetailDef { Sides = S3, TextHint = "область" })) },
                    new CriterionDef { Id = "ausc", Label = "Аускультативно дыхание", Options = new[] { "везикулярное", "жёсткое", "бронхиальное", "пуэрильное", "стенотическое" } },
                    new CriterionDef { Id = "weak", Label = "Ослабление дыхания", Options = new[] { "нет", "ослаблено", "не выслушивается" },
                        Details = D((1, new DetailDef { Sides = S3, TextHint = "область" }), (2, new DetailDef { Sides = S3, TextHint = "область" })) },
                    new CriterionDef { Id = "wh", Label = "Хрипы", Options = new[] { "нет", "сухие", "влажные" },
                        Details = D((1, new DetailDef { Sides = S3 }), (2, new DetailDef { Sides = S3, Chips = new[] { "мелкопузырчатые", "среднепузырчатые", "крупнопузырчатые" } })) },
                    new CriterionDef { Id = "noise", Label = "Патологические шумы", Options = new[] { "нет", "крепитация", "шум трения плевры" },
                        Details = D((1, new DetailDef { Sides = S3 }), (2, new DetailDef { Sides = S3 })) },
                }
            },
            new SectionDef
            {
                Id = "cardio", Title = "Сердечно-сосудистая система",
                Criteria = new[]
                {
                    new CriterionDef { Id = "tones", Label = "Тоны сердца", Options = new[] { "ясные", "приглушены", "глухие" } },
                    new CriterionDef { Id = "rh", Label = "Ритм", Options = new[] { "правильный", "неправильный" },
                        Details = D((1, new DetailDef { Chips = new[] { "фибрилляция предсердий", "экстрасистолия" } })) },
                    new CriterionDef { Id = "mur", Label = "Шумы", Options = new[] { "нет", "систолический", "диастолический" },
                        Details = D((1, new DetailDef { TextHint = "точка выслушивания" }), (2, new DetailDef { TextHint = "точка выслушивания" })) },
                    new CriterionDef { Id = "hr", Label = "ЧСС", Number = new NumberDef { Unit = "в минуту", Low = 60, High = 90, Default = "76", Hint = "76" } },
                    new CriterionDef { Id = "bp", Label = "АД", Number = new NumberDef { Unit = "мм рт. ст.", Default = "120/80", Hint = "120/80", FreeText = true } },
                    new CriterionDef { Id = "pulse", Label = "Пульс на периферии", Options = new[] { "сохранён", "ослаблен", "отсутствует" },
                        Details = D((1, new DetailDef { Sides = S3, TextHint = "артерии" }), (2, new DetailDef { Sides = S3, TextHint = "артерии" })) },
                }
            },
            new SectionDef
            {
                Id = "gi", Title = "Органы пищеварения и мочевыделения",
                Criteria = new[]
                {
                    new CriterionDef { Id = "tongue", Label = "Язык", Options = new[] { "влажный, чистый", "сухой", "обложен" } },
                    new CriterionDef { Id = "abd", Label = "Живот", Options = new[] { "не вздут", "вздут" }, Details = D((1, new DetailDef { TextHint = "за счёт чего" })) },
                    new CriterionDef { Id = "apalp", Label = "При пальпации", Options = new[] { "мягкий, безболезненный", "болезненный", "напряжён" },
                        Details = D((1, new DetailDef { TextHint = "область" }), (2, new DetailDef { TextHint = "область" })) },
                    new CriterionDef { Id = "periton", Label = "Симптомы раздражения брюшины", Options = new[] { "отрицательные", "положительные" },
                        Details = D((1, new DetailDef { Chips = new[] { "Щёткина–Блюмберга", "Ситковского", "Ровзинга", "Ортнера", "Мерфи" } })) },
                    new CriterionDef { Id = "liver", Label = "Печень", Options = new[] { "не выступает из-под края рёберной дуги", "выступает" }, Details = D((1, new DetailDef { TextHint = "на сколько см" })) },
                    new CriterionDef { Id = "spleen", Label = "Селезёнка", Options = new[] { "не пальпируется", "пальпируется" } },
                    new CriterionDef { Id = "perist", Label = "Перистальтика", Options = new[] { "выслушивается", "ослаблена", "отсутствует" } },
                    new CriterionDef { Id = "stool", Label = "Стул", Options = new[] { "без особенностей", "жидкий", "мелена", "задержка" } },
                    new CriterionDef { Id = "uri", Label = "Мочеиспускание", Options = new[] { "свободное, безболезненное", "дизурия", "олигурия" } },
                    new CriterionDef { Id = "past", Label = "Симптом поколачивания", Options = new[] { "отрицательный", "положительный" }, Details = D((1, new DetailDef { Sides = S3 })) },
                }
            },
            new SectionDef
            {
                Id = "neuro", Title = "Нервно-психический статус",
                Criteria = new[]
                {
                    new CriterionDef { Id = "mening", Label = "Менингеальные знаки", Options = new[] { "отрицательные", "положительные" },
                        Details = D((1, new DetailDef { Chips = new[] { "ригидность затылочных мышц", "Кернига", "Брудзинского" } })) },
                    new CriterionDef { Id = "focal", Label = "Очаговая симптоматика", Options = new[] { "нет", "есть" }, Details = D((1, new DetailDef { TextHint = "описание" })) },
                    new CriterionDef { Id = "pupil", Label = "Зрачки", Options = new[] { "D = S", "анизокория" }, Details = D((1, new DetailDef { Chips = new[] { "D > S", "S > D" } })) },
                    new CriterionDef { Id = "pupw", Label = "Ширина зрачков", Options = new[] { "нормальной ширины", "миоз", "мидриаз" },
                        Details = D((1, new DetailDef { Sides = S3 }), (2, new DetailDef { Sides = S3 })) },
                    new CriterionDef { Id = "photo", Label = "Фотореакция", Options = new[] { "живая", "вялая", "отсутствует" },
                        Details = D((1, new DetailDef { Sides = S3 }), (2, new DetailDef { Sides = S3 })) },
                    new CriterionDef { Id = "speech", Label = "Нарушение речи", Options = new[] { "нет", "есть" },
                        Details = D((1, new DetailDef { Chips = new[] { "моторная афазия", "сенсорная афазия", "дизартрия" }, TextHint = "уточнение" })) },
                    new CriterionDef { Id = "power", Label = "Сила в конечностях", Options = new[] { "сохранена", "снижена" },
                        Details = D((1, new DetailDef { Sides = S3, Chips = new[] { "в руке", "в ноге", "в руке и ноге" }, TextHint = "до скольких баллов" })) },
                    new CriterionDef { Id = "tone", Label = "Тонус в конечностях", Options = new[] { "не изменён", "повышен", "снижен" },
                        Details = D((1, new DetailDef { Sides = S3 }), (2, new DetailDef { Sides = S3 })) },
                    new CriterionDef { Id = "coord", Label = "Координационные пробы", Options = new[] { "выполняет уверенно", "с интенцией", "с промахиванием", "не выполняет" },
                        Details = D((1, new DetailDef { Sides = S3, Chips = new[] { "пальценосовая", "пяточно-коленная" } }),
                                    (2, new DetailDef { Sides = S3, Chips = new[] { "пальценосовая", "пяточно-коленная" } })) },
                    new CriterionDef { Id = "romberg", Label = "В позе Ромберга", Options = new[] { "устойчив", "пошатывается", "неустойчив", "не выполняет" } },
                    new CriterionDef { Id = "mental", Label = "Психический статус", Options = new[] { "не изменён", "изменён" }, Details = D((1, new DetailDef { TextHint = "описание" })) },
                    new CriterionDef { Id = "gait", Label = "Шаткость походки", Options = new[] { "нет", "есть" } },
                    new CriterionDef { Id = "smot", Label = "Нарушение моторики речи", Options = new[] { "нет", "есть" } },
                    new CriterionDef { Id = "alco", Label = "Запах алкоголя изо рта", Options = new[] { "нет", "есть" } },
                }
            },
        };
    }
}
