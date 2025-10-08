
using System.Data;
using Microsoft.Data.SqlClient;

namespace ParkingSystem.DAL
{
    public class VehicleOwnerDAL
    {
        public void Add(string first, string last, string phone, string? email)
        {
            var sql = "INSERT INTO VehicleOwner (FirstName, LastName, PhoneNumber, Email) VALUES (@fn,@ln,@ph,@em)";
            DbHelper.ExecuteNonQuery(sql,
                new SqlParameter("@fn", first),
                new SqlParameter("@ln", last),
                new SqlParameter("@ph", phone),
                new SqlParameter("@em", (object?)email ?? DBNull.Value));
        }

        public DataTable GetAll()
        {
            return DbHelper.GetDataTable("SELECT * FROM VehicleOwner ORDER BY LastName, FirstName");
        }
    }
}

