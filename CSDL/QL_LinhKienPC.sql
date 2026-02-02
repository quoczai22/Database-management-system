CREATE DATABASE QL_LinhKien_PC
ON PRIMARY
(
    NAME = QL_LinhKien_Primary,
    FILENAME = 'D:\BaiTap\HQTCSDL\QL_LinhKien_Main.mdf',
    SIZE = 10MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
),
(
    NAME = QL_LinhKien_Secondary,
    FILENAME = 'D:\BaiTap\HQTCSDL\QL_LinhKien_Sub.ndf',
    SIZE = 5MB,
    MAXSIZE = 50MB,
    FILEGROWTH = 1MB
)
LOG ON
(
    NAME = QL_LinhKien_Log,
    FILENAME = 'D:\BaiTap\HQTCSDL\QL_LinhKien_Log.ldf',
    SIZE = 5MB,
    MAXSIZE = 20MB,
    FILEGROWTH = 1MB
)

USE QL_LinhKien_PC

SET DATEFORMAT DMY

CREATE TABLE LoaiLK
(
    MaLoai char(3) NOT NULL,
    TenLoai nvarchar(40),
    CONSTRAINT PK_LoaiLK PRIMARY KEY (MaLoai)
)

CREATE TABLE LinhKien
(
    MaLK char(6) NOT NULL,
    TenLK nvarchar(50),
    NgaySX date,
    TGBH tinyint,
    MaLoai char(3) NOT NULL,
    NSX nvarchar(40),
    DVT nvarchar(10),
    CONSTRAINT PK_LinhKien PRIMARY KEY (MaLK),
    CONSTRAINT FK_LK_LoaiLK FOREIGN KEY (MaLoai) REFERENCES LoaiLK(MaLoai)
)

CREATE TABLE KhachHang
(
    MaKH char(6) NOT NULL,
    TenKH nvarchar(30),
    DChi nvarchar(50),
    DThoai char(10),
    CONSTRAINT PK_KhachHang PRIMARY KEY (MaKH)
)

CREATE TABLE NhanVien
(
    MaNV char(6) NOT NULL,
    TenNV nvarchar(40),
    GioiTinh nvarchar(5),
    NgaySinh date,
    SDT char(10),
    CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV)
)

CREATE TABLE HoaDon
(   
    MaHD char(5) NOT NULL,
    NgayHD date,
    MaKH char(6) NOT NULL,
    MaNV char(6) NOT NULL,
    TongTien int NULL DEFAULT 0,
    CONSTRAINT PK_HoaDon PRIMARY KEY (MaHD),
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
)

CREATE TABLE ChiTietHD
(
    MaHD char(5) NOT NULL,
    MaLK char(6) NOT NULL,
    SoLuong tinyint, 
    DonGia int,
    CONSTRAINT PK_CTHD PRIMARY KEY (MaHD, MaLK),
    CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    CONSTRAINT FK_CTHD_LinhKien FOREIGN KEY (MaLK) REFERENCES LinhKien(MaLK) 
)

INSERT INTO LoaiLK VALUES
('MOU', N'Chuột máy tính'),
('LAP', N'Máy tính xách tay'),
('CPU', N'Bộ vi xử lý'),
('PCX', N'Máy tính để bàn'),
('MAI', N'Bo mạch chủ (Mainboard)'),
('RAM', N'Bộ nhớ trong (RAM)'),
('HDD', N'Ổ cứng HDD'),
('SSD', N'Ổ cứng SSD'),
('VGA', N'Card màn hình'),
('KEY', N'Bàn phím cơ')

INSERT INTO LinhKien VALUES
('MOU001', N'Chuột quang có dây', '01-01-2023', 12, 'MOU', 'Genius', N'Cái'),
('MOU002', N'Chuột Logitech G102', '04-02-2023', 24, 'MOU', 'Logitech', N'Cái'),
('RAM001', N'RAM Kingston 8GB', '15-05-2023', 36, 'RAM', 'Kingston', N'Thanh'),
('CPU001', N'CPU Intel Core i5', '05-04-2023', 36, 'CPU', 'Intel', N'Con'),
('CPU002', N'CPU AMD Ryzen 5', '07-02-2023', 36, 'CPU', 'AMD', N'Con'),
('MAI001', N'Mainboard ASUS B450', '04-12-2023', 36, 'MAI', 'ASUS', N'Cái'),
('SSD001', N'SSD Samsung 500GB', '03-03-2023', 60, 'SSD', 'Samsung', N'Cái'),
('VGA001', N'VGA RTX 3060', '14-04-2023', 36, 'VGA', 'Gigabyte', N'Cái'),
('KEY001', N'Phím cơ Keychron K2', '19-10-2023', 12, 'KEY', 'Keychron', N'Cái'),
('PCX001', N'PC Gaming H510', '20-11-2023', 24, 'PCX', 'H hành', N'Bộ')

INSERT INTO KhachHang VALUES
('KH001', N'Ngụy Hạo Nhiên', N'Thanh Hóa', '0989751723'),
('KH002', N'Đinh Bảo Lộc', N'Lâm Đồng', '0918234654'),
('KH003', N'Trần Thanh Diệu', N'TP.HCM', '0978123765'),
('KH004', N'Nguyễn Nhật Minh Quân', N'TP.HCM', '0909456768'),
('KH005', N'Huỳnh Kim Ánh', N'Khánh Hòa', '0932987567'),
('KH006', N'Lê Văn Việt', N'Đà Nẵng', '0905123456'),
('KH007', N'Lương Văn Quan', N'Long An', '0913567890'),
('KH008', N'Vũ Thị Mai', N'Hải Phòng', '0988666777'),
('KH009', N'Trịnh Hữu Kiến Quốc', N'TP.HCM', '0933444555'),
('KH010', N'Hồ Đại Phong', N'Kon Tum', '0977888999')

INSERT INTO NhanVien VALUES
('NV001', N'Phạm Văn Mách', N'Nam', '15-05-1995', '0901234567'),
('NV002', N'Trần Thị Dung', N'Nữ', '20-10-1998', '0902234567'),
('NV003', N'Lê Văn Anh', N'Nam', '05-09-1992', '0903234567'),
('NV004', N'Nguyễn Thị Điệp', N'Nữ', '12-12-2000', '0904234567'),
('NV005', N'Hoàng Văn Tuấn', N'Nam', '01-01-1997', '0905234567'),
('NV006', N'Đặng Thị Hà Anh', N'Nữ', '14-02-1999', '0906234567'),
('NV007', N'Bùi Văn Quốc', N'Nam', '30-04-1994', '0907234567'),
('NV008', N'Đỗ Thị Ngọc Huyền', N'Nữ', '02-09-1996', '0908234567'),
('NV009', N'Võ Văn An', N'Nam', '22-12-1993', '0909234567'),
('NV010', N'Lý Thị Nhung', N'Nữ', '08-03-2001', '0910234567')

INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV) VALUES
('HD001', '01-04-2023', 'KH001', 'NV001'),
('HD002', '15-05-2023', 'KH005', 'NV002'),
('HD003', '14-06-2023', 'KH004', 'NV001'),
('HD004', '03-06-2023', 'KH005', 'NV003'),
('HD005', '05-06-2023', 'KH001', 'NV002'),
('HD006', '07-07-2023', 'KH003', 'NV004'),
('HD007', '12-08-2023', 'KH002', 'NV005'),
('HD008', '25-09-2023', 'KH003', 'NV001'),
('HD009', '10-10-2023', 'KH008', 'NV006'),
('HD010', '11-11-2023', 'KH010', 'NV007')

INSERT INTO ChiTietHD VALUES
('HD001', 'MOU001', 2, 150000),
('HD002', 'MOU002', 1, 450000),
('HD003', 'RAM001', 2, 850000),
('HD004', 'CPU001', 1, 4500000),
('HD005', 'CPU002', 1, 3200000),
('HD006', 'MAI001', 1, 1800000),
('HD007', 'SSD001', 2, 1200000),
('HD007', 'VGA001', 1, 8900000),
('HD008', 'KEY001', 1, 1650000),
('HD009', 'PCX001', 1, 10500000),
('HD010', 'MOU001', 5, 140000)

UPDATE HoaDon
SET TongTien = (
    SELECT SUM(ct.SoLuong * ct.DonGia)
    FROM ChiTietHD ct
    WHERE ct.MaHD = HoaDon.MaHD
)
WHERE EXISTS (
    SELECT 1 FROM ChiTietHD ct WHERE ct.MaHD = HoaDon.MaHD
)

SELECT * FROM LoaiLK
SELECT * FROM LinhKien
SELECT * FROM KhachHang
SELECT * FROM NhanVien
SELECT * FROM HoaDon
SELECT * FROM ChiTietHD