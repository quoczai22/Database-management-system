using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
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
        public DateOnly? NgaySx { get; set; }
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
                var db = DataProvider.Ins.DB;

                // Load với navigation property để lấy TenLoai
                var list = db.LinhKiens
                    .AsNoTracking()
                    .Include(lk => lk.MaLoaiNavigation)
                    .Select(lk => new LinhKienDisplay
                    {
                        MaLk = lk.MaLk,
                        TenLk = lk.TenLk,
                        TenLoai = lk.MaLoaiNavigation.TenLoai,
                        Nsx = lk.Nsx,
                        Dvt = lk.Dvt,
                        Tgbh = lk.Tgbh,
                        NgaySx = lk.NgaySx
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
                || (item.NgaySx?.ToString().Contains(TimKiem) ?? false);

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
            ThemLinhKienCommand = new RelayCommand<object>(
                _ => true,
                _ => MessageBox.Show(
                    "Chức năng thêm linh kiện đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            SuaLinhKienCommand = new RelayCommand<LinhKienDisplay>(
                _ => true,
                lk =>
                {
                    if (lk == null) return;
                    MessageBox.Show(
                        $"Chức năng sửa linh kiện [{lk.TenLk}] đang được phát triển.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                });

            XoaLinhKienCommand = new RelayCommand<LinhKienDisplay>(
                _ => true,
                lk =>
                {
                    if (lk == null) return;
                    var res = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa linh kiện [{lk.TenLk}]?",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                        ThucHienXoa(lk);
                });

            LamMoiCommand = new RelayCommand<object>(
                _ => true,
                _ =>
                {
                    TimKiem = string.Empty;
                    LoaiChon = null;
                    TaiDuLieu();
                });
        }

        // ── Xóa linh kiện ────────────────────────────────────────────────────
        private void ThucHienXoa(LinhKienDisplay lk)
        {
            try
            {
                var db = DataProvider.Ins.DB;
                var entity = db.LinhKiens.Find(lk.MaLk);
                if (entity == null) return;

                db.LinhKiens.Remove(entity);
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