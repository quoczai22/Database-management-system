using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyLinhKienMayTinh
{
    public partial class MainWindow : Window
    {
        string currentUsername;
        DatabaseHelper db = new DatabaseHelper();

        int soNhanVienGoc = 0;
        int soLinhKienGoc = 0;
        int soHoaDonGoc = 0;

        public MainWindow(string username)
        {
            InitializeComponent();
            currentUsername = username;
            txtUsername.Text = username;

            LaySoLuongGoc();

            MainFrame.Navigate(new TrangChu());
        }

        private void LaySoLuongGoc()
        {
            try
            {
                soNhanVienGoc = db.ExecuteScalarInt("SELECT COUNT(*) FROM NhanVien");
                soLinhKienGoc = db.ExecuteScalarInt("SELECT COUNT(*) FROM LinhKien");
                soHoaDonGoc = db.ExecuteScalarInt("SELECT COUNT(*) FROM HoaDon");
            }
            catch
            {
            }
        }

        private void btnThongBao_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int soNhanVienMoi = db.ExecuteScalarInt("SELECT COUNT(*) FROM NhanVien");
                int soLinhKienMoi = db.ExecuteScalarInt("SELECT COUNT(*) FROM LinhKien");
                int soHoaDonMoi = db.ExecuteScalarInt("SELECT COUNT(*) FROM HoaDon");

                List<string> danhSachThongBao = new List<string>();

                if (soNhanVienMoi > soNhanVienGoc)
                    danhSachThongBao.Add($"- Có {soNhanVienMoi - soNhanVienGoc} nhân viên MỚI gia nhập.");
                else if (soNhanVienMoi < soNhanVienGoc)
                    danhSachThongBao.Add($"- Có {soNhanVienGoc - soNhanVienMoi} nhân viên ĐÃ NGHỈ VIỆC (bị xóa).");

                if (soLinhKienMoi > soLinhKienGoc)
                    danhSachThongBao.Add($"- Có {soLinhKienMoi - soLinhKienGoc} mã linh kiện MỚI được thêm vào kho.");
                else if (soLinhKienMoi < soLinhKienGoc)
                    danhSachThongBao.Add($"- Có {soLinhKienGoc - soLinhKienMoi} mã linh kiện đã bị xóa khỏi kho.");

                if (soHoaDonMoi > soHoaDonGoc)
                    danhSachThongBao.Add($"- Có {soHoaDonMoi - soHoaDonGoc} hóa đơn MỚI được xuất.");

                if (danhSachThongBao.Count > 0)
                {
                    MessageBox.Show("Hệ thống có các thay đổi mới sau kể từ lúc bạn đăng nhập:\n\n" +
                                    string.Join("\n", danhSachThongBao),
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    soNhanVienGoc = soNhanVienMoi;
                    soLinhKienGoc = soLinhKienMoi;
                    soHoaDonGoc = soHoaDonMoi;
                }
                else
                {
                    MessageBox.Show("Hiện tại chưa có sự thay đổi dữ liệu nào mới.",
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông báo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TrangChu());
        }

        private void btnLinhKien_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LinhKien());
        }

        private void btnLoaiLK_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoaiLinhKien());
        }

        private void btnKhachHang_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new KhachHang());
        }

        private void btnHoaDon_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HoaDon());
        }

        private void btnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NhanVien());
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có muốn đăng xuất không?", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoginApp login = new LoginApp();
                login.Show();
                this.Close();
            }
        }
    }
}