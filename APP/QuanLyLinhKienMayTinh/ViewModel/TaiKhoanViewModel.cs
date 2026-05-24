using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using QuanLyLinhKienMayTinh.Models;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class TaiKhoanViewModel : BaseViewModel
    {
        private ObservableCollection<NhanVien> _dsNhanVienKhongTaiKhoan;
        public ObservableCollection<NhanVien> DSNhanVienKhongTaiKhoan
        {
            get => _dsNhanVienKhongTaiKhoan;
            set { _dsNhanVienKhongTaiKhoan = value; OnPropertyChanged(); }
        }
        private string _tenDangNhapMoi;
        public string TenDangNhapMoi
        {
            get => _tenDangNhapMoi;
            set { _tenDangNhapMoi = value; OnPropertyChanged(); }
        }

        private string _matKhauMoi;
        public string MatKhauMoi
        {
            get => _matKhauMoi;
            set { _matKhauMoi = value; OnPropertyChanged(); }
        }

        private NhanVien _selectedNhanVien;
        public NhanVien SelectedNhanVien
        {
            get => _selectedNhanVien;
            set
            {
                _selectedNhanVien = value;
                OnPropertyChanged();

                if (_selectedNhanVien != null)
                {
                    ChucVuHienTai = _selectedNhanVien.ChucVu;
                    if (ChucVuHienTai == "Quản lý") SelectedChucVu = "Quản lý toàn bộ";
                    else if (ChucVuHienTai == "Nhân viên thu ngân") SelectedChucVu = "Thu ngân";
                    else if (ChucVuHienTai == "Nhân viên chăm sóc khách hàng") SelectedChucVu = "Chăm sóc khách hàng";
                    else if (ChucVuHienTai == "Nhân viên kho") SelectedChucVu = "Kho";
                }
                else
                {
                    ChucVuHienTai = string.Empty;
                    SelectedChucVu = string.Empty;
                }
            }
        }
        private string _chucVuHienTai;
        public string ChucVuHienTai
        {
            get => _chucVuHienTai;
            set { _chucVuHienTai = value; OnPropertyChanged(); }
        }
        private string _selectedChucVu;
        public string SelectedChucVu
        {
            get => _selectedChucVu;
            set { _selectedChucVu = value; OnPropertyChanged(); }
        }

        public ICommand TaoTaiKhoanCommand { get; set; }

        public TaiKhoanViewModel()
        {
            DSNhanVienKhongTaiKhoan = new ObservableCollection<NhanVien>();
            LoadNhanVien();
            TaoTaiKhoanCommand = new RelayCommand<object>((p) => true, (p) => TaoTaiKhoanMoi());
        }

        public void LoadNhanVien()
        {
            using (var db = DataProvider.Ins.GetContext())
            {
                var listNV = db.NhanViens
                               .Where(n => !db.TaiKhoans.Any(t => t.MaNv == n.MaNv) && n.DaNghiViec == false)
                               .ToList();
                DSNhanVienKhongTaiKhoan.Clear();
                foreach (var nv in listNV)
                {
                    DSNhanVienKhongTaiKhoan.Add(nv);
                }
            }
        }

        private void TaoTaiKhoanMoi()
        {
            if (SelectedNhanVien == null || string.IsNullOrEmpty(TenDangNhapMoi) || string.IsNullOrEmpty(MatKhauMoi))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin: Chọn nhân viên, Tên ĐN, Mật khẩu!");
                return;
            }

            using (var db = DataProvider.Ins.GetContext())
            {
                if (db.TaiKhoans.Any(t => t.TenDn == TenDangNhapMoi))
                {
                    MessageBox.Show("Tên đăng nhập này đã có người sử dụng. Vui lòng chọn tên khác!");
                    return;
                }

                try
                {
                    var nv = db.NhanViens.FirstOrDefault(n => n.MaNv == SelectedNhanVien.MaNv);
                    if (nv != null)
                    {
                        nv.Quyen = SelectedChucVu;
                    }
                    db.TaiKhoans.Add(new TaiKhoan
                    {
                        TenDn = TenDangNhapMoi,
                        MatKhau = MatKhauMoi,
                        MaNv = SelectedNhanVien.MaNv
                    });
                    db.SaveChanges();

                    MessageBox.Show($"Đã tạo tài khoản thành công cho nhân viên {SelectedNhanVien.TenNv} với quyền {SelectedChucVu}!");
                    TenDangNhapMoi = string.Empty;
                    MatKhauMoi = string.Empty;
                    LoadNhanVien(); 
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu tài khoản: " + ex.Message);
                }
            }
        }
    }
}