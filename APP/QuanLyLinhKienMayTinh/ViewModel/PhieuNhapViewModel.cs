using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
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
                    TongSoLuong = pn.tongsoluong ?? 0,
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

        private void TaoPhieuNhapMoi()
        {
            MessageBox.Show(
                "SQL đã có sp_NhapLinhKien để xử lý tạo phiếu nhập. Cần thêm dialog nhập liệu giống ThemHoaDonDialog để chọn NSX, linh kiện, số lượng và đơn giá nhập.",
                "Thiếu màn hình nhập liệu",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void InPhieuNhap()
        {
            if (PhieuNhapChon == null) return;

            var content = new StringBuilder();
            content.AppendLine("PHIẾU NHẬP KHO");
            content.AppendLine($"Mã PN: {PhieuNhapChon.MaPN}");
            content.AppendLine($"Ngày nhập: {PhieuNhapChon.NgayNhap:dd/MM/yyyy}");
            content.AppendLine($"Nhân viên: {PhieuNhapChon.TenNhanVien}");
            content.AppendLine($"Nhà sản xuất: {PhieuNhapChon.TenNhaCungCap}");
            content.AppendLine("------------------------------------------");
            content.AppendLine("Linh kiện\tSL\tĐơn giá\tThành tiền");

            foreach (var item in ChiTietLinhKienNhap)
            {
                content.AppendLine($"{item.TenLinhKien}\t{item.SoLuongNhap}\t{item.DonGiaNhap:N0}\t{item.ThanhTien:N0}");
            }

            content.AppendLine("------------------------------------------");
            content.AppendLine($"Tổng số lượng: {PhieuNhapChon.TongSoLuong}");
            content.AppendLine($"Tổng chi phí: {PhieuNhapChon.TongTien:N0} VNĐ");

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"PhieuNhap_{PhieuNhapChon.MaPN}.txt",
                Filter = "Text File (*.txt)|*.txt"
            };

            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, content.ToString());
                MessageBox.Show("Đã xuất phiếu nhập thành công!", "In phiếu nhập");
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
