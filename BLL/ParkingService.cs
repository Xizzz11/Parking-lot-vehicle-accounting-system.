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

        // ======== Добавление и редактирование владельцев / машин / мест ========

        // Email обязателен и уникален, телефон обязателен (формат +7XXXXXXXXXX) и уникален
        public void AddOwner(string first, string last, string phone, string email)
        {
            ValidateNames(first, last);

            phone = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("Email обязателен");
            if (email.Length > 100)
                throw new Exception("Email не должен превышать 100 символов");
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Неверный формат email (например, user@example.com)");

            if (_ctx.VehicleOwners.Any(o => o.PhoneNumber == phone))
                throw new Exception("Пользователь с таким телефоном уже существует");

            if (_ctx.VehicleOwners.Any(o => o.Email != null && o.Email == email))
                throw new Exception("Пользователь с таким email уже существует");

            _ctx.VehicleOwners.Add(new VehicleOwner
            {
                FirstName = first.Trim(),
                LastName = last.Trim(),
                PhoneNumber = phone,
                Email = email.Trim()
            });
            _ctx.SaveChanges();
        }

        public void EditOwner(string fullName, string newPhone, string? newEmail)
        {
            var owner = _ctx.VehicleOwners
                .FirstOrDefault(o => (o.LastName + " " + o.FirstName) == fullName)
                ?? throw new Exception("Владелец не найден");

            if (!string.IsNullOrWhiteSpace(newPhone))
            {
                var phone = NormalizePhone(newPhone);
                if (_ctx.VehicleOwners.Any(o => o.OwnerID != owner.OwnerID && o.PhoneNumber == phone))
                    throw new Exception("Этот телефон уже используется другим владельцем");
                owner.PhoneNumber = phone;
            }

            if (!string.IsNullOrWhiteSpace(newEmail))
            {
                if (newEmail.Length > 100) throw new Exception("Email не должен превышать 100 символов");
                if (!Regex.IsMatch(newEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new Exception("Неверный формат email");
                var mail = newEmail.Trim();
                if (_ctx.VehicleOwners.Any(o => o.OwnerID != owner.OwnerID && o.Email != null && o.Email == mail))
                    throw new Exception("Этот email уже используется другим владельцем");
                owner.Email = mail;
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

        public void AddSpace(string spaceNumber)
        {
            if (string.IsNullOrWhiteSpace(spaceNumber) || spaceNumber.Length > 10)
                throw new Exception("Номер места обязателен и не должен превышать 10 символов");

            if (_ctx.ParkingSpaces.Any(p => p.SpaceNumber == spaceNumber))
                throw new Exception("Место с таким номером уже существует");

            _ctx.ParkingSpaces.Add(new ParkingSpace
            {
                SpaceNumber = spaceNumber.Trim(),
                IsOccupied = false
            });
            _ctx.SaveChanges();
        }

        public void EditSpace(string currentNumber, string newNumber)
        {
            if (string.IsNullOrWhiteSpace(currentNumber))
                throw new Exception("Текущий номер места не указан");
            if (string.IsNullOrWhiteSpace(newNumber) || newNumber.Length > 10)
                throw new Exception("Новый номер обязателен и не должен превышать 10 символов");

            var space = _ctx.ParkingSpaces.FirstOrDefault(p => p.SpaceNumber == currentNumber)
                ?? throw new Exception("Место не найдено");

            if (_ctx.ParkingSpaces.Any(p => p.SpaceNumber == newNumber && p.ParkingSpaceID != space.ParkingSpaceID))
                throw new Exception("Место с таким номером уже существует");

            space.SpaceNumber = newNumber.Trim();
            _ctx.SaveChanges();
        }

        public bool SpaceExists(string spaceNumber)
            => !string.IsNullOrWhiteSpace(spaceNumber) && _ctx.ParkingSpaces.Any(p => p.SpaceNumber == spaceNumber);

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
                ?? throw new Exception("Билет с таким номером не найден");
            if (ticket.ExitTime == null) throw new Exception("Нельзя удалить активный билет");

            _ctx.ParkingTickets.Remove(ticket);
            _ctx.SaveChanges();
        }

        // ======== Просмотр/поиск (строковые таблицы для UI.Table) ========

        public IEnumerable<string[]> GetAllOwners() =>
            _ctx.VehicleOwners
                .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
                .Select(o => new[] { o.LastName + " " + o.FirstName, o.PhoneNumber, o.Email ?? "" })
                .ToList();

        public IEnumerable<string[]> GetAllVehicles() =>
            _ctx.Vehicles.Include(v => v.Owner)
                .OrderBy(v => v.LicensePlate)
                .Select(v => new[]
                {
                    v.LicensePlate,
                    v.Make,
                    v.Model,
                    v.Color,
                    v.Owner == null ? "" : (v.Owner.LastName + " " + v.Owner.FirstName)
                })
                .ToList();

        public IEnumerable<string[]> GetAllSpaces() =>
            _ctx.ParkingSpaces
                .OrderBy(p => p.SpaceNumber)
                .Select(p => new[] { p.SpaceNumber, p.IsOccupied ? "занято" : "свободно" })
                .ToList();

        public IEnumerable<string[]> GetTickets() =>
            _ctx.ParkingTickets.Include(t => t.Vehicle).Include(t => t.ParkingSpace)
                .OrderByDescending(t => t.EntryTime)
                .Select(t => new[]
                {
                    "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    t.Vehicle != null ? t.Vehicle.LicensePlate : "",
                    t.ParkingSpace != null ? t.ParkingSpace.SpaceNumber : "",
                    t.EntryTime.ToString("yyyy-MM-dd HH:mm"),
                    t.ExitTime.HasValue ? t.ExitTime.Value.ToString("yyyy-MM-dd HH:mm") : "",
                    t.Fee.HasValue ? t.Fee.Value.ToString("0.00") : ""
                })
                .ToList();

        public IEnumerable<string[]> GetParkingSpaceByLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate) || licensePlate.Length < 6 || licensePlate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            return _ctx.ParkingTickets
                .Include(t => t.ParkingSpace)
                .Include(t => t.Vehicle)
                .Where(t => t.Vehicle != null && t.Vehicle.LicensePlate == licensePlate && t.ExitTime == null)
                .Select(t => new[]
                {
                    t.ParkingSpace != null ? t.ParkingSpace.SpaceNumber : "",
                    t.ParkingSpace != null && t.ParkingSpace.IsOccupied ? "занято" : "свободно",
                    "TICK-" + t.TicketID.ToString().PadLeft(3, '0'),
                    t.EntryTime.ToString("yyyy-MM-dd HH:mm"),
                    t.ExitTime.HasValue ? t.ExitTime.Value.ToString("yyyy-MM-dd HH:mm") : ""
                })
                .ToList();
        }

        public IEnumerable<string[]> FindOwnerByPhone(string inputPhone)
        {
            var phone = NormalizePhone(inputPhone);

            return _ctx.VehicleOwners
                .Where(o => o.PhoneNumber == phone)
                .Select(o => new[] { o.LastName + " " + o.FirstName, o.PhoneNumber, o.Email ?? "" })
                .ToList();
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

            if (ticket.ParkingSpace != null)
                ticket.ParkingSpace.IsOccupied = false;

            _ctx.SaveChanges();
        }

        // ======== Доп. проверки и смена госномера ========

        public bool OwnerExistsByFullName(string ownerFullName)
        {
            if (string.IsNullOrWhiteSpace(ownerFullName)) return false;
            return _ctx.VehicleOwners.Any(o => (o.LastName + " " + o.FirstName) == ownerFullName);
        }

        public string? GetOwnerNameByLicensePlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return null;
            var v = _ctx.Vehicles.Include(v => v.Owner).FirstOrDefault(v => v.LicensePlate == plate);
            return v == null || v.Owner == null ? null : (v.Owner.LastName + " " + v.Owner.FirstName);
        }

        public bool PlateExists(string plate) =>
            !string.IsNullOrWhiteSpace(plate) && _ctx.Vehicles.Any(v => v.LicensePlate == plate);

        public void EditVehiclePlate(string currentPlate, string newPlate)
        {
            if (string.IsNullOrWhiteSpace(currentPlate)) throw new Exception("Текущий номер не указан");
            if (string.IsNullOrWhiteSpace(newPlate)) throw new Exception("Новый номер не указан");
            if (newPlate.Length < 6 || newPlate.Length > 10) throw new Exception("Госномер должен содержать от 6 до 10 символов");

            var vehicle = _ctx.Vehicles.Include(v => v.Owner)
                .FirstOrDefault(v => v.LicensePlate == currentPlate)
                ?? throw new Exception("Машина с указанным текущим номером не найдена");

            if (string.Equals(currentPlate, newPlate, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Этот номер уже принадлежит вам");

            var conflict = _ctx.Vehicles.Include(v => v.Owner).FirstOrDefault(v => v.LicensePlate == newPlate);

            if (conflict != null)
            {
                if (conflict.VehicleID == vehicle.VehicleID || conflict.OwnerID == vehicle.OwnerID)
                    throw new Exception("Этот номер уже принадлежит вам");
                else
                    throw new Exception("Этот номер уже принадлежит другому человеку");
            }

            vehicle.LicensePlate = newPlate.Trim();
            _ctx.SaveChanges();
        }

        // ======== Вспомогательные ========

        private static void ValidateNames(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new Exception("Имя и фамилия обязательны");

            bool ok(string s)
            {
                s = s.Trim();
                if (s.Length < 2 || s.Length > 50) return false;
                foreach (var ch in s)
                {
                    if (ch == '-' || ch == ' ') continue;
                    bool isRu = (ch >= 'А' && ch <= 'Я') || (ch >= 'а' && ch <= 'я') || ch == 'Ё' || ch == 'ё';
                    bool isEn = (ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z');
                    if (!(isRu || isEn)) return false;
                }
                return true;
            }

            if (!ok(first)) throw new Exception("Имя: только буквы (рус/англ), длина 2–50.");
            if (!ok(last)) throw new Exception("Фамилия: только буквы (рус/англ), длина 2–50.");
        }

        private static string NormalizePhone(string phone)
        {
            phone = (phone ?? "").Trim();
            // убрать все, кроме цифр и возможного ведущего + 
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // поддержка '8XXXXXXXXXX' -> +7XXXXXXXXXX
            if (digits.Length == 11 && digits.StartsWith("8"))
                digits = "7" + digits.Substring(1);

            if (digits.Length == 10) // без кода страны
                digits = "7" + digits;

            if (digits.Length != 11 || !digits.StartsWith("7"))
                throw new Exception("Телефон должен быть российским формата +7XXXXXXXXXX");

            return "+7" + digits.Substring(1);
        }

        private static int GetTicketIdFromUniqueNumber(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber))
                throw new Exception("Номер билета не указан");

            if (int.TryParse(uniqueNumber, out int plainId))
                return plainId;

            if (uniqueNumber.StartsWith("TICK-", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(uniqueNumber.Substring(5), out int idFromPrefix))
                    return idFromPrefix;
            }

            throw new Exception("Неверный формат номера билета (введите, например, 1 или TICK-001)");
        }
    }
}
