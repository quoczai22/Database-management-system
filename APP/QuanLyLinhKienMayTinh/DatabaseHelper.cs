using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Data;

namespace QuanLyLinhKienMayTinh
{
    public class DatabaseHelper
    {
        private string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=QL_LinhKien_PC;Trusted_Connection=True;";

        public bool UserExists(string username)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "SELECT COUNT(*) FROM TaiKhoan WHERE tendangnhap=@username";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        public bool Register(string username, string password)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "INSERT INTO TaiKhoan(tendangnhap,matkhau) VALUES(@username,@password)";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Login(string username, string password)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "SELECT COUNT(*) FROM TaiKhoan WHERE tendangnhap=@username AND matkhau=@password";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }

        public int ExecuteScalarInt(string query)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            using SqlCommand cmd = new SqlCommand(query, conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            using SqlCommand cmd = new SqlCommand(query, conn);
            using SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
    }
}