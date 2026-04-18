using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class TrangChuViewModel : BaseViewModel
    {
        private SeriesCollection _doanhThu;
        public SeriesCollection DoanhThu
        {
            get { return _doanhThu; }
            set
            {
                _doanhThu = value;
                OnPropertyChanged();
            }
        }

        private List<string> _lables;
        public List<string> Labels
        {
            get { return _lables; }
            set
            {
                _lables = value;
                OnPropertyChanged();
            }
        }
        private SeriesCollection _chuVu;
        public SeriesCollection ChucVu
        {
            get { return _chuVu; }
            set
            {
                _chuVu = value;
                OnPropertyChanged();
            }
        }
        private List<ThongKeHang> _danhSachThongKeHang;
        public List<ThongKeHang> DanhSachThongKeBanHang
        {
            get { return _danhSachThongKeHang; }
            set
            {
                _danhSachThongKeHang = value;
                OnPropertyChanged();
            }
        }
        private int _tongNV;

        public int TongNV
        {
            get { return _tongNV; }
            set
            {
                _tongNV = value;
                OnPropertyChanged();
            }
        }

        private int _tongKH;

        public int TongKH
        {
            get { return _tongKH; }
            set
            {
                _tongKH = value;
                OnPropertyChanged();
            }
        }

        private int _tongLK;
        public int TongLK
        {
            get { return _tongLK; }
            set
            {
                _tongLK = value;
                OnPropertyChanged();
            }
        }

        private int _tongLoaiLK;
        public int TongLoaiLK
        {
            get { return _tongLoaiLK; }
            set
            {
                _tongLoaiLK = value;
                OnPropertyChanged();
            }
        }

        private int _tongHD;
        public int TongHD
        {
            get { return _tongHD; }
            set
            {
                _tongHD = value;
                OnPropertyChanged();
            }
        }

        public TrangChuViewModel()
        {
            DoanhThu = new SeriesCollection();
            Labels = new List<string>();
            ChucVu = new SeriesCollection();
            DanhSachThongKeBanHang = new List<ThongKeHang>();
            ChucVu = new SeriesCollection();

            TaiDuLieu();
            LoadPieChart();
        }
        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.DB;
                TongNV = db.NhanViens.Count();
                TongKH = db.KhachHangs.Count();
                TongLK = db.LinhKiens.Count();
                TongLoaiLK = db.LoaiLks.Count();
                TongHD = db.HoaDons.Count();

                var topSanPham = db.ChiTietHds
                    .GroupBy(ct => ct.MaLkNavigation.TenLk)
                    .Select(g => new { TenLk = g.Key, TongBan = g.Sum(ct => ct.SoLuong) })
                    .OrderByDescending(x => x.TongBan)
                    .Take(5)
                    .ToList();

                var giaTriSanPham = new ChartValues<double>();
                var tenDanhSachLK = new List<string>();

                foreach (var sp in topSanPham)
                {
                    tenDanhSachLK.Add(sp.TenLk);
                    giaTriSanPham.Add(Convert.ToDouble(sp.TongBan ?? 0));
                }

                Labels = tenDanhSachLK;

                DoanhThu.Add(new LineSeries
                {
                    Title = "Đã bán (Cái/Bộ)",
                    Values = giaTriSanPham,
                    AreaLimit = 0,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7b61ff")),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#337b61ff"))
                });

                var hangSX = db.LinhKiens
                    .GroupBy(lk => lk.Nsx)
                    .Select(g => new ThongKeHang { HangSX = g.Key, SoLuong = g.Count() })
                    .ToList();

                DanhSachThongKeBanHang = hangSX;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void LoadPieChart()
        {
            try
            {
                ChucVu.Clear();
                var db = DataProvider.Ins.DB;
                var chucVuNV = db.NhanViens
                    .GroupBy(nv => nv.ChucVu)
                    .Select(g => new { ChucVu = g.Key, SoLuong = g.Count() })
                    .ToList();

                var danhSachMau = new List<string> { "#ff9800", "#e91e63", "#2196f3", "#4caf50", "#9c27b0" };

                for (int i = 0; i < chucVuNV.Count; i++)
                {
                    ChucVu.Add(new PieSeries
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
