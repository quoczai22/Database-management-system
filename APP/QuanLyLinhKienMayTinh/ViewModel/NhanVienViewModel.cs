using LiveCharts.Wpf;
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
    public class NhanVienDisplay
    {
        public string MaNv { get; set; }
        public string HoTen { get; set; }
        public string ChucVu { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public DateOnly? NgayVaoLam { get; set; }
    }

    public class ChucVuItem
    {
        public string TenChucVu { get; set; }
        public override string ToString() => TenChucVu;
    }

    public class NhanVienViewModel : BaseViewModel, ISearchable
    {
        private ObservableCollection<NhanVienDisplay> _all;
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

        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachNhanVien?.Refresh(); }
        }

        public ICommand ThemNhanVienCommand { get; private set; }
        public ICommand SuaNhanVienCommand { get; private set; }
        public ICommand XoaNhanVienCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public NhanVienViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        public void TaiDuLieu()
        {
            try
            {
                var db = DataProvider.Ins.GetContext();

                var list = db.NhanViens
                .AsNoTracking()
                .Where(x => x.DaNghiViec == false)
                .Select(nv => new NhanVienDisplay
                {
                    MaNv = nv.MaNv,
                    HoTen = nv.TenNv,
                    ChucVu = nv.ChucVu,
                    Sdt = nv.Sdt,
                    Email = nv.Email,
                    NgayVaoLam = nv.NgayVaoLam
                }).ToList();

                _all = new ObservableCollection<NhanVienDisplay>(list);
                DanhSachNhanVien = CollectionViewSource.GetDefaultView(_all);
                DanhSachNhanVien.Filter = Filter;

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
                MessageBox.Show("Lỗi tải dữ liệu nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool Filter(object obj)
        {
            if (obj is not NhanVienDisplay item) return false;

            bool matchSearch = string.IsNullOrWhiteSpace(TimKiem)
                || (item.MaNv?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.HoTen?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Sdt?.ToLower().Contains(TimKiem.ToLower()) ?? false)
                || (item.Email?.ToLower().Contains(TimKiem.ToLower()) ?? false);

            bool matchChucVu = ChucVuChon == null
                || ChucVuChon.TenChucVu == "-- Tất cả --"
                || item.ChucVu == ChucVuChon.TenChucVu;

            return matchSearch && matchChucVu;
        }

        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        private void KhoiTaoCommands()
        {
            ThemNhanVienCommand = new RelayCommand<object>(p => true, p => ThucHienThem());
            SuaNhanVienCommand = new RelayCommand<NhanVienDisplay>(p => true, nv => ThucHienSua(nv));
            XoaNhanVienCommand = new RelayCommand<NhanVienDisplay>(p => true, nv => ThucHienXoa(nv));
            LamMoiCommand = new RelayCommand<object>(p => true, p => ThucHienLamMoi());
        }

        private void ThucHienThem()
        {
            try
            {
                var dialog = new ThemSuaNhanVienDialog();
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    string quyen = LayQuyenTuChucVu(dialog.ChucVu);
                    string maNvMoi = QL_LinhKien_PC_Context.fn_TaoMaNhanVienMoi();
                    var nv = new NhanVien
                    {
                        MaNv = maNvMoi,
                        TenNv = dialog.HoTen,
                        ChucVu = dialog.ChucVu,
                        Quyen = quyen,
                        GioiTinh = dialog.GioiTinh,
                        Sdt = dialog.Sdt,
                        Email = dialog.Email,
                        NgaySinh = dialog.NgaySinh,
                        NgayVaoLam = dialog.NgayVaoLam,
                        DaNghiViec = false
                    };
                    using var db = DataProvider.Ins.GetContext();
                    db.NhanViens.Add(nv);
                    db.SaveChanges();
                    MessageBox.Show($"Thêm nhân viên thành công!\nMã NV: {maNvMoi}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    TaiDuLieu();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienSua(NhanVienDisplay nv)
        {
            if (LuuTrangThai.QuyenDangNhap != "Quản lý toàn bộ")
            {
                MessageBox.Show("Chỉ tài khoản quản lý (machpv) mới được phép sửa nhân viên!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var dialog = new ThemSuaNhanVienDialog(nv);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    using var db = DataProvider.Ins.GetContext();
                    var entity = db.NhanViens.Find(dialog.MaNv);
                    if (entity != null)
                    {
                        entity.TenNv = dialog.HoTen;
                        entity.ChucVu = dialog.ChucVu;
                        entity.Quyen = LayQuyenTuChucVu(dialog.ChucVu);
                        entity.GioiTinh = dialog.GioiTinh;
                        entity.Sdt = dialog.Sdt;
                        entity.Email = dialog.Email;
                        entity.NgaySinh = dialog.NgaySinh;
                        entity.NgayVaoLam = dialog.NgayVaoLam;

                        db.SaveChanges();
                        TaiDuLieu();

                        MessageBox.Show("Cập nhật nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienXoa(NhanVienDisplay nv)
        {
            if (LuuTrangThai.QuyenDangNhap != "Quản lý toàn bộ")
            {
                MessageBox.Show("Chỉ tài khoản quản lý (machpv) mới được phép xóa nhân viên!", "Từ chối truy cập", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var res = MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên [{nv.HoTen}] không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                using var db = DataProvider.Ins.GetContext();
                var entity = db.NhanViens.Find(nv.MaNv);
                if (entity == null) return;

                entity.DaNghiViec = true;
                db.SaveChanges();

                _all.Remove(nv);

                MessageBox.Show("Xóa nhân viên thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa nhân viên: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienLamMoi()
        {
            TimKiem = string.Empty;
            ChucVuChon = null;
            TaiDuLieu();
            DanhSachNhanVien.Refresh();
        }

        private string LayQuyenTuChucVu(string chucVu)
        {
            return chucVu switch
            {
                "Quản lý" => "Quản lý toàn bộ",
                "Nhân viên thu ngân" => "Thu ngân",
                "Nhân viên bán hàng" => "Bán hàng",
                "Nhân viên kỹ thuật" => "Kỹ thuật",
                "Nhân viên kho" => "Kho",
                _ => "Bán hàng"
            };
        }
    }
}