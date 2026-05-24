using QuanLyLinhKienMayTinh.Models;
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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        // Xóa hết suPassVisible và suConfirmVisible, chỉ giữ lại của Login
        bool liPassVisible = false;

        public LoginView()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
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

        void LiPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.LoginPassword = liPassword.Password;
            }
        }
    }
}