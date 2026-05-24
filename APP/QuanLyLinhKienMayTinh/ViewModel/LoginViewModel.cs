using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        string _loginUsername;
        public string LoginUsername
        {
            get => _loginUsername;
            set { _loginUsername = value; OnPropertyChanged(); }
        }

        string _loginPassword;
        public string LoginPassword
        {
            get => _loginPassword;
            set { _loginPassword = value; OnPropertyChanged(); }
        }
        string _signUpMaNV;
        public string SignUpMaNV
        {
            get => _signUpMaNV;
            set { _signUpMaNV = value; OnPropertyChanged(); }
        }

        string _signUpUsername;
        public string SignUpUsername
        {
            get => _signUpUsername;
            set { _signUpUsername = value; OnPropertyChanged(); }
        }

        string _signUpPassword;
        public string SignUpPassword
        {
            get => _signUpPassword;
            set { _signUpPassword = value; OnPropertyChanged(); }
        }

        string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        Visibility _loginVisibility = Visibility.Visible;
        public Visibility LoginVisibility
        {
            get => _loginVisibility;
            set { _loginVisibility = value; OnPropertyChanged(); }
        }

        Visibility _signUpVisibility = Visibility.Collapsed;
        public Visibility SignUpVisibility
        {
            get => _signUpVisibility;
            set { _signUpVisibility = value; OnPropertyChanged(); }
        }

        string _message = "Vui lòng đăng nhập để tiếp tục";
        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        bool _isDark = false; // cờ để theo dõi trạng thái theme hiện tại, mặc định là light

        public ICommand ToggleThemeCommand { get; }
        public ICommand ShowLoginCommand { get; set; }
        public ICommand ShowSignUpCommand { get; set; }
        public ICommand LoginCommand { get; set; }
        public ICommand SignUpCommand { get; set; }

        public LoginViewModel()
        {
            ToggleThemeCommand = new RelayCommand<object>(p => true, p => ExecuteToggleTheme(p));
            LoginCommand = new RelayCommand<object>(p => true, p => ThucHienDangNhap());
        }
        public void ThucHienDangNhap()
        {
            if (string.IsNullOrEmpty(LoginUsername) || string.IsNullOrEmpty(LoginPassword))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            try
            {
                var db = DataProvider.Ins.GetContext();

                var acc = db.TaiKhoans
                            .Include(t => t.MaNvNavigation)
                            .FirstOrDefault(t => t.TenDn == LoginUsername && t.MatKhau == LoginPassword); // lấy thông tin tài khoản cùng với thông tin nhân viên

                if (acc != null) 
                {
                    LuuTrangThai.MaNVDangNhap = acc.MaNv;
                    LuuTrangThai.QuyenDangNhap = acc.MaNvNavigation.Quyen;

                    DataProvider.Ins.ChangeConnectionByRole(LuuTrangThai.QuyenDangNhap);

                    MainWindow main = new MainWindow(LoginUsername);
                    main.Show();

                    foreach (Window item in Application.Current.Windows)
                    {
                        if (item is Views.LoginView)
                        {
                            item.Close();
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        void ExecuteToggleTheme(object obj)
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