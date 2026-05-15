using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class SuaHoaDonDialog : Window
    {
        private readonly string _maHd;
        private readonly HoaDon _hoaDonGoc;
        // Danh sách chi tiết cũ (để hoàn kho khi cần)
        private readonly List<ChiTietHd> _chiTietCu;
        // Giỏ hàng mới
        private ObservableCollection<GioHangItem> _gioHang = new();

        public SuaHoaDonDialog(string maHd)
        {
            InitializeComponent();
            _maHd = maHd;
            TxtMaHdTitle.Text = $"Mã hóa đơn: {maHd} | Trạng thái: Chưa thanh toán";

            var db = DataProvider.Ins.GetContext();
            _hoaDonGoc = db.HoaDons
                .Include(h => h.ChiTietHds)
                .FirstOrDefault(h => h.MaHd == maHd);

            _chiTietCu = _hoaDonGoc?.ChiTietHds.ToList() ?? new List<ChiTietHd>();

            TaiDuLieu();
            TaiChiTietHienTai();
            DgGioHang.ItemsSource = _gioHang;
        }

        private void TaiDuLieu()
        {
            var db = DataProvider.Ins.GetContext();

            var danhSachKh = db.KhachHangs.AsNoTracking().OrderBy(kh => kh.TenKh).ToList();
            CboKhachHang.ItemsSource = danhSachKh;

            var danhSachNv = db.NhanViens.AsNoTracking()
                .Where(nv => nv.DaNghiViec == false)
                .OrderBy(nv => nv.TenNv)
                .ToList();
            CboNhanVien.ItemsSource = danhSachNv;

            CboLinhKien.ItemsSource = db.LinhKiens.AsNoTracking()
                .Where(lk => lk.NgungKinhDoanh == false)
                .OrderBy(lk => lk.TenLk)
                .ToList();

            // Select khách hàng và nhân viên hiện tại
            if (_hoaDonGoc != null)
            {
                CboKhachHang.SelectedItem = danhSachKh.FirstOrDefault(kh => kh.MaKh == _hoaDonGoc.MaKh);
                CboNhanVien.SelectedItem = danhSachNv.FirstOrDefault(nv => nv.MaNv == _hoaDonGoc.MaNv);
            }
        }

        private void TaiChiTietHienTai()
        {
            if (_hoaDonGoc == null) return;

            var db = DataProvider.Ins.GetContext();
            foreach (var ct in _chiTietCu)
            {
                var lk = db.LinhKiens.AsNoTracking().FirstOrDefault(l => l.MaLk == ct.MaLk);
                if (lk != null)
                {
                    _gioHang.Add(new GioHangItem
                    {
                        MaLk = ct.MaLk,
                        TenLk = lk.TenLk,
                        SoLuong = ct.SoLuong ?? 1,
                        DonGia = (int)(ct.DonGia ?? lk.DonGiaBan ?? 0),
                        // Tồn kho thực tế = tồn hiện tại + số lượng đang có trong HD này
                        TonKho = (lk.SoLuongTon ?? 0) + (ct.SoLuong ?? 0)
                    });
                }
            }
            CapNhatTongTien();
        }

        private void BtnThemVaoGio_Click(object sender, RoutedEventArgs e)
        {
            if (CboLinhKien.SelectedItem is not LinhKien lk)
            {
                MessageBox.Show("Vui lòng chọn linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng phải là số nguyên dương!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existItem = _gioHang.FirstOrDefault(g => g.MaLk == lk.MaLk);
            // Tồn kho có hiệu = tồn hiện tại + số lượng trong HD cũ (nếu đã có)
            int chiTietCuSoLuong = _chiTietCu.FirstOrDefault(ct => ct.MaLk == lk.MaLk)?.SoLuong ?? 0;
            int tonKhoHieuLuc = (lk.SoLuongTon ?? 0) + chiTietCuSoLuong;

            int soLuongDaChon = existItem?.SoLuong ?? 0;
            if (soLuong > tonKhoHieuLuc)
            {
                MessageBox.Show($"Linh kiện '{lk.TenLk}' chỉ có {tonKhoHieuLuc} cái khả dụng!",
                    "Không đủ hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (existItem != null)
            {
                existItem.SoLuong = soLuong; // Thay thế, không cộng thêm
                DgGioHang.Items.Refresh();
            }
            else
            {
                _gioHang.Add(new GioHangItem
                {
                    MaLk = lk.MaLk,
                    TenLk = lk.TenLk,
                    SoLuong = soLuong,
                    DonGia = (int)(lk.DonGiaBan ?? 0),
                    TonKho = tonKhoHieuLuc
                });
            }

            CapNhatTongTien();
        }

        private void BtnXoaKhoiGio_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.DataContext is GioHangItem item)
            {
                _gioHang.Remove(item);
                CapNhatTongTien();
            }
        }

        private void CapNhatTongTien()
        {
            long tong = _gioHang.Sum(g => (long)g.ThanhTien);
            TxtTongTien.Text = $"{tong:N0} ₫";
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (CboKhachHang.SelectedItem is not KhachHang kh)
            {
                MessageBox.Show("Vui lòng chọn khách hàng!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CboNhanVien.SelectedItem is not NhanVien nv)
            {
                MessageBox.Show("Vui lòng chọn nhân viên!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_gioHang.Count == 0)
            {
                MessageBox.Show("Hóa đơn phải có ít nhất một sản phẩm!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var db = DataProvider.Ins.GetContext();
            using var giaoDich = db.Database.BeginTransaction();

            try
            {
                var entity = db.HoaDons.Include(h => h.ChiTietHds).FirstOrDefault(h => h.MaHd == _maHd);
                if (entity == null)
                {
                    MessageBox.Show("Không tìm thấy hóa đơn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Hoàn kho của chi tiết cũ
                foreach (var ct in entity.ChiTietHds.ToList())
                {
                    var linhKien = db.LinhKiens.Find(ct.MaLk);
                    if (linhKien != null)
                        linhKien.SoLuongTon += ct.SoLuong;
                }

                // Xóa chi tiết cũ
                db.ChiTietHds.RemoveRange(entity.ChiTietHds);

                // Cập nhật thông tin hóa đơn
                entity.MaKh = kh.MaKh;
                entity.MaNv = nv.MaNv;
                entity.TongTien = _gioHang.Sum(g => g.ThanhTien);

                // Thêm chi tiết mới + trừ kho
                foreach (var item in _gioHang)
                {
                    var linhKien = db.LinhKiens.Find(item.MaLk);
                    if (linhKien == null || linhKien.SoLuongTon < item.SoLuong)
                        throw new Exception($"Linh kiện '{item.TenLk}' không đủ hàng trong kho!");

                    linhKien.SoLuongTon -= item.SoLuong;

                    db.ChiTietHds.Add(new ChiTietHd
                    {
                        MaHd = _maHd,
                        MaLk = item.MaLk,
                        SoLuong = (byte)item.SoLuong,
                        DonGia = item.DonGia
                    });
                }

                db.SaveChanges();
                giaoDich.Commit();

                MessageBox.Show("Cập nhật hóa đơn thành công!", "Thành công",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                giaoDich.Rollback();
                MessageBox.Show("Lỗi khi sửa hóa đơn: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
