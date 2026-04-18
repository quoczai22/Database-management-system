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
    // Display class khớp với binding trong NhanVienView.xaml
    public class NhanVienDisplay
    {
        public string MaNv { get; set; }
        public string HoTen { get; set; }   // map TenNv
        public string ChucVu { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }   // map TenDn (login name)
        public DateOnly? NgayVaoLam { get; set; } // map NgaySinh
    }

    // Item cho ComboBox lọc chức vụ
    public class ChucVuItem
    {
        public string TenChucVu { get; set; }
        public override string ToString() => TenChucVu;
    }

    public class NhanVienViewModel : BaseViewModel, ISearchable
    {
        // ── Backing collection ──────────────────────────────────────────────
        private ObservableCollection<NhanVienDisplay> _all;

        // ── Bound to DataGrid ────────────────────────────────────────────────
        private ICollectionView _danhSachNhanVien;
        public ICollectionView DanhSachNhanVien
        {
            get => _danhSachNhanVien;
            set { _danhSachNhanVien = value; OnPropertyChanged(); }
        }

        private NhanVienDisplay _nhanVienChon;
        public NhanVienDisplay NhanVienChon
        {
            get => _nhanVienChon;
            set { _nhanVienChon = value; OnPropertyChanged(); }
        }

        // ── ComboBox chức vụ ────────────────────────────────────────────────
        private ObservableCollection<ChucVuItem> _danhSachChucVu;
        public ObservableCollection<ChucVuItem> DanhSachChucVu
        {
            get => _danhSachChucVu;
            set { _danhSachChucVu = value; OnPropertyChanged(); }
        }

        private ChucVuItem _chucVuChon;
        public ChucVuItem ChucVuChon
        {
            get => _chucVuChon;
            set { _chucVuChon = value; OnPropertyChanged(); DanhSachNhanVien?.Refresh(); }
        }

        // ── Search box ───────────────────────────────────────────────────────
        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachNhanVien?.Refresh(); }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand ThemNhanVienCommand { get; private set; }
        public ICommand SuaNhanVienCommand { get; private set; }
        public ICommand XoaNhanVienCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public NhanVienViewModel()
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

                var list = db.NhanViens
                    .AsNoTracking()
                    .Select(nv => new NhanVienDisplay
                    {
                        MaNv = nv.MaNv,
                        HoTen = nv.TenNv,
                        ChucVu = nv.ChucVu,
                        Sdt = nv.Sdt,
                        Email = nv.TenDn,
                        NgayVaoLam = nv.NgaySinh
                    }).ToList();

                _all = new ObservableCollection<NhanVienDisplay>(list);
                DanhSachNhanVien = CollectionViewSource.GetDefaultView(_all);
                DanhSachNhanVien.Filter = Filter;

                // Danh sách chức vụ cho ComboBox
                var cacChucVu = db.NhanViens
                    .AsNoTracking()
                    .Where(nv => nv.ChucVu != null)
                    .Select(nv => nv.ChucVu)
                    .Distinct()
                    .OrderBy(cv => cv)
                    .Select(cv => new ChucVuItem { TenChucVu = cv })
                    .ToList();

                cacChucVu.Insert(0, new ChucVuItem { TenChucVu = "-- Tất cả --" });
                DanhSachChucVu = new ObservableCollection<ChucVuItem>(cacChucVu);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu nhân viên: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filter ───────────────────────────────────────────────────────────
        private bool Filter(object obj)
        {
            if (obj is not NhanVienDisplay item) return false;

            // Lọc theo từ khóa tìm kiếm
            bool matchSearch = string.IsNullOrWhiteSpace(TimKiem)
                || (item.MaNv?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.HoTen?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Sdt?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Email?.ToLower().Contains(TimKiem.ToLower()) ?? false);

            // Lọc theo chức vụ
            bool matchChucVu = ChucVuChon == null
                || ChucVuChon.TenChucVu == "-- Tất cả --"
                || item.ChucVu == ChucVuChon.TenChucVu;

            return matchSearch && matchChucVu;
        }

        // ── ISearchable ──────────────────────────────────────────────────────
        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        // ── Khởi tạo Commands ────────────────────────────────────────────────
        private void KhoiTaoCommands()
        {
            ThemNhanVienCommand = new RelayCommand<object>(
                _ => true,
                _ => MessageBox.Show(
                    "Chức năng thêm nhân viên đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            SuaNhanVienCommand = new RelayCommand<NhanVienDisplay>(
                _ => true,
                nv =>
                {
                    if (nv == null) return;
                    MessageBox.Show(
                        $"Chức năng sửa nhân viên [{nv.HoTen}] đang được phát triển.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                });

            XoaNhanVienCommand = new RelayCommand<NhanVienDisplay>(
                _ => true,
                nv =>
                {
                    if (nv == null) return;
                    var res = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa nhân viên [{nv.HoTen}]?",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                        ThucHienXoa(nv);
                });

            LamMoiCommand = new RelayCommand<object>(
                _ => true,
                _ =>
                {
                    TimKiem = string.Empty;
                    ChucVuChon = null;
                    TaiDuLieu();
                });
        }

        // ── Xóa nhân viên ────────────────────────────────────────────────────
        private void ThucHienXoa(NhanVienDisplay nv)
        {
            try
            {
                var db = DataProvider.Ins.DB;
                var entity = db.NhanViens.Find(nv.MaNv);
                if (entity == null) return;

                db.NhanViens.Remove(entity);
                db.SaveChanges();
                _all.Remove(nv);

                MessageBox.Show("Xóa nhân viên thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa nhân viên: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}