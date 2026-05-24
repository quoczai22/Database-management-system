using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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

        private Visibility _menuDashboardVisibility = Visibility.Visible;
        public Visibility MenuDashboardVisibility
        {
            get => _menuDashboardVisibility;
            set { _menuDashboardVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _menuLinhKienVisibility = Visibility.Collapsed;
        public Visibility MenuLinhKienVisibility
        {
            get => _menuLinhKienVisibility;
            set { _menuLinhKienVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _menuLoaiLKVisibility = Visibility.Collapsed;
        public Visibility MenuLoaiLKVisibility
        {
            get => _menuLoaiLKVisibility;
            set { _menuLoaiLKVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _menuNhanVienVisibility = Visibility.Collapsed;
        public Visibility MenuNhanVienVisibility
        {
            get => _menuNhanVienVisibility;
            set { _menuNhanVienVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _menuKhachHangVisibility = Visibility.Collapsed;
        public Visibility MenuKhachHangVisibility
        {
            get => _menuKhachHangVisibility;
            set { _menuKhachHangVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _menuHoaDonVisibility = Visibility.Collapsed;
        public Visibility MenuHoaDonVisibility
        {
            get => _menuHoaDonVisibility;
            set { _menuHoaDonVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _menuBaoMatVisibility = Visibility.Collapsed;
        public Visibility MenuBaoMatVisibility
        {
            get => _menuBaoMatVisibility;
            set { _menuBaoMatVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _menuTaiKhoanVisibility = Visibility.Collapsed;
        public Visibility MenuTaiKhoanVisibility
        {
            get => _menuTaiKhoanVisibility;
            set { _menuTaiKhoanVisibility = value; OnPropertyChanged(); }
        }

        bool _isDark = false; 

        public ICommand ThongBaoCommand { get; set; }
        public ICommand LogOutCommand { get; set; }
        public ICommand ToggleThemeCommand { get; set; }

        public MainViewModel(string username)
        {
            CurrentUsername = username;
            LaySoLuongGoc();
            PhanQuyenGiaoDien();

            ThongBaoCommand = new RelayCommand<object>((p) => true, (p) => ThongBao());
            LogOutCommand = new RelayCommand<object>((p) => true, (p) => LogOut());
            ToggleThemeCommand = new RelayCommand<object>((p) => true, (p) => ExecuteToggleTheme(p));
        }
        private void PhanQuyenGiaoDien()
        {
            string quyen = LuuTrangThai.QuyenDangNhap;
            if (quyen == "Quản lý toàn bộ")
            {
                MenuDashboardVisibility = Visibility.Visible;
                MenuLinhKienVisibility = Visibility.Visible;
                MenuLoaiLKVisibility = Visibility.Visible;
                MenuNhanVienVisibility = Visibility.Visible;
                MenuKhachHangVisibility = Visibility.Visible;
                MenuHoaDonVisibility = Visibility.Visible;
                MenuBaoMatVisibility = Visibility.Visible;
                MenuTaiKhoanVisibility = Visibility.Visible;
            }
            else if (quyen == "Thu ngân")
            {
                MenuDashboardVisibility = Visibility.Visible;
                MenuLinhKienVisibility = Visibility.Collapsed;
                MenuLoaiLKVisibility = Visibility.Collapsed;
                MenuNhanVienVisibility = Visibility.Collapsed;
                MenuKhachHangVisibility = Visibility.Visible;
                MenuHoaDonVisibility = Visibility.Visible;
                MenuBaoMatVisibility = Visibility.Collapsed;
                MenuTaiKhoanVisibility = Visibility.Collapsed;
            }
            else if (quyen == "Chăm sóc khách hàng")
            {
                MenuDashboardVisibility = Visibility.Visible;
                MenuLinhKienVisibility = Visibility.Collapsed;
                MenuLoaiLKVisibility = Visibility.Collapsed;
                MenuNhanVienVisibility = Visibility.Collapsed;
                MenuKhachHangVisibility = Visibility.Visible;
                MenuHoaDonVisibility = Visibility.Collapsed;
                MenuBaoMatVisibility = Visibility.Collapsed;
                MenuTaiKhoanVisibility = Visibility.Collapsed;
            }
            else if (quyen == "Quản lý kho")
            {
                MenuDashboardVisibility = Visibility.Visible;
                MenuLinhKienVisibility = Visibility.Visible;
                MenuLoaiLKVisibility = Visibility.Visible;
                MenuNhanVienVisibility = Visibility.Collapsed;
                MenuKhachHangVisibility = Visibility.Collapsed;
                MenuHoaDonVisibility = Visibility.Collapsed;
                MenuBaoMatVisibility = Visibility.Collapsed;
                MenuTaiKhoanVisibility = Visibility.Collapsed;
            }
            else
            {
                MenuDashboardVisibility = Visibility.Collapsed;
                MenuLinhKienVisibility = Visibility.Collapsed;
                MenuLoaiLKVisibility = Visibility.Collapsed;
                MenuNhanVienVisibility = Visibility.Collapsed;
                MenuKhachHangVisibility = Visibility.Collapsed;
                MenuHoaDonVisibility = Visibility.Collapsed;
                MenuBaoMatVisibility = Visibility.Visible;
                MenuTaiKhoanVisibility = Visibility.Collapsed;
            }
        }
        private void LaySoLuongGoc()
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                soNhanVienGoc = db.NhanViens.Count();
                soLinhKienGoc = db.LinhKiens.Count();
                soHoaDonGoc = db.HoaDons.Count();
            }
            catch { }
        }
        public async void ThongBao()
        {
            try
            {
                using (var db = DataProvider.Ins.GetContext())
                {
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

                    var spSapHetHang = await db.Procedures.sp_baocaotonkhoAsync();

                    if (spSapHetHang != null && spSapHetHang.Count > 0)
                    {
                        danhSachThongBao.Add($"- CẢNH BÁO: Có {spSapHetHang.Count} linh kiện sắp hết hàng!");
                    }

                    var khachHangChuaTT = await db.Procedures.sp_DanhSacKhachHangChuaTTAsync();

                    if (khachHangChuaTT != null && khachHangChuaTT.Count > 0)
                    {
                        danhSachThongBao.Add($"- CẢNH BÁO: Có {khachHangChuaTT.Count} hóa đơn KHÁCH CHƯA THANH TOÁN!");
                    }

                    if (danhSachThongBao.Count > 0)
                    {
                        MessageBox.Show("Hệ thống có thay đổi:\n\n" + string.Join("\n", danhSachThongBao),
                                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (spSapHetHang != null && spSapHetHang.Count > 0)
                        {
                            var window = new ThongBaoTonKhoWindow(spSapHetHang);
                            window.Owner = Application.Current.MainWindow;
                            window.ShowDialog();
                        }

                        if (khachHangChuaTT != null && khachHangChuaTT.Count > 0)
                        {
                            var windowKhachNo = new ThongBaoKhachHangWindow(khachHangChuaTT);
                            windowKhachNo.Owner = Application.Current.MainWindow;
                            windowKhachNo.ShowDialog();
                        }

                        soNhanVienGoc = soNhanVienMoi;
                        soLinhKienGoc = soLinhKienMoi;
                        soHoaDonGoc = soHoaDonMoi;
                    }
                    else
                    {
                        MessageBox.Show("Mọi thứ đang hoạt động ổn định, không có thông báo mới!",
                                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
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
        private void ExecuteToggleTheme(object obj)
        {

            _isDark = !_isDark;
            string themeFile = _isDark ? "../Themes/ThemeDark.xaml" : "../Themes/ThemeLight.xaml";

            try
            {
                var newThemeDict = new ResourceDictionary
                {
                    Source = new Uri(themeFile, UriKind.RelativeOrAbsolute) // tạo ResourceDictionary mới với đường dẫn đến file theme tương ứng
                };

                var mergedDicts = Application.Current.Resources.MergedDictionaries; // lấy danh sách ResourceDictionary đã được gộp vào tài nguyên ứng dụng
                var oldThemeDict = mergedDicts.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme")); // tìm ResourceDictionary có chứa "Theme" 
                mergedDicts.Add(newThemeDict); // thêm ResourceDictionary mới vào danh sách gộp để áp dụng theme mới cho toàn bộ ứng dụng
                if (oldThemeDict != null)
                {
                    mergedDicts.Remove(oldThemeDict);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi chuyển theme: {ex.Message}");
            }
        }
    }
}
