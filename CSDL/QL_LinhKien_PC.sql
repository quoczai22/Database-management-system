USE master;
GO

DROP DATABASE IF EXISTS QL_LinhKien_PC;
GO

CREATE DATABASE QL_LinhKien_PC
ON PRIMARY
(
    NAME = QL_LinhKien_Primary,
    FILENAME = 'C:\SQLData\QL_LinhKien_Main.mdf',
    SIZE = 10MB,
    MAXSIZE = 100MB,
    FILEGROWTH = 5MB
),
(
    NAME = QL_LinhKien_Secondary,
    FILENAME = 'C:\SQLData\QL_LinhKien_Sub.ndf',
    SIZE = 5MB,
    MAXSIZE = 50MB,
    FILEGROWTH = 1MB
)
LOG ON
(
    NAME = QL_LinhKien_Log,
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

CREATE TABLE NhaSanXuat (
    MaNSX char(5) NOT NULL,
    TenNSX nvarchar(50),
    QuocGia nvarchar(50),
    CONSTRAINT PK_NhaSanXuat PRIMARY KEY (MaNSX)
);

CREATE TABLE LoaiLK (
    MaLoai char(3) NOT NULL,
    TenLoai nvarchar(40),
    CONSTRAINT PK_LoaiLK PRIMARY KEY (MaLoai)
);

CREATE TABLE LinhKien (
    MaLK char(6) NOT NULL,
    TenLK nvarchar(50),
    NgaySX date,
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
    CONSTRAINT PK_KhachHang PRIMARY KEY (MaKH)
);

CREATE TABLE NhanVien (
    MaNV char(6) NOT NULL,
    TenNV nvarchar(40),
    GioiTinh nvarchar(5),
    NgaySinh date,
    SDT char(10),
    ChucVu nvarchar(20),
    Quyen nvarchar(20)
    CONSTRAINT PK_NhanVien PRIMARY KEY (MaNV)
);

CREATE TABLE HoaDon (   
    MaHD char(5) NOT NULL,
    NgayHD date,
    MaKH char(6) NOT NULL,
    MaNV char(6) NOT NULL,
    TongTien int NULL DEFAULT 0,
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

CREATE TRIGGER trg_CapNhatTongTien ON ChiTietHD
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE HoaDon
    SET TongTien = ISNULL((SELECT SUM(SoLuong * DonGia) FROM ChiTietHD WHERE ChiTietHD.MaHD = HoaDon.MaHD), 0)
    WHERE MaHD IN (SELECT MaHD FROM inserted UNION SELECT MaHD FROM deleted);
END;
GO

CREATE TRIGGER trg_TruTonKhoKhiBan ON ChiTietHD
AFTER INSERT
AS
BEGIN
    UPDATE LinhKien
    SET SoLuongTon = SoLuongTon - i.SoLuong
    FROM LinhKien lk JOIN inserted i ON lk.MaLK = i.MaLK;
END;
GO

-- Tự động CỘNG tồn kho khi nhập hàng
CREATE TRIGGER trg_CongTonKhoKhiNhap ON ChiTietPN
AFTER INSERT
AS
BEGIN
    UPDATE LinhKien
    SET SoLuongTon = SoLuongTon + i.SoLuongNhap
    FROM LinhKien lk JOIN inserted i ON lk.MaLK = i.MaLK;
END;
GO

INSERT INTO NhaSanXuat VALUES
('NSX01', 'Genius', 'Taiwan'), ('NSX02', 'Logitech', 'Switzerland'),
('NSX03', 'Kingston', 'USA'), ('NSX04', 'Intel', 'USA'),
('NSX05', 'AMD', 'USA'), ('NSX06', 'ASUS', 'Taiwan'),
('NSX07', 'Samsung', 'South Korea'), ('NSX08', 'Gigabyte', 'Taiwan'),
('NSX09', 'Keychron', 'China'), ('NSX10', 'H hành', 'Vietnam');

INSERT INTO LoaiLK VALUES
('MOU', N'Chuột máy tính'), ('LAP', N'Máy tính xách tay'),
('CPU', N'Bộ vi xử lý'), ('PCX', N'Máy tính để bàn'),
('MAI', N'Bo mạch chủ (Mainboard)'), ('RAM', N'Bộ nhớ trong (RAM)'),
('HDD', N'Ổ cứng HDD'), ('SSD', N'Ổ cứng SSD'),
('VGA', N'Card màn hình'), ('KEY', N'Bàn phím cơ');

INSERT INTO LinhKien VALUES
('MOU001', N'Chuột quang có dây', '01-01-2023', 12, 'MOU', 'NSX01', N'Cái', 50, 150000),
('MOU002', N'Chuột Logitech G102', '04-02-2023', 24, 'MOU', 'NSX02', N'Cái', 30, 450000),
('RAM001', N'RAM Kingston 8GB', '15-05-2023', 36, 'RAM', 'NSX03', N'Thanh', 40, 850000),
('CPU001', N'CPU Intel Core i5', '05-04-2023', 36, 'CPU', 'NSX04', N'Con', 15, 4500000),
('CPU002', N'CPU AMD Ryzen 5', '07-02-2023', 36, 'CPU', 'NSX05', N'Con', 20, 3200000),
('MAI001', N'Mainboard ASUS B450', '04-12-2023', 36, 'MAI', 'NSX06', N'Cái', 10, 1800000),
('SSD001', N'SSD Samsung 500GB', '03-03-2023', 60, 'SSD', 'NSX07', N'Cái', 25, 1200000),
('VGA001', N'VGA RTX 3060', '14-04-2023', 36, 'VGA', 'NSX08', N'Cái', 5, 8900000),
('KEY001', N'Phím cơ Keychron K2', '19-10-2023', 12, 'KEY', 'NSX09', N'Cái', 15, 1650000),
('PCX001', N'PC Gaming H510', '20-11-2023', 24, 'PCX', 'NSX10', N'Bộ', 5, 10500000);

INSERT INTO KhachHang VALUES
('KH001', N'Ngụy Hạo Nhiên', N'Thanh Hóa', '0989751723'), ('KH002', N'Đinh Bảo Lộc', N'Lâm Đồng', '0918234654'),
('KH003', N'Trần Thanh Diệu', N'TP.HCM', '0978123765'), ('KH004', N'Nguyễn Nhật Minh Quân', N'TP.HCM', '0909456768'),
('KH005', N'Huỳnh Kim Ánh', N'Khánh Hòa', '0932987567'), ('KH006', N'Lê Văn Việt', N'Đà Nẵng', '0905123456'),
('KH007', N'Lương Văn Quan', N'Long An', '0913567890'), ('KH008', N'Vũ Thị Mai', N'Hải Phòng', '0988666777'),
('KH009', N'Trịnh Hữu Kiến Quốc', N'TP.HCM', '0933444555'), ('KH010', N'Hồ Đại Phong', N'Kon Tum', '0977888999')

INSERT INTO NhanVien (MaNV, TenNV, GioiTinh, NgaySinh, SDT, ChucVu, Quyen) VALUES
('NV001', N'Phạm Văn Mách', N'Nam', '1995-05-15', '0901234567', N'Quản lý', N'Quản lý toàn bộ'),
('NV002', N'Trần Thị Dung', N'Nữ', '1998-10-20', '0902234567', N'Nhân viên thu ngân', N'Thu ngân'),
('NV003', N'Lý Thị Nhung', N'Nữ', '2001-03-08', '0910234567', N'Nhân viên thu ngân', N'Thu ngân'),
('NV004', N'Lê Văn Anh', N'Nam', '1992-09-05', '0903234567', N'Nhân viên bán hàng', N'Bán hàng'),
('NV005', N'Nguyễn Thị Điệp', N'Nữ', '2000-12-12', '0904234567', N'Nhân viên bán hàng', N'Bán hàng'),
('NV006', N'Hoàng Văn Tuấn', N'Nam', '1997-01-01', '0905234567', N'Nhân viên kỹ thuật', N'Kỹ thuật'),
('NV007', N'Bùi Văn Quốc', N'Nam', '1994-04-30', '0907234567', N'Nhân viên kỹ thuật', N'Kỹ thuật'),
('NV008', N'Đặng Thị Hà Anh', N'Nữ', '1999-02-14', '0906234567', N'Nhân viên kho', N'Kho'),
('NV009', N'Đỗ Thị Ngọc Huyền', N'Nữ', '1996-09-02', '0908234567', N'Nhân viên kho', N'Kho'),
('NV010', N'Võ Văn An', N'Nam', '1993-12-22', '0909234567', N'Nhân viên bán hàng', N'Bán hàng');

INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV) VALUES
('HD001', '01-04-2023', 'KH001', 'NV001'), ('HD002', '15-05-2023', 'KH005', 'NV002'),
('HD003', '14-06-2023', 'KH004', 'NV001'), ('HD004', '03-06-2023', 'KH005', 'NV003'),
('HD005', '05-06-2023', 'KH001', 'NV002'), ('HD006', '07-07-2023', 'KH003', 'NV004'),
('HD007', '12-08-2023', 'KH002', 'NV005'), ('HD008', '25-09-2023', 'KH003', 'NV001'),
('HD009', '10-10-2023', 'KH008', 'NV006'), ('HD010', '11-11-2023', 'KH010', 'NV007')

INSERT INTO ChiTietHD VALUES
('HD001', 'MOU001', 2, 150000), ('HD002', 'MOU002', 1, 450000),
('HD003', 'RAM001', 2, 850000), ('HD004', 'CPU001', 1, 4500000),
('HD005', 'CPU002', 1, 3200000), ('HD006', 'MAI001', 1, 1800000),
('HD007', 'SSD001', 2, 1200000), ('HD007', 'VGA001', 1, 8900000),
('HD008', 'KEY001', 1, 1650000), ('HD009', 'PCX001', 1, 10500000),
('HD010', 'MOU001', 5, 140000)

INSERT INTO PhieuNhap (MaPN, NgayNhap, MaNV) VALUES 
('PN001', '10-01-2023', 'NV008'),
('PN002', '15-02-2023', 'NV009'),
('PN003', '20-03-2023', 'NV008'),
('PN004', '05-04-2023', 'NV009'),
('PN005', '12-05-2023', 'NV008'),
('PN006', '18-06-2023', 'NV009'),
('PN007', '22-07-2023', 'NV008'),
('PN008', '08-08-2023', 'NV009'),
('PN009', '30-09-2023', 'NV008'),
('PN010', '14-10-2023', 'NV009');

INSERT INTO ChiTietPN (MaPN, MaLK, SoLuongNhap, DonGiaNhap) VALUES 
('PN001', 'MOU001', 50, 100000),
('PN002', 'MOU002', 30, 300000),
('PN003', 'RAM001', 40, 600000),
('PN004', 'CPU001', 15, 4000000),
('PN005', 'CPU002', 20, 2800000),
('PN006', 'MAI001', 10, 1500000),
('PN007', 'SSD001', 25, 900000),
('PN008', 'VGA001', 5, 8000000),
('PN009', 'KEY001', 15, 1200000),
('PN010', 'PCX001', 5, 9500000);

INSERT INTO TaiKhoan (TenDN, MatKhau, MaNV) VALUES 
('machpv', '123456', 'NV001'),
('dungtt', '123456', 'NV002'),
('nhunglt', '123456', 'NV003'),
('anhlv', '123456', 'NV004'),
('diepnt', '123456', 'NV005'),
('tuanhv', '123456', 'NV006'),
('quocbv', '123456', 'NV007'),
('anhdth', '123456', 'NV008'),
('huyendtn', '123456', 'NV009'),
('anvv', '123456', 'NV010');
GO

--function và cursor

CREATE FUNCTION fn_DoanhThuTheoThang (@Thang INT, @Nam INT)
RETURNS INT
AS
BEGIN
    DECLARE @DoanhThu INT;
    SELECT @DoanhThu = SUM(TongTien) FROM HoaDon WHERE MONTH(NgayHD) = @Thang AND YEAR(NgayHD) = @Nam;
    RETURN ISNULL(@DoanhThu, 0);
END;
GO

CREATE PROCEDURE sp_BaoCaoTonKho
AS
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
EXEC sp_BaoCaoTonKho;
--thủ tục và giao tác

CREATE PROCEDURE sp_BanLinhKien 
    @MaHD char(5), @NgayHD date, @MaKH char(6), @MaNV char(6), @MaLK char(6), @SoLuongBan tinyint
AS
BEGIN
    BEGIN TRAN;
    BEGIN TRY
        DECLARE @TonKhoHienTai INT;
        SELECT @TonKhoHienTai = SoLuongTon FROM LinhKien WHERE MaLK = @MaLK;

        IF (@TonKhoHienTai < @SoLuongBan)
        BEGIN
            PRINT N'LỖI GIAO TÁC: Số lượng tồn kho không đủ để bán! Hóa đơn bị hủy.';
            ROLLBACK TRAN; 
            RETURN;
        END

        INSERT INTO HoaDon (MaHD, NgayHD, MaKH, MaNV) VALUES (@MaHD, @NgayHD, @MaKH, @MaNV);

        DECLARE @GiaBan INT;
        SELECT @GiaBan = DonGiaBan FROM LinhKien WHERE MaLK = @MaLK;
        
        INSERT INTO ChiTietHD (MaHD, MaLK, SoLuong, DonGia) VALUES (@MaHD, @MaLK, @SoLuongBan, @GiaBan);

        COMMIT TRAN;
        PRINT N'THÀNH CÔNG: Đã tạo hóa đơn và cập nhật tồn kho tự động.';
    END TRY
    BEGIN CATCH
        ROLLBACK TRAN;
        PRINT N'LỖI HỆ THỐNG: Đã hủy giao tác do có lỗi xảy ra - ' + ERROR_MESSAGE();
    END CATCH
END;
GO

--Quản trị người dùng

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

--backup và restore

BACKUP DATABASE QL_LinhKien_PC
TO DISK = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
WITH FORMAT, NAME = 'Full Backup';

USE master;
RESTORE DATABASE QL_LinhKien_PC
FROM DISK = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
WITH REPLACE;
