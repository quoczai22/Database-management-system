using System;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace QuanLyLinhKienMayTinh
{
    public partial class LoginApp : Window
    {
        bool suPassVisible = false;
        bool suConfirmVisible = false;
        bool liPassVisible = false;

        string connectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=QL_LinhKien_PC;Trusted_Connection=True;";

        public LoginApp()
        {
            InitializeComponent();
        }

        void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            SignUpPanel.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;
            txtMessage.Text = "Vui lòng đăng nhập để tiếp tục";
        }

        void ShowSignUp_Click(object sender, RoutedEventArgs e)
        {
            SignUpPanel.Visibility = Visibility.Visible;
            LoginPanel.Visibility = Visibility.Collapsed;
            txtMessage.Text = "Vui lòng đăng ký để tiếp tục";
        }

        string GetSuPassword()
        {
            return suPassVisible ? suPasswordVisible.Text : suPassword.Password;
        }

        string GetSuConfirmPassword()
        {
            return suConfirmVisible ? suConfirmPasswordVisible.Text : suConfirmPassword.Password;
        }

        string GetLiPassword()
        {
            return liPassVisible ? liPasswordVisible.Text : liPassword.Password;
        }

        void ToggleSuPassword_Click(object sender, RoutedEventArgs e)
        {
            if (suPassVisible)
            {
                suPassword.Password = suPasswordVisible.Text;
                suPassword.Visibility = Visibility.Visible;
                suPasswordVisible.Visibility = Visibility.Collapsed;
            }
            else
            {
                suPasswordVisible.Text = suPassword.Password;
                suPassword.Visibility = Visibility.Collapsed;
                suPasswordVisible.Visibility = Visibility.Visible;
            }
            suPassVisible = !suPassVisible;
        }

        void ToggleSuConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            if (suConfirmVisible)
            {
                suConfirmPassword.Password = suConfirmPasswordVisible.Text;
                suConfirmPassword.Visibility = Visibility.Visible;
                suConfirmPasswordVisible.Visibility = Visibility.Collapsed;
            }
            else
            {
                suConfirmPasswordVisible.Text = suConfirmPassword.Password;
                suConfirmPassword.Visibility = Visibility.Collapsed;
                suConfirmPasswordVisible.Visibility = Visibility.Visible;
            }
            suConfirmVisible = !suConfirmVisible;
        }

        void ToggleLiPassword_Click(object sender, RoutedEventArgs e)
        {
            if (liPassVisible)
            {
                liPassword.Password = liPasswordVisible.Text;
                liPassword.Visibility = Visibility.Visible;
                liPasswordVisible.Visibility = Visibility.Collapsed;
            }
            else
            {
                liPasswordVisible.Text = liPassword.Password;
                liPassword.Visibility = Visibility.Collapsed;
                liPasswordVisible.Visibility = Visibility.Visible;
            }
            liPassVisible = !liPassVisible;
        }

        void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string username = suUsername.Text.Trim();
            string password = GetSuPassword();
            string confirm = GetSuConfirmPassword();

            if (username == "")
            {
                MessageBox.Show("Chưa nhập tên đăng nhập");
                return;
            }

            if (password == "")
            {
                MessageBox.Show("Chưa nhập mật khẩu");
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không đúng");
                return;
            }

            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();

                string checkQuery =
                "SELECT COUNT(*) FROM TaiKhoan WHERE tendangnhap=@username";

                using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);

                checkCmd.Parameters.AddWithValue("@username", username);

                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    MessageBox.Show("Tài khoản đã tồn tại");
                    return;
                }

                string insertQuery =
                "INSERT INTO Taikhoan(tendangnhap,matkhau) VALUES(@username,@password)";

                using SqlCommand insertCmd = new SqlCommand(insertQuery, conn);

                insertCmd.Parameters.AddWithValue("@username", username);
                insertCmd.Parameters.AddWithValue("@password", password);

                insertCmd.ExecuteNonQuery();

                MessageBox.Show("Đăng ký thành công");

                ShowLogin_Click(null!, null!);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi database: " + ex.Message);
            }
        }

        void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = liUsername.Text.Trim();
            string password = GetLiPassword();

            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);

                conn.Open();

                string query =
                "SELECT COUNT(*) FROM TaiKhoan WHERE tendangnhap=@username AND matkhau=@password";

                using SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);

                int result = (int)cmd.ExecuteScalar();

                if (result > 0)
                {
                    MessageBox.Show("Đăng nhập thành công");
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi database: " + ex.Message);
            }
        }
    }
}