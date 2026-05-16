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
        public string TuKhoanTimKiem { get => _tuKhoanTimKiem; set { _tuKhoanTimKiem = value; OnPropertyChanged(); } }

        private ObservableCollection<string> _danhSachTrangThai;
        public ObservableCollection<string> DanhSachTrangThai { get => _danhSachTrangThai; set { _danhSachTrangThai = value; OnPropertyChanged(); } }

        private string _trangThaiChon;
        public string TrangThaiChon { get => _trangThaiChon; set { _trangThaiChon = value; OnPropertyChanged(); } }

        private DateTime? _tuNgay;
        public DateTime? TuNgay { get => _tuNgay; set { _tuNgay = value; OnPropertyChanged(); } }

        private DateTime? _denNgay;
        public DateTime? DenNgay { get => _denNgay; set { _denNgay = value; OnPropertyChanged(); } }

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

        public HoaDonViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        private void KhoiTaoCommands()
        {
            TaoHoaDonCommand = new RelayCommand<object>(p => true, p => ThucHienTaoHoaDon());
            SuaHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienSuaHoaDon());
            InHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienInHoaDon());
            XoaHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienXoaHoaDon());
            LocHoaDonCommand = new RelayCommand<object>(p => true, p => LocHoaDon());
            ThanhToanHoaDonCommand = new RelayCommand<object>(p => HoaDonChon != null, p => ThucHienThanhToan());
        }

        // ── HÀM TẠO MÃ TỰ ĐỘNG NỘI BỘ (THAY THẾ SERVICE) ─────────────────────
        private string TaoMaTuDong(string tienTo, string maCu)
        {
            if (string.IsNullOrEmpty(maCu)) return tienTo + "001";
            string soCuStr = maCu.Substring(tienTo.Length);
            if (int.TryParse(soCuStr, out int soCu))
            {
                return tienTo + (soCu + 1).ToString("D3");
            }
            return tienTo + "001";
        }

        private void ThucHienTaoHoaDon()
        {
            try
            {
                var dbRead = DataProvider.Ins.GetContext();
                var lastID = dbRead.HoaDons
                    .OrderByDescending(x => x.MaHd)
                    .Select(x => x.MaHd).FirstOrDefault();

                string newID = TaoMaTuDong("HD", lastID);

                var dialog = new ThemHoaDonDialog(newID);
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

        private void ThucHienThanhToan()
        {
            var res = MessageBox.Show($"Xác nhận thanh toán TIỀN MẶT cho hóa đơn [{HoaDonChon.MaHoaDon}]?", "Thanh toán", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    var db = DataProvider.Ins.GetContext();
                    var hd = db.HoaDons.Find(HoaDonChon.MaHoaDon);
                    if (hd != null)
                    {
                        hd.TrangThai = "Đã thanh toán";
                        hd.PhuongThucThanhToan = "Tiền mặt";
                        hd.NgayThanhToan = DateOnly.FromDateTime(DateTime.Now);
                        db.SaveChanges();

                        MessageBox.Show("Thanh toán thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                        TaiDuLieu();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi thanh toán: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ThucHienXoaHoaDon()
        {
            var res = MessageBox.Show($"Bạn có chắc muốn xóa hóa đơn [{HoaDonChon.MaHoaDon}]?\nHàng tồn kho sẽ được hoàn trả lại.", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
            {
                try
                {
                    var db = DataProvider.Ins.GetContext();
                    var entity = db.HoaDons.Include(h => h.ChiTietHds).FirstOrDefault(h => h.MaHd == HoaDonChon.MaHoaDon);
                    if (entity == null) return;

                    using (var giaoDich = db.Database.BeginTransaction())
                    {
                        foreach (var chiTiet in entity.ChiTietHds)
                        {
                            var linhKien = db.LinhKiens.Find(chiTiet.MaLk);
                            if (linhKien != null) linhKien.SoLuongTon += chiTiet.SoLuong;
                        }
                        db.ChiTietHds.RemoveRange(entity.ChiTietHds);
                        db.HoaDons.Remove(entity);
                        db.SaveChanges();
                        giaoDich.Commit();

                        TaiDuLieu();
                        MessageBox.Show("Xóa hóa đơn thành công! Đã hoàn trả hàng.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        // ── LOAD DỮ LIỆU & ÉP KIỂU DECIMAL ──────────────────────
        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                var list = db.HoaDons.AsNoTracking().Include(hd => hd.MaKhNavigation).Include(hd => hd.MaNvNavigation)
                             .OrderByDescending(hd => hd.NgayHd).ToList().Select(hd => MapToDisplay(hd)).ToList();

                _all = new ObservableCollection<HoaDonDisplay>(list);
                DanhSachHoaDon = new ObservableCollection<HoaDonDisplay>(list);
                CapNhatThongKe(_all);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void TaiChiTietSanPham(string maHoaDon)
        {
            if (string.IsNullOrEmpty(maHoaDon)) { ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>(); return; }
            try
            {
                var db = DataProvider.Ins.GetContext();
                var chiTiet = db.ChiTietHds.AsNoTracking().Include(ct => ct.MaLkNavigation).Where(ct => ct.MaHd == maHoaDon).ToList()
                    .Select(ct => new ChiTietSanPhamDisplay
                    {
                        TenSanPham = ct.MaLkNavigation?.TenLk ?? "Linh kiện ẩn",
                        SoLuong = ct.SoLuong,
                        DonGia = (decimal?)ct.DonGia,
                        HanBaoHanhHienThi = (ct.MaLkNavigation?.Tgbh != null) ? "Có" : "Không có"
                    }).ToList();
                ChiTietSanPham = new ObservableCollection<ChiTietSanPhamDisplay>(chiTiet);
            }
            catch { }
        }

        private static HoaDonDisplay MapToDisplay(HoaDon hd)
        {
            string trangThai = hd.TrangThai ?? "Chưa thanh toán";
            return new HoaDonDisplay
            {
                MaHoaDon = hd.MaHd,
                TenKhachHang = hd.MaKhNavigation?.TenKh ?? hd.MaKh,
                TenNhanVien = hd.MaNvNavigation?.TenNv ?? hd.MaNv,
                NgayTao = hd.NgayHd.HasValue ? hd.NgayHd.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                TongTien = (decimal?)hd.TongTien,
                TamTinh = (decimal?)hd.TongTien,
                GiamGia = 0,
                TrangThai = trangThai,
                TrangThaiMauNen = trangThai == "Đã thanh toán" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e8f5e9")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff3e0")),
                TrangThaiMauChu = trangThai == "Đã thanh toán" ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2e7d32")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e65100")),
                SoDienThoai = hd.MaKhNavigation?.Sdt ?? "",
                PhuongThucThanhToan = hd.PhuongThucThanhToan ?? "Tiền mặt"
            };
        }

        private void LocHoaDon() { /* Viết nội dung lọc vào đây nếu cần */ }
        public void ApplySearch(string keyword) { TuKhoanTimKiem = keyword?.Trim() ?? string.Empty; LocHoaDon(); }

        private void CapNhatThongKe(IEnumerable<HoaDonDisplay> ds)
        {
            TongSoHoaDon = ds.Count();
            TongDoanhThu = (long)ds.Where(hd => hd.TrangThai == "Đã thanh toán").Sum(hd => hd.TongTien ?? 0);
            SoHoaDonDaThanhToan = ds.Count(hd => hd.TrangThai == "Đã thanh toán");
            SoHoaDonChoXuLy = ds.Count(hd => hd.TrangThai == "Chưa thanh toán");
        }
        private async void LuuHoaDonAnToan(HoaDon hoaDonMoi, List<ChiTietHd> chiTietHds)
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                DateOnly ngayLapHD = hoaDonMoi.NgayHd ?? DateOnly.FromDateTime(DateTime.Now);
                foreach (var ct in chiTietHds)
                {
                    await db.Procedures.sp_BanLinhKienAsync(
                        hoaDonMoi.MaHd,
                        ngayLapHD,
                        hoaDonMoi.MaKh,
                        hoaDonMoi.MaNv,
                        ct.MaLk,
                        ct.SoLuong 
                    );
                }

                MessageBox.Show($"Tạo thành công hóa đơn [{hoaDonMoi.MaHd}] thông qua Stored Procedure!",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống kho khi thực hiện bán hàng:\n" + ex.Message,
                                "Lỗi SQL Server", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}