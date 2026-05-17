using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
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

        public Func<double, string> Formatter { get; set; }

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
        private List<TopKhachHang> _danhSachTopKhachHang;
        public List<TopKhachHang> DanhSachTopKhachHang
        {
            get { return _danhSachTopKhachHang; }
            set
            {
                _danhSachTopKhachHang = value;
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

            TaiDuLieu();
            LoadPieChart();
        }
        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                TongNV = db.NhanViens.Count();
                TongKH = db.KhachHangs.Count();
                TongLK = db.LinhKiens.Sum(lk => lk.SoLuongTon ?? 0);
                TongLoaiLK = db.LoaiLks.Count();
                TongHD = db.HoaDons.Count();
                var year = Enumerable.Range(2023, 4).ToList();

                // 1. Khởi tạo dữ liệu
                var giaTriDoanhThu = new ChartValues<double>(); // Dữ liệu doanh thu theo tháng
                var danhSachThang = new List<string>(); // danh sách tháng theo trục hoành

                db.Database.OpenConnection();
                using (var command = db.Database.GetDbConnection().CreateCommand())
                {
                    foreach (int y in year)
                    {
                        for (int m = 1; m <= 12; m +=3)
                        {
                            // Truyền lệnh SQL gọi trực tiếp Function của bạn
                            command.CommandText = $"SELECT dbo.fn_DoanhThuTheoThang({m}, {y})";
                            var result = command.ExecuteScalar();

                            // Xử lý giá trị trả về
                            double doanhThuThang = (result != null && result != DBNull.Value) ? Convert.ToDouble(result) : 0;

                            giaTriDoanhThu.Add(doanhThuThang);
                            danhSachThang.Add($"Tháng {m}/{y}");
                        }
                    }
                }
                db.Database.CloseConnection();

                // 4. Cập nhật UI
                Labels = danhSachThang; // cập nhật danh sách tháng cho trục hoành
                Formatter = value => value.ToString("N0") + " đ"; // định dạng giá trị doanh thu hiển thị trên biểu đồ 

                DoanhThu = new SeriesCollection
    {
                new LineSeries
                {
                    Title = $"Doanh thu từ năm {year.Min()} - {year.Max()} ",
                    Values = giaTriDoanhThu,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3f51b5")),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#153f51b5"))
                }
            };

                var top5KhachHang = db.HoaDons
                .Include(hd => hd.MaKhNavigation)
                .Where(hd => hd.TrangThai == "Đã thanh toán") 
                .GroupBy(hd => new { hd.MaKhNavigation.MaKh, hd.MaKhNavigation.TenKh })
                .Select(g => new TopKhachHang
                {
                    MaKh = g.Key.MaKh,
                    TenKh = g.Key.TenKh,
                    TongTien = (double)g.Sum(hd => hd.TongTien ?? 0)
                })
                .OrderByDescending(x => x.TongTien) 
                .Take(5) 
                .ToList();

                DanhSachTopKhachHang = top5KhachHang;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // hàm này cho dữ liệu biểu đồ tròn về số lượng nhân viên theo chức vụ
        public void LoadPieChart()
        {
            try
            {
                ChucVu.Clear(); // nếu có dữ liệu cũ thì xóa đi để tránh trùng lặp khi tải lại dữ liệu
                var db = DataProvider.Ins.GetContext(); // lấy dữ liệu từ database
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
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(danhSachMau[i % danhSachMau.Count])) // tô màu cho từng nhân viên theo chức vụ 
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
