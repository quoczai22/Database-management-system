using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    // Item giỏ hàng tạm thời
    public class GioHangItem
    {
        public string MaLk { get; set; }
        public string TenLk { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
        public int TonKho { get; set; }
    }

    public partial class ThemHoaDonDialog : Window
    {
        public HoaDon HoaDonMoi { get; private set; }
        public List<ChiTietHd> ChiTietHds { get; private set; }

        private ObservableCollection<GioHangItem> _gioHang = new();

        public ThemHoaDonDialog(string maHdMoi)
        {
            InitializeComponent();
            TxtMaHd.Text = maHdMoi;
            TaiDuLieu();
            DgGioHang.ItemsSource = _gioHang;
        }

        private void TaiDuLieu()
        {
            var db = DataProvider.Ins.GetContext();

            // Load khách hàng
            CboKhachHang.ItemsSource = db.KhachHangs.AsNoTracking().OrderBy(kh => kh.TenKh).ToList();

            // Load nhân viên đang làm việc
            CboNhanVien.ItemsSource = db.NhanViens.AsNoTracking()
                .Where(nv => nv.DaNghiViec == false)
                .OrderBy(nv => nv.TenNv)
                .ToList();

            // Load linh kiện còn hàng
            CboLinhKien.ItemsSource = db.LinhKiens.AsNoTracking()
                .Where(lk => lk.NgungKinhDoanh == false && lk.SoLuongTon > 0)
                .OrderBy(lk => lk.TenLk)
                .ToList();
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
                TxtSoLuong.Focus();
                return;
            }

            // Kiểm tra tồn kho
            int tonKho = lk.SoLuongTon ?? 0;
            var existItem = _gioHang.FirstOrDefault(g => g.MaLk == lk.MaLk);
            //int soLuongDaChon = existItem?.SoLuong ?? 0;
            //if (soLuongDaChon + soLuong > tonKho)
            //{
            //    MessageBox.Show($"Linh kiện '{lk.TenLk}' chỉ còn {tonKho} cái trong kho (đã chọn {soLuongDaChon})!",
            //        "Không đủ hàng", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}

            if (existItem != null)
            {
                // Đã có trong giỏ → tăng số lượng
                existItem.SoLuong += soLuong;
                DgGioHang.Items.Refresh();
            }
            else
            {
                _gioHang.Add(new GioHangItem
                {
                    MaLk = lk.MaLk,
                    TenLk = lk.TenLk,
                    SoLuong = soLuong,
                    DonGia = lk.DonGiaBan ?? 0,
                    TonKho = tonKho
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
                MessageBox.Show("Vui lòng thêm ít nhất một sản phẩm vào hóa đơn!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string maHd = TxtMaHd.Text.Trim();
            decimal tongTien = _gioHang.Sum(g => g.ThanhTien);

            string phuongThuc = "Tiền mặt";
            if (CboPhuongThuc.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem)
            {
                phuongThuc = selectedItem.Content.ToString();
            }

            HoaDonMoi = new HoaDon
            {
                MaHd = maHd,
                NgayHd = DateOnly.FromDateTime(DateTime.Now),
                MaKh = kh.MaKh,
                MaNv = nv.MaNv,
                TongTien = tongTien,
                TrangThai = "Chưa thanh toán",
                PhuongThucThanhToan = phuongThuc
            };

            ChiTietHds = _gioHang.Select(g => new ChiTietHd
            {
                MaHd = maHd,
                MaLk = g.MaLk,
                SoLuong = (byte)g.SoLuong,
                DonGia = g.DonGia
            }).ToList();

            DialogResult = true;
            Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
