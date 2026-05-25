using LiveCharts.Wpf;
using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class HoaDonDisplay
    {
        public string MaHoaDon { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgayTao { get; set; }
        public decimal? TongTien { get; set; }
        public decimal? TamTinh { get; set; }
        public decimal? GiamGia { get; set; }
        public string TrangThai { get; set; }
        public Brush TrangThaiMauNen { get; set; }
        public Brush TrangThaiMauChu { get; set; }
        public string SoDienThoai { get; set; }
        public string PhuongThucThanhToan { get; set; }
    }

    public class ChiTietSanPhamDisplay
    {
        public string TenSanPham { get; set; }
        public byte? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public string HanBaoHanhHienThi { get; set; }
    }

    public class HoaDonViewModel : BaseViewModel, ISearchable
    {
        // ── CÁC BIẾN ───────────────────────────────────
        private ObservableCollection<HoaDonDisplay> _all;
        private ObservableCollection<HoaDonDisplay> _danhSachHoaDon;
        public ObservableCollection<HoaDonDisplay> DanhSachHoaDon { get => _danhSachHoaDon; set { _danhSachHoaDon = value; OnPropertyChanged(); } }

        private HoaDonDisplay _hoaDonChon;
        public HoaDonDisplay HoaDonChon
        {
            get => _hoaDonChon;
            set
            {
                _hoaDonChon = value;
                OnPropertyChanged();
                ChiTietVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
                ChuaChonHoaDonVisibility = value == null ? Visibility.Visible : Visibility.Collapsed;
                TaiChiTietSanPham(value?.MaHoaDon);
                ThanhToanButtonVisibility = (value != null && value.TrangThai != "Đã thanh toán") ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private ObservableCollection<ChiTietSanPhamDisplay> _chiTietSanPham;
        public ObservableCollection<ChiTietSanPhamDisplay> ChiTietSanPham { get => _chiTietSanPham; set { _chiTietSanPham = value; OnPropertyChanged(); } }

        private Visibility _chiTietVisibility = Visibility.Collapsed;
        public Visibility ChiTietVisibility { get => _chiTietVisibility; set { _chiTietVisibility = value; OnPropertyChanged(); } }

        private Visibility _chuaChonHoaDonVisibility = Visibility.Visible;
        public Visibility ChuaChonHoaDonVisibility { get => _chuaChonHoaDonVisibility; set { _chuaChonHoaDonVisibility = value; OnPropertyChanged(); } }

        private string _tuKhoanTimKiem = string.Empty;
        public string TuKhoanTimKiem
        {
            get => _tuKhoanTimKiem;
            set { _tuKhoanTimKiem = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _danhSachTrangThai;
        public ObservableCollection<string> DanhSachTrangThai
        {
            get => _danhSachTrangThai;
            set { _danhSachTrangThai = value; OnPropertyChanged(); }
        }

        private string _trangThaiChon;
        public string TrangThaiChon
        {
            get => _trangThaiChon;
            set { _trangThaiChon = value; OnPropertyChanged(); }
        }
        private ObservableCollection<string> _danhSachPhuongThuc;
        public ObservableCollection<string> DanhSachPhuongThuc
        {
            get => _danhSachPhuongThuc;
            set { _danhSachPhuongThuc = value; OnPropertyChanged(); }
        }

        private DateTime? _tuNgay;
        public DateTime? TuNgay
        {
            get => _tuNgay;
            set { _tuNgay = value; OnPropertyChanged(); }
        }

        private DateTime? _denNgay;
        public DateTime? DenNgay
        {
            get => _denNgay;
            set { _denNgay = value; OnPropertyChanged(); }
        }

        private int _tongSoHoaDon;
        public int TongSoHoaDon { get => _tongSoHoaDon; set { _tongSoHoaDon = value; OnPropertyChanged(); } }

        private long _tongDoanhThu;
        public long TongDoanhThu { get => _tongDoanhThu; set { _tongDoanhThu = value; OnPropertyChanged(); } }

        private int _soHoaDonDaThanhToan;
        public int SoHoaDonDaThanhToan { get => _soHoaDonDaThanhToan; set { _soHoaDonDaThanhToan = value; OnPropertyChanged(); } }

        private int _soHoaDonChoXuLy;
        public int SoHoaDonChoXuLy { get => _soHoaDonChoXuLy; set { _soHoaDonChoXuLy = value; OnPropertyChanged(); } }
        private Visibility _thanhToanButtonVisibility = Visibility.Collapsed;
        public Visibility ThanhToanButtonVisibility { get => _thanhToanButtonVisibility; set { _thanhToanButtonVisibility = value; OnPropertyChanged(); } }

        // ── COMMANDS ─────────────────────────────────────────────────────────
        public ICommand TaoHoaDonCommand { get; private set; }
        public ICommand SuaHoaDonCommand { get; private set; }
        public ICommand InHoaDonCommand { get; private set; }
        public ICommand XoaHoaDonCommand { get; private set; }
        public ICommand LocHoaDonCommand { get; private set; }
        public ICommand ThanhToanHoaDonCommand { get; private set; }
        public ICommand ResetLocCommand { get; private set; }

        public HoaDonViewModel()
        {
            DanhSachTrangThai = new ObservableCollection<string>
            {
                "--Tất cả--", "Đã thanh toán", "Chưa thanh toán"
            };
            TrangThaiChon = "--Tất cả--";
            LocHoaDon();
            KhoiTaoCommands();
        }

        private void KhoiTaoCommands()
        {
            TaoHoaDonCommand = new RelayCommand<object>(p => true, p => ThucHienTaoHoaDon());
            SuaHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienSuaHoaDon());
            InHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienInHoaDon());
            XoaHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienXoaHoaDon());
            LocHoaDonCommand = new RelayCommand<object>(p => true, p => ThucHienLocHoaDon());
            ThanhToanHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienThanhToan());
            ResetLocCommand = new RelayCommand<object>(p => true, p => ThucHienResetLoc());
        }

        private void ThucHienTaoHoaDon()
        {
            try
            {
                var dialog = new ThemHoaDonDialog();
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    LuuHoaDonAnToan(dialog.HoaDonMoi, dialog.ChiTietHds);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienSuaHoaDon()
        {
            if (HoaDonChon.TrangThai == "Đã thanh toán")
            {
                MessageBox.Show("Hóa đơn đã thanh toán, không thể chỉnh sửa!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var dialog = new SuaHoaDonDialog(HoaDonChon.MaHoaDon);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true) TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienInHoaDon()
        {
            var hd = HoaDonChon;
            string content = $"HÓA ĐƠN BÁN HÀNG\nMã HD: {hd.MaHoaDon}\nNgày: {hd.NgayTao:dd/MM/yyyy}\nKhách hàng: {hd.TenKhachHang}\nSĐT: {hd.SoDienThoai}\n------------------------------------------\nSản phẩm\tSL\tĐơn giá\n";

            foreach (var item in ChiTietSanPham)
            {
                content += $"{item.TenSanPham}\t{item.SoLuong}\t{item.DonGia:N0}\n";
            }
            content += $"------------------------------------------\nTỔNG TIỀN: {hd.TongTien:N0} VNĐ";

            var sfd = new Microsoft.Win32.SaveFileDialog { FileName = $"HoaDon_{hd.MaHoaDon}.txt", Filter = "Text File (*.txt)|*.txt" };
            if (sfd.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(sfd.FileName, content);
                MessageBox.Show("Đã xuất hóa đơn thành công!", "In hóa đơn");
            }
        }

        private async void ThucHienThanhToan()
        {
            var res = MessageBox.Show($"Xác nhận thanh toán TIỀN MẶT cho hóa đơn [{HoaDonChon.MaHoaDon}]?", "Thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = DataProvider.Ins.GetContext();
                    await db.Procedures.sp_ThanhToanHoaDonAsync(HoaDonChon.MaHoaDon);

                    MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    TaiDuLieu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thanh toán từ CSDL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void ThucHienXoaHoaDon()
        {
            var res = MessageBox.Show($"Bạn có chắc muốn xóa hóa đơn [{HoaDonChon.MaHoaDon}]?\nHàng tồn kho sẽ được hoàn trả lại.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    using var db = DataProvider.Ins.GetContext();
                    await db.Procedures.sp_XoaHoaDonAsync(HoaDonChon.MaHoaDon);

                    MessageBox.Show("Xóa hóa đơn thành công! Đã hoàn trả hàng.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    TaiDuLieu();
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }
        private void ThucHienLocHoaDon()
        {
            LocHoaDon();
        }
        private void ThucHienResetLoc()
        {
            TuKhoanTimKiem = string.Empty;
            TrangThaiChon = "--Tất cả--";
            TuNgay = null;
            DenNgay = null;
            LocHoaDon();
        }
        // ── LOAD DỮ LIỆU & ÉP KIỂU DECIMAL ──────────────────────
        public void TaiDuLieu()
        {
            LocHoaDon();
        }

        private async void TaiChiTietSanPham(string maHoaDon)
        {
            if (string.IsNullOrEmpty(maHoaDon))
            {
                ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>();
                return;
            }
            try
            {
                using var db = DataProvider.Ins.GetContext();

                var result = await db.Procedures.sp_TaiChiTietSPAsync(maHoaDon);

                var chiTiet = result.Select(ct => new ChiTietSanPhamDisplay
                {
                    TenSanPham = ct.TenSanPham ?? "Linh kiện ẩn",
                    SoLuong = ct.SoLuong,
                    DonGia = (decimal?)ct.DonGia,
                    HanBaoHanhHienThi = (ct.HanBaoHanh != null) ? "Có" : "Không có"
                }).ToList();

                ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>(chiTiet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết sản phẩm: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LocHoaDon()
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();

                string tuKhoa = string.IsNullOrWhiteSpace(TuKhoanTimKiem) ? "" : TuKhoanTimKiem.Trim();
                string trangThai = (TrangThaiChon == "--Tất cả--" || string.IsNullOrEmpty(TrangThaiChon)) ? "" : TrangThaiChon;

                DateOnly? tuNgayParam = TuNgay.HasValue ? DateOnly.FromDateTime(TuNgay.Value) : null;
                DateOnly? denNgayParam = DenNgay.HasValue ? DateOnly.FromDateTime(DenNgay.Value) : null;

                var resultList = await db.Procedures.sp_locdanhsachhoadonAsync(tuKhoa, trangThai, tuNgayParam, denNgayParam);


                var listDisplay = resultList.Select(hd => new HoaDonDisplay
                {
                    MaHoaDon = hd.mahd,
                    TenKhachHang = hd.tenkh,
                    TenNhanVien = hd.tennv,
                    NgayTao = hd.ngayhd.HasValue ? hd.ngayhd.Value.ToDateTime(TimeOnly.MinValue) : null,
                    TongTien = (decimal?)hd.tongtien,
                    TamTinh = (decimal?)hd.tongtien,
                    PhuongThucThanhToan = hd.phuongthucthanhtoan ?? "Tiền mặt",
                    TrangThai = hd.trangthai ?? "Chưa thanh toán",

                    TrangThaiMauNen = hd.trangthai == "Đã thanh toán" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8f5e9")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff3e0")),
                    TrangThaiMauChu = hd.trangthai == "Đã thanh toán" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2e7d32")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e65100"))
                }).ToList();

                DanhSachHoaDon = new ObservableCollection<HoaDonDisplay>(listDisplay);

                await CapNhatThongKe();

                HoaDonChon = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc dữ liệu từ SQL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ApplySearch(string keyword)
        {
            TuKhoanTimKiem = keyword?.Trim() ?? string.Empty;
            LocHoaDon();
        }

        private async Task CapNhatThongKe()
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();

                var TongHD = new OutputParameter<int?>();
                var TongDT = new OutputParameter<decimal?>();
                var DaTT = new OutputParameter<int?>();
                var ChuaTT = new OutputParameter<int?>();

                await db.Procedures.sp_ThongKeAsync(TongHD, TongDT, DaTT, ChuaTT);

                TongSoHoaDon = TongHD.Value ?? 0;
                TongDoanhThu = (long)(TongDT.Value ?? 0);
                SoHoaDonDaThanhToan = DaTT.Value ?? 0;
                SoHoaDonChoXuLy = ChuaTT.Value ?? 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải thống kê từ SQL: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task LuuHoaDonAnToan(HoaDon hoaDonMoi, List<ChiTietHd> dsSanPham)
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();
                var ngayLap = hoaDonMoi.NgayHd ?? DateOnly.FromDateTime(DateTime.Now);

                string maHD = null;

                foreach (var monHang in dsSanPham)
                {
                    var MaSP = new OutputParameter<string> { _value = maHD };

                    await db.Procedures.sp_BanLinhKienAsync(
                        ngayLap,
                        hoaDonMoi.MaKh,
                        hoaDonMoi.MaNv,
                        monHang.MaLk,
                        monHang.SoLuong,
                        MaSP
                    );

                    maHD = MaSP.Value;
                }

                MessageBox.Show("Thành công! Mã hóa đơn: " + maHD, "Thông báo");
                TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Báo lỗi");
            }
        }
    }
}