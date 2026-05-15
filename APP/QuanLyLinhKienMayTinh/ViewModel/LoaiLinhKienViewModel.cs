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
    public class LoaiLkDisplay
    {
        public string MaLoai { get; set; }
        public string TenLoai { get; set; }
        public string MoTa { get; set; }
    }

    public class LoaiLinhKienViewModel : BaseViewModel, ISearchable
    {
        private ObservableCollection<LoaiLkDisplay> _all;

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

        private string _timKiem = string.Empty;
        public string TimKiem
        {
            get => _timKiem;
            set { _timKiem = value; OnPropertyChanged(); DanhSachLoaiLinhKien?.Refresh(); }
        }

        public ICommand ThemLoaiCommand { get; private set; }
        public ICommand SuaLoaiCommand { get; private set; }
        public ICommand XoaLoaiCommand { get; private set; }
        public ICommand LamMoiCommand { get; private set; }

        public LoaiLinhKienViewModel()
        {
            TaiDuLieu();
            KhoiTaoCommands();
        }

        public void TaiDuLieu()
        {
            try
            {
                var list = DataProvider.Ins.GetContext().LoaiLks
                    .AsNoTracking()
                    .Select(lk => new LoaiLkDisplay
                    {
                        MaLoai = lk.MaLoai,
                        TenLoai = lk.TenLoai,
                        MoTa = lk.MoTa,
                    }).ToList();

                _all = new ObservableCollection<LoaiLkDisplay>(list);
                DanhSachLoaiLinhKien = CollectionViewSource.GetDefaultView(_all);
                DanhSachLoaiLinhKien.Filter = Filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải dữ liệu loại linh kiện: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool Filter(object obj)
        {
            if (obj is not LoaiLkDisplay item) return false;
            if (string.IsNullOrWhiteSpace(TimKiem)) return true;

            string kw = TimKiem.ToLower();
            return (item.MaLoai?.ToLower().Contains(kw) ?? false)
                || (item.TenLoai?.ToLower().Contains(kw) ?? false);
        }

        public void ApplySearch(string keyword)
        {
            TimKiem = keyword?.Trim() ?? string.Empty;
        }

        private void KhoiTaoCommands()
        {
            ThemLoaiCommand = new RelayCommand<object>(_ => true, _ => ThucHienThemLoai());
            SuaLoaiCommand = new RelayCommand<LoaiLkDisplay>(_ => true, loai => ThucHienSuaLoai(loai));
            XoaLoaiCommand = new RelayCommand<LoaiLkDisplay>(_ => true, loai => ThucHienXoaLoai(loai));
            LamMoiCommand = new RelayCommand<object>(_ => true, _ => ThucHienLamMoi());
        }

        private void ThucHienThemLoai()
        {
            try
            {
                // Bỏ luôn AutoIDService. Cho người dùng tự điền (VD: CPU, VGA...)
                string newID = "";

                var dialog = new ThemSuaLoaiLinhKienDialog(newID);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    var kq = dialog.KetQua;
                    var loaiMoi = new LoaiLk
                    {
                        MaLoai = kq.MaLoai,
                        TenLoai = kq.TenLoai,
                        MoTa = kq.MoTa
                    };

                    var dbSave = DataProvider.Ins.GetContext();
                    dbSave.LoaiLks.Add(loaiMoi);
                    dbSave.SaveChanges();
                    TaiDuLieu();

                    MessageBox.Show("Thêm loại linh kiện thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm loại linh kiện: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienSuaLoai(LoaiLkDisplay loai)
        {
            try
            {
                var dialog = new ThemSuaLoaiLinhKienDialog(loai);
                dialog.Owner = Application.Current.MainWindow;
                if (dialog.ShowDialog() == true)
                {
                    var kq = dialog.KetQua;
                    var db = DataProvider.Ins.GetContext();
                    var entity = db.LoaiLks.Find(kq.MaLoai);
                    if (entity != null)
                    {
                        entity.TenLoai = kq.TenLoai;
                        entity.MoTa = kq.MoTa;

                        db.SaveChanges();
                        TaiDuLieu();

                        MessageBox.Show("Cập nhật loại linh kiện thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi sửa loại linh kiện: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ThucHienXoaLoai(LoaiLkDisplay loai)
        {
            var res = MessageBox.Show(
                    $"Bạn có chắc chắn muốn xóa loại [{loai.TenLoai}] không?\n" +
                    "Lưu ý: Không thể xóa nếu còn linh kiện thuộc loại này.",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.Yes) ThucHienXoa(loai);
        }

        private void ThucHienLamMoi()
        {
            TimKiem = string.Empty;
            TaiDuLieu();
            DanhSachLoaiLinhKien.Refresh();
        }

        private void ThucHienXoa(LoaiLkDisplay loai)
        {
            try
            {
                var db = DataProvider.Ins.GetContext();
                var entity = db.LoaiLks.Find(loai.MaLoai);
                if (entity == null) return;

                db.LoaiLks.Remove(entity);
                db.SaveChanges();
                _all.Remove(loai);

                MessageBox.Show("Xóa loại linh kiện thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xóa loại linh kiện: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}