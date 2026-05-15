using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class ThemSuaNhanVienDialog : Window
    {
        public string MaNv { get; private set; }
        public string HoTen { get; private set; }
        public string ChucVu { get; private set; }
        public string GioiTinh { get; private set; }
        public string Sdt { get; private set; }
        public string Email { get; private set; }
        public DateOnly? NgaySinh { get; private set; }
        public DateOnly? NgayVaoLam { get; private set; }

        /// <summary>Mở ở chế độ THÊM</summary>
        public ThemSuaNhanVienDialog(string maNvMoi)
        {
            InitializeComponent();
            TitleText.Text = "Thêm Nhân Viên";
            TxtMaNv.Text = maNvMoi;
            DpNgayVaoLam.SelectedDate = DateTime.Now;
            TaiDanhSachChucVu();
        }

        /// <summary>Mở ở chế độ SỬA</summary>
        public ThemSuaNhanVienDialog(NhanVienDisplay nv)
        {
            InitializeComponent();
            TitleText.Text = "Sửa Nhân Viên";
            BtnLuu.Content = "Cập nhật";
            TxtMaNv.Text = nv.MaNv;
            TxtMaNv.IsReadOnly = true;
            TxtMaNv.Opacity = 0.6;
            TxtHoTen.Text = nv.HoTen;
            TxtSdt.Text = nv.Sdt;
            TxtEmail.Text = nv.Email;

            if (nv.NgayVaoLam.HasValue)
                DpNgayVaoLam.SelectedDate = nv.NgayVaoLam.Value.ToDateTime(TimeOnly.MinValue);

            TaiDanhSachChucVu();

            // Set chức vụ sau khi đã load danh sách
            if (!string.IsNullOrEmpty(nv.ChucVu))
            {
                var chucVuItem = CboChucVu.Items.Cast<ChucVuItem>()
                    .FirstOrDefault(cv => cv.TenChucVu == nv.ChucVu);
                if (chucVuItem != null)
                    CboChucVu.SelectedItem = chucVuItem;
            }

            // Load thêm giới tính và ngày sinh từ DB
            using (var db = DataProvider.Ins.GetContext())
            {
                var entity = db.NhanViens.Find(nv.MaNv);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(entity.GioiTinh))
                    {
                        foreach (ComboBoxItem item in CboGioiTinh.Items)
                        {
                            if (item.Content.ToString() == entity.GioiTinh)
                            {
                                CboGioiTinh.SelectedItem = item;
                                break;
                            }
                        }
                    }
                    if (entity.NgaySinh.HasValue)
                        DpNgaySinh.SelectedDate = entity.NgaySinh.Value.ToDateTime(TimeOnly.MinValue);
                }
            }
        }

        private void TaiDanhSachChucVu()
        {
            // Lấy các chức vụ từ DB (distinct)
            var cacChucVuDb = DataProvider.Ins.GetContext().NhanViens
                .AsNoTracking()
                .Where(nv => nv.ChucVu != null && nv.ChucVu != "")
                .Select(nv => nv.ChucVu)
                .Distinct()
                .OrderBy(cv => cv)
                .ToList();

            // Đảm bảo luôn có các chức vụ cơ bản
            var chucVuMacDinh = new List<string>
            {
                "Quản lý",
                "Nhân viên thu ngân",
                "Nhân viên bán hàng",
                "Nhân viên kỹ thuật",
                "Nhân viên kho"
            };

            var tatCaChucVu = cacChucVuDb.Union(chucVuMacDinh)
                .OrderBy(cv => cv)
                .Select(cv => new ChucVuItem { TenChucVu = cv })
                .ToList();

            CboChucVu.ItemsSource = tatCaChucVu;
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên nhân viên!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtHoTen.Focus();
                return;
            }

            if (CboChucVu.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn chức vụ!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CboChucVu.Focus();
                return;
            }

            MaNv = TxtMaNv.Text.Trim();
            HoTen = TxtHoTen.Text.Trim();
            ChucVu = ((ChucVuItem)CboChucVu.SelectedItem).TenChucVu;
            Sdt = TxtSdt.Text.Trim();
            Email = TxtEmail.Text.Trim();

            if (CboGioiTinh.SelectedItem is ComboBoxItem gioiTinhItem)
                GioiTinh = gioiTinhItem.Content.ToString();

            if (DpNgaySinh.SelectedDate.HasValue)
                NgaySinh = DateOnly.FromDateTime(DpNgaySinh.SelectedDate.Value);
            if (DpNgayVaoLam.SelectedDate.HasValue)
                NgayVaoLam = DateOnly.FromDateTime(DpNgayVaoLam.SelectedDate.Value);

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
