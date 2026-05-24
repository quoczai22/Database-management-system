using QuanLyLinhKienMayTinh.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace QuanLyLinhKienMayTinh.Views
{
    /// <summary>
    /// Interaction logic for TaiKhoanView.xaml
    /// </summary>
    public partial class TaiKhoanView : Page
    {
        public TaiKhoanView()
        {
            InitializeComponent();
            this.DataContext = new TaiKhoanViewModel();
        }

        // Thêm hàm này vào để tự cập nhật danh sách
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TaiKhoanViewModel vm)
            {
                vm.LoadNhanVien(); 
            }
        }
    }
}
