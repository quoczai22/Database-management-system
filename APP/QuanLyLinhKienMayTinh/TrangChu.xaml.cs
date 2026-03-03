using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuanLyLinhKienMayTinh
{
    public class ThongKeHang
    {
        public int SoLuong { get; set; }
        public string HangSX { get; set; }
    }

    public partial class TrangChu : Page
    {
        public SeriesCollection RevenueSeries { get; set; }
        public List<string> Labels { get; set; }
        public SeriesCollection RoleSeries { get; set; }
        public List<ThongKeHang> DanhSachThongKeHang { get; set; }

        public TrangChu()
        {
            InitializeComponent();

            RevenueSeries = new SeriesCollection();
            Labels = new List<string>();
            RoleSeries = new SeriesCollection();
            DanhSachThongKeHang = new List<ThongKeHang>();

            TaiDuLieu();
            DataContext = this;
        }

        private void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.DB;

                txtTongNV.Text = db.NhanViens.Count().ToString();
                txtTongKH.Text = db.KhachHangs.Count().ToString();
                txtTongLK.Text = db.LinhKiens.Count().ToString();
                txtTongLoaiLK.Text = db.LoaiLks.Count().ToString();
                txtTongHD.Text = db.HoaDons.Count().ToString();

                var topSanPham = DataProvider.Ins.DB.ChiTietHds
                    .GroupBy(ct => ct.MaLkNavigation.TenLk)
                    .Select(g => new {
                        TenLK = g.Key,
                        TongBan = g.Sum(ct => ct.SoLuong)
                    })
                    .OrderByDescending(x => x.TongBan)
                    .Take(5)
                    .ToList();

                ChartValues<double> giaTriSanPham = new ChartValues<double>();
                List<string> danhSachTenLK = new List<string>();
                List<double> danhSachTongBan = new List<double>();

                foreach (var sp in topSanPham)
                {
                    danhSachTenLK.Add(sp.TenLK);
                    danhSachTongBan.Add(Convert.ToDouble(sp.TongBan ?? 0));
                }

                Labels.AddRange(danhSachTenLK);
                giaTriSanPham.AddRange(danhSachTongBan);

                RevenueSeries.Add(new LineSeries
                {
                    Title = "Đã bán (Cái/Bộ)",
                    Values = giaTriSanPham,
                    AreaLimit = 0,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7b61ff")),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#337b61ff"))
                });

                var hangSX = db.LinhKiens
                    .GroupBy(lk => lk.Nsx)
                    .Select(g => new ThongKeHang
                    {
                        HangSX = g.Key,
                        SoLuong = g.Count()
                    })
                    .ToList();

                DanhSachThongKeHang = hangSX;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PieChart_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleSeries.Clear();
                var db = DataProvider.Ins.DB;

                var chucVuNV = db.NhanViens
                    .GroupBy(nv => nv.ChucVu)
                    .Select(g => new { ChucVu = g.Key, SoLuong = g.Count() })
                    .ToList();

                List<string> danhSachMau = new List<string> { "#ff9800", "#e91e63", "#2196f3", "#4caf50", "#9c27b0" };

                for (int i = 0; i < chucVuNV.Count; i++)
                {
                    RoleSeries.Add(new PieSeries
                    {
                        Title = chucVuNV[i].ChucVu,
                        Values = new ChartValues<double> { chucVuNV[i].SoLuong },
                        DataLabels = true,
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(danhSachMau[i % danhSachMau.Count]))
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
