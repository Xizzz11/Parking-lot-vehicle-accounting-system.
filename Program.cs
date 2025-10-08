using System;
using System.Data;
using ParkingSystem.BLL;
using ParkingSystem.DAL;


    // ... далее обычное меню



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
                Console.WriteLine("=== Parking Management ===");
                Console.WriteLine("1. Список владельцев");
                Console.WriteLine("2. Список машин");
                Console.WriteLine("3. Добавить владельца");
                Console.WriteLine("4. Добавить машину");
                Console.WriteLine("5. Список парковочных мест");
                Console.WriteLine("6. Начать парковку");
                Console.WriteLine("7. Завершить парковку");
                Console.WriteLine("8. Показать билеты");
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
                        case "0": return;
                        default: Console.WriteLine("Неверно."); break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка: " + ex.Message);
                }

                Console.WriteLine("\nНажмите любую клавишу...");
                Console.ReadKey();
            }
        }

        static void Show(DataTable dt)
        {
            Console.Clear();
            foreach (DataColumn col in dt.Columns)
                Console.Write($"{col.ColumnName}\t");
            Console.WriteLine("\n--------------------------------------------------");
            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                    Console.Write($"{item}\t");
                Console.WriteLine();
            }
        }

        static void AddOwner()
        {
            Console.Write("Имя: "); var fn = Console.ReadLine();
            Console.Write("Фамилия: "); var ln = Console.ReadLine();
            Console.Write("Телефон: "); var ph = Console.ReadLine();
            Console.Write("Email: "); var em = Console.ReadLine();
            service.AddOwner(fn!, ln!, ph!, em);
            Console.WriteLine("Владелец добавлен!");
        }

        static void AddVehicle()
        {
            Console.Write("Госномер: "); var lp = Console.ReadLine();
            Console.Write("Марка: "); var mk = Console.ReadLine();
            Console.Write("Модель: "); var md = Console.ReadLine();
            Console.Write("Цвет: "); var cl = Console.ReadLine();
            Console.Write("ID владельца: "); var id = int.Parse(Console.ReadLine()!);
            service.AddVehicle(lp!, mk!, md!, cl!, id);
            Console.WriteLine("Машина добавлена!");
        }

        static void StartParking()
        {
            Console.Write("VehicleID: "); var v = int.Parse(Console.ReadLine()!);
            Console.Write("Номер места: "); var s = Console.ReadLine();
            service.StartParking(v, s!);
            Console.WriteLine("Парковка начата!");
        }

        static void EndParking()
        {
            Console.Write("TicketID: "); var t = int.Parse(Console.ReadLine()!);
            service.EndParking(t);
            Console.WriteLine("Парковка завершена!");
        }
    }
}

