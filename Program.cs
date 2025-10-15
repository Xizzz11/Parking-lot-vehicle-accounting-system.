using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ParkingSystem.BLL;
using ParkingSystem.Data;

namespace ParkingSystem
{
    internal class Program
    {
        private static readonly ParkingService service = new ParkingService(new ParkingContext());

        // ================= UI =================
        public static class UI
        {
            // Мягкая палитра
            public static ConsoleColor Primary = ConsoleColor.Cyan;
            public static ConsoleColor Accent = ConsoleColor.Blue;
            public static ConsoleColor Success = ConsoleColor.DarkGreen;
            public static ConsoleColor Warning = ConsoleColor.DarkYellow;
            public static ConsoleColor Danger = ConsoleColor.DarkRed;
            public static ConsoleColor Text = ConsoleColor.Gray;
            public static ConsoleColor Border = ConsoleColor.DarkGray;
            public static ConsoleColor Muted = ConsoleColor.DarkGray;

            private static int SafeWidth()
            {
                try { return Math.Clamp(Console.WindowWidth - 2, 60, 160); }
                catch { return 100; }
            }

            // Безопасный бип (без CA1416)
            private static void TryBeep(int freq, int dur)
            {
                try { if (OperatingSystem.IsWindows()) Console.Beep(freq, dur); } catch { /* ignore */ }
            }

            // ───── Шапка ─────
            public static void Header(string title)
            {
                var w = SafeWidth();
                Console.ForegroundColor = Border;
                Console.WriteLine("╔" + new string('═', w) + "╗");

                Console.ForegroundColor = Primary;
                string t = $"  {title}  ";
                int pad = Math.Max(0, (w - t.Length) / 2);
                Console.WriteLine("║" + new string(' ', pad) + t + new string(' ', Math.Max(0, w - pad - t.Length)) + "║");

                Console.ForegroundColor = Border;
                Console.WriteLine("╚" + new string('═', w) + "╝");
                Console.ResetColor();
            }

            // ───── Разделитель ─────
            public static void Rule(string label = "")
            {
                int w = SafeWidth();
                Console.ForegroundColor = Border;
                if (string.IsNullOrWhiteSpace(label))
                {
                    Console.WriteLine(new string('─', w));
                }
                else
                {
                    string mid = $" {label} ";
                    int left = Math.Max(0, (w - mid.Length) / 2);
                    Console.WriteLine(new string('─', left) + mid + new string('─', Math.Max(0, w - left - mid.Length)));
                }
                Console.ResetColor();
            }

            // ───── Панель/карточка (вернул) ─────
            public static void Panel(string title, params string[] lines)
            {
                var w = SafeWidth();
                int width = Math.Min(Math.Max(30, Math.Max(title.Length + 6, (lines.Any() ? lines.Max(s => s.Length) + 6 : 0))), w);

                Console.ForegroundColor = Border;
                Console.WriteLine("╭" + new string('─', width) + "╮");

                Console.Write("│ ");
                Console.ForegroundColor = Primary;
                Console.Write(title.PadRight(width - 2));
                Console.ForegroundColor = Border;
                Console.WriteLine(" │");

                Console.WriteLine("├" + new string('─', width) + "┤");

                foreach (var l in lines)
                {
                    Console.Write("│ ");
                    Console.ForegroundColor = Text;
                    var s = l.Length > width - 2 ? l[..(width - 2)] : l.PadRight(width - 2);
                    Console.Write(s);
                    Console.ForegroundColor = Border;
                    Console.WriteLine(" │");
                }

                Console.WriteLine("╰" + new string('─', width) + "╯");
                Console.ResetColor();
            }

            // ───── Сообщения ─────
            public static void Ok(string m) { Console.ForegroundColor = Success; Console.WriteLine("✔ " + m); Console.ResetColor(); }
            public static void Warn(string m) { Console.ForegroundColor = Warning; Console.WriteLine("⚠ " + m); Console.ResetColor(); }
            public static void Error(string m) { Console.ForegroundColor = Danger; Console.WriteLine("✖ " + m); Console.ResetColor(); }
            public static void Info(string m) { Console.ForegroundColor = Muted; Console.WriteLine("ℹ " + m); Console.ResetColor(); }

            // ───── Базовый Prompt (нужен местами) ─────
            public static string Prompt(string label, string? prefix = null)
            {
                Console.ForegroundColor = Muted; Console.Write("• ");
                Console.ForegroundColor = Text; Console.Write(label);
                Console.ForegroundColor = Accent;
                if (prefix == null)
                {
                    Console.Write(": "); Console.ResetColor();
                    return (Console.ReadLine() ?? "").Trim();
                }
                else
                {
                    Console.Write(": " + prefix); Console.ResetColor();
                    var tail = Console.ReadLine() ?? "";
                    return (prefix + tail).Trim();
                }
            }

            // ─────────────────────────────────────────────────────────────────
            //   ВВОД С ОГРАНИЧЕНИЯМИ (проверка сразу после Enter)
            //   • Только русские буквы для Фамилии/Имени
            //   • Телефон с маской +7 и 10 цифр
            // ─────────────────────────────────────────────────────────────────

            private static bool IsRussianLetter(char ch)
            {
                return (ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || ch == 'Ё' || ch == 'ё';
            }

            /// Ввод слова из русских букв. Enter завершает поле, Backspace работает.
            public static string InputRussianWord(string fieldName)
            {
                while (true)
                {
                    Console.ForegroundColor = Muted; Console.Write("• ");
                    Console.ForegroundColor = Text; Console.Write(fieldName);
                    Console.ForegroundColor = Accent; Console.Write(": ");
                    Console.ResetColor();

                    string buffer = "";
                    while (true)
                    {
                        var key = Console.ReadKey(true);

                        if (key.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            if (buffer.Length < 2)
                            {
                                Error($"{fieldName} слишком короткая. Минимум 2 буквы.");
                                break; // повторить весь ввод поля
                            }
                            return buffer;
                        }
                        else if (key.Key == ConsoleKey.Backspace)
                        {
                            if (buffer.Length > 0)
                            {
                                buffer = buffer[..^1];
                                Console.Write("\b \b");
                            }
                        }
                        else
                        {
                            char ch = key.KeyChar;
                            if (IsRussianLetter(ch))
                            {
                                if (buffer.Length < 50)
                                {
                                    buffer += ch;
                                    Console.Write(ch);
                                }
                            }
                            else
                            {
                                TryBeep(800, 60); // не даём вводить лишнего
                            }
                        }
                    }
                }
            }

            /// Телефон: печатаем +7 и принимаем ровно 10 цифр. Backspace и Enter.
            public static string InputPhoneMasked(string label = "Телефон")
            {
                while (true)
                {
                    Console.ForegroundColor = Muted; Console.Write("• ");
                    Console.ForegroundColor = Text; Console.Write(label);
                    Console.ForegroundColor = Accent; Console.Write(": +7");
                    Console.ResetColor();

                    string digits = "";
                    while (true)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            if (digits.Length != 10)
                            {
                                Error("После +7 должно быть ровно 10 цифр.");
                                break; // повторяем ввод
                            }
                            return "+7" + digits;
                        }
                        else if (key.Key == ConsoleKey.Backspace)
                        {
                            if (digits.Length > 0)
                            {
                                digits = digits[..^1];
                                Console.Write("\b \b");
                            }
                        }
                        else if (char.IsDigit(key.KeyChar))
                        {
                            if (digits.Length < 10)
                            {
                                digits += key.KeyChar;
                                Console.Write(key.KeyChar);
                            }
                            else
                            {
                                TryBeep(800, 60);
                            }
                        }
                        else
                        {
                            TryBeep(800, 60);
                        }
                    }
                }
            }

            // ───── Таблица (как раньше) ─────
            private static List<string> WrapCell(string text, int width)
            {
                text ??= "";
                if (width <= 3) return new List<string> { text.Length <= width ? text : text[..width] };

                var words = text.Split(' ', StringSplitOptions.None);
                var lines = new List<string>();
                var cur = "";

                foreach (var w in words)
                {
                    if (cur.Length == 0)
                    {
                        if (w.Length <= width) cur = w;
                        else
                        {
                            for (int i = 0; i < w.Length; i += width)
                                lines.Add(w.Substring(i, Math.Min(width, w.Length - i)));
                        }
                    }
                    else
                    {
                        if (cur.Length + 1 + w.Length <= width) cur += " " + w;
                        else
                        {
                            lines.Add(cur);
                            cur = w.Length <= width ? w : w[..width];
                            for (int i = width; i < w.Length; i += width)
                                lines.Add(w.Substring(i, Math.Min(width, w.Length - i)));
                        }
                    }
                }
                if (cur.Length > 0) lines.Add(cur);
                if (lines.Count == 0) lines.Add("");
                return lines;
            }

            public static void Table(IEnumerable<object> rows)
            {
                var list = rows?.ToList() ?? new List<object>();
                if (list.Count == 0) { Warn("Нет данных для отображения."); return; }

                var props = list.First().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var headers = props.Select(p => p.Name).ToArray();

                var raw = list.Select(item => props.Select(p =>
                {
                    var v = p.GetValue(item);
                    return v switch
                    {
                        bool b when p.Name == "IsOccupied" => b ? "Занято" : "Свободно",
                        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
                        _ => v?.ToString() ?? ""
                    };
                }).ToArray()).ToList();

                int cols = headers.Length;
                int pad = 1;
                int minCol = 8;
                int maxCol = 40;
                int[] widths = new int[cols];

                for (int c = 0; c < cols; c++)
                {
                    int maxCell = Math.Max(headers[c].Length, raw.Max(r => r[c].Length));
                    widths[c] = Math.Clamp(maxCell, minCol, maxCol);
                }

                int TotalWidth() => widths.Sum() + (pad * 2 + 1) * cols + 1;
                int maxW = SafeWidth();
                while (TotalWidth() > maxW && widths.Any(w => w > minCol))
                {
                    int i = Array.IndexOf(widths, widths.Max());
                    widths[i]--;
                }
                int spare = maxW - TotalWidth();
                while (spare > 0)
                {
                    bool grown = false;
                    for (int i = 0; i < cols && spare > 0; i++)
                    {
                        if (widths[i] < maxCol) { widths[i]++; spare--; grown = true; }
                    }
                    if (!grown) break;
                }

                void H(char L, char M, char R)
                {
                    Console.ForegroundColor = Border;
                    Console.Write(L);
                    for (int i = 0; i < cols; i++)
                    {
                        Console.Write(new string('─', widths[i] + pad * 2));
                        Console.Write(i == cols - 1 ? R : M);
                    }
                    Console.WriteLine();
                }

                H('╔', '╦', '╗');
                Console.Write("║");
                for (int i = 0; i < cols; i++)
                {
                    Console.Write(new string(' ', pad));
                    Console.ForegroundColor = Primary;
                    string h = headers[i];
                    int left = Math.Max(0, (widths[i] - h.Length) / 2);
                    Console.Write(new string(' ', left));
                    Console.Write(h.Length > widths[i] ? h[..widths[i]] : h);
                    Console.Write(new string(' ', Math.Max(0, widths[i] - left - Math.Min(h.Length, widths[i]))));
                    Console.ForegroundColor = Border;
                    Console.Write(new string(' ', pad) + "║");
                }
                Console.WriteLine();
                H('╟', '╫', '╢');

                foreach (var row in raw)
                {
                    var wrapped = new List<List<string>>(cols);
                    int height = 1;
                    for (int i = 0; i < cols; i++)
                    {
                        var lines = WrapCell(row[i], widths[i]);
                        wrapped.Add(lines);
                        height = Math.Max(height, lines.Count);
                    }

                    for (int line = 0; line < height; line++)
                    {
                        Console.ForegroundColor = Border;
                        Console.Write("║");
                        for (int i = 0; i < cols; i++)
                        {
                            string cellLine = line < wrapped[i].Count ? wrapped[i][line] : "";
                            Console.Write(new string(' ', pad));
                            Console.ForegroundColor = Text;
                            Console.Write(cellLine.PadRight(widths[i]));
                            Console.ForegroundColor = Border;
                            Console.Write(new string(' ', pad) + "║");
                        }
                        Console.WriteLine();
                    }
                }
                H('╚', '╩', '╝');
                Console.ResetColor();
            }
        }

        // ================= MAIN =================
        private static void Main()
        {
            Console.Title = "Parking System — Console";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();

            UI.Header("ПАРКОВОЧНАЯ СИСТЕМА");
            try
            {
                using var ctx = new ParkingContext();
                if (ctx.Database.CanConnect()) UI.Ok("Подключение к базе данных успешно.");
                else UI.Warn("Не удалось проверить подключение к БД.");
            }
            catch (Exception ex) { UI.Error("Ошибка БД: " + ex.Message); }

            UI.Rule("Готово к работе");

            while (true)
            {
                UI.Panel("Главное меню",
                    "1. Просмотр",
                    "2. Добавить",
                    "3. Удалить",
                    "4. Редактировать",
                    "5. Поиск",
                    "6. Начать парковку",
                    "7. Завершить парковку",
                    "0. Выход");

                string c = UI.Prompt("Выберите действие (0-7)");
                try
                {
                    switch (c)
                    {
                        case "1": ShowViewMenu(); break;
                        case "2": ShowAddMenu(); break;
                        case "3": ShowDeleteMenu(); break;
                        case "4": EditEntity(); break;
                        case "5": ShowSearchMenu(); break;
                        case "6": StartParking(); break;
                        case "7": EndParking(); break;
                        case "0": ExitProgram(); return;
                        default: UI.Error("Неверный выбор."); break;
                    }
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    UI.Error("Ошибка сохранения в БД: " + (ex.InnerException?.Message ?? ex.Message));
                }
                catch (Exception ex)
                {
                    UI.Error(ex.Message);
                }

                UI.Info("Нажмите любую клавишу, чтобы продолжить…");
                Console.ReadKey(true);
                Console.Clear();
                UI.Header("ПАРКОВОЧНАЯ СИСТЕМА");
            }
        }

        // ================= Подменю =================
        private static void ShowViewMenu()
        {
            UI.Panel("Просмотр", "1. Владельцы", "2. Машины", "3. Парковочные места", "4. Билеты");
            var sub = UI.Prompt("Выберите (1-4)");
            switch (sub)
            {
                case "1": UI.Table(service.GetAllOwners()); break;
                case "2": UI.Table(service.GetAllVehicles()); break;
                case "3": UI.Table(service.GetAllSpaces()); break;
                case "4": UI.Table(service.GetTickets()); break;
                default: UI.Error("Неверный выбор."); break;
            }
        }

        private static void ShowAddMenu()
        {
            UI.Panel("Добавить", "1. Владельца", "2. Машину");
            var sub = UI.Prompt("Выберите (1-2)");
            switch (sub)
            {
                case "1": AddOwner(); break;
                case "2": AddVehicle(); break;
                default: UI.Error("Неверный выбор."); break;
            }
        }

        private static void ShowDeleteMenu()
        {
            UI.Panel("Удалить", "1. Владельца", "2. Машину", "3. Парковочное место", "4. Билет");
            var sub = UI.Prompt("Выберите (1-4)");
            switch (sub)
            {
                case "1": RemoveOwner(); break;
                case "2": RemoveVehicle(); break;
                case "3": RemoveSpace(); break;
                case "4": RemoveTicket(); break;
                default: UI.Error("Неверный выбор."); break;
            }
        }

        private static void ShowSearchMenu()
        {
            UI.Panel("Поиск", "1. Место по номеру машины", "2. Владелец по телефону");
            var sub = UI.Prompt("Выберите (1-2)");
            switch (sub)
            {
                case "1": SearchParkingByLicensePlate(); break;
                case "2": SearchOwnerByPhone(); break;
                default: UI.Error("Неверный выбор."); break;
            }
        }

        // ================= Формы =================
        private static void AddOwner()
        {
            // Живой ввод: только русские буквы; ошибка — сразу
            var ln = UI.InputRussianWord("Фамилия");
            var fn = UI.InputRussianWord("Имя");

            // Телефон — маска +7 и 10 цифр; ошибка — сразу
            var phone = UI.InputPhoneMasked();

            // Email — простая проверка (или пусто)
            var email = UI.Prompt("Email (можно оставить пустым)");
            if (!string.IsNullOrWhiteSpace(email))
            {
                int at = email.IndexOf('@');
                int dot = email.LastIndexOf('.');
                while (!(at > 0 && dot > at + 1 && dot < email.Length - 1))
                {
                    UI.Error("Email должен содержать '@' и точку после неё (или оставьте поле пустым).");
                    email = UI.Prompt("Email (можно оставить пустым)");
                    if (string.IsNullOrWhiteSpace(email)) break;
                    at = email.IndexOf('@');
                    dot = email.LastIndexOf('.');
                }
            }

            service.AddOwner(fn, ln, phone, email);
            UI.Ok("Владелец добавлен.");
        }

        private static void AddVehicle()
        {
            var lp = UI.Prompt("Госномер");
            var mk = UI.Prompt("Марка");
            var md = UI.Prompt("Модель");
            var cl = UI.Prompt("Цвет");

            // «Фамилия Имя» — проверка сразу
            string owner;
            while (true)
            {
                owner = UI.Prompt("ФИО владельца (Фамилия Имя — русскими буквами)");
                var parts = owner.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                bool ok = parts.Length == 2
                          && parts[0].All(ch => (ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || ch == 'Ё' || ch == 'ё')
                          && parts[1].All(ch => (ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || ch == 'Ё' || ch == 'ё')
                          && parts[0].Length >= 2 && parts[1].Length >= 2;
                if (ok) break;
                UI.Error("Введите ФИО в формате «Фамилия Имя», только русские буквы, от 2 символов каждая часть.");
            }

            service.AddVehicle(lp, mk, md, cl, owner);
            UI.Ok("Машина добавлена.");
        }

        private static void EditEntity()
        {
            UI.Panel("Редактировать", "1. Владельца", "2. Машину");
            var choice = UI.Prompt("Выберите (1-2)");
            if (choice == "1")
            {
                var name = UI.Prompt("ФИО владельца (Фамилия Имя)");

                string phone = UI.Prompt("Новый телефон (+7XXXXXXXXXX, Enter — пропустить)", "+7");
                if (phone != "+7")
                {
                    var d = new string(phone.StartsWith("+7") ? phone[2..].Where(char.IsDigit).ToArray() : phone.Where(char.IsDigit).ToArray());
                    while (d.Length != 10)
                    {
                        UI.Error("После +7 должно быть ровно 10 цифр. Попробуйте ещё раз.");
                        phone = UI.Prompt("Новый телефон (+7XXXXXXXXXX, Enter — пропустить)", "+7");
                        if (phone == "+7") { d = ""; break; }
                        d = new string(phone.StartsWith("+7") ? phone[2..].Where(char.IsDigit).ToArray() : phone.Where(char.IsDigit).ToArray());
                    }
                    phone = d == "" ? "" : "+7" + d;
                }
                else phone = "";

                var email = UI.Prompt("Новый email (Enter — пропустить)");
                if (!string.IsNullOrWhiteSpace(email))
                {
                    int at = email.IndexOf('@');
                    int dot = email.LastIndexOf('.');
                    while (!(at > 0 && dot > at + 1 && dot < email.Length - 1))
                    {
                        UI.Error("Email должен содержать '@' и точку после неё (или оставьте пустым).");
                        email = UI.Prompt("Новый email (Enter — пропустить)");
                        if (string.IsNullOrWhiteSpace(email)) break;
                        at = email.IndexOf('@');
                        dot = email.LastIndexOf('.');
                    }
                }

                service.EditOwner(name, phone, email);
                UI.Ok("Данные владельца обновлены.");
            }
            else if (choice == "2")
            {
                var plate = UI.Prompt("Госномер машины");
                var make = UI.Prompt("Новая марка (Enter — пропустить)");
                var model = UI.Prompt("Новая модель (Enter — пропустить)");
                var color = UI.Prompt("Новый цвет (Enter — пропустить)");
                service.EditVehicle(plate, make, model, color);
                UI.Ok("Данные машины обновлены.");
            }
            else UI.Error("Неверный выбор.");
        }

        private static void RemoveOwner()
        {
            var name = UI.Prompt("ФИО владельца (Фамилия Имя)");
            service.RemoveOwner(name);
            UI.Ok("Владелец удалён.");
        }

        private static void RemoveVehicle()
        {
            var plate = UI.Prompt("Госномер машины");
            service.RemoveVehicle(plate);
            UI.Ok("Машина удалена.");
        }

        private static void RemoveSpace()
        {
            var num = UI.Prompt("Номер места");
            service.RemoveSpace(num);
            UI.Ok("Парковочное место удалено.");
        }

        private static void RemoveTicket()
        {
            var id = UI.Prompt("Уникальный номер билета (например, TICK-001)");
            service.RemoveTicket(id);
            UI.Ok("Билет удалён.");
        }

        private static void SearchParkingByLicensePlate()
        {
            var plate = UI.Prompt("Госномер машины для поиска");
            var result = service.GetParkingSpaceByLicensePlate(plate);
            if (result.Any()) UI.Table(result);
            else UI.Warn("Не найдено активных парковок для этого номера.");
        }

        private static void SearchOwnerByPhone()
        {
            var phone = UI.InputPhoneMasked("Телефон для поиска");
            var result = service.FindOwnerByPhone(phone);
            if (result.Any()) UI.Table(result);
            else UI.Warn("Владелец с таким телефоном не найден.");
        }

        private static void StartParking()
        {
            var free = service.GetFreeSpaces().ToList();
            if (free.Count == 0) UI.Warn("Нет свободных мест.");
            else { UI.Panel("Свободные места", "Список ниже:"); UI.Table(free); }

            var v = UI.Prompt("Госномер машины");
            service.StartParking(v);
            UI.Ok("Парковка начата.");
        }

        private static void EndParking()
        {
            var t = UI.Prompt("Уникальный номер билета (например, TICK-001)");
            service.EndParking(t);
            UI.Ok("Парковка завершена.");
        }

        private static void ExitProgram()
        {
            UI.Rule();
            UI.Info("Завершение работы…");
            Thread.Sleep(400);
        }
    }
}
