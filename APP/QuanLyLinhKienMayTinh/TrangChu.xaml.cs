using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
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
        DatabaseHelper db = new DatabaseHelper();

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
                txtTongNV.Text = db.ExecuteScalarInt("SELECT COUNT(*) FROM NhanVien").ToString();
                txtTongKH.Text = db.ExecuteScalarInt("SELECT COUNT(*) FROM KhachHang").ToString();
                txtTongLK.Text = db.ExecuteScalarInt("SELECT COUNT(*) FROM LinhKien").ToString();
                txtTongLoaiLK.Text = db.ExecuteScalarInt("SELECT COUNT(*) FROM LoaiLK").ToString();
                txtTongHD.Text = db.ExecuteScalarInt("SELECT COUNT(*) FROM HoaDon").ToString();

                string truyVanTopSanPham = @"
                    SELECT TOP 5 lk.TenLK, SUM(ct.SoLuong) AS TongBan
                    FROM ChiTietHD ct
                    JOIN LinhKien lk ON ct.MaLK = lk.MaLK
                    GROUP BY lk.TenLK
                    ORDER BY TongBan DESC";

                DataTable bangTopSanPham = db.GetDataTable(truyVanTopSanPham);
                ChartValues<double> giaTriSanPham = new ChartValues<double>();

                List<string> danhSachTenLK = new List<string>();
                List<double> danhSachTongBan = new List<double>();

                foreach (DataRow dong in bangTopSanPham.Rows)
                {
                    danhSachTenLK.Add(dong["TenLK"]?.ToString() ?? "");
                    danhSachTongBan.Add(Convert.ToDouble(dong["TongBan"]));
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

                string truyVanHangSX = "SELECT NSX, COUNT(MaLK) AS SoLuong FROM LinhKien GROUP BY NSX";
                DataTable bangHangSX = db.GetDataTable(truyVanHangSX);

                List<ThongKeHang> danhSachTam = new List<ThongKeHang>();

                foreach (DataRow dong in bangHangSX.Rows)
                {
                    danhSachTam.Add(new ThongKeHang
                    {
                        HangSX = dong["NSX"]?.ToString() ?? "",
                        SoLuong = Convert.ToInt32(dong["SoLuong"])
                    });
                }

                DanhSachThongKeHang = danhSachTam;
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

                string truyVanQuyen = "SELECT ChucVu, COUNT(MaNV) AS SoLuong FROM NhanVien GROUP BY ChucVu";
                DataTable bangQuyen = db.GetDataTable(truyVanQuyen);

                List<string> danhSachQuyen = new List<string>();
                List<double> danhSachSoLuong = new List<double>();
                List<string> danhSachMau = new List<string> { "#ff9800", "#e91e63", "#2196f3", "#4caf50", "#9c27b0" };

                foreach (DataRow dong in bangQuyen.Rows)
                {
                    danhSachQuyen.Add(dong["ChucVu"]?.ToString() ?? "");
                    danhSachSoLuong.Add(Convert.ToDouble(dong["SoLuong"]));
                }

                for (int i = 0; i < danhSachQuyen.Count; i++)
                {
                    RoleSeries.Add(new PieSeries
                    {
                        Title = danhSachQuyen[i],
                        Values = new ChartValues<double> { danhSachSoLuong[i] },
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