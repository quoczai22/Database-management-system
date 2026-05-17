using QuanLyLinhKienMayTinh.ViewModels;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class ThemSuaKhachHangDialog : Window
    {
        public KhachHangDisplay KetQua { get; private set; }
        private readonly bool _laSua;

        /// <summary>
        /// Mở dialog ở chế độ THÊM
        /// </summary>
        public ThemSuaKhachHangDialog()
        {
            InitializeComponent();
            _laSua = false;
            TitleText.Text = "Thêm Khách Hàng";
        }

        /// <summary>
        /// Mở dialog ở chế độ SỬA
        /// </summary>
        public ThemSuaKhachHangDialog(KhachHangDisplay kh)
        {
            InitializeComponent();
            _laSua = true;
            TitleText.Text = "Sửa Khách Hàng";
            BtnLuu.Content = "Cập nhật";
            TxtHoTen.Text = kh.HoTen;
            TxtSdt.Text = kh.Sdt;
            TxtEmail.Text = kh.Email;
            TxtDiaChi.Text = kh.DiaChi;
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên khách hàng!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHoTen.Focus();
                return;
            }

            KetQua = new KhachHangDisplay
            {
                MaKh = null,
                HoTen = TxtHoTen.Text.Trim(),
                Sdt = TxtSdt.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                DiaChi = TxtDiaChi.Text.Trim()
            };

            DialogResult = true;
            Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
