using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.ViewModels;
using System;
using System.Linq;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class ThemSuaLinhKienDialog : Window
    {
        public string MaLk { get; private set; }
        public string TenLk { get; private set; }
        public string MaLoai { get; private set; }
        public string MaNsx { get; private set; }
        public string Dvt { get; private set; }
        public byte? Tgbh { get; private set; }
        public int? DonGiaBan { get; private set; }
        public int? SoLuongTon { get; private set; }
        public DateOnly? NgayNhap { get; private set; }

        private readonly bool _laMoiThem;

        /// <summary>Mở ở chế độ THÊM</summary>
        public ThemSuaLinhKienDialog(string maLkGoiY)
        {
            InitializeComponent();
            _laMoiThem = true;
            TitleText.Text = "Thêm Linh Kiện";
            TxtMaLk.Text = maLkGoiY;
            TxtMaLkHint.Visibility = Visibility.Visible;
            DpNgayNhap.SelectedDate = DateTime.Now;
            TaiDanhSachComboBox();
        }

        /// <summary>Mở ở chế độ SỬA</summary>
        public ThemSuaLinhKienDialog(LinhKienDisplay lk)
        {
            InitializeComponent();
            _laMoiThem = false;
            TitleText.Text = "Sửa Linh Kiện";
            BtnLuu.Content = "Cập nhật";
            TxtMaLk.Text = lk.MaLk;
            TxtMaLk.IsReadOnly = true;
            TxtMaLk.Opacity = 0.6;
            TxtMaLkHint.Visibility = Visibility.Collapsed;
            TxtTenLk.Text = lk.TenLk;
            TxtDvt.Text = lk.Dvt;
            TxtTgbh.Text = lk.Tgbh?.ToString();
            if (lk.NgayNhap.HasValue)
                DpNgayNhap.SelectedDate = lk.NgayNhap.Value.ToDateTime(TimeOnly.MinValue);

            TaiDanhSachComboBox();

            // Load thêm thông tin từ DB cho Sửa
            var entity = DataProvider.Ins.GetContext().LinhKiens.AsNoTracking().FirstOrDefault(x => x.MaLk == lk.MaLk);
            if (entity != null)
            {
                TxtDonGia.Text = entity.DonGiaBan?.ToString();
                TxtSoLuong.Text = entity.SoLuongTon?.ToString();

                // Select đúng item trong ComboBox
                for (int i = 0; i < CboLoai.Items.Count; i++)
                {
                    if (((LoaiLk)CboLoai.Items[i]).MaLoai == entity.MaLoai)
                    { CboLoai.SelectedIndex = i; break; }
                }
                for (int i = 0; i < CboNsx.Items.Count; i++)
                {
                    if (((NhaSanXuat)CboNsx.Items[i]).MaNsx == entity.MaNsx)
                    { CboNsx.SelectedIndex = i; break; }
                }
            }
        }

        private void TaiDanhSachComboBox()
        {
            var db = DataProvider.Ins.GetContext();
            CboLoai.ItemsSource = db.LoaiLks.AsNoTracking().OrderBy(l => l.TenLoai).ToList();
            CboNsx.ItemsSource = db.NhaSanXuats.AsNoTracking().OrderBy(n => n.TenNsx).ToList();
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            // Validate MaLk
            string maLk = TxtMaLk.Text.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(maLk))
            {
                MessageBox.Show("Vui lòng nhập mã linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMaLk.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTenLk.Text))
            {
                MessageBox.Show("Vui lòng nhập tên linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenLk.Focus();
                return;
            }
            if (CboLoai.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn loại linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CboNsx.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhà sản xuất!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate số liệu
            if (!string.IsNullOrWhiteSpace(TxtDonGia.Text) && !int.TryParse(TxtDonGia.Text, out _))
            {
                MessageBox.Show("Đơn giá bán phải là số nguyên!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDonGia.Focus();
                return;
            }
            if (!string.IsNullOrWhiteSpace(TxtSoLuong.Text) && !int.TryParse(TxtSoLuong.Text, out _))
            {
                MessageBox.Show("Số lượng tồn phải là số nguyên!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtSoLuong.Focus();
                return;
            }
            if (!string.IsNullOrWhiteSpace(TxtTgbh.Text) && !byte.TryParse(TxtTgbh.Text, out _))
            {
                MessageBox.Show("Thời gian bảo hành phải là số nguyên (0-255)!", "Dữ liệu không hợp lệ",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTgbh.Focus();
                return;
            }

            // Kiểm tra trùng mã khi thêm mới
            if (_laMoiThem)
            {
                bool trung = DataProvider.Ins.GetContext().LinhKiens
                    .AsNoTracking()
                    .Any(lk => lk.MaLk == maLk);
                if (trung)
                {
                    MessageBox.Show($"Mã linh kiện '{maLk}' đã tồn tại! Vui lòng nhập mã khác.",
                        "Trùng mã", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMaLk.Focus();
                    return;
                }
            }

            MaLk = maLk;
            TenLk = TxtTenLk.Text.Trim();
            MaLoai = ((LoaiLk)CboLoai.SelectedItem).MaLoai;
            MaNsx = ((NhaSanXuat)CboNsx.SelectedItem).MaNsx;
            Dvt = TxtDvt.Text.Trim();

            if (byte.TryParse(TxtTgbh.Text, out byte tgbh))
                Tgbh = tgbh;
            if (int.TryParse(TxtDonGia.Text, out int donGia))
                DonGiaBan = donGia;
            if (int.TryParse(TxtSoLuong.Text, out int soLuong))
                SoLuongTon = soLuong;
            if (DpNgayNhap.SelectedDate.HasValue)
                NgayNhap = DateOnly.FromDateTime(DpNgayNhap.SelectedDate.Value);

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
