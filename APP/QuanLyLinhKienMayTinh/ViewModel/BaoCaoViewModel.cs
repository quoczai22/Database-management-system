using QuanLyLinhKienMayTinh.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace QuanLyLinhKienMayTinh.ViewModels
{
    public class ThongKeBanHang
    {
        public string HangSX { get; set; }
        public int SoLuongBan { get; set; }
        public double DoanhThu { get; set; }
        public double GiaTrungBinh { get; set; }
        public int SoDonHang { get; set; }
    }

    public class BaoCaoViewModel : BaseViewModel
    {
        public FlowDocument TaoBaoCaoHoaDon(HoaDonDisplay hd, IEnumerable<ChiTietSanPhamDisplay> chiTiet)
        {
            var document = TaoTaiLieuMacDinh();

            var header = new Paragraph
            {
                TextAlignment = TextAlignment.Center, // căn giữa
                LineHeight = 24 // khoảng cách giữa các dòng trong phần header
            };
            header.Inlines.Add(new Run("CỬA HÀNG LINH KIỆN MÁY TÍNH NHÓM 8")
            {
                FontWeight = FontWeights.Bold,
                FontSize = 18
            });
            header.Inlines.Add(new LineBreak());// xuống dòng
            header.Inlines.Add(new Run("Địa chỉ: Trường Đại học Công Thương TP.HCM"));// thêm địa chỉ cửa hàng
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("Điện thoại: 0901 234 567")); 
            document.Blocks.Add(header);

            var title = new Paragraph(new Run("HÓA ĐƠN BÁN HÀNG"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 22,
                Margin = new Thickness(0, 18, 0, 14)
            };
            document.Blocks.Add(title);

            var info = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 14)
            };
            info.Columns.Add(new TableColumn { Width = new GridLength(190) }); // cột 1 rộng 190
            info.Columns.Add(new TableColumn { Width = new GridLength(230) }); 
            info.Columns.Add(new TableColumn { Width = new GridLength(150) }); 
            info.Columns.Add(new TableColumn { Width = new GridLength(180) }); 
            var infoGroup = new TableRowGroup();
            info.RowGroups.Add(infoGroup);
            ThemDongThongTin(infoGroup, "Mã hóa đơn:", hd.MaHoaDon, "Ngày lập:", $"{hd.NgayTao:dd/MM/yyyy}"); // hiển thị ngày tháng theo định dạng dd/MM/yyyy
            ThemDongThongTin(infoGroup, "Khách hàng:", hd.TenKhachHang, "SĐT:", hd.SoDienThoai);
            ThemDongThongTin(infoGroup, "Nhân viên:", hd.TenNhanVien, "Thanh toán:", hd.PhuongThucThanhToan);
            document.Blocks.Add(info);

            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(45) });// cột STT
            table.Columns.Add(new TableColumn { Width = new GridLength(310) });
            table.Columns.Add(new TableColumn { Width = new GridLength(75) });
            table.Columns.Add(new TableColumn { Width = new GridLength(120) });
            table.Columns.Add(new TableColumn { Width = new GridLength(130) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            var headerRow = new TableRow();
            group.Rows.Add(headerRow);
            ThemCell(headerRow, "STT", true, TextAlignment.Center); // thêm tiêu đề cột STT, căn giữa, in đậm
            ThemCell(headerRow, "Tên linh kiện", true, TextAlignment.Center);
            ThemCell(headerRow, "SL", true, TextAlignment.Center);
            ThemCell(headerRow, "Đơn giá", true, TextAlignment.Center);
            ThemCell(headerRow, "Thành tiền", true, TextAlignment.Center);

            int stt = 1;
            decimal tongTien = 0;
            foreach (var item in chiTiet)
            {
                decimal soLuong = item.SoLuong ?? 0;
                decimal donGia = item.DonGia ?? 0;
                decimal thanhTien = soLuong * donGia;
                tongTien += thanhTien;

                var row = new TableRow();
                group.Rows.Add(row);
                ThemCell(row, stt.ToString(), false, TextAlignment.Center); // thêm số thứ tự, căn giữa
                ThemCell(row, item.TenSanPham, false, TextAlignment.Left);
                ThemCell(row, soLuong.ToString(), false, TextAlignment.Center);
                ThemCell(row, $"{donGia:N0}", false, TextAlignment.Right);
                ThemCell(row, $"{thanhTien:N0}", false, TextAlignment.Right);
                stt++;
            }

            var totalRow = new TableRow();
            group.Rows.Add(totalRow);
            var totalLabel = TaoCell("TỔNG CỘNG", true, TextAlignment.Right); // tạo ô "TỔNG CỘNG", in đậm, căn phải
            totalLabel.ColumnSpan = 4;
            totalRow.Cells.Add(totalLabel);
            totalRow.Cells.Add(TaoCell($"{(hd.TongTien ?? tongTien):N0} VNĐ", true, TextAlignment.Right)); // hiển thị tổng tiền từ hóa đơn nếu có, nếu không thì hiển thị tổng tính toán, in đậm, căn phải
            document.Blocks.Add(table);

            var note = new Paragraph(new Run("Ghi chú: Hóa đơn được tạo từ phần mềm Quản lý Linh Kiện Máy Tính."))
            {
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 12, 0, 18)
            };
            document.Blocks.Add(note); // thêm ghi chú về nguồn gốc hóa đơn

            ThemBangChuKy(document, "Khách hàng", "Người lập hóa đơn", 16);
            return document;
        }

        public FlowDocument TaoBaoCaoDoanhThuTheoHang(IEnumerable<ThongKeBanHang> danhSachThongKe) // hàm tạo báo cáo doanh thu theo hãng sản xuất, nhận vào một tập hợp các đối tượng ThongKeBanHang để hiển thị dữ liệu thống kê trong báo cáo
        {
            var document = TaoTaiLieuMacDinh();
            var danhSach = danhSachThongKe.ToList();

            var header = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                LineHeight = 24
            };
            header.Inlines.Add(new Run("CỬA HÀNG LINH KIỆN MÁY TÍNH NHÓM 8")
            {
                FontWeight = FontWeights.Bold,
                FontSize = 18
            });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("Báo cáo được lập từ dữ liệu hóa đơn bán hàng"));
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}"));
            document.Blocks.Add(header);

            var title = new Paragraph(new Run("BÁO CÁO DOANH THU THEO HÃNG SẢN XUẤT"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 21,
                Margin = new Thickness(0, 18, 0, 12)
            };
            document.Blocks.Add(title);

            var description = new Paragraph(new Run("Báo cáo gom nhóm dữ liệu chi tiết hóa đơn theo hãng sản xuất, thống kê số lượng bán, doanh thu, giá bán trung bình và số đơn hàng phát sinh."))
            {
                TextAlignment = TextAlignment.Justify,
                Margin = new Thickness(0, 0, 0, 12)
            };
            document.Blocks.Add(description);

            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(45) });
            table.Columns.Add(new TableColumn { Width = new GridLength(185) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });
            table.Columns.Add(new TableColumn { Width = new GridLength(140) });
            table.Columns.Add(new TableColumn { Width = new GridLength(115) });
            table.Columns.Add(new TableColumn { Width = new GridLength(90) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            var headerRow = new TableRow();
            group.Rows.Add(headerRow);
            ThemCell(headerRow, "STT", true, TextAlignment.Center);
            ThemCell(headerRow, "Hãng sản xuất", true, TextAlignment.Center);
            ThemCell(headerRow, "SL bán", true, TextAlignment.Center);
            ThemCell(headerRow, "Doanh thu", true, TextAlignment.Center);
            ThemCell(headerRow, "Giá TB", true, TextAlignment.Center);
            ThemCell(headerRow, "Số đơn", true, TextAlignment.Center);

            int stt = 1;
            int tongSoLuong = 0;
            double tongDoanhThu = 0;
            int tongSoDon = 0;

            foreach (var item in danhSach.OrderByDescending(x => x.DoanhThu))
            {
                tongSoLuong += item.SoLuongBan;
                tongDoanhThu += item.DoanhThu;
                tongSoDon += item.SoDonHang;

                var row = new TableRow();
                group.Rows.Add(row);
                ThemCell(row, stt.ToString(), false, TextAlignment.Center);
                ThemCell(row, item.HangSX ?? "Không xác định", false, TextAlignment.Left);
                ThemCell(row, item.SoLuongBan.ToString(), false, TextAlignment.Center);
                ThemCell(row, $"{item.DoanhThu:N0}", false, TextAlignment.Right);
                ThemCell(row, $"{item.GiaTrungBinh:N0}", false, TextAlignment.Right);
                ThemCell(row, item.SoDonHang.ToString(), false, TextAlignment.Center);
                stt++;
            }

            var totalRow = new TableRow();
            group.Rows.Add(totalRow);
            var totalLabel = TaoCell("TỔNG CỘNG", true, TextAlignment.Right);
            totalLabel.ColumnSpan = 2;
            totalRow.Cells.Add(totalLabel);
            totalRow.Cells.Add(TaoCell(tongSoLuong.ToString(), true, TextAlignment.Center));
            totalRow.Cells.Add(TaoCell($"{tongDoanhThu:N0}", true, TextAlignment.Right));
            totalRow.Cells.Add(TaoCell("-", true, TextAlignment.Center));
            totalRow.Cells.Add(TaoCell(tongSoDon.ToString(), true, TextAlignment.Center));
            document.Blocks.Add(table);

            var summary = new Paragraph
            {
                Margin = new Thickness(0, 14, 0, 10),
                LineHeight = 23
            };
            summary.Inlines.Add(new Run("Tổng hợp: ") { FontWeight = FontWeights.Bold });
            summary.Inlines.Add(new Run($"{danhSach.Count} hãng sản xuất, {tongSoLuong} sản phẩm đã bán, {tongSoDon} đơn hàng, tổng doanh thu {tongDoanhThu:N0} VNĐ."));
            document.Blocks.Add(summary);

            ThemBangChuKy(document, "Người lập báo cáo", "Quản lý", 20);
            return document;
        }

        public FlowDocument TaoBaoCaoPhieuNhap(PhieuNhapDisplay pn, IEnumerable<ChiTietLinhKienNhapDisplay> chiTiet)
        {
            var document = TaoTaiLieuMacDinh();

            var header = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                LineHeight = 24
            };
            header.Inlines.Add(new Run("CỬA HÀNG LINH KIỆN MÁY TÍNH NHÓM 8")
            {
                FontWeight = FontWeights.Bold,
                FontSize = 18
            });
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("Địa chỉ: Trường Đại học Công Thương TP.HCM"));
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run("Điện thoại: 0901 234 567"));
            document.Blocks.Add(header);

            var title = new Paragraph(new Run("PHIẾU NHẬP KHO"))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 22,
                Margin = new Thickness(0, 18, 0, 14)
            };
            document.Blocks.Add(title);

            var info = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, 0, 0, 14)
            };
            info.Columns.Add(new TableColumn { Width = new GridLength(190) });
            info.Columns.Add(new TableColumn { Width = new GridLength(230) });
            info.Columns.Add(new TableColumn { Width = new GridLength(150) });
            info.Columns.Add(new TableColumn { Width = new GridLength(180) });
            var infoGroup = new TableRowGroup();
            info.RowGroups.Add(infoGroup);
            ThemDongThongTin(infoGroup, "Mã phiếu nhập:", pn.MaPN, "Ngày nhập:", $"{pn.NgayNhap:dd/MM/yyyy}");
            ThemDongThongTin(infoGroup, "Nhân viên:", pn.TenNhanVien, "Nhà cung cấp:", pn.TenNhaCungCap);
            ThemDongThongTin(infoGroup, "SĐT NCC:", pn.SoDienThoaiNCC, "Tổng số lượng:", $"{pn.TongSoLuong}");
            document.Blocks.Add(info);

            var table = new Table
            {
                CellSpacing = 0,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            table.Columns.Add(new TableColumn { Width = new GridLength(45) });
            table.Columns.Add(new TableColumn { Width = new GridLength(335) });
            table.Columns.Add(new TableColumn { Width = new GridLength(85) });
            table.Columns.Add(new TableColumn { Width = new GridLength(130) });
            table.Columns.Add(new TableColumn { Width = new GridLength(130) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);
            var headerRow = new TableRow();
            group.Rows.Add(headerRow);
            ThemCell(headerRow, "STT", true, TextAlignment.Center);
            ThemCell(headerRow, "Tên linh kiện", true, TextAlignment.Center);
            ThemCell(headerRow, "SL nhập", true, TextAlignment.Center);
            ThemCell(headerRow, "Đơn giá nhập", true, TextAlignment.Center);
            ThemCell(headerRow, "Thành tiền", true, TextAlignment.Center);

            int stt = 1;
            decimal tongTien = 0;
            foreach (var item in chiTiet)
            {
                int soLuong = item.SoLuongNhap;
                decimal donGia = item.DonGiaNhap;
                decimal thanhTien = soLuong * donGia;
                tongTien += thanhTien;

                var row = new TableRow();
                group.Rows.Add(row);
                ThemCell(row, stt.ToString(), false, TextAlignment.Center);
                ThemCell(row, item.TenLinhKien, false, TextAlignment.Left);
                ThemCell(row, soLuong.ToString(), false, TextAlignment.Center);
                ThemCell(row, $"{donGia:N0}", false, TextAlignment.Right);
                ThemCell(row, $"{thanhTien:N0}", false, TextAlignment.Right);
                stt++;
            }

            var totalRow = new TableRow();
            group.Rows.Add(totalRow);
            var totalLabel = TaoCell("TỔNG CHI PHÍ", true, TextAlignment.Right);
            totalLabel.ColumnSpan = 4;
            totalRow.Cells.Add(totalLabel);
            totalRow.Cells.Add(TaoCell($"{(pn.TongTien != 0 ? pn.TongTien : tongTien):N0} VNĐ", true, TextAlignment.Right));
            document.Blocks.Add(table);

            var note = new Paragraph(new Run("Ghi chú: Phiếu nhập được tạo từ phần mềm Quản lý Linh Kiện Máy Tính."))
            {
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 12, 0, 18)
            };
            document.Blocks.Add(note);

            ThemBangChuKy(document, "Người lập phiếu", "Thủ kho", 16);
            return document;
        }

        private FlowDocument TaoTaiLieuMacDinh() // hàm tạo một đối tượng FlowDocument với các thiết lập mặc định về kích thước trang, lề, font chữ và cỡ chữ để đảm bảo tính nhất quán
        {
            return new FlowDocument
            {
                PageWidth = 793,// kích thước trang A4 theo chiều rộng
                PageHeight = 1122,// kích thước trang A4 theo chiều cao
                PagePadding = new Thickness(48),// lề trang 48 điểm (tương đương 0.5 inch)
                FontFamily = new FontFamily("Times New Roman"), // sử dụng font chữ Times New Roman cho toàn bộ tài liệu
                FontSize = 14 // cỡ chữ mặc định 14 điểm
            };
        }

        private void ThemDongThongTin(TableRowGroup group, string label1, string value1, string label2, string value2)// hàm này thêm một dòng thông tin vào một nhóm dòng trong bảng, nhận vào hai cặp nhãn và giá trị để hiển thị thông tin chi tiết của hóa đơn hoặc báo cáo
        {
            var row = new TableRow();
            group.Rows.Add(row);
            row.Cells.Add(TaoCell(label1, true, TextAlignment.Left, false));
            row.Cells.Add(TaoCell(value1 ?? string.Empty, false, TextAlignment.Left, false));
            row.Cells.Add(TaoCell(label2, true, TextAlignment.Left, false));
            row.Cells.Add(TaoCell(value2 ?? string.Empty, false, TextAlignment.Left, false));
        }

        private void ThemCell(TableRow row, string text, bool bold, TextAlignment alignment)// hàm này thêm một ô vào một dòng trong bảng, nhận vào nội dung văn bản, kiểu chữ in đậm hay không và căn chỉnh văn bản để tạo ra các ô dữ liệu trong bảng báo cáo
        {
            row.Cells.Add(TaoCell(text, bold, alignment));
        }

        private TableCell TaoCell(string text, bool bold, TextAlignment alignment, bool border = true) // hàm này tạo một ô trong bảng với nội dung văn bản, kiểu chữ in đậm hay không, căn chỉnh văn bản và có đường viền hay không để sử dụng cho cả phần tiêu đề và phần dữ liệu trong bảng báo cáo
        {
            var paragraph = new Paragraph(new Run(text ?? string.Empty))
            {
                TextAlignment = alignment,// căn chỉnh văn bản theo tham số truyền vào
                Margin = new Thickness(4, 3, 4, 3) // khoảng cách giữa các ô trong bảng
            };
            return new TableCell(paragraph) // tạo ô với đoạn văn bản đã định dạng
            {
                Padding = new Thickness(4),
                BorderBrush = Brushes.Black,
                BorderThickness = border ? new Thickness(0.5) : new Thickness(0),
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
            };
        }

        private void ThemBangChuKy(FlowDocument document, string cotTrai, string cotPhai, int topMargin) // hàm này thêm một bảng vào cuối tài liệu để hiển thị phần chữ ký của người lập báo cáo và quản lý, nhận vào tiêu đề của hai cột chữ ký và khoảng cách từ phần nội dung chính đến phần chữ ký
        {
            var footer = new Table
            {
                CellSpacing = 0,
                Margin = new Thickness(0, topMargin, 0, 0)
            };
            footer.Columns.Add(new TableColumn { Width = new GridLength(350) });
            footer.Columns.Add(new TableColumn { Width = new GridLength(350) });
            var footerGroup = new TableRowGroup();
            footer.RowGroups.Add(footerGroup);
            var footerRow = new TableRow();
            footerGroup.Rows.Add(footerRow);
            footerRow.Cells.Add(TaoChuKy(cotTrai));
            footerRow.Cells.Add(TaoChuKy(cotPhai));
            document.Blocks.Add(footer);
        }

        private TableCell TaoChuKy(string title) // hàm tạo một ô trong bảng để hiển thị phần chữ ký, nhận vào tiêu đề của phần chữ ký 
        {
            var paragraph = new Paragraph
            {
                TextAlignment = TextAlignment.Center,// căn giữa nội dung trong ô chữ ký
                LineHeight = 24 // khoảng cách giữa các dòng trong phần chữ ký để tạo không gian cho chữ ký và tên người ký
            };
            paragraph.Inlines.Add(new Run(title) { FontWeight = FontWeights.Bold });
            paragraph.Inlines.Add(new LineBreak());// xuống dòng
            paragraph.Inlines.Add(new Run("(Ký và ghi rõ họ tên)"));
            paragraph.Inlines.Add(new LineBreak());// xuống dòng để tạo khoảng cách giữa phần tiêu đề và phần ghi chú về chữ ký
            paragraph.Inlines.Add(new LineBreak());
            paragraph.Inlines.Add(new LineBreak());
            return new TableCell(paragraph); // tạo ô chữ ký với đoạn văn bản đã định dạng
        }
    }
}
