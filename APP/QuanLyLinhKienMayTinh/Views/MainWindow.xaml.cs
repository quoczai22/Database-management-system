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
            txtQuyen.Text = LuuTrangThai.QuyenDangNhap;
            MainFrame.Navigate(new TrangChuView());
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TrangChuView());
        }

        private void btnLinhKien_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LinhKienView());
        }

        private void btnLoaiLK_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoaiLinhKienView());
        }

        private void btnKhachHang_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new KhachHangView());
        }

        private void btnHoaDon_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HoaDonView());
        }

        private void btnNhanVien_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NhanVienView());
        }
        void btnBaoMat_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new BaoMatView());
        }
    }
}