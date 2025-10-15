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

        private static void Main()
        {
            DisplayLogo();

            try
            {
                using var ctx = new ParkingContext();
                ctx.Database.CanConnect();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" Подключение к базе данных успешно!");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Ошибка подключения к базе данных:");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ResetColor();
            }

            AnimateLoading("Загрузка системы...");

            while (true)
            {
                Console.Clear();
                DisplayMenu();
                Console.Write("\nВыберите действие (0-13): ");
                string c = Console.ReadLine() ?? "";
                try
                {
                    switch (c)
                    {
                        case "1": PrintTable(service.GetAllOwners()); break;
                        case "2": PrintTable(service.GetAllVehicles()); break;
                        case "3": AddOwner(); break;
                        case "4": AddVehicle(); break;
                        case "5": PrintTable(service.GetAllSpaces()); break;
                        case "6": StartParking(); break;
                        case "7": EndParking(); break;
                        case "8": PrintTable(service.GetTickets()); break;
                        case "9": RemoveOwner(); break;
                        case "10": RemoveVehicle(); break;
                        case "11": RemoveSpace(); break;
                        case "12": RemoveTicket(); break;
                        case "13": SearchParkingByLicensePlate(); break;
                        case "0": ExitProgram(); return;
                        default: DisplayError("Неверный выбор. Попробуйте снова."); break;
                    }
                }
                catch (Exception ex)
                {
                    DisplayError($"Ошибка: {ex.Message}");
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        private static void DisplayLogo()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
   ____  _       _         ____  _                 _            
  / ___|| | __ _| |_ ___  |  _ \| | __ _ _ __   __| | ___ _ __  
 | |    | |/ _` | __/ _ \ | |_) | |/ _` | '_ \ / _` |/ _ \ '__| 
 | |___ | | (_| | ||  __/ |  _ <| | (_| | | | | (_| |  __/ |    
  \____|_|_\__,_|\__\___| |_| \_\_|\__,_|_| |_|_\__,_|\___|_|    
");
            Console.ResetColor();
            Console.WriteLine($"Текущая дата и время: {DateTime.Now:dd.MM.yyyy HH:mm:ss} PDT\n");
        }

        private static void AnimateLoading(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (char c in message)
            {
                Console.Write(c);
                Thread.Sleep(30);
            }
            for (int i = 0; i < 3; i++)
            {
                Console.Write(".");
                Thread.Sleep(200);
            }
            Console.WriteLine("\n");
            Console.ResetColor();
        }

        private static void DisplayMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n===    ПАРКОВОЧНАЯ СИСТЕМА         ===");
            Console.WriteLine("══════════════════════════════════════");
            string[] options = {
                "1. Список владельцев",
                "2. Список машин",
                "3. Добавить владельца",
                "4. Добавить машину",
                "5. Список парковочных мест",
                "6. Начать парковку",
                "7. Завершить парковку",
                "8. Показать билеты",
                "9. Удалить владельца",
                "10. Удалить машину",
                "11. Удалить парковочное место",
                "12. Удалить билет",
                "13. Поиск места по номеру авто",
                "0. Выход"
            };

            foreach (string option in options)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("➤ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(option);
            }
            Console.ResetColor();
        }

        private static void DisplayError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ {message} ❌");
            Console.ResetColor();
        }

        private static void DisplaySuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n🌟 {message} 🌟");
            Console.ResetColor();
        }

        private static void ExitProgram()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nДо свидания! Возвращайтесь снова! 😊");
            Console.ResetColor();
            Thread.Sleep(1000);
        }

        // Универсальная печать таблиц (через отражение)
        private static void PrintTable(IEnumerable<object> rows)
        {
            var list = rows?.ToList() ?? new List<object>();
            if (list.Count == 0)
            {
                DisplayError("Нет данных для отображения.");
                return;
            }

            var props = list.First().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var headers = props.Select(p => p.Name).ToArray();
            var table = new List<string[]>();

            foreach (var item in list)
            {
                table.Add(props.Select(p =>
                {
                    var val = p.GetValue(item);
                    if (val is bool b && p.Name == "IsOccupied")
                        return b ? "Занято" : "Свободно";
                    return val?.ToString() ?? "";
                }).ToArray());
            }

            int cols = headers.Length;
            int[] widths = new int[cols];
            for (int i = 0; i < cols; i++)
            {
                widths[i] = headers[i].Length;
                foreach (var row in table)
                    widths[i] = Math.Max(widths[i], row[i].Length);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("╭");
            for (int i = 0; i < cols; i++)
            {
                Console.Write(new string('─', widths[i] + 2));
                if (i < cols - 1) Console.Write("┬");
            }
            Console.WriteLine("╮");

            Console.Write("│ ");
            for (int i = 0; i < cols; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(headers[i].PadRight(widths[i]) + " │ ");
            }
            Console.WriteLine();
            Console.ResetColor();

            Console.Write("├");
            for (int i = 0; i < cols; i++)
            {
                Console.Write(new string('─', widths[i] + 2));
                if (i < cols - 1) Console.Write("┼");
            }
            Console.WriteLine("┤");

            foreach (var row in table)
            {
                Console.Write("│ ");
                for (int i = 0; i < cols; i++)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(row[i].PadRight(widths[i]) + " │ ");
                }
                Console.WriteLine();
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("╰");
            for (int i = 0; i < cols; i++)
            {
                Console.Write(new string('─', widths[i] + 2));
                if (i < cols - 1) Console.Write("┴");
            }
            Console.WriteLine("╯");
            Console.ResetColor();
        }

        // ====== ФОРМЫ ======

        private static void AddOwner()
        {
            Console.Write("Фамилия: "); var ln = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ln)) throw new Exception("Фамилия не может быть пустой");

            Console.Write("Имя: "); var fn = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fn)) throw new Exception("Имя не может быть пустым");

            // >>> автоподстановка +7 в поле ввода
            Console.Write("Телефон: +7");
            string tail = Console.ReadLine() ?? ""; // пользователь вводит оставшиеся символы
            tail = tail.Trim();

            string ph;
            if (tail.StartsWith("+7")) ph = tail;
            else if (tail.StartsWith("8")) ph = "+7" + tail.Substring(1);
            else ph = "+7" + tail;

            Console.Write("Email: "); var em = Console.ReadLine();

            service.AddOwner(fn, ln, ph, em);
            DisplaySuccess("Владелец добавлен!");
        }

        private static void AddVehicle()
        {
            Console.Write("Госномер: "); var lp = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lp)) throw new Exception("Госномер не может быть пустым");
            Console.Write("Марка: "); var mk = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(mk)) throw new Exception("Марка не может быть пустой");
            Console.Write("Модель: "); var md = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(md)) throw new Exception("Модель не может быть пустой");
            Console.Write("Цвет: "); var cl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(cl)) throw new Exception("Цвет не может быть пустым");
            Console.Write("ФИО владельца (Фамилия Имя): "); var ownerName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ownerName)) throw new Exception("ФИО владельца не может быть пустым");
            service.AddVehicle(lp, mk, md, cl, ownerName);
            DisplaySuccess("Машина добавлена!");
        }

        private static void StartParking()
        {
            var free = service.GetFreeSpaces().ToList();
            if (free.Count == 0)
            {
                DisplayError("Нет свободных парковочных мест.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nСвободные парковочные места:");
                Console.ResetColor();
                PrintTable(free);
            }

            Console.Write("Госномер машины: "); var v = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(v)) throw new Exception("Госномер не может быть пустым");
            service.StartParking(v);
            DisplaySuccess("Парковка начата!");
        }

        private static void EndParking()
        {
            Console.Write("Уникальный номер билета (например, TICK-001): "); var t = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(t)) throw new Exception("Уникальный номер не может быть пустым");
            service.EndParking(t);
            DisplaySuccess("Парковка завершена!");
        }

        private static void RemoveOwner()
        {
            Console.Write("ФИО владельца (Фамилия Имя): "); var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("ФИО владельца не может быть пустым");
            service.RemoveOwner(name);
            DisplaySuccess("Владелец удален!");
        }

        private static void RemoveVehicle()
        {
            Console.Write("Госномер машины: "); var lp = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lp)) throw new Exception("Госномер не может быть пустым");
            service.RemoveVehicle(lp);
            DisplaySuccess("Машина удалена!");
        }

        private static void RemoveSpace()
        {
            Console.Write("Номер места: "); var num = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(num)) throw new Exception("Номер места не может быть пустым");
            service.RemoveSpace(num);
            DisplaySuccess("Парковочное место удалено!");
        }

        private static void RemoveTicket()
        {
            Console.Write("Уникальный номер билета (например, TICK-001): "); var id = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(id)) throw new Exception("Уникальный номер не может быть пустым");
            service.RemoveTicket(id);
            DisplaySuccess("Билет удален!");
        }

        private static void SearchParkingByLicensePlate()
        {
            Console.Write("Введите госномер машины для поиска: "); var plate = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(plate)) throw new Exception("Госномер не может быть пустым");
            var result = service.GetParkingSpaceByLicensePlate(plate);
            if (result.Any())
                PrintTable(result);
            else
                DisplayError("Машина с таким госномером не найдена или не заняла место.");
        }
    }
}
