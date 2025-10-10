using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using ParkingSystem.DAL;

namespace ParkingSystem.BLL
{
    public class ParkingService
    {
        private readonly VehicleOwnerDAL ownerDAL = new();
        private readonly VehicleDAL vehicleDAL = new();
        private readonly ParkingDAL parkingDAL = new();

        public void AddOwner(string first, string last, string phone, string? email)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
                throw new Exception("Имя и фамилия обязательны");
            if (first.Any(char.IsDigit) || last.Any(char.IsDigit))
                throw new Exception("Имя и фамилия не должны содержать цифры");
            if (string.IsNullOrWhiteSpace(phone) || !Regex.IsMatch(phone, @"^(\+7|8)\d{10}$"))
                throw new Exception("Телефон должен начинаться с +7 или 8 и содержать 11 цифр (например, +79991234567 или 89991234567)");
            if (email != null && !string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Неверный формат email (например, user@example.com)");

            ownerDAL.Add(first, last, phone, email);
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

            var parts = ownerFullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new Exception("Введите ФИО в формате 'Фамилия Имя'");
            var lastName = parts[0];
            var firstName = parts[1];

            var ownerId = ownerDAL.GetAllWithId().AsEnumerable()
                .Where(r => r["FullName"].ToString() == ownerFullName)
                .Select(r => (int)r["OwnerID"])
                .FirstOrDefault();
            if (ownerId == 0)
                throw new Exception("Владелец с таким ФИО не найден");

            vehicleDAL.Add(plate, make, model, color, ownerId);
        }

        public void RemoveOwner(string ownerFullName)
        {
            var parts = ownerFullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2) throw new Exception("Введите ФИО в формате 'Фамилия Имя'");
            var lastName = parts[0];
            var firstName = parts[1];

            var ownerId = ownerDAL.GetAllWithId().AsEnumerable()
                .Where(r => r["FullName"].ToString() == ownerFullName)
                .Select(r => (int)r["OwnerID"])
                .FirstOrDefault();
            if (ownerId == 0)
                throw new Exception("Владелец с таким ФИО не найден");
            var vehicles = vehicleDAL.GetAll().AsEnumerable().Where(r => (int)r["OwnerID"] == ownerId).Any();
            if (vehicles)
                throw new Exception("Нельзя удалить владельца, у которого есть машины");
            ownerDAL.Remove(ownerId);
        }

        public void RemoveVehicle(string plate)
        {
            var vehicleId = vehicleDAL.GetAll().AsEnumerable()
                .Where(r => r["LicensePlate"] != DBNull.Value && r["LicensePlate"].ToString() == plate)
                .Select(r => (int?)r["VehicleID"])
                .FirstOrDefault();
            if (!vehicleId.HasValue)
                throw new Exception("Машина с таким госномером не найдена");
            var activeTickets = parkingDAL.GetTickets().AsEnumerable()
                .Where(r => r["VehicleID"] != DBNull.Value && (int)r["VehicleID"] == vehicleId.Value && r["ExitTime"] == DBNull.Value)
                .Any();
            if (activeTickets)
                throw new Exception("Нельзя удалить машину с активным билетом");
            vehicleDAL.Remove(vehicleId.Value);
        }

        public void RemoveSpace(string spaceNumber)
        {
            if (string.IsNullOrWhiteSpace(spaceNumber) || spaceNumber.Length > 10)
                throw new Exception("Номер места обязателен и не должен превышать 10 символов");
            var occupied = parkingDAL.GetSpaces().AsEnumerable()
                .Where(r => r["SpaceNumber"] != DBNull.Value && (string)r["SpaceNumber"] == spaceNumber && Convert.ToBoolean(r["IsOccupied"]))
                .Any();
            if (occupied)
                throw new Exception("Нельзя удалить занятое место");
            parkingDAL.RemoveSpace(spaceNumber);
        }

        public void RemoveTicket(int ticketId)
        {
            if (ticketId <= 0)
                throw new Exception("ID билета должен быть положительным числом");
            var activeTicket = parkingDAL.GetTickets().AsEnumerable()
                .Where(r => r["TicketID"] != DBNull.Value && (int)r["TicketID"] == ticketId && r["ExitTime"] == DBNull.Value)
                .Any();
            if (activeTicket)
                throw new Exception("Нельзя удалить активный билет");
            parkingDAL.RemoveTicket(ticketId);
        }

        public DataTable GetAllOwners() => ownerDAL.GetAll();

        public DataTable GetAllVehicles() => vehicleDAL.GetAll();

        public DataTable GetAllSpaces() => parkingDAL.GetSpaces();

        public DataTable GetTickets() => parkingDAL.GetTickets();

        public void StartParking(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate) || plate.Length < 6 || plate.Length > 10)
                throw new Exception("Госномер обязателен и должен содержать от 6 до 10 символов");

            var vehicleId = vehicleDAL.GetAll().AsEnumerable()
                .Where(r => r["LicensePlate"] != DBNull.Value && r["LicensePlate"].ToString() == plate)
                .Select(r => (int?)r["VehicleID"])
                .FirstOrDefault();
            if (!vehicleId.HasValue)
                throw new Exception("Машина с таким госномером не найдена");

            var availableSpace = parkingDAL.GetSpaces().AsEnumerable()
                .Where(r => r["IsOccupied"] != DBNull.Value && !Convert.ToBoolean(r["IsOccupied"]))
                .Select(r => new { SpaceId = (int)r["ParkingSpaceID"], SpaceNumber = (string)r["SpaceNumber"] })
                .FirstOrDefault();

            if (availableSpace == null)
                throw new Exception("Нет свободных парковочных мест");

            parkingDAL.StartParking(vehicleId.Value, availableSpace.SpaceNumber);
        }

        public void EndParking(int ticketId)
        {
            if (ticketId <= 0)
                throw new Exception("ID билета должен быть положительным числом");

            parkingDAL.EndParking(ticketId);
        }

        public int GetTicketIdFromUniqueNumber(string uniqueNumber)
        {
            if (string.IsNullOrWhiteSpace(uniqueNumber) || !uniqueNumber.StartsWith("TICK-"))
                throw new Exception("Неверный формат уникального номера (например, TICK-001)");
            int ticketId;
            if (int.TryParse(uniqueNumber.Replace("TICK-", ""), out ticketId))
                return ticketId;
            throw new Exception("Невозможно преобразовать уникальный номер в ID билета");
        }
    }
}