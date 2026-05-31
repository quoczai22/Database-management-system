using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    public class PhieuNhapTamItem
    {
        public string MaLk { get; set; }
        public string TenLk { get; set; }
        public int SoLuongNhap { get; set; }
        public int DonGiaNhap { get; set; }
        public int ThanhTien => SoLuongNhap * DonGiaNhap;
    }

    public partial class ThemPhieuNhapDialog : Window
    {
        public DateOnly NgayNhap { get; private set; }
        public string MaNv { get; private set; }
        public string MaNsx { get; private set; }
        public List<PhieuNhapTamItem> ChiTietNhap { get; private set; } = new();

        private readonly ObservableCollection<PhieuNhapTamItem> _chiTietTam = new();
        private List<LinhKien> _linhKiens = new();
        private string _maNsxDangNhap;

        public ThemPhieuNhapDialog()
        {
            InitializeComponent();
            DpNgayNhap.SelectedDate = DateTime.Today;
            DgPhieuNhap.ItemsSource = _chiTietTam;
            TaiDuLieu();
        }

        private void TaiDuLieu()
        {
            using var db = DataProvider.Ins.GetContext();

            CboNhaSanXuat.ItemsSource = db.NhaSanXuats.AsNoTracking()
                .OrderBy(nsx => nsx.TenNsx)
                .ToList();

            CboNhanVien.ItemsSource = db.NhanViens.AsNoTracking()
                .Where(nv => nv.DaNghiViec == false && (nv.ChucVu == "Nhân viên kho" || nv.ChucVu == "Quản lý"))
                .OrderBy(nv => nv.TenNv)
                .ToList();

            _linhKiens = db.LinhKiens.AsNoTracking()
                .Where(lk => lk.NgungKinhDoanh == false)
                .OrderBy(lk => lk.TenLk)
                .ToList();
        }

        private void CboNhaSanXuat_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CboNhaSanXuat.SelectedItem is not NhaSanXuat nsx)
            {
                CboLinhKien.ItemsSource = null;
                CboLinhKien.IsEnabled = false;
                TxtGoiYLinhKien.Text = "Chọn nhà sản xuất trước để tải danh sách linh kiện.";
                return;
            }

            if (_chiTietTam.Count > 0 && _maNsxDangNhap != nsx.MaNsx)
            {
                var result = MessageBox.Show(
                    "Đổi nhà sản xuất sẽ xóa danh sách linh kiện đang nhập. Bạn muốn tiếp tục?",
                    "Đổi nhà sản xuất",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    var nsxCu = CboNhaSanXuat.Items.OfType<NhaSanXuat>()
                        .FirstOrDefault(item => item.MaNsx == _maNsxDangNhap);

                    CboNhaSanXuat.SelectionChanged -= CboNhaSanXuat_SelectionChanged;
                    CboNhaSanXuat.SelectedItem = nsxCu;
                    CboNhaSanXuat.SelectionChanged += CboNhaSanXuat_SelectionChanged;
                    return;
                }

                _chiTietTam.Clear();
                CapNhatTongTien();
            }

            _maNsxDangNhap = nsx.MaNsx;
            var linhKienTheoNsx = _linhKiens.Where(lk => lk.MaNsx == nsx.MaNsx).ToList();

            CboLinhKien.ItemsSource = linhKienTheoNsx;
            CboLinhKien.IsEnabled = linhKienTheoNsx.Count > 0;
            CboLinhKien.SelectedIndex = linhKienTheoNsx.Count > 0 ? 0 : -1;
            TxtGoiYLinhKien.Text = linhKienTheoNsx.Count > 0
                ? $"Đang hiển thị {linhKienTheoNsx.Count} linh kiện của {nsx.TenNsx}."
                : "Nhà sản xuất này chưa có linh kiện đang kinh doanh.";
        }

        private void BtnThemVaoPhieu_Click(object sender, RoutedEventArgs e)
        {
            if (CboNhaSanXuat.SelectedItem is not NhaSanXuat)
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CboLinhKien.SelectedItem is not LinhKien lk)
            {
                MessageBox.Show("Vui lòng chọn linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(TxtSoLuong.Text.Trim(), out int soLuong) || soLuong <= 0)
            {
                MessageBox.Show("Số lượng nhập phải là số nguyên dương!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoLuong.Focus();
                return;
            }

            var donGiaText = TxtDonGiaNhap.Text.Trim().Replace(",", "").Replace(".", "");
            if (!int.TryParse(donGiaText, out int donGiaNhap) || donGiaNhap <= 0)
            {
                MessageBox.Show("Đơn giá nhập phải là số nguyên dương!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDonGiaNhap.Focus();
                return;
            }

            var item = _chiTietTam.FirstOrDefault(ct => ct.MaLk == lk.MaLk);
            if (item != null)
            {
                item.SoLuongNhap += soLuong;
                item.DonGiaNhap = donGiaNhap;
                DgPhieuNhap.Items.Refresh();
            }
            else
            {
                _chiTietTam.Add(new PhieuNhapTamItem
                {
                    MaLk = lk.MaLk,
                    TenLk = lk.TenLk,
                    SoLuongNhap = soLuong,
                    DonGiaNhap = donGiaNhap
                });
            }

            CapNhatTongTien();
        }

        private void BtnXoaKhoiPhieu_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.Button)?.DataContext is PhieuNhapTamItem item)
            {
                _chiTietTam.Remove(item);
                CapNhatTongTien();
            }
        }

        private void CapNhatTongTien()
        {
            long tong = _chiTietTam.Sum(ct => (long)ct.ThanhTien);
            TxtTongTien.Text = $"{tong:N0} ₫";
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (CboNhaSanXuat.SelectedItem is not NhaSanXuat nsx)
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CboNhanVien.SelectedItem is not NhanVien nv)
            {
                MessageBox.Show("Vui lòng chọn nhân viên nhập kho!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_chiTietTam.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một linh kiện vào phiếu nhập!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NgayNhap = DateOnly.FromDateTime(DpNgayNhap.SelectedDate ?? DateTime.Today);
            MaNv = nv.MaNv;
            MaNsx = nsx.MaNsx;
            ChiTietNhap = _chiTietTam.Select(ct => new PhieuNhapTamItem
            {
                MaLk = ct.MaLk,
                TenLk = ct.TenLk,
                SoLuongNhap = ct.SoLuongNhap,
                DonGiaNhap = ct.DonGiaNhap
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
