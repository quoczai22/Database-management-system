using QuanLyLinhKienMayTinh.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _loginUsername;
        public string LoginUsername { get => _loginUsername; set { _loginUsername = value; OnPropertyChanged(); } }

        private string _signUpUsername;
        public string SignUpUsername { get => _signUpUsername; set { _signUpUsername = value; OnPropertyChanged(); } }

        private Visibility _loginVisibility = Visibility.Visible;
        public Visibility LoginVisibility { get => _loginVisibility; set { _loginVisibility = value; OnPropertyChanged(); } }

        private Visibility _signUpVisibility = Visibility.Collapsed;
        public Visibility SignUpVisibility { get => _signUpVisibility; set { _signUpVisibility = value; OnPropertyChanged(); } }

        private string _message = "Vui lòng đăng nhập để tiếp tục";
        public string Message { get => _message; set { _message = value; OnPropertyChanged(); } }

        public ICommand ShowLoginCommand { get; set; }
        public ICommand ShowSignUpCommand { get; set; }

        public LoginViewModel()
        {
            ShowSignUpCommand = new RelayCommand<object>((p) => true, (p) => {
                LoginVisibility = Visibility.Collapsed;
                SignUpVisibility = Visibility.Visible;
                Message = "Vui lòng điền thông tin để đăng ký tài khoản mới";
            });

            ShowLoginCommand = new RelayCommand<object>((p) => true, (p) => {
                SignUpVisibility = Visibility.Collapsed;
                LoginVisibility = Visibility.Visible;
                Message = "Vui lòng đăng nhập để tiếp tục";
            });
        }

        public void ThucHienDangNhap(string password, Window window)
        {
            if (string.IsNullOrEmpty(LoginUsername) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return;
            }

            try
            {
                var db = DataProvider.Ins.DB;
                bool hopLe = db.TaiKhoans.Any(t => t.Tendangnhap == LoginUsername && t.Matkhau == password);

                if (hopLe)
                {
                    MainWindow main = new MainWindow(LoginUsername);
                    main.Show();
                    window?.Close();
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

        public void ThucHienDangKy(string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(SignUpUsername))
            {
                MessageBox.Show("Chưa nhập tên đăng nhập");
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Chưa nhập mật khẩu");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không đúng");
                return;
            }

            try
            {
                var db = DataProvider.Ins.DB;
                if (db.TaiKhoans.Any(t => t.Tendangnhap == SignUpUsername))
                {
                    MessageBox.Show("Tài khoản đã tồn tại");
                    return;
                }

                db.TaiKhoans.Add(new TaiKhoan { Tendangnhap = SignUpUsername, Matkhau = password });
                db.SaveChanges();

                MessageBox.Show("Đăng ký thành công");

                ShowLoginCommand.Execute(null);
                SignUpUsername = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}