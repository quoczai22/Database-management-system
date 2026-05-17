using Microsoft.EntityFrameworkCore;
namespace QuanLyLinhKienMayTinh.Models

{

    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins => _ins ??= new DataProvider();

        private string _currentConnStr;

        private DataProvider()
        {
            _currentConnStr = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=QL_LinhKien_PC;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
        }

        public QL_LinhKien_PC_Context GetContext()
        {
            var options = new DbContextOptionsBuilder<QL_LinhKien_PC_Context>()
                .UseSqlServer(_currentConnStr)
                .Options;

            return new QL_LinhKien_PC_Context(options);
        }

        public void ChangeToQuanLyConnection()
        {
            _currentConnStr = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=QL_LinhKien_PC;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
        }

        public void ChangeToNhanVienConnection()
        {
            _currentConnStr = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=QL_LinhKien_PC;Integrated Security=True;TrustServerCertificate=True;Encrypt=False";
        }
    }
}