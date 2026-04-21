--trước khi chạy phải tạo folder SQLData trong ổ C nếu ko có sẽ lỗi
USE master;
GO

DROP DATABASE IF EXISTS QL_LinhKien_PC;
GO

CREATE DATABASE QL_LinhKien_PC
ON PRIMARY
(
    NAME = QL_LinhKien_Primary,
    FILENAME = 'C:\SQLData\QL_LinhKien_Main.mdf',
    SIZE = 15MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
),
(
    NAME = QL_LinhKien_Secondary_NET,
    FILENAME = 'C:\SQLData\QL_LinhKien_Sub.ndf', 
    SIZE = 5MB,
    MAXSIZE = 50MB,
    FILEGROWTH = 1MB
)
LOG ON
(
    NAME = QL_LinhKien_Log_NET,
    FILENAME = 'C:\SQLData\QL_LinhKien_Log.ldf',
    SIZE = 5MB,
    MAXSIZE = 20MB,
    FILEGROWTH = 1MB
);
GO

USE QL_LinhKien_PC;
GO
SET DATEFORMAT DMY; 
GO

--tạo bảng

CREATE TABLE NhaSanXuat (
    MaNSX char(5) NOT NULL,
    TenNSX nvarchar(50),
    QuocGia nvarchar(50),
    CONSTRAINT PK_NhaSanXuat PRIMARY KEY (MaNSX)
);

CREATE TABLE LoaiLK (
    MaLoai char(3) NOT NULL,
    TenLoai nvarchar(40),
    MoTa nvarchar(100), 
    CONSTRAINT PK_LoaiLK PRIMARY KEY (MaLoai)
);

CREATE TABLE LinhKien (
    MaLK char(6) NOT NULL,
    TenLK nvarchar(50),
    NgayNhap date, 
    TGBH tinyint,
    MaLoai char(3) NOT NULL,
    MaNSX char(5) NOT NULL,
    DVT nvarchar(10),
    SoLuongTon int DEFAULT 0,
    DonGiaBan int,
    CONSTRAINT PK_LinhKien PRIMARY KEY (MaLK),
    CONSTRAINT FK_LK_LoaiLK FOREIGN KEY (MaLoai) REFERENCES LoaiLK(MaLoai),
    CONSTRAINT FK_LK_NhaSanXuat FOREIGN KEY (MaNSX) REFERENCES NhaSanXuat(MaNSX)
);

CREATE TABLE KhachHang (
    MaKH char(6) NOT NULL,
    TenKH nvarchar(30),
    DChi nvarchar(50),
    SDT char(10),
    Email varchar(50) NULL,
    CONSTRAINT PK_KhachHang PRIMARY KEY (MaKH)
);

CREATE TABLE NhanVien (
    MaNV char(6) NOT NULL,
    TenNV nvarchar(40),
    GioiTinh nvarchar(5),
    NgaySinh date,
    SDT char(10),
    ChucVu nvarchar(20),
    Quyen nvarchar(20),
    Email varchar(50) NULL,      
    NgayVaoLam date NULL,      
    CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV)
);

CREATE TABLE HoaDon (   
    MaHD char(5) NOT NULL,
    NgayHD date,
    MaKH char(6) NOT NULL,
    MaNV char(6) NOT NULL,
    TongTien int NULL DEFAULT 0,
    TrangThai nvarchar(30) DEFAULT N'Chưa thanh toán',
    CONSTRAINT PK_HoaDon PRIMARY KEY (MaHD),
    CONSTRAINT FK_HoaDon_KhachHang FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    CONSTRAINT FK_HoaDon_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietHD (
    MaHD char(5) NOT NULL,
    MaLK char(6) NOT NULL,
    SoLuong tinyint, 
    DonGia int,
    CONSTRAINT PK_CTHD PRIMARY KEY (MaHD, MaLK),
    CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    CONSTRAINT FK_CTHD_LinhKien FOREIGN KEY (MaLK) REFERENCES LinhKien(MaLK) 
);

CREATE TABLE PhieuNhap (
    MaPN char(5) NOT NULL,
    NgayNhap date,
    MaNV char(6) NOT NULL,
    CONSTRAINT PK_PhieuNhap PRIMARY KEY (MaPN),
    CONSTRAINT FK_PhieuNhap_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietPN (
    MaPN char(5) NOT NULL,
    MaLK char(6) NOT NULL,
    SoLuongNhap int,
    DonGiaNhap int,
    CONSTRAINT PK_CTPN PRIMARY KEY (MaPN, MaLK),
    CONSTRAINT FK_CTPN_PhieuNhap FOREIGN KEY (MaPN) REFERENCES PhieuNhap(MaPN),
    CONSTRAINT FK_CTPN_LinhKien FOREIGN KEY (MaLK) REFERENCES LinhKien(MaLK)
);

CREATE TABLE TaiKhoan (
    TenDN varchar(30) NOT NULL,
    MatKhau varchar(50) NOT NULL,
    MaNV char(6) NOT NULL,
    CONSTRAINT PK_TaiKhoan PRIMARY KEY (TenDN),
    CONSTRAINT FK_TaiKhoan_NhanVien FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV),
    CONSTRAINT UQ_TaiKhoan_MaNV UNIQUE (MaNV) 
);
GO

--thêm dữ liệu

INSERT INTO NhaSanXuat VALUES
('NSX01', 'Genius', 'Taiwan'), ('NSX02', 'Logitech', 'Switzerland'),
('NSX03', 'Kingston', 'USA'), ('NSX04', 'Intel', 'USA'),
('NSX05', 'AMD', 'USA'), ('NSX06', 'ASUS', 'Taiwan'),
('NSX07', 'Samsung', 'South Korea'), ('NSX08', 'Gigabyte', 'Taiwan'),
('NSX09', 'Keychron', 'China'), ('NSX10', 'H hành', 'Vietnam');

INSERT INTO LoaiLK VALUES
('MOU', N'Chuột máy tính', N'Chuột gaming, chuột văn phòng các loại'), 
('LAP', N'Máy tính xách tay', N'Laptop học tập, làm việc, chơi game'),
('CPU', N'Bộ vi xử lý', N'Chip máy tính (Intel, AMD...)'), 
('PCX', N'Máy tính để bàn', N'Thùng máy PC ráp sẵn nguyên bộ'),
('MAI', N'Bo mạch chủ (Mainboard)', N'Bo mạch chính để cắm linh kiện'), 
('RAM', N'Bộ nhớ trong (RAM)', N'Thanh RAM cho PC và Laptop'),
('HDD', N'Ổ cứng HDD', N'Ổ cứng dung lượng cao lưu dữ liệu'), 
('SSD', N'Ổ cứng SSD', N'Ổ cứng tốc độ cao chạy Win'),
('VGA', N'Card màn hình', N'Card đồ họa rời chơi game, làm mượt ảnh'), 
('KEY', N'Bàn phím cơ', N'Bàn phím gõ văn bản, phím cơ gaming');

INSERT INTO LinhKien VALUES
('MOU001', N'Chuột quang có dây', '01-01-2023', 12, 'MOU', 'NSX01', N'Cái', 50, 150000),
('MOU002', N'Chuột Logitech G102', '04-02-2023', 24, 'MOU', 'NSX02', N'Cái', 30, 450000),
('MOU003', N'Chuột không dây Genius NX', '05-12-2023', 12, 'MOU', 'NSX01', N'Cái', 40, 250000),
('RAM001', N'RAM Kingston 8GB', '15-05-2023', 36, 'RAM', 'NSX03', N'Thanh', 40, 850000),
('RAM002', N'RAM Samsung 16GB DDR4', '10-09-2023', 36, 'RAM', 'NSX07', N'Thanh', 30, 1200000),
('RAM003', N'RAM Kingston Fury 32GB', '12-09-2023', 36, 'RAM', 'NSX03', N'Thanh', 20, 2500000),
('RAM004', N'RAM Kingston Fury 64GB DDR5', '22-02-2024', 36, 'RAM', 'NSX03', N'Thanh', 10, 4800000),
('CPU001', N'CPU Intel Core i5', '05-04-2023', 36, 'CPU', 'NSX04', N'Con', 15, 4500000),
('CPU002', N'CPU AMD Ryzen 5', '07-02-2023', 36, 'CPU', 'NSX05', N'Con', 20, 3200000),
('CPU003', N'CPU Intel Core i7 13700K', '10-10-2023', 36, 'CPU', 'NSX04', N'Con', 15, 9500000),
('CPU004', N'CPU AMD Ryzen 9 7950X', '01-02-2024', 36, 'CPU', 'NSX05', N'Con', 5, 14500000),
('MAI001', N'Mainboard ASUS B450', '04-12-2023', 36, 'MAI', 'NSX06', N'Cái', 10, 1800000),
('MAI002', N'Mainboard Gigabyte B660M', '20-10-2023', 36, 'MAI', 'NSX08', N'Cái', 15, 2800000),
('MAI003', N'Mainboard ASUS ROG Strix B550', '05-11-2023', 36, 'MAI', 'NSX06', N'Cái', 10, 4500000),
('MAI004', N'Mainboard ASUS TUF Gaming X570', '15-01-2024', 36, 'MAI', 'NSX06', N'Cái', 8, 5200000),
('SSD001', N'SSD Samsung 500GB', '03-03-2023', 60, 'SSD', 'NSX07', N'Cái', 25, 1200000),
('SSD002', N'SSD Samsung 980 PRO 1TB M.2', '18-01-2024', 60, 'SSD', 'NSX07', N'Cái', 20, 2100000),
('HDD001', N'Ổ cứng HDD WD Blue 1TB', '15-08-2023', 24, 'HDD', 'NSX07', N'Cái', 30, 950000),
('HDD002', N'Ổ cứng HDD Seagate Barracuda 2TB', '10-01-2024', 24, 'HDD', 'NSX07', N'Cái', 15, 1350000),
('LAP001', N'Laptop ASUS Vivobook 14 OLED', '10-09-2023', 24, 'LAP', 'NSX06', N'Cái', 10, 16500000),
('LAP002', N'Laptop Gigabyte Aorus 15 Gaming', '20-02-2024', 24, 'LAP', 'NSX08', N'Cái', 5, 25000000),
('VGA001', N'VGA RTX 3060', '14-04-2023', 36, 'VGA', 'NSX08', N'Cái', 5, 8900000),
('VGA002', N'VGA Gigabyte RTX 4060', '15-12-2023', 36, 'VGA', 'NSX08', N'Cái', 10, 8500000),
('VGA003', N'VGA ASUS TUF RX 6700 XT', '20-12-2023', 36, 'VGA', 'NSX06', N'Cái', 8, 9200000),
('VGA004', N'VGA ASUS ROG Strix RTX 4090', '10-03-2024', 36, 'VGA', 'NSX06', N'Cái', 2, 45000000),
('KEY001', N'Phím cơ Keychron K2', '19-10-2023', 12, 'KEY', 'NSX09', N'Cái', 15, 1650000),
('KEY002', N'Phím cơ Logitech G Pro', '01-11-2023', 24, 'KEY', 'NSX02', N'Cái', 20, 2500000),
('KEY003', N'Phím cơ Keychron K4', '15-11-2023', 12, 'KEY', 'NSX09', N'Cái', 25, 1850000),
('KEY004', N'Phím cơ Logitech G815 RGB', '05-03-2024', 24, 'KEY', 'NSX02', N'Cái', 10, 3500000),
('PCX001', N'PC Gaming H510', '20-11-2023', 24, 'PCX', 'NSX10', N'Bộ', 5, 10500000),
('PCX002', N'PC Office Intel Core i3', '25-11-2023', 24, 'PCX', 'NSX10', N'Bộ', 10, 6500000),
('PCX003', N'PC Workstation AMD Ryzen 7', '28-11-2023', 36, 'PCX', 'NSX10', N'Bộ', 5, 18500000);

INSERT INTO KhachHang (MaKH, TenKH, DChi, SDT, Email) VALUES
('KH001', N'Ngụy Hạo Nhiên', N'Thanh Hóa', '0989751723', 'nhien1999@gmail.com'),
('KH002', N'Đinh Bảo Lộc', N'Lâm Đồng', '0918234654', 'loc1998@gmail.com'),
('KH003', N'Trần Thanh Diệu', N'TP.HCM', '0978123765', 'dieu1995@gmail.com'),
('KH004', N'Nguyễn Nhật Minh Quân', N'TP.HCM', '0909456768', 'quan2000@gmail.com'),
('KH005', N'Huỳnh Kim Ánh', N'Khánh Hòa', '0932987567', 'anh1992@gmail.com'),
('KH006', N'Lê Văn Việt', N'Đà Nẵng', '0905123456', 'viet1988@gmail.com'),
('KH007', N'Lương Văn Quan', N'Long An', '0913567890', 'quan1995@gmail.com'),
('KH008', N'Vũ Thị Mai', N'Hải Phòng', '0988666777', 'mai1991@gmail.com'),
('KH009', N'Trịnh Hữu Kiến Quốc', N'TP.HCM', '0933444555', 'quoc1996@gmail.com'),
('KH010', N'Hồ Đại Phong', N'Kon Tum', '0977888999', 'phong1994@gmail.com');

INSERT INTO NhanVien (MaNV, TenNV, GioiTinh, NgaySinh, SDT, ChucVu, Quyen, Email, NgayVaoLam) VALUES
('NV001', N'Phạm Văn Mách', N'Nam', '15-05-1995', '0901234567', N'Quản lý', N'Quản lý toàn bộ', 'mach1995@gmail.com', '10-06-2020'),
('NV002', N'Trần Thị Dung', N'Nữ', '20-10-1998', '0902234567', N'Nhân viên thu ngân', N'Thu ngân', 'dung1998@gmail.com', '15-08-2021'),
('NV003', N'Lý Thị Nhung', N'Nữ', '08-03-2001', '0910234567', N'Nhân viên thu ngân', N'Thu ngân', 'nhung2001@gmail.com', '20-02-2023'),
('NV004', N'Lê Văn Anh', N'Nam', '05-09-1992', '0903234567', N'Nhân viên bán hàng', N'Bán hàng', 'anh1992@gmail.com', '05-01-2018'),
('NV005', N'Nguyễn Thị Điệp', N'Nữ', '12-12-2000', '0904234567', N'Nhân viên bán hàng', N'Bán hàng', 'diep2000@gmail.com', '12-11-2022'),
('NV006', N'Hoàng Văn Tuấn', N'Nam', '01-01-1997', '0905234567', N'Nhân viên kỹ thuật', N'Kỹ thuật', 'tuan1997@gmail.com', '01-04-2021'),
('NV007', N'Bùi Văn Quốc', N'Nam', '30-04-1994', '0907234567', N'Nhân viên kỹ thuật', N'Kỹ thuật', 'quoc1994@gmail.com', '18-09-2019'),
('NV008', N'Đặng Thị Hà Anh', N'Nữ', '14-02-1999', '0906234567', N'Nhân viên kho', N'Kho', 'anh1999@gmail.com', '25-05-2022'),
('NV009', N'Đỗ Thị Ngọc Huyền', N'Nữ', '02-09-1996', '0908234567', N'Nhân viên kho', N'Kho', 'huyen1996@gmail.com', '03-07-2020'),
('NV010', N'Võ Văn An', N'Nam', '22-12-1993', '0909234567', N'Nhân viên bán hàng', N'Bán hàng', 'an1993@gmail.com', '11-10-2018');

INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV) VALUES
('HD001', '01-04-2023', 'KH001', 'NV001'), ('HD002', '15-05-2023', 'KH005', 'NV002'),
('HD003', '14-06-2023', 'KH004', 'NV001'), ('HD004', '03-06-2023', 'KH005', 'NV003'),
('HD005', '05-06-2023', 'KH001', 'NV002'), ('HD006', '07-07-2023', 'KH003', 'NV004'),
('HD007', '12-08-2023', 'KH002', 'NV005'), ('HD008', '25-09-2023', 'KH003', 'NV001'),
('HD009', '10-10-2023', 'KH008', 'NV006'), ('HD010', '11-11-2023', 'KH010', 'NV007');

UPDATE HoaDon SET TrangThai = N'Đã thanh toán' WHERE MaHD IN ('HD001', 'HD002', 'HD005');

INSERT INTO ChiTietHD VALUES
('HD001', 'MOU001', 2, 150000), ('HD002', 'MOU002', 1, 450000),
('HD003', 'RAM001', 2, 850000), ('HD004', 'CPU001', 1, 4500000),
('HD005', 'CPU002', 1, 3200000), ('HD006', 'MAI001', 1, 1800000),
('HD007', 'SSD001', 2, 1200000), ('HD007', 'VGA001', 1, 8900000),
('HD008', 'KEY001', 1, 1650000), ('HD009', 'PCX001', 1, 10500000),
('HD010', 'MOU001', 5, 140000);

INSERT INTO PhieuNhap (MaPN, NgayNhap, MaNV) VALUES 
('PN001', '10-01-2023', 'NV008'), ('PN002', '15-02-2023', 'NV009'),
('PN003', '20-03-2023', 'NV008'), ('PN004', '05-04-2023', 'NV009'),
('PN005', '12-05-2023', 'NV008'), ('PN006', '18-06-2023', 'NV009'),
('PN007', '22-07-2023', 'NV008'), ('PN008', '08-08-2023', 'NV009'),
('PN009', '30-09-2023', 'NV008'), ('PN010', '14-10-2023', 'NV009');

INSERT INTO ChiTietPN (MaPN, MaLK, SoLuongNhap, DonGiaNhap) VALUES 
('PN001', 'MOU001', 50, 100000), ('PN002', 'MOU002', 30, 300000),
('PN003', 'RAM001', 40, 600000), ('PN004', 'CPU001', 15, 4000000),
('PN005', 'CPU002', 20, 2800000), ('PN006', 'MAI001', 10, 1500000),
('PN007', 'SSD001', 25, 900000), ('PN008', 'VGA001', 5, 8000000),
('PN009', 'KEY001', 15, 1200000), ('PN010', 'PCX001', 5, 9500000);

INSERT INTO TaiKhoan (TenDN, MatKhau, MaNV) VALUES 
('machpv', '123456', 'NV001'), ('dungtt', '123456', 'NV002'),
('nhunglt', '123456', 'NV003'), ('anhlv', '123456', 'NV004'),
('diepnt', '123456', 'NV005'), ('tuanhv', '123456', 'NV006'),
('quocbv', '123456', 'NV007'), ('anhdth', '123456', 'NV008'),
('huyendtn', '123456', 'NV009'), ('anvv', '123456', 'NV010');
GO

--tạo trigger

CREATE TRIGGER trg_CapNhatTonKho_Full
ON ChiTietHD
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE LinhKien
    SET SoLuongTon = SoLuongTon - T.SoLuong
    FROM LinhKien
    JOIN (
        SELECT MaLK, SUM(SoLuong) AS SoLuong
        FROM inserted
        GROUP BY MaLK
    ) T ON LinhKien.MaLK = T.MaLK;
    UPDATE LinhKien
    SET SoLuongTon = SoLuongTon + T.SoLuong
    FROM LinhKien
    JOIN (
        SELECT MaLK, SUM(SoLuong) AS SoLuong
        FROM deleted
        GROUP BY MaLK
    ) T ON LinhKien.MaLK = T.MaLK;
END;

CREATE TRIGGER trg_CongTonKhoKhiNhap
ON ChiTietPN
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE LinhKien
    SET SoLuongTon = LinhKien.SoLuongTon + HangVuaNhap.SoLuongNhap
    FROM LinhKien
    JOIN inserted HangVuaNhap ON LinhKien.MaLK = HangVuaNhap.MaLK;
END;
GO

CREATE TRIGGER trg_CapNhatTongTien
ON ChiTietHD
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE HoaDon
    SET TongTien = (
        SELECT ISNULL(SUM(SoLuong * DonGia), 0) 
        FROM ChiTietHD 
        WHERE ChiTietHD.MaHD = HoaDon.MaHD
    )
    WHERE MaHD IN (SELECT MaHD FROM inserted)  
       OR MaHD IN (SELECT MaHD FROM deleted); 
END;
GO

--tạo hàm và thủ tục

CREATE FUNCTION fn_DoanhThuTheoThang (@Thang INT, @Nam INT)
RETURNS INT AS
BEGIN
    DECLARE @DoanhThu INT;
    SELECT @DoanhThu = SUM(TongTien) FROM HoaDon WHERE MONTH(NgayHD) = @Thang AND YEAR(NgayHD) = @Nam;
    RETURN ISNULL(@DoanhThu, 0);
END;
GO

CREATE PROCEDURE sp_BaoCaoTonKho AS
BEGIN
    DECLARE cur_KiemTraKho CURSOR FOR SELECT MaLK, TenLK, SoLuongTon FROM LinhKien WHERE SoLuongTon < 10;
    DECLARE @MaLK char(6), @TenLK nvarchar(50), @TonKho int;

    OPEN cur_KiemTraKho;
    FETCH NEXT FROM cur_KiemTraKho INTO @MaLK, @TenLK, @TonKho;

    PRINT N'--- BÁO CÁO NHỮNG LINH KIỆN SẮP HẾT HÀNG ---'
    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT N'CẢNH BÁO: Linh kiện ' + @TenLK + ' (Mã: ' + @MaLK + N') chỉ còn: ' + CAST(@TonKho AS varchar) + N' cái. Cần nhập gấp!';
        FETCH NEXT FROM cur_KiemTraKho INTO @MaLK, @TenLK, @TonKho;
    END;

    CLOSE cur_KiemTraKho;
    DEALLOCATE cur_KiemTraKho;
END;
GO

CREATE PROCEDURE sp_BanLinhKien 
    @MaHD char(5), 
    @NgayHD date, 
    @MaKH char(6), 
    @MaNV char(6), 
    @MaLK char(6), 
    @SoLuongBan tinyint
AS
BEGIN
    BEGIN TRAN;
    BEGIN TRY
        DECLARE @TonKhoHienTai INT;
        SELECT @TonKhoHienTai = SoLuongTon FROM LinhKien WHERE MaLK = @MaLK;

        IF (@TonKhoHienTai < @SoLuongBan)
        BEGIN
            PRINT N'LỖI: Không đủ hàng trong kho!';
            ROLLBACK TRAN; 
            RETURN;
        END
        DECLARE @GiaBanHienHanh INT;
        SELECT @GiaBanHienHanh = DonGiaBan FROM LinhKien WHERE MaLK = @MaLK;
        IF NOT EXISTS (SELECT 1 FROM HoaDon WHERE MaHD = @MaHD)
        BEGIN
            INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV) 
            VALUES (@MaHD, @NgayHD, @MaKH, @MaNV);
        END

        INSERT INTO ChiTietHD (MaHD, MaLK, SoLuong, DonGia) 
        VALUES (@MaHD, @MaLK, @SoLuongBan, @GiaBanHienHanh);

        COMMIT TRAN;
        PRINT N'THÀNH CÔNG: Đã bán hàng với giá ' + CAST(@GiaBanHienHanh AS VARCHAR) + N' VND/cái.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        PRINT N'LỖI: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

--quản trị người dùng

IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'QuanLyLogin') DROP LOGIN QuanLyLogin;
IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'NhanVienBanHangLogin') DROP LOGIN NhanVienBanHangLogin;
GO

CREATE LOGIN QuanLyLogin WITH PASSWORD = '123';
CREATE LOGIN NhanVienBanHangLogin WITH PASSWORD = '123';
GO

CREATE USER QuanLyUser FOR LOGIN QuanLyLogin;
CREATE USER NhanVienBanHangUser FOR LOGIN NhanVienBanHangLogin;
GO

ALTER ROLE db_owner ADD MEMBER QuanLyUser;
GRANT SELECT ON SCHEMA::dbo TO NhanVienBanHangUser;
GRANT INSERT ON HoaDon TO NhanVienBanHangUser;
GRANT INSERT ON ChiTietHD TO NhanVienBanHangUser;
DENY UPDATE, DELETE ON NhanVien TO NhanVienBanHangUser;
GO

INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV, TrangThai) VALUES
('HD011', '10-01-2026', 'KH001', 'NV002', N'Chưa thanh toán'),
('HD012', '15-02-2026', 'KH002', 'NV003', N'Đã thanh toán'),
('HD013', '20-03-2026', 'KH003', 'NV004', N'Chưa thanh toán'),
('HD014', '05-04-2026', 'KH004', 'NV005', N'Đã thanh toán'),
('HD015', '12-04-2026', 'KH005', 'NV002', N'Chưa thanh toán'),
('HD016', '20-05-2025', 'KH006', 'NV003', N'Đã thanh toán'),
('HD017', '11-08-2025', 'KH007', 'NV004', N'Chưa thanh toán'),
('HD018', '25-12-2025', 'KH008', 'NV005', N'Đã thanh toán'),
('HD019', '14-03-2024', 'KH009', 'NV006', N'Chưa thanh toán'),
('HD020', '30-10-2024', 'KH010', 'NV007', N'Đã thanh toán');
GO

INSERT INTO ChiTietHD (MaHD, MaLK, SoLuong, DonGia) VALUES
('HD011', 'MOU001', 2, 150000),
('HD012', 'RAM002', 1, 1200000),
('HD013', 'CPU003', 1, 9500000),
('HD014', 'VGA002', 1, 8500000),
('HD015', 'LAP001', 1, 16500000),
('HD016', 'SSD002', 1, 2100000),
('HD017', 'KEY003', 2, 1850000),
('HD018', 'PCX002', 1, 6500000),
('HD019', 'MOU003', 3, 250000),
('HD020', 'RAM004', 1, 4800000);
GO

ALTER TABLE HoaDon ADD PhuongThucThanhToan nvarchar(50) DEFAULT N'Tiền mặt';
ALTER TABLE HoaDon ADD NgayThanhToan date NULL;
GO

UPDATE HoaDon
SET PhuongThucThanhToan = N'Tiền mặt'
WHERE PhuongThucThanhToan IS NULL;
GO

UPDATE HoaDon
SET NgayThanhToan = NgayHD
WHERE TrangThai = N'Đã thanh toán' AND NgayThanhToan IS NULL;
GO

ALTER TABLE NhanVien ADD DaNghiViec bit DEFAULT 0 NOT NULL;

ALTER TABLE LinhKien ADD NgungKinhDoanh bit DEFAULT 0 NOT NULL;
GO

--sao lưu và backup khi cần và khi chạy phải comment backup với restore

--BACKUP DATABASE QL_LinhKien_PC
--TO DISK = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
--WITH FORMAT, NAME = 'Full Backup';
--GO

--USE master;
--GO

--RESTORE DATABASE QL_LinhKien_PC
--FROM DISK = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
--WITH REPLACE;
--GO
