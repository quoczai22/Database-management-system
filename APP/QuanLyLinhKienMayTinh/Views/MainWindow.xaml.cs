using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.ViewModels;
using QuanLyLinhKienMayTinh.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyLinhKienMayTinh
{

    public partial class MainWindow : Window
    {

        public MainWindow(string username)
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(username);
            MainFrame.Navigate(new TrangChuView());
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TrangChuView());
        }

        private void btnLinhKien_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LinhKien());
        }

        private void btnLoaiLK_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoaiLinhKienView());
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
    }
}
