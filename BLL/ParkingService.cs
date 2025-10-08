
using System;
using System.Data;
using ParkingSystem.DAL;

namespace ParkingSystem.BLL
{
    public class ParkingService
    {
        private readonly VehicleOwnerDAL ownerDAL = new();
        private readonly VehicleDAL vehicleDAL = new();
        private readonly ParkingDAL parkingDAL = new();

        public void AddOwner(string f, string l, string p, string? e)
        {
            if (string.IsNullOrWhiteSpace(f) || string.IsNullOrWhiteSpace(l))
                throw new Exception("Имя и фамилия обязательны");
            ownerDAL.Add(f, l, p, e);
        }

        public void AddVehicle(string plate, string make, string model, string color, int ownerId)
        {
            if (string.IsNullOrWhiteSpace(plate))
                throw new Exception("Госномер обязателен");
            vehicleDAL.Add(plate, make, model, color, ownerId);
        }

        public DataTable GetAllOwners() => ownerDAL.GetAll();
        public DataTable GetAllVehicles() => vehicleDAL.GetAll();
        public DataTable GetAllSpaces() => parkingDAL.GetSpaces();
        public DataTable GetTickets() => parkingDAL.GetTickets();

        public void StartParking(int vehicleId, string spaceNumber) => parkingDAL.StartParking(vehicleId, spaceNumber);
        public void EndParking(int ticketId) => parkingDAL.EndParking(ticketId);
    }
}

