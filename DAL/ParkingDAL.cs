
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ParkingSystem.DAL
{
    public class ParkingDAL
    {
        public DataTable GetSpaces() => DbHelper.GetDataTable("SELECT * FROM ParkingSpace ORDER BY SpaceNumber");

        public void StartParking(int vehicleId, string spaceNumber)
        {
            var dt = DbHelper.GetDataTable("SELECT ParkingSpaceID, IsOccupied FROM ParkingSpace WHERE SpaceNumber=@num",
                new SqlParameter("@num", spaceNumber));

            if (dt.Rows.Count == 0) throw new Exception("Место не найдено");

            var spaceId = (int)dt.Rows[0]["ParkingSpaceID"];
            var occupied = (bool)dt.Rows[0]["IsOccupied"];
            if (occupied) throw new Exception("Место занято");

            DbHelper.ExecuteNonQuery("INSERT INTO ParkingTicket (VehicleID, ParkingSpaceID, EntryTime) VALUES (@v,@s,GETDATE())",
                new SqlParameter("@v", vehicleId),
                new SqlParameter("@s", spaceId));

            DbHelper.ExecuteNonQuery("UPDATE ParkingSpace SET IsOccupied=1 WHERE ParkingSpaceID=@s",
                new SqlParameter("@s", spaceId));
        }

        public void EndParking(int ticketId)
        {
            var dt = DbHelper.GetDataTable("SELECT TicketID, EntryTime, ParkingSpaceID FROM ParkingTicket WHERE TicketID=@id",
                new SqlParameter("@id", ticketId));
            if (dt.Rows.Count == 0) throw new Exception("Билет не найден");

            var entry = (DateTime)dt.Rows[0]["EntryTime"];
            var psid = (int)dt.Rows[0]["ParkingSpaceID"];
            var exit = DateTime.Now;
            var hours = Math.Ceiling((exit - entry).TotalHours);
            var fee = hours * 100;

            DbHelper.ExecuteNonQuery("UPDATE ParkingTicket SET ExitTime=@e, Fee=@f WHERE TicketID=@id",
                new SqlParameter("@e", exit),
                new SqlParameter("@f", fee),
                new SqlParameter("@id", ticketId));

            DbHelper.ExecuteNonQuery("UPDATE ParkingSpace SET IsOccupied=0 WHERE ParkingSpaceID=@p",
                new SqlParameter("@p", psid));
        }

        public DataTable GetTickets()
        {
            return DbHelper.GetDataTable(@"
                SELECT t.TicketID, v.LicensePlate, p.SpaceNumber, t.EntryTime, t.ExitTime, t.Fee
                FROM ParkingTicket t
                JOIN Vehicle v ON v.VehicleID=t.VehicleID
                JOIN ParkingSpace p ON p.ParkingSpaceID=t.ParkingSpaceID
                ORDER BY t.EntryTime DESC");
        }
    }
}

