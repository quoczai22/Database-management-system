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
    // Display class khớp với binding trong KhachHangView.xaml
    public class KhachHangDisplay
    {
        public string MaKh { get; set; }
        public string HoTen { get; set; }  // map TenKh
        public string Sdt { get; set; }  // map Dthoai
        public string Email { get; set; }  // không có trong DB, để trống
        public string DiaChi { get; set; }  // map Dchi
    }

    public class KhachHangViewModel : BaseViewModel, ISearchable
    {
        // ── Backing collection ──────────────────────────────────────────────
        private ObservableCollection<KhachHangDisplay> _all;

        // ── Bound to DataGrid ────────────────────────────────────────────────
        private ICollectionView _danhSachKhachHang;
        public ICollectionView DanhSachKhachHang
        {
            get => _danhSachKhachHang;
            set { _danhSachKhachHang = value; OnPropertyChanged(); }
        }

        private KhachHangDisplay _khachHangChon;
        public KhachHangDisplay KhachHangChon
        {
            get => _khachHangChon;
            set { _khachHangChon = value; OnPropertyChanged(); }
        }

        // ── Search box ───────────────────────────────────────────────────────
        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachKhachHang?.Refresh(); }
        }

        // ── Commands ─────────────────────────────────────────────────────────
        public ICommand ThemKhachHangCommand { get; private set; }
        public ICommand SuaKhachHangCommand { get; private set; }
        public ICommand XoaKhachHangCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public KhachHangViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        // ── Tải dữ liệu ─────────────────────────────────────────────────────
        public void TaiDuLieu()
        {
            try
            {
                var list = DataProvider.Ins.DB.KhachHangs
                    .AsNoTracking()
                    .Select(kh => new KhachHangDisplay
                    {
                        MaKh = kh.MaKh,
                        HoTen = kh.TenKh,
                        Sdt = kh.Dthoai,
                        Email = string.Empty,
                        DiaChi = kh.Dchi
                    }).ToList();

                _all = new ObservableCollection<KhachHangDisplay>(list);
                DanhSachKhachHang = CollectionViewSource.GetDefaultView(_all);
                DanhSachKhachHang.Filter = Filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu khách hàng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filter ───────────────────────────────────────────────────────────
        private bool Filter(object obj)
        {
            if (obj is not KhachHangDisplay item) return false;
            if (string.IsNullOrWhiteSpace(TimKiem)) return true;

            string kw = TimKiem.ToLower();
            return (item.MaKh?.ToLower().Contains(kw) ?? false)
                || (item.HoTen?.ToLower().Contains(kw) ?? false)
                || (item.Sdt?.ToLower().Contains(kw) ?? false)
                || (item.DiaChi?.ToLower().Contains(kw) ?? false);
        }

        // ── ISearchable ──────────────────────────────────────────────────────
        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        // ── Khởi tạo Commands ────────────────────────────────────────────────
        private void KhoiTaoCommands()
        {
            ThemKhachHangCommand = new RelayCommand<object>(
                _ => true,
                _ => MessageBox.Show(
                    "Chức năng thêm khách hàng đang được phát triển.",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information));

            SuaKhachHangCommand = new RelayCommand<KhachHangDisplay>(
                _ => true,
                kh =>
                {
                    if (kh == null) return;
                    MessageBox.Show(
                        $"Chức năng sửa khách hàng [{kh.HoTen}] đang được phát triển.",
                        "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                });

            XoaKhachHangCommand = new RelayCommand<KhachHangDisplay>(
                _ => true,
                kh =>
                {
                    if (kh == null) return;
                    var res = MessageBox.Show(
                        $"Bạn có chắc chắn muốn xóa khách hàng [{kh.HoTen}]?",
                        "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.Yes)
                        ThucHienXoa(kh);
                });

            LamMoiCommand = new RelayCommand<object>(
                _ => true,
                _ => { TimKiem = string.Empty; TaiDuLieu(); });
        }

        // ── Xóa khách hàng ───────────────────────────────────────────────────
        private void ThucHienXoa(KhachHangDisplay kh)
        {
            try
            {
                var db = DataProvider.Ins.DB;
                var entity = db.KhachHangs.Find(kh.MaKh);
                if (entity == null) return;

                db.KhachHangs.Remove(entity);
                db.SaveChanges();
                _all.Remove(kh);

                MessageBox.Show("Xóa khách hàng thành công!",
                    "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message,
                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}