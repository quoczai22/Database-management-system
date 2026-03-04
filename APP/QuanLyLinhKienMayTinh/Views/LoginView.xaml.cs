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
        bool suPassVisible = false;
        bool suConfirmVisible = false;
        bool liPassVisible = false;

        public LoginView()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel();
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
            var vm = DataContext as LoginViewModel;
            vm?.ThucHienDangKy(GetSuPassword(), GetSuConfirmPassword());
        }

        void Login_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            vm?.ThucHienDangNhap(GetLiPassword(), this);
        }
    }
}
