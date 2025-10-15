using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ParkingSystem.Data;
using ParkingSystem.Models;

namespace ParkingSystem.BLL
{
    public class ParkingService
    {
        private readonly ParkingContext _ctx;

        public ParkingService(ParkingContext ctx) => _ctx = ctx;

        // ======== Добавление и редактирование владельцев / машин ========

        public void AddOwner(string first, string last, string phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new Exception("Имя и фамилия обязательны");
            if (first.Any(char.IsDigit) || last.Any(char.IsDigit))
                throw new Exception("Имя и фамилия не должны содержать цифры");

            // длины под типовую схему БД
            if (first.Length > 50) throw new Exception("Имя не должно превышать 50 символов");
            if (last.Length > 50) throw new Exception("Фамилия не должна превышать 50 символов");
            if (!string.IsNullOrEmpty(email) && email.Length > 100)
                throw new Exception("Email не должен превышать 100 символов");

            // Нормализация телефона: всегда +7XXXXXXXXXX
            phone = NormalizePhone(phone);

            if (!string.IsNullOrWhiteSpace(email) &&
                !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Неверный формат email (например, user@example.com)");

            _ctx.VehicleOwners.Add(new VehicleOwner
            {
                FirstName = first.Trim(),
                LastName = last.Trim(),
                PhoneNumber = phone,
                Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim()
            });
            _ctx.SaveChanges();
        }

        public void EditOwner(string fullName, string newPhone, string? newEmail)
        {
            var owner = _ctx.VehicleOwners
                .FirstOrDefault(o => (o.LastName + " " + o.FirstName) == fullName)
                ?? throw new Exception("Владелец не найден");

            if (!string.IsNullOrWhiteSpace(newPhone))
                owner.PhoneNumber = NormalizePhone(newPhone);

            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                if (newEmail.Length > 100) throw new Exception("Email не должен превышать 100 символов");
                if (!Regex.IsMatch(newEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new Exception("Неверный формат email");
                owner.Email = newEmail.Trim();
            }

            _ctx.SaveChanges();
        }

        public void AddVehicle(string plate, string make, string model, string color, string ownerFullName)
        {
            if (string.IsNullOrWhiteSpace(plate) || plate.Length < 6 || plate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");
            if (string.IsNullOrWhiteSpace(make)) throw new Exception("Марка автомобиля обязательна");
            if (string.IsNullOrWhiteSpace(model)) throw new Exception("Модель автомобиля обязательна");
            if (string.IsNullOrWhiteSpace(color)) throw new Exception("Цвет автомобиля обязателен");

            var owner = _ctx.VehicleOwners
                .FirstOrDefault(o => (o.LastName + " " + o.FirstName) == ownerFullName)
                ?? throw new Exception("Владелец с таким ФИО не найден");

            if (_ctx.Vehicles.Any(v => v.LicensePlate == plate))
                throw new Exception("Машина с таким госномером уже существует");

            _ctx.Vehicles.Add(new Vehicle
            {
                LicensePlate = plate.Trim(),
                Make = make.Trim(),
                Model = model.Trim(),
                Color = color.Trim(),
                OwnerID = owner.OwnerID
            });
            _ctx.SaveChanges();
        }

        public void EditVehicle(string plate, string? make, string? model, string? color)
        {
            var vehicle = _ctx.Vehicles.FirstOrDefault(v => v.LicensePlate == plate)
                ?? throw new Exception("Машина не найдена");

            if (!string.IsNullOrWhiteSpace(make)) vehicle.Make = make.Trim();
            if (!string.IsNullOrWhiteSpace(model)) vehicle.Model = model.Trim();
            if (!string.IsNullOrWhiteSpace(color)) vehicle.Color = color.Trim();

            _ctx.SaveChanges();
        }

        // ======== Удаление ========

        public void RemoveOwner(string ownerFullName)
        {
            var owner = _ctx.VehicleOwners
                .Include(o => o.Vehicles)
                .FirstOrDefault(o => (o.LastName + " " + o.FirstName) == ownerFullName)
                ?? throw new Exception("Владелец с таким ФИО не найден");

            if (owner.Vehicles.Any())
                throw new Exception("Нельзя удалить владельца, у которого есть машины");

            _ctx.VehicleOwners.Remove(owner);
            _ctx.SaveChanges();
        }

        public void RemoveVehicle(string plate)
        {
            var vehicle = _ctx.Vehicles.FirstOrDefault(v => v.LicensePlate == plate)
                ?? throw new Exception("Машина с таким госномером не найдена");

            var hasActiveTickets = _ctx.ParkingTickets.Any(t => t.VehicleID == vehicle.VehicleID && t.ExitTime == null);
            if (hasActiveTickets) throw new Exception("Нельзя удалить машину с активным билетом");

            _ctx.Vehicles.Remove(vehicle);
            _ctx.SaveChanges();
        }

        public void RemoveSpace(string spaceNumber)
        {
            if (string.IsNullOrWhiteSpace(spaceNumber) || spaceNumber.Length > 10)
                throw new Exception("Номер места обязателен и не должен превышать 10 символов");

            var ps = _ctx.ParkingSpaces.FirstOrDefault(p => p.SpaceNumber == spaceNumber)
                ?? throw new Exception("Место не найдено");
            if (ps.IsOccupied) throw new Exception("Нельзя удалить занятое место");

            _ctx.ParkingSpaces.Remove(ps);
            _ctx.SaveChanges();
        }

        public void RemoveTicket(string uniqueNumber)
        {
            int ticketId = GetTicketIdFromUniqueNumber(uniqueNumber);

            var ticket = _ctx.ParkingTickets.FirstOrDefault(t => t.TicketID == ticketId)
                ?? throw new Exception("Билет с таким уникальным номером не найден");
            if (ticket.ExitTime == null) throw new Exception("Нельзя удалить активный билет");

            _ctx.ParkingTickets.Remove(ticket);
            _ctx.SaveChanges();
        }

        // ======== Просмотр/поиск ========

        public IEnumerable<object> GetAllOwners() =>
            _ctx.VehicleOwners
                .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
                .Select(o => new { FullName = o.LastName + " " + o.FirstName, o.PhoneNumber, o.Email })
                .ToList<object>();

        public IEnumerable<object> GetAllVehicles() =>
            _ctx.Vehicles.Include(v => v.Owner)
                .OrderBy(v => v.LicensePlate)
                .Select(v => new { v.LicensePlate, v.Make, v.Model, v.Color, Owner = v.Owner!.LastName + " " + v.Owner!.FirstName })
                .ToList<object>();

        public IEnumerable<object> GetAllSpaces() =>
            _ctx.ParkingSpaces
                .OrderBy(p => p.SpaceNumber)
                .Select(p => new { p.SpaceNumber, p.IsOccupied })
                .ToList<object>();

        public IEnumerable<object> GetFreeSpaces() =>
            _ctx.ParkingSpaces
                .Where(p => !p.IsOccupied)
                .OrderBy(p => p.SpaceNumber)
                .Select(p => new { p.SpaceNumber, p.IsOccupied })
                .ToList<object>();

        public IEnumerable<object> GetTickets() =>
            _ctx.ParkingTickets.Include(t => t.Vehicle).Include(t => t.ParkingSpace)
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new {
                    UniqueNumber = "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    LicensePlate = t.Vehicle!.LicensePlate,
                    SpaceNumber = t.ParkingSpace!.SpaceNumber,
                    t.EntryTime,
                    t.ExitTime,
                    t.Fee
                })
                .ToList<object>();

        public IEnumerable<object> GetParkingSpaceByLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate) || licensePlate.Length < 6 || licensePlate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            return _ctx.ParkingTickets.Include(t => t.ParkingSpace).Include(t => t.Vehicle)
                .Where(t => t.Vehicle!.LicensePlate == licensePlate && t.ExitTime == null)
                .Select(t => new {
                    t.ParkingSpace!.SpaceNumber,
                    t.ParkingSpace!.IsOccupied,
                    TicketNumber = "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    t.EntryTime,
                    t.ExitTime
                })
                .ToList<object>();
        }

        public IEnumerable<object> FindOwnerByPhone(string inputPhone)
        {
            var phone = NormalizePhone(inputPhone);

            return _ctx.VehicleOwners
                .Where(o => o.PhoneNumber == phone)
                .Select(o => new { FullName = o.LastName + " " + o.FirstName, o.PhoneNumber, o.Email })
                .ToList<object>();
        }

        // ======== Парковка ========

        public void StartParking(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate) || plate.Length < 6 || plate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            var vehicle = _ctx.Vehicles.FirstOrDefault(v => v.LicensePlate == plate)
                ?? throw new Exception("Машина с таким госномером не найдена");

            var space = _ctx.ParkingSpaces.FirstOrDefault(s => !s.IsOccupied)
                ?? throw new Exception("Нет свободных парковочных мест");

            _ctx.ParkingTickets.Add(new ParkingTicket
            {
                VehicleID = vehicle.VehicleID,
                ParkingSpaceID = space.ParkingSpaceID,
                EntryTime = DateTime.Now
            });
            space.IsOccupied = true;
            _ctx.SaveChanges();
        }

        public void EndParking(string uniqueNumber)
        {
            int ticketId = GetTicketIdFromUniqueNumber(uniqueNumber);

            var ticket = _ctx.ParkingTickets.Include(t => t.ParkingSpace)
                .FirstOrDefault(t => t.TicketID == ticketId)
                ?? throw new Exception("Билет не найден");

            if (ticket.ExitTime != null) throw new Exception("Билет уже закрыт");

            var exit = DateTime.Now;
            var hours = Math.Ceiling((exit - ticket.EntryTime).TotalHours);
            ticket.ExitTime = exit;
            ticket.Fee = (decimal)hours * 100m;

            ticket.ParkingSpace!.IsOccupied = false;
            _ctx.SaveChanges();
        }

        // ======== Вспомогательные ========

        private static string NormalizePhone(string phone)
        {
            phone = (phone ?? "").Trim();
            if (phone.StartsWith("8")) phone = "+7" + phone[1..];
            if (Regex.IsMatch(phone, @"^\d{10}$")) phone = "+7" + phone;
            if (!Regex.IsMatch(phone, @"^\+7\d{10}$"))
                throw new Exception("Телефон должен быть в формате +7XXXXXXXXXX (например, +79991234567)");
            return phone;
        }

        private static int GetTicketIdFromUniqueNumber(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber) || !uniqueNumber.StartsWith("TICK-"))
                throw new Exception("Неверный формат уникального номера (например, TICK-001)");
            if (int.TryParse(uniqueNumber[5..], out int id)) return id;
            throw new Exception("Невозможно преобразовать уникальный номер в ID билета");
        }
    }
}
