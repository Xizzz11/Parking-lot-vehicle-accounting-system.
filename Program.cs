using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ParkingSystem.BLL;
using ParkingSystem.DAL;

namespace ParkingSystem
{
    internal class Program
    {
        static ParkingService service = new();

        static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=== Parking Management ===");
                Console.ResetColor();
                Console.WriteLine("1. Список владельцев");
                Console.WriteLine("2. Список машин");
                Console.WriteLine("3. Добавить владельца");
                Console.WriteLine("4. Добавить машину");
                Console.WriteLine("5. Список парковочных мест");
                Console.WriteLine("6. Начать парковку");
                Console.WriteLine("7. Завершить парковку");
                Console.WriteLine("8. Показать билеты");
                Console.WriteLine("9. Удалить владельца");
                Console.WriteLine("10. Удалить машину");
                Console.WriteLine("11. Удалить парковочное место");
                Console.WriteLine("12. Удалить билет");
                Console.WriteLine("13. Поиск владельца по ФИО");
                Console.WriteLine("14. Поиск машины по госномеру");
                Console.WriteLine("0. Выход");
                Console.Write("Выбор: ");
                var c = Console.ReadLine();
                try
                {
                    switch (c)
                    {
                        case "1": Show(service.GetAllOwners()); break;
                        case "2": Show(service.GetAllVehicles()); break;
                        case "3": AddOwner(); break;
                        case "4": AddVehicle(); break;
                        case "5": Show(service.GetAllSpaces()); break;
                        case "6": StartParking(); break;
                        case "7": EndParking(); break;
                        case "8": Show(service.GetTickets()); break;
                        case "9": RemoveOwner(); break;
                        case "10": RemoveVehicle(); break;
                        case "11": RemoveSpace(); break;
                        case "12": RemoveTicket(); break;
                        case "13": SearchOwnerByName(); break;
                        case "14": SearchVehicleByPlate(); break;
                        case "0": return;
                        default: Console.WriteLine("Неверно."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Ошибка: " + ex.Message);
                    Console.ResetColor();
                }

                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static void Show(DataTable dt)
        {
            if (dt.Rows.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Нет данных для отображения.");
                Console.ResetColor();
                return;
            }

            int[] maxLengths = new int[dt.Columns.Count];
            foreach (DataColumn col in dt.Columns)
            {
                maxLengths[col.Ordinal] = col.ColumnName.Length;
                foreach (DataRow row in dt.Rows)
                {
                    string value = row[col].ToString();
                    if (col.ColumnName == "IsOccupied")
                    {
                        bool isOccupied;
                        if (bool.TryParse(value, out isOccupied))
                        {
                            value = isOccupied ? "Занято" : "Свободно";
                        }
                        else
                        {
                            value = "Неизвестно";
                        }
                    }
                    int length = value.Length;
                    if (length > maxLengths[col.Ordinal]) maxLengths[col.Ordinal] = length;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                Console.Write("│ " + dt.Columns[i].ColumnName.PadRight(maxLengths[i]) + " ");
            }
            Console.WriteLine("│");
            Console.WriteLine(new string('─', maxLengths.Sum() + (dt.Columns.Count * 3) + 1));

            Console.ResetColor();
            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    string value = row[i].ToString();
                    if (dt.Columns[i].ColumnName == "IsOccupied")
                    {
                        bool isOccupied;
                        if (bool.TryParse(value, out isOccupied))
                        {
                            value = isOccupied ? "Занято" : "Свободно";
                        }
                        else
                        {
                            value = "Неизвестно";
                        }
                    }
                    Console.Write("│ " + value.PadRight(maxLengths[i]) + " ");
                }
                Console.WriteLine("│");
            }
            Console.WriteLine(new string('─', maxLengths.Sum() + (dt.Columns.Count * 3) + 1));
        }

        static void ShowFreeSpaces()
        {
            DataTable allSpaces = service.GetAllSpaces();
            var freeSpaces = allSpaces.AsEnumerable()
                .Where(r =>
                {
                    bool isOccupied;
                    return bool.TryParse(r["IsOccupied"].ToString(), out isOccupied) && !isOccupied;
                })
                .CopyToDataTable();
            if (freeSpaces.Rows.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Нет свободных парковочных мест.");
                Console.ResetColor();
                return;
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Свободные парковочные места:");
            Console.ResetColor();
            Show(freeSpaces);
        }

        static void AddOwner()
        {
            Console.Write("Фамилия: "); var ln = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ln)) throw new Exception("Фамилия не может быть пустой");
            Console.Write("Имя: "); var fn = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fn)) throw new Exception("Имя не может быть пустым");
            Console.Write("Телефон: "); var ph = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ph)) throw new Exception("Телефон не может быть пустым");
            Console.Write("Email: "); var em = Console.ReadLine();
            service.AddOwner(fn, ln, ph, em);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Владелец добавлен!");
            Console.ResetColor();
        }

        static void AddVehicle()
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
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Машина добавлена!");
            Console.ResetColor();
        }

        static void StartParking()
        {
            ShowFreeSpaces();
            Console.Write("Госномер машины: "); var v = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(v)) throw new Exception("Госномер не может быть пустым");
            service.StartParking(v);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Парковка начата! Использовано место: " + v); // Добавлено для информативности
            Console.ResetColor();
        }

        static void EndParking()
        {
            Console.Write("Уникальный номер билета (например, TICK-001): "); var t = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(t)) throw new Exception("Уникальный номер не может быть пустым");
            int ticketId = service.GetTicketIdFromUniqueNumber(t);
            service.EndParking(ticketId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Парковка завершена!");
            Console.ResetColor();
        }

        static void RemoveOwner()
        {
            Console.Write("ФИО владельца (Фамилия Имя): "); var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("ФИО владельца не может быть пустым");
            service.RemoveOwner(name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Владелец удален!");
            Console.ResetColor();
        }

        static void RemoveVehicle()
        {
            Console.Write("Госномер машины: "); var lp = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lp)) throw new Exception("Госномер не может быть пустым");
            service.RemoveVehicle(lp);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Машина удалена!");
            Console.ResetColor();
        }

        static void RemoveSpace()
        {
            Console.Write("Номер места: "); var num = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(num)) throw new Exception("Номер места не может быть пустым");
            service.RemoveSpace(num);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Парковочное место удалено!");
            Console.ResetColor();
        }

        static void RemoveTicket()
        {
            Console.Write("Уникальный номер билета (например, TICK-001): "); var id = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(id)) throw new Exception("Уникальный номер не может быть пустым");
            int ticketId = service.GetTicketIdFromUniqueNumber(id);
            service.RemoveTicket(ticketId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Билет удален!");
            Console.ResetColor();
        }

        static void SearchOwnerByName()
        {
            Console.Write("Введите ФИО для поиска (Фамилия Имя, например, Иванов Иван): ");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("ФИО не может быть пустым");

            var owners = service.GetAllOwners().AsEnumerable()
                .Where(r => r["FullName"].ToString()
                    .Contains(name, StringComparison.OrdinalIgnoreCase))
                .CopyToDataTable();

            if (owners.Rows.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Владельцы не найдены.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Результаты поиска владельцев:");
                Console.ResetColor();
                Show(owners);
            }
        }

        static void SearchVehicleByPlate()
        {
            Console.Write("Введите госномер для поиска (например, X123AB): ");
            var plate = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(plate)) throw new Exception("Госномер не может быть пустым");

            var vehicles = service.GetAllVehicles().AsEnumerable()
                .Where(r => r["LicensePlate"].ToString()
                    .Contains(plate, StringComparison.OrdinalIgnoreCase))
                .CopyToDataTable();

            if (vehicles.Rows.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Машины не найдены.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Результаты поиска машин:");
                Console.ResetColor();
                Show(vehicles);
            }
        }
    }
}