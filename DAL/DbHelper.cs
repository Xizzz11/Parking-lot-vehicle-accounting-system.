using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ParkingSystem.DAL
{
    public static class DbHelper
    {
        private static readonly string _connection =
            "Server=192.168.9.203\\SQLEXPRESS;Database=ParkingLotDB;User Id=student1;Password=123456;TrustServerCertificate=True;";

        public static SqlConnection GetConnection() => new SqlConnection(_connection);

        public static bool TestConnection()
        {
            try
            {
                using var conn = new SqlConnection(_connection);
                conn.Open();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" Подключение к базе данных успешно!");
                Console.ResetColor();
                conn.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" Ошибка подключения к базе данных:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return false;
            }
        }

        public static DataTable GetDataTable(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var da = new SqlDataAdapter(sql, conn);
            if (parameters.Length > 0)
                da.SelectCommand.Parameters.AddRange(parameters);

            var dt = new DataTable();
            da.Fill(dt);
            return dt;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public static object? ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using var conn = GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteScalar();
        }
    }
}