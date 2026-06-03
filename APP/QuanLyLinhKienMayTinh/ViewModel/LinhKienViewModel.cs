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
        public string TenLoai { get; set; }  
        public string Nsx { get; set; }
        public string Dvt { get; set; }
        public byte? Tgbh { get; set; }
        public DateOnly? NgayNhap { get; set; }
        public int? SoluongTon { get; internal set; }
    }
    public class LinhKienViewModel : BaseViewModel, ISearchable
    {
        // collection gốc
        private ObservableCollection<LinhKienDisplay> _all;

        // binding lên datagrid
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

        // combobox lọc loại
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

        // ô tìm kiếm bind đến TimKiem
        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachLinhKienView?.Refresh(); }
        }
        // commands
        public ICommand ThemLinhKienCommand { get; private set; }
        public ICommand SuaLinhKienCommand { get; private set; }
        public ICommand XoaLinhKienCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public LinhKienViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        // tải dữ liệu
        public void TaiDuLieu()
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();
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

        // lọc dữ liệu
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

        // xử lý tìm kiếm chung
        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        // khởi tạo commands
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
                var dialog = new ThemSuaLinhKienDialog();
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    string maLoaiChon = dialog.MaLoai;
                    using var db = DataProvider.Ins.GetContext();
                    string newMaLk = QL_LinhKien_PC_Context.fn_TaoMaLinhKienMoi(maLoaiChon);
                    if (string.IsNullOrEmpty(newMaLk))
                    {
                        newMaLk = maLoaiChon + "001";
                    }
                    var LkMoi = new LinhKien
                    {
                        MaLk = newMaLk,
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
                    db.LinhKiens.Add(LkMoi);
                    db.SaveChanges();
                    MessageBox.Show($"Thêm linh kiện thành công!\nMã LK: {newMaLk}","Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    TaiDuLieu();
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
                    using var db = DataProvider.Ins.GetContext();
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

        // xóa linh kiện
        private void ThucHienXoa(LinhKienDisplay lk)
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();
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
