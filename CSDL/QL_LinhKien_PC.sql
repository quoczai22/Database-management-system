-- trước khi chạy phải tạo folder SQLData trong ổ C nếu ko có sẽ lỗi
use master;
go

drop database if exists QL_LinhKien_PC;
go

create database QL_LinhKien_PC
on primary
(
    name = QL_LinhKien_Primary,
    filename = 'C:\SQLData\QL_LinhKien_Main.mdf',
    size = 15MB,
    maxsize = 100MB,
    filegrowth = 5MB
),
(
    name = QL_LinhKien_Secondary_NET,
    filename = 'C:\SQLData\QL_LinhKien_Sub.ndf', 
    size = 5MB,
    maxsize = 50MB,
    filegrowth = 1MB
)
log on
(
    name = QL_LinhKien_Log_NET,
    filename = 'C:\SQLData\QL_LinhKien_Log.ldf',
    size = 5MB,
    maxsize = 20MB,
    filegrowth = 1MB
);
go

use QL_LinhKien_PC;
go
set dateformat dmy; 
go

-- tạo bảng
create table NhaSanXuat (
    MaNSX char(5) not null,
    TenNSX nvarchar(50),
    QuocGia nvarchar(50),
    constraint PK_NhaSanXuat primary key (MaNSX)
);
go

create table LoaiLK (
    MaLoai char(3) not null,
    TenLoai nvarchar(40),
    MoTa nvarchar(100), 
    constraint PK_LoaiLK primary key (MaLoai)
);
go

create table LinhKien (
    MaLK char(6) not null,
    TenLK nvarchar(50),
    NgayNhap date, 
    TGBH tinyint,
    MaLoai char(3) not null,
    MaNSX char(5) not null,
    DVT nvarchar(10),
    SoLuongTon int default 0,
    DonGiaBan money,
    constraint PK_LinhKien primary key (MaLK),
    constraint FK_LK_LoaiLK foreign key (MaLoai) references LoaiLK(MaLoai),
    constraint FK_LK_NhaSanXuat foreign key (MaNSX) references NhaSanXuat(MaNSX)
);
go

create table KhachHang (
    MaKH char(6) not null,
    TenKH nvarchar(30),
    DChi nvarchar(50),
    SDT char(10),
    Email varchar(50) null,
    constraint PK_KhachHang primary key (MaKH)
);
go

create table NhanVien (
    MaNV char(6) not null,
    TenNV nvarchar(40),
    GioiTinh nvarchar(5),
    NgaySinh date,
    SDT char(10),
    ChucVu nvarchar(30),
    Quyen nvarchar(20),
    Email varchar(50) null,      
    NgayVaoLam date null,      
    constraint PK_NhanVien primary key (MaNV)
);
go

create table HoaDon (   
    MaHD char(5) not null,
    NgayHD date,
    MaKH char(6) not null,
    MaNV char(6) not null,
    TongTien money null default 0,
    TrangThai nvarchar(30) default N'Chưa thanh toán',
    constraint PK_HoaDon primary key (MaHD),
    constraint FK_HoaDon_KhachHang foreign key (MaKH) references KhachHang(MaKH),
    constraint FK_HoaDon_NhanVien foreign key (MaNV) references NhanVien(MaNV)
);
go

create table ChiTietHD (
    MaHD char(5) not null,
    MaLK char(6) not null,
    SoLuong tinyint, 
    DonGia money,
    constraint PK_CTHD primary key (MaHD, MaLK),
    constraint FK_CTHD_HoaDon foreign key (MaHD) references HoaDon(MaHD),
    constraint FK_CTHD_LinhKien foreign key (MaLK) references LinhKien(MaLK) 
);
go

create table PhieuNhap (
    MaPN char(5) not null,
    NgayNhap date,
    MaNV char(6) not null,
    constraint PK_PhieuNhap primary key (MaPN),
    constraint FK_PhieuNhap_NhanVien foreign key (MaNV) references NhanVien(MaNV)
);
go

create table ChiTietPN (
    MaPN char(5) not null,
    MaLK char(6) not null,
    SoLuongNhap int,
    DonGiaNhap money,
    constraint PK_CTPN primary key (MaPN, MaLK),
    constraint FK_CTPN_PhieuNhap foreign key (MaPN) references PhieuNhap(MaPN),
    constraint FK_CTPN_LinhKien foreign key (MaLK) references LinhKien(MaLK)
);
go

create table TaiKhoan (
    TenDN varchar(30) not null,
    MatKhau varchar(50) not null,
    MaNV char(6) not null,
    constraint PK_TaiKhoan primary key (TenDN),
    constraint FK_TaiKhoan_NhanVien foreign key (MaNV) references NhanVien(MaNV),
    constraint UQ_TaiKhoan_MaNV unique (MaNV) 
);
go

-- thêm dữ liệu
insert into NhaSanXuat values
('NSX01', 'Genius', 'Taiwan'), 
('NSX02', 'Logitech', 'Switzerland'),
('NSX03', 'Kingston', 'USA'), 
('NSX04', 'Intel', 'USA'),
('NSX05', 'AMD', 'USA'), 
('NSX06', 'ASUS', 'Taiwan'),
('NSX07', 'Samsung', 'South Korea'), 
('NSX08', 'Gigabyte', 'Taiwan'),
('NSX09', 'Keychron', 'China'), 
('NSX10', 'H hành', 'Vietnam');
go

insert into LoaiLK values
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
go

insert into LinhKien values
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
go

insert into KhachHang (MaKH, TenKH, DChi, SDT, Email) values
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
go

insert into NhanVien (MaNV, TenNV, GioiTinh, NgaySinh, SDT, ChucVu, Quyen, Email, NgayVaoLam) values
('NV001', N'Phạm Văn Mách', N'Nam', '15-05-1995', '0901234567', N'Quản lý', N'Quản lý toàn bộ', 'mach1995@gmail.com', '10-06-2020'),
('NV002', N'Trần Thị Dung', N'Nữ', '20-10-1998', '0902234567', N'Nhân viên thu ngân', N'Thu ngân', 'dung1998@gmail.com', '15-08-2021'),
('NV003', N'Lý Thị Nhung', N'Nữ', '08-03-2001', '0910234567', N'Nhân viên thu ngân', N'Thu ngân', 'nhung2001@gmail.com', '20-02-2023'),
('NV004', N'Lê Văn Anh', N'Nam', '05-09-1992', '0903234567', N'Nhân viên thu ngân', N'Thu ngân', 'anh1992@gmail.com', '05-01-2018'),
('NV005', N'Nguyễn Thị Điệp', N'Nữ', '12-12-2000', '0904234567', N'Nhân viên chăm sóc khách hàng', N'Chăm sóc khách hàng', 'diep2000@gmail.com', '12-11-2022'),
('NV006', N'Hoàng Văn Tuấn', N'Nam', '01-01-1997', '0905234567', N'Nhân viên chăm sóc khách hàng', N'Chăm sóc khách hàng', 'tuan1997@gmail.com', '01-04-2021'),
('NV007', N'Bùi Văn Quốc', N'Nam', '30-04-1994', '0907234567', N'Nhân viên chăm sóc khách hàng', N'Chăm sóc khách hàng', 'quoc1994@gmail.com', '18-09-2019'),
('NV008', N'Đặng Thị Hà Anh', N'Nữ', '14-02-1999', '0906234567', N'Nhân viên kho', N'Kho', 'anh1999@gmail.com', '25-05-2022'),
('NV009', N'Đỗ Thị Ngọc Huyền', N'Nữ', '02-09-1996', '0908234567', N'Nhân viên kho', N'Kho', 'huyen1996@gmail.com', '03-07-2020'),
('NV010', N'Võ Văn An', N'Nam', '22-12-1993', '0909234567', N'Nhân viên kho', N'Kho', 'an1993@gmail.com', '11-10-2018');
go

insert into HoaDon (MaHD, NgayHD, MaKH, MaNV, TrangThai) values
('HD001', '01-04-2023', 'KH001', 'NV001', N'Đã thanh toán'), 
('HD002', '15-05-2023', 'KH005', 'NV002', N'Đã thanh toán'),
('HD003', '14-06-2023', 'KH004', 'NV001', N'Chưa thanh toán'), 
('HD004', '03-06-2023', 'KH005', 'NV003', N'Chưa thanh toán'),
('HD005', '05-06-2023', 'KH001', 'NV002', N'Đã thanh toán'), 
('HD006', '07-07-2023', 'KH003', 'NV004', N'Chưa thanh toán'),
('HD007', '12-08-2023', 'KH002', 'NV005', N'Chưa thanh toán'), 
('HD008', '25-09-2023', 'KH003', 'NV001', N'Chưa thanh toán'),
('HD009', '10-10-2023', 'KH008', 'NV006', N'Chưa thanh toán'), 
('HD010', '11-11-2023', 'KH010', 'NV007', N'Chưa thanh toán'),
('HD011', '14-03-2024', 'KH001', 'NV002', N'Chưa thanh toán'),
('HD012', '30-10-2024', 'KH002', 'NV003', N'Đã thanh toán'),
('HD013', '20-05-2025', 'KH003', 'NV004', N'Chưa thanh toán'),
('HD014', '11-08-2025', 'KH004', 'NV005', N'Đã thanh toán'),
('HD015', '25-12-2025', 'KH005', 'NV002', N'Chưa thanh toán'),
('HD016', '10-01-2026', 'KH006', 'NV003', N'Đã thanh toán'),
('HD017', '15-02-2026', 'KH007', 'NV004', N'Chưa thanh toán'),
('HD018', '20-03-2026', 'KH008', 'NV005', N'Đã thanh toán'),
('HD019', '05-04-2026', 'KH009', 'NV006', N'Chưa thanh toán'),
('HD020', '12-04-2026', 'KH010', 'NV007', N'Đã thanh toán');
go

insert into ChiTietHD (MaHD, MaLK, SoLuong, DonGia) values
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
('HD010', 'MOU001', 5, 140000),
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
go

insert into PhieuNhap (MaPN, NgayNhap, MaNV) values 
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
go

insert into ChiTietPN (MaPN, MaLK, SoLuongNhap, DonGiaNhap) values 
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
go

insert into TaiKhoan (TenDN, MatKhau, MaNV) values 
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
go

-- tổng tiền hóa đơn
update HoaDon
set TongTien = isnull((
    select sum(SoLuong * DonGia)
    from ChiTietHD cthd
    where cthd.MaHD = HoaDon.MaHD
), 0);
go

-- tạo trigger
create trigger trg_CapNhatTonKho_Full
on ChiTietHD
after insert, update, delete
as
begin
    set nocount on;
    update LinhKien
    set SoLuongTon = SoLuongTon - T.SoLuong
    from LinhKien
    join (
        select MaLK, sum(SoLuong) as SoLuong
        from inserted
        group by MaLK
    ) T on LinhKien.MaLK = T.MaLK;
    
    update LinhKien
    set SoLuongTon = SoLuongTon + T.SoLuong
    from LinhKien
    join (
        select MaLK, sum(SoLuong) as SoLuong
        from deleted
        group by MaLK
    ) T on LinhKien.MaLK = T.MaLK;
end;
go

create trigger trg_CongTonKhoKhiNhap
on ChiTietPN
after insert
as
begin
    set nocount on;
    update LinhKien
    set SoLuongTon = LinhKien.SoLuongTon + HangVuaNhap.SoLuongNhap
    from LinhKien
    join inserted HangVuaNhap on LinhKien.MaLK = HangVuaNhap.MaLK;
end;
go

create trigger trg_CapNhatTongTien
on ChiTietHD
after insert, update, delete
as
begin
    set nocount on;
    update HoaDon
    set TongTien = (
        select isnull(sum(SoLuong * DonGia), 0) 
        from ChiTietHD 
        where ChiTietHD.MaHD = HoaDon.MaHD
    )
    where MaHD in (select MaHD from inserted)  
       or MaHD in (select MaHD from deleted); 
end;
go

-- tạo hàm và thủ tục
create function fn_DoanhThuTheoThang (@Thang int, @Nam int)
returns int 
as
begin
    declare @DoanhThu money;
    select @DoanhThu = sum(TongTien) from HoaDon 
    where month(NgayHD) = @Thang and year(NgayHD) = @Nam and TrangThai = N'Đã thanh toán';
    return isnull(@DoanhThu, 0);
end;
go

create procedure sp_BaoCaoTonKho
as
begin
    select 
        MaLK, 
        TenLK, 
        SoLuongTon,
        DVT,
        DonGiaBan
    from LinhKien
    where SoLuongTon < 10;
end;
go

create procedure sp_BanLinhKien 
    @MaHD char(5), 
    @NgayHD date, 
    @MaKH char(6), 
    @MaNV char(6), 
    @MaLK char(6), 
    @SoLuongBan tinyint
as
begin
    begin tran;
    begin try
        declare @TonKhoHienTai int;
        select @TonKhoHienTai = SoLuongTon from LinhKien where MaLK = @MaLK;

        if (@TonKhoHienTai < @SoLuongBan)
        begin
            print N'LỖI: Không đủ hàng trong kho!';
            rollback tran; 
            return;
        end
        
        declare @GiaBanHienHanh money;
        select @GiaBanHienHanh = DonGiaBan from LinhKien where MaLK = @MaLK;
        
        if not exists (select 1 from HoaDon where MaHD = @MaHD)
        begin
            insert into HoaDon (MaHD, NgayHD, MaKH, MaNV) 
            values (@MaHD, @NgayHD, @MaKH, @MaNV);
        end

        insert into ChiTietHD (MaHD, MaLK, SoLuong, DonGia) 
        values (@MaHD, @MaLK, @SoLuongBan, @GiaBanHienHanh);

        commit tran;
        print N'THÀNH CÔNG: Đã bán hàng với giá ' + cast(@GiaBanHienHanh as varchar) + N' VND/cái.';
    end try
    begin catch
        rollback tran;
        print N'LỖI: ' + error_message();
    end catch
end;
go

-- tạo cursor báo cáo danh sách khách hàng chưa thanh toán
create procedure sp_DanhSacKhachHangChuaTT
as
begin
    declare @MaKH char(6);
    declare @TenKH nvarchar(30);
    declare @TongNo money;
    declare cur_NoKH cursor for
    select kh.MaKH, kh.TenKH, sum(hd.TongTien) as TongNo
    from KhachHang kh
    join HoaDon hd on kh.MaKH = hd.MaKH
    where hd.TrangThai = N'Chưa thanh toán'
    group by kh.MaKH, kh.TenKH;
    
    open cur_NoKH;
    fetch next from cur_NoKH into @MaKH, @TenKH, @TongNo;
    
    print N'--- DANH SÁCH KHÁCH HÀNG CÒN NỢ ---';

    while @@fetch_status = 0
    begin
        print N'Khách hàng: ' + @TenKH + N' (Mã: ' + @MaKH + N') - Tổng nợ: ' + format(@TongNo, 'c', 'vi-VN');
        fetch next from cur_NoKH into @MaKH, @TenKH, @TongNo;
    end;
    
    close cur_NoKH;
    deallocate cur_NoKH;
end;
go

-- quản trị người dùng
if exists (select * from sys.server_principals where name = 'QuanLyLogin') drop login QuanLyLogin;
if exists (select * from sys.server_principals where name = 'NhanVienBanHangLogin') drop login NhanVienBanHangLogin;
go

create login QuanLyLogin with password = '123';
create login NhanVienBanHangLogin with password = '123';
go

create user QuanLyUser for login QuanLyLogin;
create user NhanVienBanHangUser for login NhanVienBanHangLogin;
go

alter role db_owner add member QuanLyUser;
grant select on schema::dbo to NhanVienBanHangUser;
grant insert on HoaDon to NhanVienBanHangUser;
grant insert on ChiTietHD to NhanVienBanHangUser;
deny update, delete on NhanVien to NhanVienBanHangUser;
go

alter table HoaDon add PhuongThucThanhToan nvarchar(50) default N'Tiền mặt';
alter table HoaDon add NgayThanhToan date null;
go

update HoaDon
set PhuongThucThanhToan = N'Tiền mặt'
where PhuongThucThanhToan is null;
go

update HoaDon
set NgayThanhToan = NgayHD
where TrangThai = N'Đã thanh toán' and NgayThanhToan is null;
go

alter table NhanVien add DaNghiViec bit default 0 not null;
alter table LinhKien add NgungKinhDoanh bit default 0 not null;
go

/*
----1. Kịch bản 1: Mức READ UNCOMMITTED 
begin tran 
set transaction isolation level read uncommitted
update linhkien set soluongton=1000 where malk='MOU001'
waitfor delay '00:00:10';
rollback tran;

begin tran
set transaction isolation level read uncommitted
select soluongton from linhkien where malk = 'MOU001'
commit tran

----2. Kịch bản 2: Mức READ COMMITTED 
set transaction isolation level read committed; 
begin tran
    select malk, tenlk, dongiaban from linhkien where malk = 'MOU001';
commit tran;

----3. Kịch bản 3: Khóa UPDLOCK 
begin tran
    declare @tonkho int;
    select @tonkho = soluongton from linhkien with (updlock) where malk = 'MOU002';
    waitfor delay '00:00:10';
    update linhkien set soluongton = @tonkho - 1 where malk = 'MOU002';
commit tran;

---- Kịch bản: Mức REPEATABLE READ
set transaction isolation level repeatable read; 
begin tran
    select malk, tenlk, dongiaban from linhkien where malk = 'MOU001';
    waitfor delay '00:00:10';
    select malk, tenlk, dongiaban from linhkien where malk = 'MOU001';
commit tran;

----4. Kịch bản 4: Mức SERIALIZABLE (Sửa lỗi Phantom)
set transaction isolation level serializable;
begin tran
    select count(*) as soluongloaichuot from linhkien where maloai = 'MOU';
    waitfor delay '00:00:10';
    select count(*) as soluongloaichuot from linhkien where maloai = 'MOU';
commit tran;

----5. Kịch bản 5: Dùng khóa XLOCK gây Tắc nghẽn (Deadlock)
begin tran
    update linhkien with (xlock) set dongiaban = 160000 where malk = 'MOU001';
    waitfor delay '00:00:05';
    update linhkien with (xlock) set dongiaban = 460000 where malk = 'MOU002';
commit tran;
*/

-- sao lưu và backup khi cần và khi chạy phải comment backup với restore

-- backup database QL_LinhKien_PC
-- to disk = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
-- with format, name = 'Full Backup';
-- go

-- use master;
-- go

-- restore database QL_LinhKien_PC
-- from disk = 'C:\SQLData\QL_LinhKien_PC_Full.bak'
-- with replace;
-- go



