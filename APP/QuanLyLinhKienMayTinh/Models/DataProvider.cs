using Microsoft.EntityFrameworkCore;
namespace QuanLyLinhKienMayTinh.Models

{

    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins => _ins ??= new DataProvider();

        private string _currentConnStr;
        //khi chạy phải đổi data source thành tên server của máy
        private DataProvider()
        {
            _currentConnStr = "Data Source=localhost;Initial Catalog=QL_LinhKien_PC;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
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
                case "Quản lý toàn bộ": dbUser = "quanlylogin"; break;
                case "Thu ngân": dbUser = "nhanvienthunganlogin"; break;
                case "Chăm sóc khách hàng": dbUser = "nhanviencskhlogin"; break;
                case "Kho": dbUser = "nhanvienkhologin"; break;
                case "Bảo mật": dbUser = "quanlylogin"; break; 
                default: dbUser = "nhanviencskhlogin"; break;
            }

            _currentConnStr = $"Data Source=localhost;Initial Catalog=QL_LinhKien_PC;User Id={dbUser};Password={dbPass};TrustServerCertificate=True;Encrypt=False";
        }
    }
}