using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.Helpers;
using QuanLyLinhKienMayTinh.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class PhieuNhapDisplay
    {
        public string MaPN { get; set; }
        public DateTime? NgayNhap { get; set; }
        public string TenNhanVien { get; set; }
        public string TenNhaCungCap { get; set; }
        public string SoDienThoaiNCC { get; set; }
        public int TongSoLuong { get; set; }
        public decimal TongTien { get; set; }
    }

    public class ChiTietLinhKienNhapDisplay
    {
        public string MaLinhKien { get; set; }
        public string TenLinhKien { get; set; }
        public int SoLuongNhap { get; set; }
        public decimal DonGiaNhap { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class PhieuNhapViewModel : BaseViewModel, ISearchable
    {
        private ObservableCollection<PhieuNhapDisplay> _danhSachPhieuNhap;
        public ObservableCollection<PhieuNhapDisplay> DanhSachPhieuNhap
        {
            get => _danhSachPhieuNhap;
            set { _danhSachPhieuNhap = value; OnPropertyChanged(); }
        }

        private PhieuNhapDisplay _phieuNhapChon;
        public PhieuNhapDisplay PhieuNhapChon
        {
            get => _phieuNhapChon;
            set
            {
                _phieuNhapChon = value;
                OnPropertyChanged();
                ChiTietVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
                ChuaChonPhieuVisibility = value == null ? Visibility.Visible : Visibility.Collapsed;
                TaiChiTietPhieuNhap(value?.MaPN);
            }
        }

        private ObservableCollection<ChiTietLinhKienNhapDisplay> _chiTietLinhKienNhap;
        public ObservableCollection<ChiTietLinhKienNhapDisplay> ChiTietLinhKienNhap
        {
            get => _chiTietLinhKienNhap;
            set { _chiTietLinhKienNhap = value; OnPropertyChanged(); }
        }

        private Visibility _chiTietVisibility = Visibility.Collapsed;
        public Visibility ChiTietVisibility
        {
            get => _chiTietVisibility;
            set { _chiTietVisibility = value; OnPropertyChanged(); }
        }

        private Visibility _chuaChonPhieuVisibility = Visibility.Visible;
        public Visibility ChuaChonPhieuVisibility
        {
            get => _chuaChonPhieuVisibility;
            set { _chuaChonPhieuVisibility = value; OnPropertyChanged(); }
        }

        private string _tuKhoanTimKiem = string.Empty;
        public string TuKhoanTimKiem
        {
            get => _tuKhoanTimKiem;
            set { _tuKhoanTimKiem = value; OnPropertyChanged(); }
        }

        private DateTime? _tuNgay;
        public DateTime? TuNgay
        {
            get => _tuNgay;
            set { _tuNgay = value; OnPropertyChanged(); }
        }

        private DateTime? _denNgay;
        public DateTime? DenNgay
        {
            get => _denNgay;
            set { _denNgay = value; OnPropertyChanged(); }
        }

        private int _tongSoPhieuNhap;
        public int TongSoPhieuNhap { get => _tongSoPhieuNhap; set { _tongSoPhieuNhap = value; OnPropertyChanged(); } }

        private decimal _tongChiPhiNhap;
        public decimal TongChiPhiNhap { get => _tongChiPhiNhap; set { _tongChiPhiNhap = value; OnPropertyChanged(); } }

        private int _tongSoLuongLinhKienNhap;
        public int TongSoLuongLinhKienNhap { get => _tongSoLuongLinhKienNhap; set { _tongSoLuongLinhKienNhap = value; OnPropertyChanged(); } }

        private int _soPhieuNhapThangNay;
        public int SoPhieuNhapThangNay { get => _soPhieuNhapThangNay; set { _soPhieuNhapThangNay = value; OnPropertyChanged(); } }

        public ICommand TaoPhieuNhapMoiCommand { get; private set; }
        public ICommand LocPhieuNhapCommand { get; private set; }
        public ICommand InPhieuNhapCommand { get; private set; }
        public ICommand LuuPhieuNhapCommand { get; private set; }
        public ICommand XoaPhieuNhapCommand { get; private set; }

        public PhieuNhapViewModel()
        {
            DanhSachPhieuNhap = new ObservableCollection<PhieuNhapDisplay>();
            ChiTietLinhKienNhap = new ObservableCollection<ChiTietLinhKienNhapDisplay>();
            KhoiTaoCommands();
            TaiDuLieu();
        }

        private void KhoiTaoCommands()
        {
            TaoPhieuNhapMoiCommand = new RelayCommand<object>(p => true, p => TaoPhieuNhapMoi());
            LocPhieuNhapCommand = new RelayCommand<object>(p => true, p => LocPhieuNhap());
            InPhieuNhapCommand = new RelayCommand<object>(p => PhieuNhapChon != null, p => InPhieuNhap());
            LuuPhieuNhapCommand = new RelayCommand<object>(p => PhieuNhapChon != null, p => LuuPhieuNhapExcel());
            XoaPhieuNhapCommand = new RelayCommand<object>(p => PhieuNhapChon != null, p => XoaPhieuNhap());
        }

        public void ApplySearch(string keyword)
        {
            TuKhoanTimKiem = keyword?.Trim() ?? string.Empty;
            LocPhieuNhap();
        }

        public void TaiDuLieu()
        {
            LocPhieuNhap();
        }

        private async void LocPhieuNhap()
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();

                string tuKhoa = string.IsNullOrWhiteSpace(TuKhoanTimKiem) ? "" : TuKhoanTimKiem.Trim();
                DateOnly? tuNgayParam = TuNgay.HasValue ? DateOnly.FromDateTime(TuNgay.Value) : null;
                DateOnly? denNgayParam = DenNgay.HasValue ? DateOnly.FromDateTime(DenNgay.Value) : null;

                var resultList = await db.Procedures.sp_locdanhsachphieunhapAsync(tuKhoa, tuNgayParam, denNgayParam);

                var listDisplay = resultList.Select(pn => new PhieuNhapDisplay
                {
                    MaPN = pn.mapn,
                    TenNhanVien = pn.tennv,
                    NgayNhap = pn.ngaynhap.HasValue ? pn.ngaynhap.Value.ToDateTime(TimeOnly.MinValue) : null,
                    TenNhaCungCap = pn.tennsx ?? "Chưa có dữ liệu",
                    SoDienThoaiNCC = pn.sdtnsx ?? "Chưa có dữ liệu",
                    TongSoLuong = pn.tongsoluong,
                    TongTien = pn.tongtien ?? 0
                }).ToList();

                DanhSachPhieuNhap = new ObservableCollection<PhieuNhapDisplay>(listDisplay);
                CapNhatThongKe();
                PhieuNhapChon = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lọc phiếu nhập từ SQL: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TaiChiTietPhieuNhap(string maPN)
        {
            if (string.IsNullOrWhiteSpace(maPN))
            {
                ChiTietLinhKienNhap = new ObservableCollection<ChiTietLinhKienNhapDisplay>();
                return;
            }

            try
            {
                using var db = DataProvider.Ins.GetContext();
                var resultList = await db.Procedures.sp_TaiChiTietPNAsync(maPN);

                var chiTiet = resultList.Select(ct => new ChiTietLinhKienNhapDisplay
                {
                    MaLinhKien = ct.MaLinhKien,
                    TenLinhKien = ct.TenLinhKien,
                    SoLuongNhap = ct.SoLuongNhap ?? 0,
                    DonGiaNhap = ct.DonGiaNhap ?? 0,
                    ThanhTien = ct.ThanhTien ?? 0
                }).ToList();

                ChiTietLinhKienNhap = new ObservableCollection<ChiTietLinhKienNhapDisplay>(chiTiet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết phiếu nhập từ SQL: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CapNhatThongKe()
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();

                var tongPN = new OutputParameter<int?>();
                var tongChiPhi = new OutputParameter<decimal?>();
                var tongSL = new OutputParameter<int?>();
                var thangNay = new OutputParameter<int?>();

                await db.Procedures.sp_ThongKePhieuNhapAsync(tongPN, tongChiPhi, tongSL, thangNay);

                TongSoPhieuNhap = tongPN.Value ?? 0;
                TongChiPhiNhap = tongChiPhi.Value ?? 0;
                TongSoLuongLinhKienNhap = tongSL.Value ?? 0;
                SoPhieuNhapThangNay = thangNay.Value ?? 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thống kê phiếu nhập từ SQL: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TaoPhieuNhapMoi()
        {
            try
            {
                var dialog = new ThemPhieuNhapDialog();
                dialog.Owner = Application.Current.MainWindow;

                if (dialog.ShowDialog() == true)
                {
                    await LuuPhieuNhapBangSql(dialog.NgayNhap, dialog.MaNv, dialog.MaNsx, dialog.ChiTietNhap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tạo phiếu nhập: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LuuPhieuNhapBangSql(DateOnly ngayNhap, string maNv, string maNsx, List<PhieuNhapTamItem> chiTietNhap)
        {
            try
            {
                using var db = DataProvider.Ins.GetContext();
                string maPN = null;

                foreach (var item in chiTietNhap)
                {
                    var maPNParam = new OutputParameter<string> { _value = maPN };

                    await db.Procedures.sp_NhapLinhKienAsync(
                        ngayNhap,
                        maNv,
                        maNsx,
                        item.MaLk,
                        item.SoLuongNhap,
                        item.DonGiaNhap,
                        maPNParam);

                    maPN = maPNParam.Value;
                }

                MessageBox.Show("Tạo phiếu nhập thành công! Mã phiếu nhập: " + maPN, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo phiếu nhập từ SQL: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InPhieuNhap()
        {
            if (PhieuNhapChon == null) return;
            try
            {
                var baoCao = new BaoCaoViewModel();
                var document = baoCao.TaoBaoCaoPhieuNhap(PhieuNhapChon, ChiTietLinhKienNhap);
                var preview = new BaoCaoPreviewWindow(document)
                {
                    Owner = Application.Current.MainWindow
                };
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất báo cáo phiếu nhập: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LuuPhieuNhapExcel()
        {
            if (PhieuNhapChon == null) return;
            try
            {
                var sfd = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"PhieuNhap_{PhieuNhapChon.MaPN}.xls",
                    Filter = "Excel 97-2003 Workbook (*.xls)|*.xls"
                };

                if (sfd.ShowDialog() == true)
                {
                    ExcelExportHelper.XuatPhieuNhap(sfd.FileName, PhieuNhapChon, ChiTietLinhKienNhap);
                    MessageBox.Show("Đã lưu phiếu nhập thành file Excel!", "Lưu Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu Excel phiếu nhập: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void XoaPhieuNhap()
        {
            if (PhieuNhapChon == null) return;

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa phiếu nhập [{PhieuNhapChon.MaPN}]?\nTồn kho sẽ được xử lý bởi trigger trong SQL.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                using var db = DataProvider.Ins.GetContext();
                await db.Procedures.sp_XoaPhieuNhapAsync(PhieuNhapChon.MaPN);

                MessageBox.Show("Xóa phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                TaiDuLieu();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xóa phiếu nhập từ SQL: " + LayThongDiepLoi(ex), "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string LayThongDiepLoi(Exception ex)
        {
            var current = ex;
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return current.Message;
        }
    }
}
