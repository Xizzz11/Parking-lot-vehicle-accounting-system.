using System.Data;
using Microsoft.Data.SqlClient;

namespace ParkingSystem.DAL
{
    public class VehicleDAL
    {
        public void Add(string plate, string make, string model, string color, int ownerId)
        {
            var sql = @"INSERT INTO Vehicle (LicensePlate, Make, Model, Color, OwnerID)
                        VALUES (@lp, @mk, @md, @cl, @oid)";
            DbHelper.ExecuteNonQuery(sql,
                new SqlParameter("@lp", plate),
                new SqlParameter("@mk", make),
                new SqlParameter("@md", model),
                new SqlParameter("@cl", color),
                new SqlParameter("@oid", ownerId));
        }

        public void Remove(int vehicleId)
        {
            var sql = "DELETE FROM Vehicle WHERE VehicleID = @id";
            DbHelper.ExecuteNonQuery(sql, new SqlParameter("@id", vehicleId));
        }

        public DataTable GetAll()
        {
            return DbHelper.GetDataTable(@"
                SELECT v.LicensePlate, v.Make, v.Model, v.Color, o.LastName + ' ' + o.FirstName AS Owner
                FROM Vehicle v 
                JOIN VehicleOwner o ON v.OwnerID = o.OwnerID
                ORDER BY v.LicensePlate");
        }
    }
}