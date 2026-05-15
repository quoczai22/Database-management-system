using QRCoder;
using QuanLyLinhKienMayTinh.Models;
using QuanLyLinhKienMayTinh.Services;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace QuanLyLinhKienMayTinh.Views
{
    public partial class MomoPaymentWindow : Window
    {
        string _maHd;
        long _soTien;
        IMomoService _momoService;
        Action _onThanhToanThanhCong;
        CancellationTokenSource _qrCancellationTokenSource; // dùng để hủy việc kiểm tra trạng thái khi cửa sổ đóng
        bool _daThanhToan = false;// đặt cờ để biết thanh toán thành công hay chưa 

        public MomoPaymentWindow(string maHd, long soTien, IMomoService momoService, Action onThanhToanThanhCong = null)
        {
            InitializeComponent();
            _maHd = maHd;
            _soTien = soTien;
            _momoService = momoService;
            _onThanhToanThanhCong = onThanhToanThanhCong;

            txtMaHD.Text = $"#{_maHd}";
            txtSoTien.Text = $"{_soTien:N0} ₫";

            Loaded += async (s, e) => await LayMaQR(); // tự động gọi lấy mã QR khi cửa sổ được tải lên
        }

        async Task LayMaQR()
        {
            try
            {
                loadingOverlay.Visibility = Visibility.Visible; // hiện thị loading 
                successPanel.Visibility = Visibility.Collapsed; // ẩn panel thành công
                failPanel.Visibility = Visibility.Collapsed; // ẩn panel thất bại
                imgQRCode.Source = null; // xóa ảnh QR cũ nếu có

                var hd = new HoaDon { MaHd = _maHd, TongTien = (int)_soTien };
                var response = await _momoService.CreatePaymentAsync(hd); // gọi API tạo thanh toán và lấy mã QR

                if (response != null && response.resultCode == 0)// nếu API trả về thành công
                {
                    string url = response.payUrl; // ưu tiên dùng payUrl nếu có, nếu không thì dùng qrCodeUrl
                    if (!string.IsNullOrEmpty(url)) // nếu có URL thanh toán thì tạo mã QR
                        GenerateQR(url); 
                    else
                        MessageBox.Show("MoMo không trả về URL.");
                }
                else
                {
                    MessageBox.Show("Lỗi từ MoMo: " + response?.message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối: " + ex.Message);
            }
            finally
            {
                loadingOverlay.Visibility = Visibility.Collapsed; // ẩn loading sau khi đã có kết quả
            }
        }

        // hàm này tạo QR 
        void GenerateQR(string content)
        {
            using (var qr = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qr.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q); // tạo dữ liệu QR từ nội dung và cho phép mức độ sửa lỗi là 25%
                using (var qrCode = new QRCode(qrCodeData))
                {
                    Bitmap qrBitmap = qrCode.GetGraphic(20); // tạo ảnh QR với kích thước mỗi module là 20 pixel
                    imgQRCode.Source = BitmapToImageSource(qrBitmap); // chuyển Bitmap sang ImageSource để hiển thị trên WPF
                    StartQRCountdown(60); // bắt đầu đếm ngược 60 giây cho mã QR
                    StartPollingTrangThai(); // bắt đầu kiểm tra trạng thái thanh toán định kỳ
                }
            }

        }

        // Hàm chuyển đổi Bitmap sang BitmapImage
        BitmapImage BitmapToImageSource(Bitmap bitmap) // chuyển đổi Bitmap sang BitmapImage để hiển thị trong WPF
        {
            using (var ms = new MemoryStream())// tạo một MemoryStream để lưu ảnh QR dưới dạng PNG
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png); // lưu ảnh QR vào MemoryStream dưới dạng PNG
                ms.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // đảm bảo ảnh được tải ngay lập tức và không bị khóa file
                bitmapImage.StreamSource = ms; // sử dụng MemoryStream làm nguồn dữ liệu cho BitmapImage
                bitmapImage.EndInit(); 
                return bitmapImage;
            }
        }

        async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LayMaQR();
        }

        void BtnDong_Click(object sender, RoutedEventArgs e)
        {
            _qrCancellationTokenSource?.Cancel();
            this.Close();
        }

        void HienThiThanhToanThanhCong()
        {
            imgQRCode.Visibility = Visibility.Collapsed;
            txtCountdown.Visibility = Visibility.Collapsed;
            loadingOverlay.Visibility = Visibility.Collapsed;
            failPanel.Visibility = Visibility.Collapsed;
            successPanel.Visibility = Visibility.Visible;
            txtSuccessAmount.Text = $"Hóa đơn #{_maHd} — {_soTien:N0} ₫";
            txtStatus.Text = "Đã thanh toán";
            statusDot.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
        }

        void HienThiThanhToanThatBai(string lyDo = "Giao dịch bị hủy.")
        {
            imgQRCode.Visibility = Visibility.Collapsed;
            txtCountdown.Visibility = Visibility.Collapsed;
            loadingOverlay.Visibility = Visibility.Collapsed;
            successPanel.Visibility = Visibility.Collapsed;
            failPanel.Visibility = Visibility.Visible;
            txtFailReason.Text = lyDo;
            txtStatus.Text = "Thanh toán thất bại";
            statusDot.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 71, 68));
        }

        void StartQRCountdown(int seconds)
        {
            int remaining = seconds; // biến đếm thời gian còn lại bắt đầu từ số giây được truyền vào
            var timer = new System.Windows.Threading.DispatcherTimer(); // tạo 1 bộ đếm thời gian 
            timer.Interval = TimeSpan.FromSeconds(1); // cứ 1 giây chạy 1 lần

            timer.Tick += (s, e) => 
            {
                remaining--;
                txtCountdown.Text = $"Mã QR hết hạn sau: {remaining}s";

                if (remaining <= 0)
                {
                    timer.Stop();
                    imgQRCode.Source = null;
                    txtCountdown.Text = "Mã QR đã hết hạn. Vui lòng bấm Làm mới.";
                }
            };

            timer.Start();
        }

        void StartPollingTrangThai()
        {
            var timer = new System.Windows.Threading.DispatcherTimer(); 
            timer.Interval = TimeSpan.FromSeconds(3); // cứ 3 giây kiểm tra trạng thái thanh toán để khi khách hàng quét mã thì cập nhật ngay 

            timer.Tick += async (s, e) =>
            {
                try
                {
                    using var client = new System.Net.Http.HttpClient();
                    var response = await client.GetAsync(
                        $"http://localhost:5048/api/payment/check-status/{_maHd}"); // kiểm tra trạng thái thanh toán 

                    if (response.IsSuccessStatusCode) // nếu API trả về thành công thì đọc nội dung phản hồi để lấy trạng thái thanh toán
                    {
                        var json = await response.Content.ReadAsStringAsync(); // đọc nội dung phản hồi dưới dạng chuỗi JSON
                        var data = System.Text.Json.JsonDocument.Parse(json).RootElement; // phân tích cú pháp JSON để lấy đối tượng gốc
                        string trangThai = data.GetProperty("trangThai").GetString(); // lấy giá trị trạng thái từ đối tượng JSON

                        if (trangThai == "Đã thanh toán")
                        {
                            timer.Stop();
                            HienThiThanhToanThanhCong();
                            _onThanhToanThanhCong?.Invoke();
                            MessageBox.Show("Thanh toán MoMo thành công!",
                                "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (trangThai == "Thanh toán thất bại")
                        {
                            timer.Stop();
                            HienThiThanhToanThatBai("Giao dịch bị hủy.");
                        }
                    }
                }
                catch { } // lỗi 
            };

            timer.Start();// bắt đầu cứ 3 giây là kiểm tra trạng thái 
        }
    }
}