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
    // Display class khớp với binding trong LoaiLinhKienView.xaml
    public class LoaiLkDisplay
    {
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string MoTa { get; set; }  // không có trong DB, để trống
    }

    public class LoaiLinhKienViewModel : BaseViewModel, ISearchable
    {
        // ── Backing collection ──────────────────────────────────────────────
        private ObservableCollection<LoaiLkDisplay> _all;

        // ── Bound to DataGrid ────────────────────────────────────────────────
        private ICollectionView _danhSachLoaiLinhKien;
        public ICollectionView DanhSachLoaiLinhKien
        {
            get => _danhSachLoaiLinhKien;
            set { _danhSachLoaiLinhKien = value; OnPropertyChanged(); }
        }

        private LoaiLkDisplay _loaiChon;
        public LoaiLkDisplay LoaiChon
        {
            get => _loaiChon;
            set { _loaiChon = value; OnPropertyChanged(); }
        }

        // ── Search box ───────────────────────────────────────────────────────
        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachLoaiLinhKien?.Refresh(); }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand ThemLoaiCommand { get; private set; }
        public ICommand SuaLoaiCommand { get; private set; }
        public ICommand XoaLoaiCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public LoaiLinhKienViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        // ── Tải dữ liệu ─────────────────────────────────────────────────────
        public void TaiDuLieu()
        {
            try
            {
                var list = DataProvider.Ins.DB.LoaiLks
                    .AsNoTracking()
                    .Select(lk => new LoaiLkDisplay
                    {
                        MaLoai = lk.MaLoai,
                        TenLoai = lk.TenLoai,
                        MoTa = string.Empty
                    }).ToList();

                _all = new ObservableCollection<LoaiLkDisplay>(list);
                DanhSachLoaiLinhKien = CollectionViewSource.GetDefaultView(_all);
                DanhSachLoaiLinhKien.Filter = Filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu loại linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filter ───────────────────────────────────────────────────────────
        private bool Filter(object obj)
        {
            if (obj is not LoaiLkDisplay item) return false;
            if (string.IsNullOrWhiteSpace(TimKiem)) return true;

            string kw = TimKiem.ToLower();
            return (item.MaLoai?.ToLower().Contains(kw) ?? false)
                || (item.TenLoai?.ToLower().Contains(kw) ?? false);
        }

        // ── ISearchable ──────────────────────────────────────────────────────
        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        // ── Khởi tạo Commands ────────────────────────────────────────────────
        private void KhoiTaoCommands()
        {
            ThemLoaiCommand = new RelayCommand<object>(
                _ => true,
                _ => MessageBox.Show(
                    "Chức năng thêm loại linh kiện đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            SuaLoaiCommand = new RelayCommand<LoaiLkDisplay>(
                _ => true,
                loai =>
                {
                    if (loai == null) return;
                    MessageBox.Show(
                        $"Chức năng sửa loại [{loai.TenLoai}] đang được phát triển.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                });

            XoaLoaiCommand = new RelayCommand<LoaiLkDisplay>(
                _ => true,
                loai =>
                {
                    if (loai == null) return;
                    var res = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa loại [{loai.TenLoai}]?\n" +
                        "Lưu ý: Không thể xóa nếu còn linh kiện thuộc loại này.",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                        ThucHienXoa(loai);
                });

            LamMoiCommand = new RelayCommand<object>(
                _ => true,
                _ => { TimKiem = string.Empty; TaiDuLieu(); });
        }

        // ── Xóa loại linh kiện ───────────────────────────────────────────────
        private void ThucHienXoa(LoaiLkDisplay loai)
        {
            try
            {
                var db = DataProvider.Ins.DB;
                var entity = db.LoaiLks.Find(loai.MaLoai);
                if (entity == null) return;

                db.LoaiLks.Remove(entity);
                db.SaveChanges();
                _all.Remove(loai);

                MessageBox.Show("Xóa loại linh kiện thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa loại linh kiện: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}