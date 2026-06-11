using Microsoft.EntityFrameworkCore;
namespace QuanLyLinhKienMayTinh.Models

{

    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins => _ins ??= new DataProvider();

        private const string ServerName = @"(localdb)\MSSQLLocalDB";
        private const string DatabaseName = "QL_LinhKien_PC";
        private string _currentConnStr;
        //khi chạy phải đổi data source thành tên server của máy
        private DataProvider()
        {
            _currentConnStr = $"Data Source={ServerName};Initial Catalog={DatabaseName};Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
        }

        public QL_LinhKien_PC_Context GetContext()
        {
            var options = new DbContextOptionsBuilder<QL_LinhKien_PC_Context>()
                .UseSqlServer(_currentConnStr)
                .Options;

            return new QL_LinhKien_PC_Context(options);
        }

        public void ChangeConnectionByRole(string quyen)
        {
            string dbUser = "";
            string dbPass = "123"; 

            switch (quyen)
            {
                case "Quản lý toàn bộ": dbUser = "quanLyLogin"; break;
                case "Thu ngân": dbUser = "nhanVienThuNganLogin"; break;
                case "Chăm sóc khách hàng": dbUser = "nhanVienCskhLogin"; break;
                case "Kho": dbUser = "nhanVienKhoLogin"; break;
                case "Bảo mật": dbUser = "quanLyLogin"; break; 
                default: dbUser = "nhanVienCskhLogin"; break;
            }

            _currentConnStr = $"Data Source={ServerName};Initial Catalog={DatabaseName};User Id={dbUser};Password={dbPass};TrustServerCertificate=True;Encrypt=False";
        }
    }
}
