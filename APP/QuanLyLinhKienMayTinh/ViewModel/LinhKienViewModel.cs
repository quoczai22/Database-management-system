using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    // Display class thêm TenLoai (không có trực tiếp trên LinhKien entity)
    public class LinhKienDisplay
    {
        public string MaLk { get; set; }
        public string TenLk { get; set; }
        public string TenLoai { get; set; }  // từ MaLoaiNavigation.TenLoai
        public string Nsx { get; set; }
        public string Dvt { get; set; }
        public byte? Tgbh { get; set; }
        public DateOnly? NgayNhap { get; set; }
        public int? SoluongTon { get; internal set; }
    }
    public class LinhKienViewModel : BaseViewModel, ISearchable
    {
        // ── Backing collection ──────────────────────────────────────────────
        private ObservableCollection<LinhKienDisplay> _all;

        // ── Bound to DataGrid ────────────────────────────────────────────────
        private ICollectionView _danhSachLinhKienView;
        public ICollectionView DanhSachLinhKienView
        {
            get => _danhSachLinhKienView;
            set { _danhSachLinhKienView = value; OnPropertyChanged(); }
        }

        private LinhKienDisplay _linhKienChon;
        public LinhKienDisplay LinhKienChon
        {
            get => _linhKienChon;
            set { _linhKienChon = value; OnPropertyChanged(); }
        }

        // ── ComboBox lọc loại ────────────────────────────────────────────────
        private ObservableCollection<LoaiLk> _danhSachLoai;
        public ObservableCollection<LoaiLk> DanhSachLoai
        {
            get => _danhSachLoai;
            set { _danhSachLoai = value; OnPropertyChanged(); }
        }

        private LoaiLk _loaiChon;
        public LoaiLk LoaiChon
        {
            get => _loaiChon;
            set { _loaiChon = value; OnPropertyChanged(); DanhSachLinhKienView?.Refresh(); }
        }

        // ── Search box (XAML bind đến TimKiem) ──────────────────────────────
        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachLinhKienView?.Refresh(); }
        }
        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand ThemLinhKienCommand { get; private set; }
        public ICommand SuaLinhKienCommand { get; private set; }
        public ICommand XoaLinhKienCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public LinhKienViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        // ── Tải dữ liệu ─────────────────────────────────────────────────────
        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.GetContext();

                // Load với navigation property để lấy TenLoai
                var list = db.LinhKiens
                    .AsNoTracking()
                    .Where(lk => lk.NgungKinhDoanh == false)
                    .Include(lk => lk.MaLoaiNavigation)
                    .Include(lk => lk.MaNsxNavigation)
                    .Select(lk => new LinhKienDisplay
                    {
                        MaLk = lk.MaLk,
                        TenLk = lk.TenLk,
                        TenLoai = lk.MaLoaiNavigation.TenLoai,
                        Nsx = lk.MaNsxNavigation.TenNsx,
                        Dvt = lk.Dvt,
                        Tgbh = lk.Tgbh,
                        NgayNhap = lk.NgayNhap,
                        SoluongTon = lk.SoLuongTon,
                    }).ToList();

                _all = new ObservableCollection<LinhKienDisplay>(list);
                DanhSachLinhKienView = CollectionViewSource.GetDefaultView(_all);
                DanhSachLinhKienView.Filter = Filter;

                // Danh sách loại cho ComboBox — thêm item "Tất cả" đầu tiên
                var cacLoai = db.LoaiLks.AsNoTracking().OrderBy(l => l.TenLoai).ToList();
                cacLoai.Insert(0, new LoaiLk { MaLoai = null, TenLoai = "-- Tất cả --" });
                DanhSachLoai = new ObservableCollection<LoaiLk>(cacLoai);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filter ───────────────────────────────────────────────────────────
        private bool Filter(object obj)
        {
            if (obj is not LinhKienDisplay item) return false;

            // Lọc theo từ khóa
            bool matchSearch = string.IsNullOrWhiteSpace(TimKiem)
                || (item.MaLk?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.TenLk?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.TenLoai?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Nsx?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Dvt?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Tgbh?.ToString().Contains(TimKiem) ?? false)
                || (item.NgayNhap?.ToString().Contains(TimKiem) ?? false);

            // Lọc theo loại
            bool matchLoai = LoaiChon == null
                || LoaiChon.MaLoai == null
                || item.TenLoai == LoaiChon.TenLoai;

            return matchSearch && matchLoai;
        }

        // ── ISearchable ──────────────────────────────────────────────────────
        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        // ── Khởi tạo Commands ────────────────────────────────────────────────
        private void KhoiTaoCommands()
        {
            ThemLinhKienCommand = new RelayCommand<object>(p => true, p => ThucHienThemLinhKien());
            SuaLinhKienCommand = new RelayCommand<LinhKienDisplay>(p => true, lk => ThucHienSuaLinhKien(lk));
            XoaLinhKienCommand = new RelayCommand<LinhKienDisplay>(p => true, lk => ThucHienXoaLinhKien(lk));
            LamMoiCommand = new RelayCommand<object>(p => true, p => ThucHienLamMoi());
        }

        private void ThucHienThemLinhKien()
        {
            try
            {
                // Tạo mã gợi ý, user có thể tự sửa trong dialog
                var dbRead = DataProvider.Ins.GetContext();
                var allMaLk = dbRead.LinhKiens
                    .AsNoTracking()
                    .Select(x => x.MaLk)
                    .ToList();
                // Lấy số lớn nhất trong tất cả mã (vd: MOU003 → 3, VGA004 → 4)
                int maxNum = 0;
                foreach (var ma in allMaLk)
                {
                    var m = System.Text.RegularExpressions.Regex.Match(ma ?? "", @"\d+$");
                    if (m.Success && int.TryParse(m.Value, out int n) && n > maxNum)
                        maxNum = n;
                }
                string newID = "LK" + (maxNum + 1).ToString("D3");

                var dialog = new ThemSuaLinhKienDialog(newID);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    var lkMoi = new LinhKien
                    {
                        MaLk = dialog.MaLk,
                        TenLk = dialog.TenLk,
                        MaLoai = dialog.MaLoai,
                        MaNsx = dialog.MaNsx,
                        Dvt = dialog.Dvt,
                        Tgbh = dialog.Tgbh,
                        DonGiaBan = dialog.DonGiaBan,
                        SoLuongTon = dialog.SoLuongTon ?? 0,
                        NgayNhap = dialog.NgayNhap,
                        NgungKinhDoanh = false
                    };

                    var dbSave = DataProvider.Ins.GetContext();
                    dbSave.LinhKiens.Add(lkMoi);
                    dbSave.SaveChanges();
                    TaiDuLieu();

                    MessageBox.Show("Thêm linh kiện thành công!",
                        "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienSuaLinhKien(LinhKienDisplay lk)
        {
            try
            {
                var dialog = new ThemSuaLinhKienDialog(lk);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    var db = DataProvider.Ins.GetContext();
                    var entity = db.LinhKiens.Find(dialog.MaLk);
                    if (entity != null)
                    {
                        entity.TenLk = dialog.TenLk;
                        entity.MaLoai = dialog.MaLoai;
                        entity.MaNsx = dialog.MaNsx;
                        entity.Dvt = dialog.Dvt;
                        entity.Tgbh = dialog.Tgbh;
                        entity.DonGiaBan = dialog.DonGiaBan;
                        if (dialog.SoLuongTon.HasValue)
                            entity.SoLuongTon = dialog.SoLuongTon;
                        entity.NgayNhap = dialog.NgayNhap;

                        db.SaveChanges();
                        TaiDuLieu();

                        MessageBox.Show("Cập nhật linh kiện thành công!",
                            "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienXoaLinhKien(LinhKienDisplay lk)
        {
            var res = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa linh kiện [{lk.TenLk}] không?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes)
                ThucHienXoa(lk);
        }

        private void ThucHienLamMoi()
        {
            TimKiem = string.Empty; 
            TaiDuLieu();
            DanhSachLinhKienView.Refresh();
        }

        // ── Xóa linh kiện ────────────────────────────────────────────────────
        private void ThucHienXoa(LinhKienDisplay lk)
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                var entity = db.LinhKiens.Find(lk.MaLk);
                if (entity == null) return;

                entity.NgungKinhDoanh = true;
                db.SaveChanges();
                _all.Remove(lk);

                MessageBox.Show("Xóa linh kiện thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}