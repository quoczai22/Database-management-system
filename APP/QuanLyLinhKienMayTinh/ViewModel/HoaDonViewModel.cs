using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    // Display class cho danh sách hóa đơn (DataGrid trái)
    public class HoaDonDisplay
    {
        public string MaHoaDon { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime? NgayTao { get; set; }
        public int? TongTien { get; set; }
        public int? TamTinh { get; set; }
        public int? GiamGia { get; set; }
        public string TrangThai { get; set; }
        public Brush TrangThaiMauNen { get; set; }
        public Brush TrangThaiMauChu { get; set; }
        public string SoDienThoai { get; set; }
    }

    // Display class cho chi tiết sản phẩm (DataGrid trong panel phải)
    public class ChiTietSanPhamDisplay
    {
        public string TenSanPham { get; set; }
        public byte? SoLuong { get; set; }
        public int? DonGia { get; set; }
    }

    public class HoaDonViewModel : BaseViewModel, ISearchable
    {
        // ── Backing data ─────────────────────────────────────────────────────
        private ObservableCollection<HoaDonDisplay> _all;

        // ── Danh sách hóa đơn (DataGrid) ────────────────────────────────────
        private ObservableCollection<HoaDonDisplay> _danhSachHoaDon;
        public ObservableCollection<HoaDonDisplay> DanhSachHoaDon
        {
            get => _danhSachHoaDon;
            set { _danhSachHoaDon = value; OnPropertyChanged(); }
        }

        // ── Hóa đơn đang chọn ───────────────────────────────────────────────
        private HoaDonDisplay _hoaDonChon;
        public HoaDonDisplay HoaDonChon
        {
            get => _hoaDonChon;
            set
            {
                _hoaDonChon = value;
                OnPropertyChanged();
                // Cập nhật panel chi tiết
                ChiTietVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
                ChuaChonHoaDonVisibility = value == null ? Visibility.Visible : Visibility.Collapsed;
                TaiChiTietSanPham(value?.MaHoaDon);
            }
        }

        // ── Chi tiết sản phẩm trong hóa đơn ─────────────────────────────────
        private ObservableCollection<ChiTietSanPhamDisplay> _chiTietSanPham;
        public ObservableCollection<ChiTietSanPhamDisplay> ChiTietSanPham
        {
            get => _chiTietSanPham;
            set { _chiTietSanPham = value; OnPropertyChanged(); }
        }

        // ── Visibility panel chi tiết ────────────────────────────────────────
        private Visibility _chiTietVisibility = Visibility.Collapsed;
        public Visibility ChiTietVisibility
        {
            get => _chiTietVisibility;
            set { _chiTietVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _chuaChonHoaDonVisibility = Visibility.Visible;
        public Visibility ChuaChonHoaDonVisibility
        {
            get => _chuaChonHoaDonVisibility;
            set { _chuaChonHoaDonVisibility = value; OnPropertyChanged(); }
        }

        // ── Toolbar: search, filter trạng thái, lọc ngày ────────────────────
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

        // ── Footer thống kê ──────────────────────────────────────────────────
        private int _tongSoHoaDon;
        public int TongSoHoaDon
        {
            get => _tongSoHoaDon;
            set { _tongSoHoaDon = value; OnPropertyChanged(); }
        }

        private long _tongDoanhThu;
        public long TongDoanhThu
        {
            get => _tongDoanhThu;
            set { _tongDoanhThu = value; OnPropertyChanged(); }
        }

        private int _soHoaDonDaThanhToan;
        public int SoHoaDonDaThanhToan
        {
            get => _soHoaDonDaThanhToan;
            set { _soHoaDonDaThanhToan = value; OnPropertyChanged(); }
        }

        private int _soHoaDonChoXuLy;
        public int SoHoaDonChoXuLy
        {
            get => _soHoaDonChoXuLy;
            set { _soHoaDonChoXuLy = value; OnPropertyChanged(); }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand TaoHoaDonMoiCommand { get; private set; }
        public ICommand SuaHoaDonCommand { get; private set; }
        public ICommand InHoaDonCommand { get; private set; }
        public ICommand XoaHoaDonCommand { get; private set; }
        public ICommand LocHoaDonCommand { get; private set; }

        public HoaDonViewModel()
        {
            DanhSachTrangThai = new ObservableCollection<string>
            {
                "-- Tất cả --", "Đã thanh toán", "Chờ xử lý"
            };
            TrangThaiChon = "-- Tất cả --";

            TaiDuLieu();
            KhoiTaoCommands();
        }

        // ── Tải toàn bộ hóa đơn ─────────────────────────────────────────────
        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.DB;

                var list = db.HoaDons
                    .AsNoTracking()
                    .Include(hd => hd.MaKhNavigation)
                    .Include(hd => hd.MaNvNavigation)
                    .OrderByDescending(hd => hd.NgayHd)
                    .ToList()
                    .Select(hd => MapToDisplay(hd))
                    .ToList();

                _all = new ObservableCollection<HoaDonDisplay>(list);
                DanhSachHoaDon = new ObservableCollection<HoaDonDisplay>(list);

                CapNhatThongKe(_all);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu hóa đơn: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Tải chi tiết sản phẩm khi chọn hóa đơn ─────────────────────────
        private void TaiChiTietSanPham(string maHoaDon)
        {
            if (string.IsNullOrEmpty(maHoaDon))
            {
                ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>();
                return;
            }

            try
            {
                var db = DataProvider.Ins.DB;
                var chiTiet = db.ChiTietHds
                    .AsNoTracking()
                    .Include(ct => ct.MaLkNavigation)
                    .Where(ct => ct.MaHd == maHoaDon)
                    .Select(ct => new ChiTietSanPhamDisplay
                    {
                        TenSanPham = ct.MaLkNavigation.TenLk,
                        SoLuong = ct.SoLuong,
                        DonGia = ct.DonGia
                    }).ToList();

                ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>(chiTiet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết hóa đơn: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Map entity → display ─────────────────────────────────────────────
        private static HoaDonDisplay MapToDisplay(HoaDon hd)
        {
            // Xác định trạng thái: có tổng tiền > 0 → đã thanh toán
            bool daThanhToan = (hd.TongTien ?? 0) > 0;
            string trangThai = daThanhToan ? "Đã thanh toán" : "Chờ xử lý";

            var mauNen = daThanhToan
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8f5e9"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff3e0"));

            var mauChu = daThanhToan
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2e7d32"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e65100"));

            // Convert DateOnly? → DateTime?
            DateTime? ngayTao = hd.NgayHd.HasValue
                ? hd.NgayHd.Value.ToDateTime(TimeOnly.MinValue)
                : (DateTime?)null;

            return new HoaDonDisplay
            {
                MaHoaDon = hd.MaHd,
                TenKhachHang = hd.MaKhNavigation?.TenKh ?? hd.MaKh,
                TenNhanVien = hd.MaNvNavigation?.TenNv ?? hd.MaNv,
                NgayTao = ngayTao,
                TongTien = hd.TongTien,
                TamTinh = hd.TongTien,   // chưa có chiết khấu riêng
                GiamGia = 0,
                TrangThai = trangThai,
                TrangThaiMauNen = mauNen,
                TrangThaiMauChu = mauChu,
                SoDienThoai = hd.MaKhNavigation?.Dthoai ?? string.Empty
            };
        }

        // ── Lọc hóa đơn theo toolbar ─────────────────────────────────────────
        private void LocHoaDon()
        {
            var filtered = _all.AsEnumerable();

            // Lọc từ khóa
            if (!string.IsNullOrWhiteSpace(TuKhoanTimKiem))
            {
                string kw = TuKhoanTimKiem.ToLower();
                filtered = filtered.Where(hd =>
                    (hd.MaHoaDon?.ToLower().Contains(kw) ?? false) ||
                    (hd.TenKhachHang?.ToLower().Contains(kw) ?? false) ||
                    (hd.TenNhanVien?.ToLower().Contains(kw) ?? false));
            }

            // Lọc trạng thái
            if (!string.IsNullOrEmpty(TrangThaiChon) && TrangThaiChon != "-- Tất cả --")
                filtered = filtered.Where(hd => hd.TrangThai == TrangThaiChon);

            // Lọc theo ngày
            if (TuNgay.HasValue)
                filtered = filtered.Where(hd => hd.NgayTao >= TuNgay.Value);
            if (DenNgay.HasValue)
                filtered = filtered.Where(hd => hd.NgayTao <= DenNgay.Value.AddDays(1));

            var result = filtered.ToList();
            DanhSachHoaDon = new ObservableCollection<HoaDonDisplay>(result);
            CapNhatThongKe(DanhSachHoaDon);
            HoaDonChon = null;
        }

        // ── Cập nhật thống kê footer ─────────────────────────────────────────
        private void CapNhatThongKe(IEnumerable<HoaDonDisplay> ds)
        {
            var list = ds.ToList();
            TongSoHoaDon = list.Count;
            TongDoanhThu = list.Sum(hd => (long)(hd.TongTien ?? 0));
            SoHoaDonDaThanhToan = list.Count(hd => hd.TrangThai == "Đã thanh toán");
            SoHoaDonChoXuLy = list.Count(hd => hd.TrangThai == "Chờ xử lý");
        }

        // ── ISearchable ──────────────────────────────────────────────────────
        public void ApplySearch(string keyword)
        {
            TuKhoanTimKiem = keyword?.Trim() ?? string.Empty;
            LocHoaDon();
        }

        // ── Khởi tạo Commands ────────────────────────────────────────────────
        private void KhoiTaoCommands()
        {
            TaoHoaDonMoiCommand = new RelayCommand<object>(
                _ => true,
                _ => MessageBox.Show(
                    "Chức năng tạo hóa đơn mới đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            SuaHoaDonCommand = new RelayCommand<object>(
                _ => HoaDonChon != null,
                _ => MessageBox.Show(
                    $"Chức năng sửa hóa đơn [{HoaDonChon?.MaHoaDon}] đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            InHoaDonCommand = new RelayCommand<object>(
                _ => HoaDonChon != null,
                _ => MessageBox.Show(
                    $"Chức năng in hóa đơn [{HoaDonChon?.MaHoaDon}] đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            XoaHoaDonCommand = new RelayCommand<object>(
                _ => HoaDonChon != null,
                _ =>
                {
                    var hd = HoaDonChon;
                    if (hd == null) return;
                    var res = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa hóa đơn [{hd.MaHoaDon}]?\n" +
                        "Tất cả chi tiết hóa đơn cũng sẽ bị xóa.",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                        ThucHienXoa(hd);
                });

            LocHoaDonCommand = new RelayCommand<object>(
                _ => true,
                _ => LocHoaDon());
        }

        // ── Xóa hóa đơn ─────────────────────────────────────────────────────
        private void ThucHienXoa(HoaDonDisplay hd)
        {
            try
            {
                var db = DataProvider.Ins.DB;
                var entity = db.HoaDons
                    .Include(h => h.ChiTietHds)
                    .FirstOrDefault(h => h.MaHd == hd.MaHoaDon);
                if (entity == null) return;

                // Xóa chi tiết trước, sau đó xóa hóa đơn
                db.ChiTietHds.RemoveRange(entity.ChiTietHds);
                db.HoaDons.Remove(entity);
                db.SaveChanges();

                _all.Remove(hd);
                DanhSachHoaDon.Remove(hd);
                HoaDonChon = null;
                CapNhatThongKe(DanhSachHoaDon);

                MessageBox.Show("Xóa hóa đơn thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa hóa đơn: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}