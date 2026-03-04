using Microsoft.EntityFrameworkCore;

namespace QuanLyLinhKienMayTinh.Models
{
    public class DataProvider
    {
        private static DataProvider _ins;
        public static DataProvider Ins
        {
            get
            {
                if (_ins == null)
                    _ins = new DataProvider();
                return _ins;
            }
            set
            {
                _ins = value;
            }
        }

        public QL_LinhKien_PC_Context DB { get; set; }

        private DataProvider()
        {
            var options = new DbContextOptionsBuilder<QL_LinhKien_PC_Context>()
                .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=QL_LinhKien_PC;Trusted_Connection=True;")
                .Options;

            DB = new QL_LinhKien_PC_Context(options);
        }
    }
}