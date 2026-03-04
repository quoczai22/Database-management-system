using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.Views;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _currentUsername;
        public string CurrentUsername
        {
            get => _currentUsername;
            set { _currentUsername = value; OnPropertyChanged(); }
        }

        private int soNhanVienGoc = 0;
        private int soLinhKienGoc = 0;
        private int soHoaDonGoc = 0;

        public ICommand ThongBaoCommand { get; set; }
        public ICommand LogOutCommand { get; set; }

        public MainViewModel(string username)
        {
            CurrentUsername = username;
            LaySoLuongGoc();

            ThongBaoCommand = new RelayCommand<object>((p) => true, (p) => ThongBao());
            LogOutCommand = new RelayCommand<object>((p) => true, (p) => LogOut());
        }

        private void LaySoLuongGoc()
        {
            try
            {
                var db = DataProvider.Ins.DB;
                soNhanVienGoc = db.NhanViens.Count();
                soLinhKienGoc = db.LinhKiens.Count();
                soHoaDonGoc = db.HoaDons.Count();
            }
            catch { }
        }

        public void ThongBao()
        {
            try
            {
                var db = DataProvider.Ins.DB;
                int soNhanVienMoi = db.NhanViens.Count();
                int soLinhKienMoi = db.LinhKiens.Count();
                int soHoaDonMoi = db.HoaDons.Count();

                List<string> danhSachThongBao = new List<string>();

                if (soNhanVienMoi > soNhanVienGoc)
                    danhSachThongBao.Add($"- Có {soNhanVienMoi - soNhanVienGoc} nhân viên MỚI gia nhập.");
                else if (soNhanVienMoi < soNhanVienGoc)
                    danhSachThongBao.Add($"- Có {soNhanVienGoc - soNhanVienMoi} nhân viên ĐÃ NGHỈ VIỆC.");

                if (soLinhKienMoi > soLinhKienGoc)
                    danhSachThongBao.Add($"- Có {soLinhKienMoi - soLinhKienGoc} mã linh kiện MỚI.");
                else if (soLinhKienMoi < soLinhKienGoc)
                    danhSachThongBao.Add($"- Có {soLinhKienGoc - soLinhKienMoi} mã linh kiện bị xóa.");

                if (soHoaDonMoi > soHoaDonGoc)
                    danhSachThongBao.Add($"- Có {soHoaDonMoi - soHoaDonGoc} hóa đơn MỚI.");

                if (danhSachThongBao.Count > 0)
                {
                    MessageBox.Show("Hệ thống có thay đổi:\n\n" + string.Join("\n", danhSachThongBao),
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    soNhanVienGoc = soNhanVienMoi;
                    soLinhKienGoc = soLinhKienMoi;
                    soHoaDonGoc = soHoaDonMoi;
                }
                else
                {
                    MessageBox.Show("Chưa có thay đổi dữ liệu mới.",
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông báo: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LogOut()
        {
            MessageBoxResult result = MessageBox.Show("Bạn có muốn đăng xuất không?", "Thông báo",
                                                      MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                LoginView login = new LoginView();
                login.Show();
                Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.Close();
            }
        }
    }
}
