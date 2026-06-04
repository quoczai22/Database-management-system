using QuanLyLinhKienMayTinh.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;

namespace QuanLyLinhKienMayTinh.Helpers
{
    public static class ExcelExportHelper
    {
        public static void XuatHoaDon(string filePath, HoaDonDisplay hoaDon, IEnumerable<ChiTietSanPhamDisplay> chiTiet)
        {
            var rows = new List<string[]>
            {
                new[] { "HÓA ĐƠN BÁN HÀNG" },
                new[] { "Mã hóa đơn", hoaDon.MaHoaDon },
                new[] { "Ngày lập", hoaDon.NgayTao?.ToString("dd/MM/yyyy") ?? "" },
                new[] { "Khách hàng", hoaDon.TenKhachHang },
                new[] { "Số điện thoại", hoaDon.SoDienThoai },
                new[] { "Nhân viên", hoaDon.TenNhanVien },
                new[] { "" },
                new[] { "STT", "Tên linh kiện", "Số lượng", "Đơn giá", "Thành tiền" }
            };

            int stt = 1;
            decimal tongTien = 0;
            foreach (var item in chiTiet)
            {
                decimal soLuong = item.SoLuong ?? 0;
                decimal donGia = item.DonGia ?? 0;
                decimal thanhTien = soLuong * donGia;
                tongTien += thanhTien;
                rows.Add(new[] { stt.ToString(), item.TenSanPham, soLuong.ToString(CultureInfo.InvariantCulture), donGia.ToString(CultureInfo.InvariantCulture), thanhTien.ToString(CultureInfo.InvariantCulture) });
                stt++;
            }

            rows.Add(new[] { "", "", "", "Tổng cộng", (hoaDon.TongTien ?? tongTien).ToString(CultureInfo.InvariantCulture) });
            GhiWorkbook(filePath, "HoaDon", rows);
        }

        public static void XuatPhieuNhap(string filePath, PhieuNhapDisplay phieuNhap, IEnumerable<ChiTietLinhKienNhapDisplay> chiTiet)
        {
            var rows = new List<string[]>
            {
                new[] { "PHIẾU NHẬP KHO" },
                new[] { "Mã phiếu nhập", phieuNhap.MaPN },
                new[] { "Ngày nhập", phieuNhap.NgayNhap?.ToString("dd/MM/yyyy") ?? "" },
                new[] { "Nhân viên", phieuNhap.TenNhanVien },
                new[] { "Nhà cung cấp", phieuNhap.TenNhaCungCap },
                new[] { "Số điện thoại NCC", phieuNhap.SoDienThoaiNCC },
                new[] { "" },
                new[] { "STT", "Mã linh kiện", "Tên linh kiện", "Số lượng nhập", "Đơn giá nhập", "Thành tiền" }
            };

            int stt = 1;
            decimal tongTien = 0;
            foreach (var item in chiTiet)
            {
                decimal thanhTien = item.ThanhTien;
                tongTien += thanhTien;
                rows.Add(new[]
                {
                    stt.ToString(),
                    item.MaLinhKien,
                    item.TenLinhKien,
                    item.SoLuongNhap.ToString(CultureInfo.InvariantCulture),
                    item.DonGiaNhap.ToString(CultureInfo.InvariantCulture),
                    thanhTien.ToString(CultureInfo.InvariantCulture)
                });
                stt++;
            }

            rows.Add(new[] { "", "", "", "", "Tổng chi phí", (phieuNhap.TongTien != 0 ? phieuNhap.TongTien : tongTien).ToString(CultureInfo.InvariantCulture) });
            GhiWorkbook(filePath, "PhieuNhap", rows);
        }

        private static void GhiWorkbook(string filePath, string sheetName, IEnumerable<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
            sb.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
            sb.AppendLine("<Worksheet ss:Name=\"" + Escape(sheetName) + "\"><Table>");

            foreach (var row in rows)
            {
                sb.AppendLine("<Row>");
                foreach (var value in row)
                {
                    if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
                    {
                        sb.AppendLine("<Cell><Data ss:Type=\"Number\">" + Escape(value) + "</Data></Cell>");
                    }
                    else
                    {
                        sb.AppendLine("<Cell><Data ss:Type=\"String\">" + Escape(value) + "</Data></Cell>");
                    }
                }
                sb.AppendLine("</Row>");
            }

            sb.AppendLine("</Table></Worksheet></Workbook>");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static string Escape(string value)
        {
            return SecurityElement.Escape(value ?? string.Empty) ?? string.Empty;
        }
    }
}
