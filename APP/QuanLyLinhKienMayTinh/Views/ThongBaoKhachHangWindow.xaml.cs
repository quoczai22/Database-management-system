using QuanLyLinhKienMayTinh.Models;
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
    /// Interaction logic for ThongBaoKhachHangWindow.xaml
    /// </summary>
    public partial class ThongBaoKhachHangWindow : Window
    {
        public ThongBaoKhachHangWindow(List<sp_DanhSacKhachHangChuaTTResult> khachHangChuaTT)
        {
            InitializeComponent();
            dgKhachNo.ItemsSource = khachHangChuaTT;
        }
    }
}
