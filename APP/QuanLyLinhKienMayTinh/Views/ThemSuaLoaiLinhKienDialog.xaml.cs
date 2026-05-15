using Microsoft.EntityFrameworkCore;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.ViewModels;
using System.Linq;
using System.Windows;

namespace QuanLyLinhKienMayTinh.Views
{
    public partial class ThemSuaLoaiLinhKienDialog : Window
    {
        public LoaiLkDisplay KetQua { get; private set; }
        private readonly bool _laMoiThem;

        /// <summary>Mở ở chế độ THÊM</summary>
        public ThemSuaLoaiLinhKienDialog(string maLoaiGoi)
        {
            InitializeComponent();
            _laMoiThem = true;
            TitleText.Text = "Thêm Loại Linh Kiện";
            // Để trống, người dùng tự nhập mã loại có nghĩa
            TxtMaLoai.Text = string.Empty;
            TxtMaLoaiHint.Visibility = Visibility.Visible;
        }

        /// <summary>Mở ở chế độ SỬA</summary>
        public ThemSuaLoaiLinhKienDialog(LoaiLkDisplay loai)
        {
            InitializeComponent();
            _laMoiThem = false;
            TitleText.Text = "Sửa Loại Linh Kiện";
            BtnLuu.Content = "Cập nhật";
            TxtMaLoai.Text = loai.MaLoai;
            TxtMaLoai.IsReadOnly = true;
            TxtMaLoai.Opacity = 0.6;
            TxtMaLoaiHint.Visibility = Visibility.Collapsed;
            TxtTenLoai.Text = loai.TenLoai;
            TxtMoTa.Text = loai.MoTa;
        }

        private void BtnLuu_Click(object sender, RoutedEventArgs e)
        {
            string maLoai = TxtMaLoai.Text.Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(maLoai))
            {
                MessageBox.Show("Vui lòng nhập mã loại linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtMaLoai.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtTenLoai.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại linh kiện!", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTenLoai.Focus();
                return;
            }

            // Kiểm tra trùng mã khi thêm mới
            if (_laMoiThem)
            {
                bool trung = DataProvider.Ins.GetContext().LoaiLks
                    .AsNoTracking()
                    .Any(l => l.MaLoai == maLoai);
                if (trung)
                {
                    MessageBox.Show($"Mã loại '{maLoai}' đã tồn tại! Vui lòng nhập mã khác.",
                        "Trùng mã", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtMaLoai.Focus();
                    return;
                }
            }

            KetQua = new LoaiLkDisplay
            {
                MaLoai = maLoai,
                TenLoai = TxtTenLoai.Text.Trim(),
                MoTa = TxtMoTa.Text.Trim()
            };

            DialogResult = true;
            Close();
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
