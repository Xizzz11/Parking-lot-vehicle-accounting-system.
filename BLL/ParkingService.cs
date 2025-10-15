using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Models;

namespace ParkingSystem.BLL
{
    public class ParkingService
    {
        private readonly ParkingContext _ctx;

        public ParkingService(ParkingContext ctx)
        {
            _ctx = ctx;
        }

        // ======== Добавление и удаление владельцев / машин ========

        public void AddOwner(string first, string last, string phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new Exception("Имя и фамилия обязательны");
            if (first.Any(char.IsDigit) || last.Any(char.IsDigit))
                throw new Exception("Имя и фамилия не должны содержать цифры");

            // === Автоматическая нормализация номера: всегда +7XXXXXXXXXX ===
            phone = (phone ?? "").Trim();

            if (phone.StartsWith("8"))
                phone = "+7" + phone.Substring(1);

            if (Regex.IsMatch(phone, @"^\d{10}$"))
                phone = "+7" + phone;

            if (!Regex.IsMatch(phone, @"^\+7\d{10}$"))
                throw new Exception("Телефон должен быть в формате +7XXXXXXXXXX (например, +79991234567)");

            // === Проверка email ===
            if (!string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Неверный формат email (например, user@example.com)");

            var owner = new VehicleOwner
            {
                FirstName = first.Trim(),
                LastName = last.Trim(),
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            };
            _ctx.VehicleOwners.Add(owner);
            _ctx.SaveChanges();
        }

        public void AddVehicle(string plate, string make, string model, string color, string ownerFullName)
        {
            if (string.IsNullOrWhiteSpace(plate) || plate.Length < 6 || plate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");
            if (string.IsNullOrWhiteSpace(make))
                throw new Exception("Марка автомобиля обязательна");
            if (string.IsNullOrWhiteSpace(model))
                throw new Exception("Модель автомобиля обязательна");
            if (string.IsNullOrWhiteSpace(color))
                throw new Exception("Цвет автомобиля обязателен");

            var parts = (ownerFullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new Exception("Введите ФИО в формате 'Фамилия Имя'");

            var owner = _ctx.VehicleOwners
                .FirstOrDefault(o => o.LastName + " " + o.FirstName == ownerFullName);
            if (owner == null) throw new Exception("Владелец с таким ФИО не найден");

            if (_ctx.Vehicles.Any(v => v.LicensePlate == plate))
                throw new Exception("Машина с таким госномером уже существует");

            var vehicle = new Vehicle
            {
                LicensePlate = plate.Trim(),
                Make = make.Trim(),
                Model = model.Trim(),
                Color = color.Trim(),
                OwnerID = owner.OwnerID
            };
            _ctx.Vehicles.Add(vehicle);
            _ctx.SaveChanges();
        }

        public void RemoveOwner(string ownerFullName)
        {
            var parts = (ownerFullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new Exception("Введите ФИО в формате 'Фамилия Имя'");

            var owner = _ctx.VehicleOwners
                .Include(o => o.Vehicles)
                .FirstOrDefault(o => o.LastName + " " + o.FirstName == ownerFullName);
            if (owner == null) throw new Exception("Владелец с таким ФИО не найден");
            if (owner.Vehicles.Any()) throw new Exception("Нельзя удалить владельца, у которого есть машины");

            _ctx.VehicleOwners.Remove(owner);
            _ctx.SaveChanges();
        }

        public void RemoveVehicle(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                throw new Exception("Госномер обязателен");

            var vehicle = _ctx.Vehicles.FirstOrDefault(v => v.LicensePlate == plate);
            if (vehicle == null) throw new Exception("Машина с таким госномером не найдена");

            var hasActiveTickets = _ctx.ParkingTickets.Any(t => t.VehicleID == vehicle.VehicleID && t.ExitTime == null);
            if (hasActiveTickets) throw new Exception("Нельзя удалить машину с активным билетом");

            _ctx.Vehicles.Remove(vehicle);
            _ctx.SaveChanges();
        }

        public void RemoveSpace(string spaceNumber)
        {
            if (string.IsNullOrWhiteSpace(spaceNumber) || spaceNumber.Length > 10)
                throw new Exception("Номер места обязателен и не должен превышать 10 символов");

            var ps = _ctx.ParkingSpaces.FirstOrDefault(p => p.SpaceNumber == spaceNumber);
            if (ps == null) throw new Exception("Место не найдено");
            if (ps.IsOccupied) throw new Exception("Нельзя удалить занятое место");

            _ctx.ParkingSpaces.Remove(ps);
            _ctx.SaveChanges();
        }

        public void RemoveTicket(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber) || !uniqueNumber.StartsWith("TICK-"))
                throw new Exception("Неверный формат уникального номера (например, TICK-001)");

            int ticketId = GetTicketIdFromUniqueNumber(uniqueNumber);

            var ticket = _ctx.ParkingTickets.FirstOrDefault(t => t.TicketID == ticketId);
            if (ticket == null) throw new Exception("Билет с таким уникальным номером не найден");
            if (ticket.ExitTime == null) throw new Exception("Нельзя удалить активный билет");

            _ctx.ParkingTickets.Remove(ticket);
            _ctx.SaveChanges();
        }

        // ======== Чтение списков ========

        public IEnumerable<object> GetAllOwners()
        {
            return _ctx.VehicleOwners
                .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
                .Select(o => new { FullName = o.LastName + " " + o.FirstName, o.PhoneNumber, o.Email })
                .ToList<object>();
        }

        public IEnumerable<object> GetAllVehicles()
        {
            return _ctx.Vehicles
                .Include(v => v.Owner)
                .OrderBy(v => v.LicensePlate)
                .Select(v => new { v.LicensePlate, v.Make, v.Model, v.Color, Owner = v.Owner!.LastName + " " + v.Owner!.FirstName })
                .ToList<object>();
        }

        public IEnumerable<object> GetAllSpaces()
        {
            return _ctx.ParkingSpaces
                .OrderBy(p => p.SpaceNumber)
                .Select(p => new { p.SpaceNumber, p.IsOccupied })
                .ToList<object>();
        }

        // >>> Новый метод: только свободные места (для StartParking)
        public IEnumerable<object> GetFreeSpaces()
        {
            return _ctx.ParkingSpaces
                .Where(p => !p.IsOccupied)
                .OrderBy(p => p.SpaceNumber)
                .Select(p => new { p.SpaceNumber, p.IsOccupied })
                .ToList<object>();
        }

        public IEnumerable<object> GetTickets()
        {
            return _ctx.ParkingTickets
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSpace)
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new
                {
                    UniqueNumber = "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    LicensePlate = t.Vehicle!.LicensePlate,
                    SpaceNumber = t.ParkingSpace!.SpaceNumber,
                    EntryTime = t.EntryTime,
                    ExitTime = t.ExitTime,
                    Fee = t.Fee
                })
                .ToList<object>();
        }

        public IEnumerable<object> GetParkingSpaceByLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate) || licensePlate.Length < 6 || licensePlate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            return _ctx.ParkingTickets
                .Include(t => t.ParkingSpace)
                .Include(t => t.Vehicle)
                .Where(t => t.Vehicle!.LicensePlate == licensePlate && t.ExitTime == null)
                .Select(t => new
                {
                    t.ParkingSpace!.SpaceNumber,
                    t.ParkingSpace!.IsOccupied,
                    TicketNumber = "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    t.EntryTime,
                    t.ExitTime
                })
                .ToList<object>();
        }

        // ======== Парковка ========

        public void StartParking(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate) || plate.Length < 6 || plate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            var vehicle = _ctx.Vehicles.FirstOrDefault(v => v.LicensePlate == plate);
            if (vehicle == null) throw new Exception("Машина с таким госномером не найдена");

            var space = _ctx.ParkingSpaces.FirstOrDefault(s => !s.IsOccupied);
            if (space == null) throw new Exception("Нет свободных парковочных мест");

            var ticket = new ParkingTicket
            {
                VehicleID = vehicle.VehicleID,
                ParkingSpaceID = space.ParkingSpaceID,
                EntryTime = DateTime.Now,
                ExitTime = null,
                Fee = null
            };

            _ctx.ParkingTickets.Add(ticket);
            space.IsOccupied = true;
            _ctx.SaveChanges();
        }

        public void EndParking(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber) || !uniqueNumber.StartsWith("TICK-"))
                throw new Exception("Неверный формат уникального номера (например, TICK-001)");

            int ticketId = GetTicketIdFromUniqueNumber(uniqueNumber);

            var ticket = _ctx.ParkingTickets
                .Include(t => t.ParkingSpace)
                .FirstOrDefault(t => t.TicketID == ticketId);

            if (ticket == null) throw new Exception("Билет не найден");
            if (ticket.ExitTime != null) throw new Exception("Билет уже закрыт");

            var exit = DateTime.Now;
            var hours = Math.Ceiling((exit - ticket.EntryTime).TotalHours);
            var fee = (decimal)hours * 100m;

            ticket.ExitTime = exit;
            ticket.Fee = fee;

            var space = ticket.ParkingSpace!;
            space.IsOccupied = false;

            _ctx.SaveChanges();
        }

        public int GetTicketIdFromUniqueNumber(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber) || !uniqueNumber.StartsWith("TICK-"))
                throw new Exception("Неверный формат уникального номера (например, TICK-001)");
            if (int.TryParse(uniqueNumber.Replace("TICK-", ""), out int ticketId))
                return ticketId;
            throw new Exception("Невозможно преобразовать уникальный номер в ID билета");
        }
    }
}
