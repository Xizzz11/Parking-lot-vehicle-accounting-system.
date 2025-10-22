// ========================= Program.cs =========================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ParkingSystem.BLL;
using ParkingSystem.Data;

// Навигация по шагам форм
public enum StepNav { Ok, Back, Exit }

namespace ParkingSystem
{
    internal class Program
    {
        private static readonly ParkingService service = new ParkingService(new ParkingContext());

        // ---------------------- THEME & FX ----------------------
        static class FX
        {
            public static readonly ConsoleColor Primary = ConsoleColor.Cyan;
            public static readonly ConsoleColor Accent = ConsoleColor.Magenta;
            public static readonly ConsoleColor Text = ConsoleColor.Gray;
            public static readonly ConsoleColor Ok = ConsoleColor.Green;
            public static readonly ConsoleColor Err = ConsoleColor.Red;
            public static readonly ConsoleColor Title = ConsoleColor.Yellow;
            public static readonly ConsoleColor Panel = ConsoleColor.DarkGray;

            public static void CenterWrite(string text, ConsoleColor? color = null)
            {
                var w = Math.Max(40, Console.WindowWidth);
                int left = Math.Max(0, (w - text.Length) / 2);
                if (color.HasValue) Console.ForegroundColor = color.Value;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine(new string(' ', left) + text);
                if (color.HasValue) Console.ResetColor();
            }

            public static void Rule(int minWidth = 60, char ch = '─')
            {
                int w = Math.Max(minWidth, Console.WindowWidth - 4);
                CenterWrite(new string(ch, w), Panel);
            }

            public static void Banner(string text = "PARKING SYSTEM")
            {
                string left = "╭────────────────────────────────────────────────────╮";
                string right = "╰────────────────────────────────────────────────────╯";
                Console.CursorVisible = false;
                Console.ForegroundColor = Title;
                CenterWrite(left);
                CenterWrite($"│   {text}   │");
                CenterWrite(right);
                Console.ResetColor();
                Console.CursorVisible = true;
            }

            // Анимация «проезжающей машины» + «Подключение к серверу…»
            public static void CarConnect()
            {
                Console.CursorVisible = false;
                string road = new string('─', Math.Max(40, Console.WindowWidth - 12));
                string car1 = "  __";
                string car2 = "_/__\\__";
                string car3 = "O    O";
                CenterWrite("Подключение к серверу...", Text);
                Thread.Sleep(200);

                int width = Math.Max(50, Console.WindowWidth - 10);
                int steps = Math.Min(80, width - 10);
                for (int i = 0; i <= steps; i++)
                {
                    Console.ForegroundColor = Panel;
                    CenterWrite(road);
                    Console.ResetColor();

                    var pad = new string(' ', i);
                    CenterWrite(pad + car1, Primary);
                    CenterWrite(pad + car2, Primary);
                    CenterWrite(pad + car3, Primary);

                    Thread.Sleep(18);
                    Console.CursorTop -= 4;
                }
                Console.CursorTop += 3;
                Console.CursorVisible = true;
            }

            public static void FramedPanel(string title, string[] items)
            {
                var width = Math.Min(Math.Max(items.Max(s => s.Length) + 8, title.Length + 8), Math.Max(72, Console.WindowWidth - 8));
                string top = "╭" + new string('─', width) + "╮";
                string bottom = "╰" + new string('─', width) + "╯";

                Console.ForegroundColor = Panel;
                CenterWrite(top);
                Console.ResetColor();

                CenterWrite($"│ {title.PadRight(width - 1)}│", Title);
                Console.ForegroundColor = Panel;
                CenterWrite("├" + new string('─', width) + "┤");
                Console.ResetColor();

                for (int i = 0; i < items.Length; i++)
                {
                    var row = $"{i + 1}. {items[i]}";
                    if (row.Length > width - 2) row = row.Substring(0, width - 5) + "...";
                    CenterWrite($"│ {row.PadRight(width - 1)}│", Text);
                }

                Console.ForegroundColor = Panel;
                CenterWrite(bottom);
                Console.ResetColor();
            }

            public static void FramedTable(IEnumerable<string[]> rows, string? header = null)
            {
                var list = rows.ToList();
                if (!list.Any())
                {
                    CenterWrite("пусто", Text);
                    return;
                }

                int cols = list.Max(r => r.Length);
                int[] widths = new int[cols];
                foreach (var r in list)
                    for (int i = 0; i < r.Length; i++)
                        widths[i] = Math.Min(Math.Max(widths[i], r[i]?.Length ?? 0), 40);

                int total = widths.Sum() + cols * 3 + 1;
                total = Math.Min(Math.Max(total, 50), Math.Max(70, Console.WindowWidth - 10));

                string top = "╭" + new string('─', total) + "╮";
                string sep = "├" + new string('─', total) + "┤";
                string bot = "╰" + new string('─', total) + "╯";

                Console.ForegroundColor = Panel;
                CenterWrite(top);
                Console.ResetColor();

                if (!string.IsNullOrWhiteSpace(header))
                {
                    CenterWrite(("│ " + header).PadRight(total + 2) + "│", Title);
                    Console.ForegroundColor = Panel;
                    CenterWrite(sep);
                    Console.ResetColor();
                }

                foreach (var r in list)
                {
                    var line = "│ ";
                    for (int i = 0; i < cols; i++)
                    {
                        var cell = i < r.Length ? (r[i] ?? "") : "";
                        if (cell.Length > widths[i]) cell = cell.Substring(0, widths[i] - 1) + "…";
                        line += cell.PadRight(widths[i]) + " │ ";
                    }
                    line = line.PadRight(total + 2) + "│";
                    CenterWrite(line, Text);
                }

                Console.ForegroundColor = Panel;
                CenterWrite(bot);
                Console.ResetColor();
            }
        }

        // ---------------------- UI ----------------------
        static class UI
        {
            public static void Info(string text) => FX.CenterWrite(text, FX.Text);
            public static void Ok(string text) => FX.CenterWrite(text, FX.Ok);
            public static void Error(string text) => FX.CenterWrite(text, FX.Err);
            public static void BannerParking() => FX.Banner("PARKING SYSTEM");
            public static void Panel(string title, params string[] items) => FX.FramedPanel(title, items);
            public static void Rule() => FX.Rule();

            public static string Prompt(string label, string? def = null)
            {
                Console.ForegroundColor = FX.Primary;
                FX.CenterWrite(label + (string.IsNullOrEmpty(def) ? "" : $" [{def}]") + ": ");
                Console.ResetColor();
                Console.SetCursorPosition(Math.Max(0, Console.WindowWidth / 2 - 5), Console.CursorTop);
                var s = Console.ReadLine();
                if (string.IsNullOrEmpty(s)) return def ?? "";
                return s;
            }

            // ==== БАЗОВЫЙ ввод с навигацией (Esc/Backspace) ====
            public static (StepNav nav, string text) PromptWithNav(string label, bool allowEmpty = false)
            {
                Console.ForegroundColor = FX.Primary;
                FX.CenterWrite(label);
                Console.ResetColor();
                FX.CenterWrite("[Backspace — назад, Esc — меню]");

                var buf = new List<char>();
                Console.CursorVisible = true;

                int left = Math.Max(0, Console.WindowWidth / 2 - 12);
                int top = Console.CursorTop;
                Console.SetCursorPosition(left, top);

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
                        return (StepNav.Exit, "");
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        if (!allowEmpty && buf.Count == 0)
                        {
                            UI.Error("Поле обязательно. Введите значение, или Esc — для выхода.");
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад, Esc — меню]");
                            Console.SetCursorPosition(left, Console.CursorTop);
                            continue;
                        }
                        return (StepNav.Ok, new string(buf.ToArray()));
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (buf.Count > 0)
                        {
                            Console.Write("\b \b");
                            buf.RemoveAt(buf.Count - 1);
                        }
                        else
                        {
                            Console.WriteLine();
                            return (StepNav.Back, "");
                        }
                        continue;
                    }

                    if (!char.IsControl(key.KeyChar))
                    {
                        buf.Add(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
            }

            // === Фильтр допустимых символов для имени ===
            private static bool IsAllowedNameChar(char ch, bool allowSpaceInside)
            {
                if (ch == '-') return true;
                if (allowSpaceInside && ch == ' ') return true;
                bool isRu = (ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || ch == 'Ё' || ch == 'ё';
                bool isEn = (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
                return isRu || isEn;
            }

            // Ввод имени с мгновенной валидацией (буквы RU/EN и дефис, пробел опционально)
            public static (StepNav nav, string text) PromptNameWithNav(string label, bool allowSpaceInside, int minLen = 2, int maxLen = 50)
            {
                Console.ForegroundColor = FX.Primary;
                FX.CenterWrite(label);
                Console.ResetColor();
                FX.CenterWrite("[Backspace — назад, Esc — меню]");

                var buf = new List<char>();
                Console.CursorVisible = true;

                int left = Math.Max(0, Console.WindowWidth / 2 - 16);
                int inputTop = Console.CursorTop;
                Console.SetCursorPosition(left, inputTop);

                void Rerender()
                {
                    Console.SetCursorPosition(left, inputTop);
                    Console.Write(new string(' ', Math.Max(1, Console.WindowWidth - left - 1)));
                    Console.SetCursorPosition(left, inputTop);
                    Console.Write(new string(buf.ToArray()));
                }

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
                        return (StepNav.Exit, "");
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        var txt = new string(buf.ToArray()).Trim();
                        if (txt.Length < minLen)
                        {
                            UI.Error($"Минимум {minLen} символа(ов). Только буквы (рус/англ){(allowSpaceInside ? " и пробел между словами" : "")}, допускается дефис.");
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад, Esc — меню]");
                            inputTop = Console.CursorTop;
                            Console.SetCursorPosition(left, inputTop);
                            Console.Write(txt);
                            continue;
                        }
                        return (StepNav.Ok, txt);
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (buf.Count > 0)
                        {
                            buf.RemoveAt(buf.Count - 1);
                            Rerender();
                        }
                        else
                        {
                            Console.WriteLine();
                            return (StepNav.Back, "");
                        }
                        continue;
                    }

                    var ch = key.KeyChar;
                    if (!char.IsControl(ch))
                    {
                        if (!IsAllowedNameChar(ch, allowSpaceInside))
                        {
                            UI.Error("Недопустимый символ: только буквы (рус/англ), дефис" + (allowSpaceInside ? " и один пробел между словами" : ""));
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад, Esc — меню]");
                            inputTop = Console.CursorTop;
                            Console.SetCursorPosition(left, inputTop);
                            Console.Write(new string(buf.ToArray()));
                            continue;
                        }
                        if ((ch == ' ' || ch == '-') && buf.Count == 0) continue;
                        if (ch == ' ' && (buf.Count == 0 || buf[buf.Count - 1] == ' ')) continue;
                        if (buf.Count >= maxLen)
                        {
                            UI.Error($"Длина не должна превышать {maxLen} символов.");
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад, Esc — меню]");
                            inputTop = Console.CursorTop;
                            Console.SetCursorPosition(left, inputTop);
                            Console.Write(new string(buf.ToArray()));
                            continue;
                        }
                        buf.Add(ch);
                        Console.Write(ch);
                    }
                }
            }

            // Телефон: автоматически +7 и ровно 10 цифр
            public static (StepNav nav, string text) PromptPhoneWithNav(string label, bool allowEmpty = false)
            {
                Console.ForegroundColor = FX.Primary;
                FX.CenterWrite(label);
                Console.ResetColor();
                FX.CenterWrite("[Backspace — назад (если поле пустое), Esc — меню]");

                var digits = new List<char>();
                Console.CursorVisible = true;

                int left = Math.Max(0, Console.WindowWidth / 2 - 14);
                int inputTop = Console.CursorTop;
                Console.SetCursorPosition(left, inputTop);
                Console.Write("+7 ");

                void Reprint()
                {
                    Console.SetCursorPosition(left, inputTop);
                    Console.Write(new string(' ', Math.Max(1, Console.WindowWidth - left - 1)));
                    Console.SetCursorPosition(left, inputTop);
                    Console.Write("+7 " + new string(digits.ToArray()));
                }

                while (true)
                {
                    var key = Console.ReadKey(intercept: true);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Console.WriteLine();
                        return (StepNav.Exit, "");
                    }

                    if (key.Key == ConsoleKey.Enter)
                    {
                        Console.WriteLine();
                        if (digits.Count == 0 && allowEmpty) return (StepNav.Ok, "");
                        if (digits.Count != 10)
                        {
                            UI.Error("Нужно ввести ровно 10 цифр после +7.");
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад (если поле пустое), Esc — меню]");
                            inputTop = Console.CursorTop;
                            Console.SetCursorPosition(left, inputTop);
                            Console.Write("+7 " + new string(digits.ToArray()));
                            continue;
                        }
                        return (StepNav.Ok, "+7" + new string(digits.ToArray()));
                    }

                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (digits.Count > 0)
                        {
                            digits.RemoveAt(digits.Count - 1);
                            Reprint();
                        }
                        else
                        {
                            Console.WriteLine();
                            return (StepNav.Back, "");
                        }
                        continue;
                    }

                    if (!char.IsControl(key.KeyChar))
                    {
                        if (!char.IsDigit(key.KeyChar))
                        {
                            UI.Error("Допустимы только цифры 0–9.");
                            FX.CenterWrite(label, FX.Primary);
                            FX.CenterWrite("[Backspace — назад (если поле пустое), Esc — меню]");
                            inputTop = Console.CursorTop;
                            Console.SetCursorPosition(left, inputTop);
                            Console.Write("+7 " + new string(digits.ToArray()));
                            continue;
                        }
                        if (digits.Count < 10)
                        {
                            digits.Add(key.KeyChar);
                            Console.Write(key.KeyChar);
                        }
                    }
                }
            }

            public static void Table(IEnumerable<string[]> rows, string? header = null) => FX.FramedTable(rows, header);
        }

        // ---------------------- MAIN ----------------------
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "Parking System — Console UI";

            Console.Clear();
            UI.BannerParking();
            FX.CarConnect(); // только при запуске

            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Главное меню",
                    "Просмотр",
                    "Добавить",
                    "Удалить",
                    "Редактировать",
                    "Поиск",
                    "Начать парковку",
                    "Завершить парковку",
                    "Выход");

                var c = UI.Prompt("Выберите пункт (1-8)");
                switch (c)
                {
                    case "1": ShowViewMenu(); break;
                    case "2": ShowAddMenu(); break;
                    case "3": ShowDeleteMenu(); break;
                    case "4": EditEntity(); break;
                    case "5": ShowSearchMenu(); break;
                    case "6": StartParkingWizard(); break;
                    case "7": EndParkingWizard(); break;
                    case "8": ExitProgram(); return;
                    default: UI.Error("Неверный выбор."); Thread.Sleep(600); break;
                }
            }
        }

        // ---------------------- VIEW ----------------------
        private static void ShowViewMenu()
        {
            Console.Clear();
            UI.BannerParking();
            UI.Panel("Просмотр",
                "Владельцы",
                "Машины",
                "Парковочные места",
                "Билеты",
                "Назад");
            var c = UI.Prompt("Выберите (1-5)");
            Console.Clear();
            UI.BannerParking();
            switch (c)
            {
                case "1": UI.Table(service.GetAllOwners(), "Список владельцев"); break;
                case "2": UI.Table(service.GetAllVehicles(), "Список машин"); break;
                case "3": UI.Table(service.GetAllSpaces(), "Список мест"); break;
                case "4": UI.Table(service.GetTickets(), "Список билетов"); break;
                default: return;
            }
            FX.Rule();
            UI.Info("Нажмите любую клавишу…");
            Console.ReadKey(true);
        }

        // ---------------------- ADD ----------------------
        private static void ShowAddMenu()
        {
            Console.Clear();
            UI.BannerParking();
            UI.Panel("Добавить", "Владельца", "Машину", "Место", "Назад");
            var c = UI.Prompt("Выберите (1-4)");
            if (c == "1") AddOwnerWizard();
            else if (c == "2") AddVehicleWizard();
            else if (c == "3") AddSpaceWizard();
        }

        private static void AddOwnerWizard()
        {
            int step = 0;
            string ln = "", fn = "", phone = "", email = "";

            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Добавление владельца", "Шаг 1: Фамилия", "Шаг 2: Имя", "Шаг 3: Телефон", "Шаг 4: Email", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptNameWithNav("Фамилия (рус/англ, без цифр)", allowSpaceInside: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            ln = r.text;
                            step++;
                            break;
                        }
                    case 1:
                        {
                            var r = UI.PromptNameWithNav("Имя (рус/англ, без цифр)", allowSpaceInside: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            fn = r.text;
                            step++;
                            break;
                        }
                    case 2:
                        {
                            var r = UI.PromptPhoneWithNav("Телефон (+7 и 10 цифр)", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            phone = r.text;
                            step++;
                            break;
                        }
                    case 3:
                        {
                            var r = UI.PromptWithNav("Email (обязательно)", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            var txt = r.text;
                            int at = txt.IndexOf('@'); int dot = txt.LastIndexOf('.');
                            if (!(at > 0 && dot > at + 1 && dot < txt.Length - 1))
                            { UI.Error("Email должен содержать '@' и точку после неё."); break; }
                            email = txt.Trim();
                            try
                            {
                                service.AddOwner(fn, ln, phone, email);
                                UI.Ok("Владелец добавлен.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                                step = 0;
                            }
                            break;
                        }
                }
            }
        }

        private static void AddVehicleWizard()
        {
            int step = 0;
            string lp = "", mk = "", md = "", cl = "", owner = "";

            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Добавление машины", "1: Госномер", "2: Марка", "3: Модель", "4: Цвет", "5: Владелец", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Госномер", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }

                            lp = (r.text ?? "").Trim();
                            if (lp.Length < 6 || lp.Length > 10)
                            { UI.Error("Госномер должен содержать от 6 до 10 символов."); break; }

                            var takenBy = service.GetOwnerNameByLicensePlate(lp);
                            if (takenBy != null) { UI.Error($"Этот номер уже принадлежит: {takenBy}"); break; }

                            step++;
                            break;
                        }
                    case 1:
                        {
                            var r = UI.PromptWithNav("Марка", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            if (string.IsNullOrWhiteSpace(r.text)) { UI.Error("Марка обязательна."); break; }
                            mk = r.text.Trim();
                            step++;
                            break;
                        }
                    case 2:
                        {
                            var r = UI.PromptWithNav("Модель", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            if (string.IsNullOrWhiteSpace(r.text)) { UI.Error("Модель обязательна."); break; }
                            md = r.text.Trim();
                            step++;
                            break;
                        }
                    case 3:
                        {
                            var r = UI.PromptWithNav("Цвет", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            if (string.IsNullOrWhiteSpace(r.text)) { UI.Error("Цвет обязателен."); break; }
                            cl = r.text.Trim();
                            step++;
                            break;
                        }
                    case 4:
                        {
                            var r = UI.PromptNameWithNav("ФИО владельца (Фамилия Имя — рус/англ)", allowSpaceInside: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            owner = (r.text ?? "").Trim();
                            var parts = owner.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            bool ok = parts.Length == 2 && parts[0].Length >= 2 && parts[1].Length >= 2;
                            if (!ok) { UI.Error("Введите строго «Фамилия Имя» — два слова, минимум по 2 символа."); break; }
                            if (!service.OwnerExistsByFullName(owner)) { UI.Error("Владелец с таким ФИО не найден. Сначала добавьте владельца."); break; }

                            try
                            {
                                service.AddVehicle(lp, mk, md, cl, owner);
                                UI.Ok("Машина добавлена.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                                step = 0;
                            }
                            break;
                        }
                }
            }
        }

        private static void AddSpaceWizard()
        {
            int step = 0;
            string number = "";
            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Добавление места", "1: Номер места", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Номер места (например, PS-021)", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            number = (r.text ?? "").Trim();
                            if (string.IsNullOrWhiteSpace(number)) { UI.Error("Номер обязателен."); break; }
                            try
                            {
                                service.AddSpace(number);
                                UI.Ok("Место добавлено.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                            }
                            break;
                        }
                }
            }
        }

        // ---------------------- DELETE ----------------------
        private static void ShowDeleteMenu()
        {
            Console.Clear();
            UI.BannerParking();
            UI.Panel("Удалить", "Владельца", "Машину", "Парковочное место", "Билет", "Назад");
            var c = UI.Prompt("Выберите (1-5)");
            Console.Clear();
            UI.BannerParking();
            switch (c)
            {
                case "1": RemoveOwner(); break;
                case "2": RemoveVehicle(); break;
                case "3": RemoveSpace(); break;
                case "4": RemoveTicket(); break;
                default: return;
            }
            Thread.Sleep(600);
        }

        private static void RemoveOwner()
        {
            var name = UI.Prompt("ФИО владельца (Фамилия Имя)");
            try
            {
                service.RemoveOwner(name);
                UI.Ok("Владелец удалён.");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
        }

        private static void RemoveVehicle()
        {
            var plate = UI.Prompt("Госномер машины");
            try
            {
                service.RemoveVehicle(plate);
                UI.Ok("Машина удалена.");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
        }

        private static void RemoveSpace()
        {
            var num = UI.Prompt("Номер места");
            try
            {
                service.RemoveSpace(num);
                UI.Ok("Парковочное место удалено.");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
        }

        private static void RemoveTicket()
        {
            var id = UI.Prompt("Уникальный номер билета (например, TICK-001 или просто 1)");
            try
            {
                service.RemoveTicket(id);
                UI.Ok("Билет удалён.");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
        }

        // ---------------------- EDIT ----------------------
        private static void EditEntity()
        {
            Console.Clear();
            UI.BannerParking();
            UI.Panel("Редактировать", "Владельца", "Машину", "Место");
            var choice = UI.Prompt("Выберите (1-3)");
            if (choice == "1") EditOwnerWizard();
            else if (choice == "2") EditVehicleWizard();
            else if (choice == "3") EditSpaceWizard();
        }

        private static void EditOwnerWizard()
        {
            int step = 0;
            string name = "", phone = "", email = "";

            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Редактирование владельца", "1: ФИО", "2: Новый телефон", "3: Новый email", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptNameWithNav("ФИО владельца (Фамилия Имя)", allowSpaceInside: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            name = (r.text ?? "").Trim();
                            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length != 2) { UI.Error("Нужно указать строго два слова: «Фамилия Имя»."); break; }
                            step++;
                            break;
                        }
                    case 1:
                        {
                            var r = UI.PromptPhoneWithNav("Новый телефон (+7 и 10 цифр, Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            phone = string.IsNullOrWhiteSpace(r.text) ? "" : r.text;
                            step++;
                            break;
                        }
                    case 2:
                        {
                            var r = UI.PromptWithNav("Новый email (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }

                            var txt = r.text;
                            if (!string.IsNullOrWhiteSpace(txt))
                            {
                                int at = txt.IndexOf('@'); int dot = txt.LastIndexOf('.');
                                while (!(at > 0 && dot > at + 1 && dot < txt.Length - 1))
                                {
                                    UI.Error("Email должен содержать '@' и точку после неё (или оставьте пустым).");
                                    var retry = UI.PromptWithNav("Новый email (Enter — пропустить)", allowEmpty: true);
                                    if (retry.nav == StepNav.Exit) return;
                                    if (retry.nav == StepNav.Back) { step--; goto NEXT_OWNER; }
                                    if (string.IsNullOrWhiteSpace(retry.text)) { txt = ""; break; }
                                    txt = retry.text;
                                    at = txt.IndexOf('@'); dot = txt.LastIndexOf('.');
                                }
                            }
                            email = (txt ?? "").Trim();

                            try
                            {
                                service.EditOwner(name, phone, email);
                                UI.Ok("Данные владельца обновлены.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                                step = 0;
                            }
                            break;
                        }
                }
            NEXT_OWNER:;
            }
        }

        private static void EditVehicleWizard()
        {
            int step = 0;
            string plate = "", newPlate = "", make = "", model = "", color = "";

            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Редактирование машины", "1: Текущий номер", "2: Новый номер", "3: Новая марка", "4: Новая модель", "5: Новый цвет", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Текущий госномер машины", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            plate = (r.text ?? "").Trim();
                            if (!service.PlateExists(plate)) { UI.Error("Машина с таким номером не найдена."); break; }
                            step++;
                            break;
                        }
                    case 1:
                        {
                            var r = UI.PromptWithNav("Новый госномер (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }

                            newPlate = (r.text ?? "").Trim();
                            if (!string.IsNullOrEmpty(newPlate))
                            {
                                try
                                {
                                    service.EditVehiclePlate(plate, newPlate);
                                    UI.Ok("Госномер обновлён.");
                                    plate = newPlate;
                                }
                                catch (Exception ex)
                                {
                                    UI.Error(ex.Message);
                                    break;
                                }
                            }
                            step++;
                            break;
                        }
                    case 2:
                        {
                            var r = UI.PromptWithNav("Новая марка (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            make = r.text ?? "";
                            step++;
                            break;
                        }
                    case 3:
                        {
                            var r = UI.PromptWithNav("Новая модель (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            model = r.text ?? "";
                            step++;
                            break;
                        }
                    case 4:
                        {
                            var r = UI.PromptWithNav("Новый цвет (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            color = r.text ?? "";

                            try
                            {
                                service.EditVehicle(plate, make, model, color);
                                UI.Ok("Данные машины обновлены.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                                step = 1;
                            }
                            break;
                        }
                }
            }
        }

        private static void EditSpaceWizard()
        {
            int step = 0;
            string number = "", newNumber = "";
            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Редактирование места", "1: Текущий номер", "2: Новый номер", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Текущий номер места", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            number = (r.text ?? "").Trim();
                            if (!service.SpaceExists(number)) { UI.Error("Место не найдено."); break; }
                            step++;
                            break;
                        }
                    case 1:
                        {
                            var r = UI.PromptWithNav("Новый номер места (Enter — пропустить)", allowEmpty: true);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { step--; break; }
                            newNumber = (r.text ?? "").Trim();
                            try
                            {
                                if (!string.IsNullOrEmpty(newNumber))
                                    service.EditSpace(number, newNumber);
                                UI.Ok("Место обновлено.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                                step = 0;
                            }
                            break;
                        }
                }
            }
        }

        // ---------------------- SEARCH ----------------------
        private static void ShowSearchMenu()
        {
            Console.Clear();
            UI.BannerParking();
            UI.Panel("Поиск", "Парковки по номеру", "Владельца по телефону", "Назад");
            var c = UI.Prompt("Выберите (1-3)");
            if (c == "1") SearchParkingByLicensePlate();
            else if (c == "2") SearchOwnerByPhone();
        }

        private static void SearchParkingByLicensePlate()
        {
            Console.Clear();
            UI.BannerParking();
            var lp = UI.Prompt("Госномер");
            try
            {
                var rows = service.GetParkingSpaceByLicensePlate(lp);
                UI.Table(rows, "Активная парковка по номеру");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
            Thread.Sleep(600);
        }

        private static void SearchOwnerByPhone()
        {
            Console.Clear();
            UI.BannerParking();
            var r = UI.PromptPhoneWithNav("Телефон владельца (+7 и 10 цифр)", allowEmpty: false);
            if (r.nav != StepNav.Ok) return;
            try
            {
                var rows = service.FindOwnerByPhone(r.text);
                UI.Table(rows, "Владелец по телефону");
            }
            catch (Exception ex)
            {
                UI.Error(ex.Message);
            }
            Thread.Sleep(600);
        }

        // ---------------------- PARKING (tickets) ----------------------
        private static void StartParkingWizard()
        {
            int step = 0;
            string plate = "";
            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Начать парковку", "1: Госномер", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Госномер машины для старта парковки", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            if (string.IsNullOrWhiteSpace(r.text)) { UI.Error("Госномер обязателен."); break; }
                            plate = r.text.Trim();
                            try
                            {
                                service.StartParking(plate);
                                UI.Ok("Парковка начата.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                            }
                            break;
                        }
                }
            }
        }

        private static void EndParkingWizard()
        {
            int step = 0;
            string tid = "";
            while (true)
            {
                Console.Clear();
                UI.BannerParking();
                UI.Panel("Завершить парковку", "1: Номер билета", "Esc — меню, Backspace — назад");

                switch (step)
                {
                    case 0:
                        {
                            var r = UI.PromptWithNav("Уникальный номер билета (например, 1 или TICK-001)", allowEmpty: false);
                            if (r.nav == StepNav.Exit) return;
                            if (r.nav == StepNav.Back) { break; }
                            if (string.IsNullOrWhiteSpace(r.text)) { UI.Error("Номер билета обязателен."); break; }
                            tid = r.text.Trim();
                            try
                            {
                                service.EndParking(tid);
                                UI.Ok("Парковка завершена.");
                                Thread.Sleep(600);
                                return;
                            }
                            catch (Exception ex)
                            {
                                UI.Error(ex.Message);
                            }
                            break;
                        }
                }
            }
        }

        // ---------------------- EXIT ----------------------
        private static void ExitProgram()
        {
            FX.Rule();
            UI.Info("Завершение работы…");
            Thread.Sleep(300);
        }
    }
}
