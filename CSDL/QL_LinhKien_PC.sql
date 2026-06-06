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
    SDT varchar(10),
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
    DonGiaBan int,
    constraint PK_LinhKien primary key (MaLK),
    constraint FK_LK_LoaiLK foreign key (MaLoai) references LoaiLK(MaLoai),
    constraint FK_LK_NhaSanXuat foreign key (MaNSX) references NhaSanXuat(MaNSX)
);
go

create table KhachHang (
    MaKH char(6) not null,
    TenKH nvarchar(30),
    DChi nvarchar(50),
    SDT varchar(10), -- Đã đồng bộ thành varchar(10)
    Email varchar(50) null,
    constraint PK_KhachHang primary key (MaKH)
);
go

create table NhanVien (
    MaNV char(6) not null,
    TenNV nvarchar(40),
    GioiTinh nvarchar(5),
    NgaySinh date,
    SDT varchar(10), 
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
    TongTien int null default 0,
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
    DonGia int,
    constraint PK_CTHD primary key (MaHD, MaLK),
    constraint FK_CTHD_HoaDon foreign key (MaHD) references HoaDon(MaHD),
    constraint FK_CTHD_LinhKien foreign key (MaLK) references LinhKien(MaLK) 
);
go

create table PhieuNhap (
    MaPN char(5) not null,
    NgayNhap date,
    MaNV char(6) not null,
    MaNSX char(5) not null, -- Đã sửa lỗi thiếu dấu phẩy tại đây
    constraint PK_PhieuNhap primary key (MaPN),
    constraint FK_PhieuNhap_NhanVien foreign key (MaNV) references NhanVien(MaNV),
    constraint FK_PhieuNhap_NhaSanXuat foreign key (MaNSX) references NhaSanXuat(MaNSX)
);
go

create table ChiTietPN (
    MaPN char(5) not null,
    MaLK char(6) not null,
    SoLuongNhap int,
    DonGiaNhap int,
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

-- THÊM DỮ LIỆU
insert into NhaSanXuat (MaNSX, TenNSX, QuocGia, SDT) values
('NSX01', N'Genius', N'Taiwan', '0283925101'), 
('NSX02', N'Logitech', N'Switzerland', '0218635401'),
('NSX03', N'Kingston', N'USA', '1800555581'), 
('NSX04', N'Intel', N'USA', '0283825201'),
('NSX05', N'AMD', N'USA', '0283910122'), 
('NSX06', N'ASUS', N'Taiwan', '18006588  '),
('NSX07', N'Samsung', N'South Korea', '1800588881'), 
('NSX08', N'Gigabyte', N'Taiwan', '0283911881'),
('NSX09', N'Keychron', N'China', '0207321556'), 
('NSX10', N'H hành', N'Vietnam', '0901234567');
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

insert into ChiTietHD values
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
('HD010', 'MOU001', 5, 140000);
go

insert into PhieuNhap (MaPN, NgayNhap, MaNV, MaNSX) values 
('PN001', '10-01-2023', 'NV008', 'NSX01'), 
('PN002', '15-02-2023', 'NV009', 'NSX02'),
('PN003', '20-03-2023', 'NV008', 'NSX03'), 
('PN004', '05-04-2023', 'NV009', 'NSX04'),
('PN005', '12-05-2023', 'NV008', 'NSX05'), 
('PN006', '18-06-2023', 'NV009', 'NSX06'),
('PN007', '22-07-2023', 'NV008', 'NSX07'), 
('PN008', '08-08-2023', 'NV009', 'NSX08'),
('PN009', '30-09-2023', 'NV008', 'NSX09'), 
('PN010', '14-10-2023', 'NV009', 'NSX10');
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

insert into ChiTietHD (MaHD, MaLK, SoLuong, DonGia) values
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

--sửa bảng
alter table TaiKhoan drop constraint FK_TaiKhoan_NhanVien;
go

alter table TaiKhoan 
add constraint FK_TaiKhoan_NhanVien 
foreign key (MaNV) references NhanVien(MaNV) 
on delete cascade;
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

-- tổng tiền hóa đơn
update HoaDon
set TongTien = isnull((
    select sum(SoLuong * DonGia)
    from ChiTietHD cthd
    where cthd.MaHD = HoaDon.MaHD
), 0);
go

-- tạo trigger
create trigger trg_CapNhatTonKho
on ChiTietHD
after insert, update, delete
as
begin
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
    update LinhKien
    set SoLuongTon = LinhKien.SoLuongTon + HangVuaNhap.SoLuongNhap
    from LinhKien
    join inserted HangVuaNhap on LinhKien.MaLK = HangVuaNhap.MaLK;
end;
go

-- trigger mới xử lý trừ tồn kho khi xóa phiếu nhập
create trigger trg_TruTonKhoKhiXoaPhieuNhap
on ChiTietPN
after delete
as
begin
    -- Kiểm tra nếu tồn kho sau khi trừ bị âm
    if exists (
        select 1 
        from LinhKien lk
        join deleted d on lk.MaLK = d.MaLK
        where (isnull(lk.SoLuongTon, 0) - isnull(d.SoLuongNhap, 0)) < 0
    )
    begin
        rollback transaction;
        throw 50003, N'Không thể xóa chi tiết phiếu nhập này. Tồn kho hiện tại không đủ để hoàn trả.', 1;
    end

    -- Cập nhật lại kho
    update lk
    set lk.SoLuongTon = lk.SoLuongTon - d.SoLuongNhap
    from LinhKien lk
    join deleted d on lk.MaLK = d.MaLK;
end;
go

create trigger trg_CapNhatTongTien
on ChiTietHD
after insert, update, delete
as
begin
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

create function fn_TaoMaHoaDonMoi()
returns varchar(10)
as
begin
    declare @MaCu varchar(10);
    declare @MaMoi varchar(10);
    declare @So int;

    select top 1 @MaCu = MaHD 
    from HoaDon 
    order by MaHD desc;

    if @MaCu is null
        set @MaMoi = 'HD001';
    else
    begin
        set @So = cast(right(@MaCu, len(@MaCu) - 2) as int) + 1;
        set @MaMoi = 'HD' + right('000' + cast(@So as varchar), 3);
    end

    return @MaMoi;
end;
go

create function fn_TaoMaKhachHangMoi()
returns varchar(10)
as
begin
    declare @MaCu varchar(10);
    declare @MaMoi varchar(10);
    declare @So int;

    select top 1 @MaCu = MaKH 
    from KhachHang 
    order by MaKH desc;

    if @MaCu is null
        set @MaMoi = 'KH001';
    else
    begin
        set @So = cast(right(@MaCu, len(@MaCu) - 2) as int) + 1;
        set @MaMoi = 'KH' + right('000' + cast(@So as varchar), 3);
    end

    return @MaMoi;
end;
go

create function fn_TaoMaNhanVienMoi()
returns varchar(10)
as
begin
    declare @MaCu varchar(10);
    declare @MaMoi varchar(10);
    declare @So int;

    select top 1 @MaCu = MaNV 
    from NhanVien 
    order by MaNV desc;

    if @MaCu is null
        set @MaMoi = 'NV001';
    else
    begin
        set @So = cast(right(@MaCu, len(@MaCu) - 2) as int) + 1;
        set @MaMoi = 'NV' + right('000' + cast(@So as varchar), 3);
    end

    return @MaMoi;
end;
go
--Nguyễn Nhật Minh Quân--
--Thực hiện 2 function fn_TaoMaLinhKienMoi và fn_TaoMaPhieuNhapMoi
create function fn_TaoMaLinhKienMoi(@MaLoai varchar(3))
returns varchar(6) 
as
begin
    declare @MaCu varchar(6);
    declare @MaMoi varchar(6);
    declare @So int;

    select top 1 @MaCu = MaLK 
    from LinhKien 
    where MaLK like @MaLoai + '%' 
    order by MaLK desc;

    if @MaCu is null
        set @MaMoi = @MaLoai + '001';
    else
    begin
        set @So = cast(right(@MaCu, 3) as int) + 1;
        set @MaMoi = @MaLoai + right('000' + cast(@So as varchar), 3);
    end

    return @MaMoi;
end;
go

-- Hàm tạo mã phiếu nhập mới
create function fn_TaoMaPhieuNhapMoi()
returns varchar(10)
as
begin
    declare @MaCu varchar(10);
    declare @MaMoi varchar(10);
    declare @So int;

    select top 1 @MaCu = MaPN 
    from PhieuNhap 
    order by MaPN desc;

    if @MaCu is null
        set @MaMoi = 'PN001';
    else
    begin
        set @So = cast(right(@MaCu, len(@MaCu) - 2) as int) + 1;
        set @MaMoi = 'PN' + right('000' + cast(@So as varchar), 3);
    end

    return @MaMoi;
end;
go

create procedure sp_ThanhToanHoaDon
    @MaHD varchar(10)
as
begin
    begin try
        if not exists (select 1 from HoaDon where MaHD = @MaHD)
        begin
            throw 50002, N'Hóa đơn không tồn tại!', 1;
        end
        update HoaDon
        set TrangThai = N'Đã thanh toán',
            PhuongThucThanhToan = N'Tiền mặt',
            NgayThanhToan = cast(getdate() as date)
        where MaHD = @MaHD;
    end try
    begin catch     
        throw;
    end catch
end;
go

create procedure sp_XoaHoaDon
    @MaHD varchar(10)
as
begin
    begin try
        begin transaction;
        delete from ChiTietHd where MaHD = @MaHD;
        delete from HoaDon where MaHD = @MaHD;
        commit transaction;
    end try
    begin catch
        if @@trancount > 0
            rollback transaction;
        throw;
    end catch
end;
go

-- Stored Procedure mới xử lý xóa Phiếu Nhập
create procedure sp_XoaPhieuNhap
    @MaPN varchar(10)
as
begin
    begin try
        begin transaction;
        -- Xóa chi tiết trước (sẽ kích hoạt trigger trg_TruTonKhoKhiXoaPhieuNhap ở trên)
        delete from ChiTietPN where MaPN = @MaPN;
        
        -- Sau đó xóa phiếu nhập
        delete from PhieuNhap where MaPN = @MaPN;
        commit transaction;
    end try
    begin catch
        if @@trancount > 0 
            rollback transaction;
        throw;
    end catch
end;
go

create procedure sp_NhapLinhKien
    @MaPN char(5) output,
    @NgayNhap date,
    @MaNV char(6),
    @MaNSX char(5),
    @MaLK char(6),
    @SoLuongNhap int,
    @DonGiaNhap int
as
begin
    begin tran;
    begin try
        if (@MaPN is null or ltrim(rtrim(@MaPN)) = '')
        begin
            set @MaPN = dbo.fn_TaoMaPhieuNhapMoi();
        end

        if not exists (select 1 from NhanVien where MaNV = @MaNV and DaNghiViec = 0)
        begin
            rollback tran;
            throw 50010, N'Nhân viên nhập kho không hợp lệ!', 1;
            return;
        end

        if not exists (select 1 from NhaSanXuat where MaNSX = @MaNSX)
        begin
            rollback tran;
            throw 50011, N'Nhà sản xuất không tồn tại!', 1;
            return;
        end

        if not exists (select 1 from LinhKien where MaLK = @MaLK and MaNSX = @MaNSX and NgungKinhDoanh = 0)
        begin
            rollback tran;
            throw 50012, N'Linh kiện không thuộc nhà sản xuất đã chọn hoặc đã ngừng kinh doanh!', 1;
            return;
        end

        if (@SoLuongNhap is null or @SoLuongNhap <= 0)
        begin
            rollback tran;
            throw 50013, N'Số lượng nhập phải lớn hơn 0!', 1;
            return;
        end

        if (@DonGiaNhap is null or @DonGiaNhap <= 0)
        begin
            rollback tran;
            throw 50014, N'Đơn giá nhập phải lớn hơn 0!', 1;
            return;
        end

        if not exists (select 1 from PhieuNhap where MaPN = @MaPN)
        begin
            insert into PhieuNhap (MaPN, NgayNhap, MaNV, MaNSX)
            values (@MaPN, @NgayNhap, @MaNV, @MaNSX);
        end
        else if exists (select 1 from PhieuNhap where MaPN = @MaPN and MaNSX <> @MaNSX)
        begin
            rollback tran;
            throw 50015, N'Phiếu nhập đã tồn tại với nhà sản xuất khác!', 1;
            return;
        end

        if exists (select 1 from ChiTietPN where MaPN = @MaPN and MaLK = @MaLK)
        begin
            rollback tran;
            throw 50016, N'Linh kiện này đã có trong phiếu nhập!', 1;
            return;
        end

        insert into ChiTietPN (MaPN, MaLK, SoLuongNhap, DonGiaNhap)
        values (@MaPN, @MaLK, @SoLuongNhap, @DonGiaNhap);

        commit tran;
    end try
    begin catch
        if @@trancount > 0 rollback tran;
        throw;
    end catch
end;
go

create procedure sp_locdanhsachphieunhap
    @tukhoan nvarchar(100),
    @tungay date = null,
    @denngay date = null
as
begin
    select
        pn.MaPN as mapn,
        nv.TenNV as tennv,
        pn.NgayNhap as ngaynhap,
        nsx.TenNSX as tennsx,
        nsx.SDT as sdtnsx,
        isnull(sum(isnull(ct.SoLuongNhap, 0)), 0) as tongsoluong,
        cast(isnull(sum(isnull(ct.SoLuongNhap, 0) * isnull(ct.DonGiaNhap, 0)), 0) as money) as tongtien
    from PhieuNhap pn
    join NhanVien nv on pn.MaNV = nv.MaNV
    join NhaSanXuat nsx on pn.MaNSX = nsx.MaNSX
    left join ChiTietPN ct on pn.MaPN = ct.MaPN
    where (@tukhoan = ''
           or pn.MaPN like '%' + @tukhoan + '%'
           or nv.TenNV like N'%' + @tukhoan + '%'
           or nsx.TenNSX like N'%' + @tukhoan + '%')
      and (@tungay is null or pn.NgayNhap >= @tungay)
      and (@denngay is null or pn.NgayNhap <= @denngay)
    group by pn.MaPN, nv.TenNV, pn.NgayNhap, nsx.TenNSX, nsx.SDT
    order by pn.NgayNhap desc;
end;
go

create procedure sp_TaiChiTietPN
    @MaPN char(5)
as
begin
    select
        lk.MaLK as MaLinhKien,
        lk.TenLK as TenLinhKien,
        ct.SoLuongNhap,
        cast(ct.DonGiaNhap as money) as DonGiaNhap,
        cast(isnull(ct.SoLuongNhap, 0) * isnull(ct.DonGiaNhap, 0) as money) as ThanhTien
    from ChiTietPN ct
    join LinhKien lk on ct.MaLK = lk.MaLK
    where ct.MaPN = @MaPN;
end;
go

create procedure sp_ThongKePhieuNhap
    @TongPhieuNhap int output,
    @TongChiPhiNhap money output,
    @TongSoLuongNhap int output,
    @SoPhieuNhapThangNay int output
as
begin
    select @TongPhieuNhap = count(*)
    from PhieuNhap;

    select
        @TongChiPhiNhap = cast(isnull(sum(isnull(SoLuongNhap, 0) * isnull(DonGiaNhap, 0)), 0) as money),
        @TongSoLuongNhap = isnull(sum(isnull(SoLuongNhap, 0)), 0)
    from ChiTietPN;

    select @SoPhieuNhapThangNay = count(*)
    from PhieuNhap
    where month(NgayNhap) = month(getdate()) and year(NgayNhap) = year(getdate());
end;
go

create procedure sp_BanLinhKien 
    @MaHD char(5) OUTPUT, 
    @NgayHD date, 
    @MaKH char(6), 
    @MaNV char(6), 
    @MaLK char(6), 
    @SoLuongBan tinyint
as
begin
    begin tran;
    begin try
        if (@MaHD is null or ltrim(rtrim(@MaHD)) = '')
        begin
            set @MaHD = dbo.fn_TaoMaHoaDonMoi();
        end

        declare @TonKhoHienTai int;
        select @TonKhoHienTai = SoLuongTon from LinhKien where MaLK = @MaLK;

        if (@TonKhoHienTai is null or @TonKhoHienTai < @SoLuongBan)
        begin
            rollback tran;
            throw 50001, N'LỖI: Không đủ số lượng hàng tồn trong kho để bán!', 1;
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
    end try
    begin catch
        if @@TRANCOUNT > 0 rollback tran;
        throw;
    end catch
end;
go

create procedure sp_ThongKe
    @TongHoaDon int output,
    @TongDoanhThu money output,
    @DaThanhToan int output,
    @ChuaThanhToan int output
as
begin
    select @TongHoaDon = count(*)  
    from HoaDon

    select @TongDoanhThu = isnull(sum(TongTien), cast(0 as money))
    from HoaDon
    where TrangThai = N'Đã thanh toán'

    select @DaThanhToan = count(*)
    from HoaDon
    where TrangThai = N'Đã thanh toán'

    select @ChuaThanhToan = count(*)
    from HoaDon
    where TrangThai = N'Chưa thanh toán'
end
go

create proc sp_TaiChiTietSP
    @MaHD char(5)
as
begin
    select lk.TenLK as TenSanPham, ct.SoLuong, ct.DonGia, case when lk.TGBH is not null then N'Có' else N'Không có' end as HanBaoHanh
    from ChiTietHD ct, LinhKien lk
    where ct.MaLK = lk.MaLK and ct.MaHD = @MaHD
end
go

create procedure sp_locdanhsachhoadon
    @tukhoan nvarchar(100),
    @trangthai nvarchar(30),
    @tungay date = null,  
    @denngay date = null  
as
begin
    select 
        hd.mahd, 
        kh.tenkh, 
        nv.tennv, 
        hd.ngayhd, 
        isnull(hd.tongtien, 0) as tongtien, 
        hd.phuongthucthanhtoan,
        hd.trangthai
    from hoadon hd, khachhang kh, nhanvien nv
    where hd.makh = kh.makh 
        and hd.manv = nv.manv

        and (@trangthai = '' or hd.trangthai = @trangthai)
        and (@tukhoan = '' or hd.mahd like '%' + @tukhoan + '%' or kh.tenkh like N'%' + @tukhoan + '%')

        and (@tungay is null or hd.ngayhd >= @tungay)
        and (@denngay is null or hd.ngayhd <= @denngay)
        
    order by hd.ngayhd desc;
end
go 


create procedure sp_DanhSacKhachHangChuaTT
as
begin
    declare @KhachHangNo table (
        MaHD char(5),
        TenKH nvarchar(30),
        SDT char(10),
        NgayLap date,
        SoTienNo money
    );
    declare @MaHD char(5), @TenKH nvarchar(30), @SDT char(10), @NgayLap date, @SoTienNo money;
    declare cur_KhachNo cursor for
        select hd.MaHD, kh.TenKH, kh.SDT, hd.NgayHD, hd.TongTien
        from HoaDon hd
        join KhachHang kh on hd.MaKH = kh.MaKH
        where hd.TrangThai = N'Chưa thanh toán';
    open cur_KhachNo;
    fetch next from cur_KhachNo into @MaHD, @TenKH, @SDT, @NgayLap, @SoTienNo;

    while @@fetch_status = 0
    begin
        insert into @KhachHangNo (MaHD, TenKH, SDT, NgayLap, SoTienNo)
        values (@MaHD, @TenKH, @SDT, @NgayLap, @SoTienNo);

        fetch next from cur_KhachNo into @MaHD, @TenKH, @SDT, @NgayLap, @SoTienNo;
    end;
    close cur_KhachNo;
    deallocate cur_KhachNo;
    select * from @KhachHangNo;
end;
go

create procedure sp_baocaotonkho
as
begin
   select MaLK,TenLK, SoLuongTon,DVT, DonGiaBan
   from LinhKien
   where SoLuongTon<10;
   end;
go
--Nguyễn Nhật Minh Quân--
--Phân quyền và backup, restore

-- quản trị người dùng
-- dọn dẹp trước khi tạo để không bị lỗi khi chạy lại script
-- quản trị người dùng
-- dọn dẹp trước khi tạo để không bị lỗi khi chạy lại script

use QL_LinhKien_PC;
go

if exists (select * from sys.database_principals where name = 'quanLyUser')
    drop user quanLyUser;
go

if exists (select * from sys.database_principals where name = 'nhanVienThuNganUser')
    drop user nhanVienThuNganUser;
go

if exists (select * from sys.database_principals where name = 'nhanVienCskhUser')
    drop user nhanVienCskhUser;
go

if exists (select * from sys.database_principals where name = 'nhanVienKhoUser')
    drop user nhanVienKhoUser;
go

if exists (select * from sys.database_principals where name = 'role_quanLy')
    drop role role_quanLy;
go

if exists (select * from sys.database_principals where name = 'role_thuNgan')
    drop role role_thuNgan;
go

if exists (select * from sys.database_principals where name = 'role_Cskh')
    drop role role_Cskh;
go

if exists (select * from sys.database_principals where name = 'role_kho')
    drop role role_kho;
go

use master;
go

if exists (select * from sys.server_principals where name = 'quanLyLogin')
    drop login quanLyLogin;
go

if exists (select * from sys.server_principals where name = 'nhanVienThuNganLogin')
    drop login nhanVienThuNganLogin;
go

if exists (select * from sys.server_principals where name = 'nhanVienCskhLogin')
    drop login nhanVienCskhLogin;
go

if exists (select * from sys.server_principals where name = 'nhanVienKhoLogin')
    drop login nhanVienKhoLogin;
go
-- tạo login
use master;
go

exec sp_addlogin 'quanLyLogin', '123';
exec sp_addlogin 'nhanVienThuNganLogin', '123';
exec sp_addlogin 'nhanVienCskhLogin', '123';
exec sp_addlogin 'nhanVienKhoLogin', '123';
go

-- tạo user
use QL_LinhKien_PC;
go

exec sp_adduser 'quanLyLogin', 'quanLyUser';
exec sp_adduser 'nhanVienThuNganLogin', 'nhanVienThuNganUser';
exec sp_adduser 'nhanVienCskhLogin', 'nhanVienCskhUser';
exec sp_adduser 'nhanVienKhoLogin', 'nhanVienKhoUser';
go

-- tạo nhóm quyền
exec sp_addrole 'role_quanLy';
exec sp_addrole 'role_thuNgan';
exec sp_addrole 'role_Cskh';
exec sp_addrole 'role_kho';
go

-- thêm user vào nhóm quyền
exec sp_addrolemember 'role_quanLy', 'quanLyUser';
exec sp_addrolemember 'role_thuNgan', 'nhanVienThuNganUser';
exec sp_addrolemember 'role_Cskh', 'nhanVienCskhUser';
exec sp_addrolemember 'role_kho', 'nhanVienKhoUser';
go

-- phân quyền cho quản lý
grant control
to role_quanLy;
go

alter role db_backupoperator add member quanLyUser;
go

use master;
go

alter server role dbcreator add member quanLyLogin;
go

use QL_LinhKien_PC;
go

-- phân quyền cho nhân viên thu ngân
grant all on KhachHang to role_thuNgan;
grant all on HoaDon to role_thuNgan;
grant all on ChiTietHD to role_thuNgan;

grant select on NhanVien to role_thuNgan;
grant select on TaiKhoan to role_thuNgan;
grant select on LoaiLK to role_thuNgan;
grant select on LinhKien to role_thuNgan;

grant execute on sp_locdanhsachhoadon  to role_thuNgan;
grant execute on sp_ThongKe to role_thuNgan;
grant execute on sp_ThanhToanHoaDon to role_thuNgan;
grant execute on sp_BanLinhKien to role_thuNgan;
grant execute on sp_XoaHoaDon to role_thuNgan;
grant execute on sp_DanhSacKhachHangChuaTT to role_thuNgan;
grant execute on fn_TaoMaHoaDonMoi to role_thuNgan;
grant execute on fn_TaoMaKhachHangMoi to role_thuNgan;
grant execute on fn_DoanhThuTheoThang to role_thuNgan;
grant execute on sp_baocaotonkho to role_thuNgan;
go

-- phân quyền cho nhân viên chăm sóc khách hàng
grant all on KhachHang to role_Cskh;

grant select on LoaiLK to role_Cskh;
grant select on LinhKien to role_Cskh;
grant select on HoaDon to role_Cskh;
grant select on TaiKhoan to role_Cskh;
grant select on NhanVien to role_Cskh;

grant execute on fn_TaoMaKhachHangMoi to role_Cskh;
grant execute on fn_DoanhThuTheoThang to role_Cskh;
grant execute on sp_baocaotonkho to role_Cskh;
grant execute on sp_DanhSacKhachHangChuaTT to role_Cskh;
go

--phân quyền cho nhân viên kho
grant all on LoaiLK to role_kho;
grant all on LinhKien to role_kho;
grant all on PhieuNhap to role_kho;
grant all on ChiTietPN to role_kho;

grant select on HoaDon to role_kho;
grant select on KhachHang to role_kho;
grant select on NhaSanXuat to role_kho;
grant select on TaiKhoan to role_kho;
grant select on NhanVien to role_kho;

grant execute on sp_baocaotonkho to role_kho;
grant execute on fn_TaoMaLinhKienMoi to role_kho;
grant execute on fn_DoanhThuTheoThang to role_kho;
grant execute on fn_TaoMaPhieuNhapMoi to role_kho;
grant execute on sp_XoaPhieuNhap to role_kho;
grant execute on sp_NhapLinhKien to role_kho;
grant execute on sp_locdanhsachphieunhap to role_kho;
grant execute on sp_TaiChiTietPN to role_kho;
grant execute on sp_ThongKePhieuNhap to role_kho;
grant execute on sp_DanhSacKhachHangChuaTT to role_kho;
go


-- sao lưu và backup nếu muốn sử dụng bỏ comment
----tạo file backup
--alter database QL_LinhKien_PC set recovery full
--go
--backup database QL_LinhKien_PC
--to disk = 'D:\BaiTap\QL_LinhKien_PC_Full.bak'
--with init
--go
--backup database QL_LinhKien_PC
--to disk = 'D:\BaiTap\QL_LinhKien_PC_Diff.bak'
--with differential, init
--go
--backup log QL_LinhKien_PC
--to disk = 'D:\BaiTap\QL_LinhKien_PC_Log.trn'
--with init
--go
----kịch bản 
--delete from ChiTietHD; 
--delete from HoaDon;   
--delete from KhachHang; 
--go
----phục hồi
--use master;
--go
--alter database QL_LinhKien_PC set single_user with rollback immediate
--go
--restore database QL_LinhKien_PC
--from disk = 'D:\BaiTap\QL_LinhKien_PC_Full.bak'
--with replace, norecovery;
--go
--restore database QL_LinhKien_PC
--from disk = 'D:\BaiTap\QL_LinhKien_PC_Diff.bak'
--with norecovery;
--go
--restore log QL_LinhKien_PC
--from disk = 'D:\BaiTap\QL_LinhKien_PC_Log.trn'
--with recovery
--go
--alter database QL_LinhKien_PC set multi_user

--1. kịch bản 1 mức cô lập read uncommited( lỗi mất dữ liệu cập nhật) 
-- giao tác a:

--Trịnh Hữu Kiến Quốc--
-- Xử lý giao tác đồng thời có rollback: sp_kichban6_giaotaca_rollback và sp_kichban6_giaotacb_docsaorollback
create procedure sp_kichban1_giaotaca
    @isfixmode bit
as
begin
    begin tran;
    declare @tonkho int;

    if @isfixmode = 1
        select @tonkho = soluongton from linhkien with (updlock) where malk = 'MOU001'; 
    else
        -- a đọc được số lượng là 50
        select @tonkho = soluongton from linhkien where malk = 'MOU001'; 

    waitfor delay '00:00:10'; -- demo thời gian chờ thực tế 
    
    -- a tính toán: 50 - 10 = 40 và ghi xuống
    update linhkien set soluongton = @tonkho - 10 where malk = 'MOU001';
    commit tran;
end
go

-- giao tác b:
create procedure sp_kichban1_giaotacb
    @isfixmode bit
as
begin
    begin tran;
    declare @tonkho int;

    if @isfixmode = 1
        select @tonkho = soluongton from linhkien with (updlock) where malk = 'MOU001'; 
    else
        -- do a không giữ khóa đọc, b vẫn đọc được số lượng cũ là 50
        select @tonkho = soluongton from linhkien where malk = 'MOU001';

    -- b tính toán: 50 - 20 = 30 và ghi xuống kho ngay lập tức
    update linhkien set soluongton = @tonkho - 20 where malk = 'MOU001';
    commit tran;
end
go

-- 2. kịch bản 2: mức read committed( lỗi đọc dữ liệu rác)
-- giao tác a:
create procedure sp_kichban2_giaotaca
as
begin
    begin tran;
    update linhkien set soluongton = 1000 where malk = 'MOU001';
    waitfor delay '00:00:10';
    rollback tran; 
end
go

-- giao tác b:
create procedure sp_kichban2_giaotacb
    @isfixmode bit
as
begin
    if @isfixmode = 1
        -- sửa lỗi bằng mức cô lập read committed
        set transaction isolation level read committed; 
    else
        -- demo lỗi đọc dữ liệu rác( dirty read)
        set transaction isolation level read uncommitted; 

    begin tran;
    select soluongton from linhkien where malk = 'MOU001';
    commit tran;
end
go

-- 3. kịch bản 3: mức cô lập repeatable read( lỗi không thể đọc lại)
-- giao tác a:
create procedure sp_kichban3_giaotaca
    @isfixmode bit
as
begin
    if @isfixmode = 1
        -- sửa lỗi bằng mức cô lập repeatable read
        set transaction isolation level repeatable read; 
    else
        -- demo lỗi không thể đọc lại( unrepeatable read)
        set transaction isolation level read committed; 

    begin tran;
    select     soluongton as tonkho from linhkien where malk = 'MOU001'; -- lần 1: số lượng gốc
    
    waitfor delay '00:00:10';
    
    select    soluongton as tonkho from linhkien where malk = 'MOU001'; -- lần 2: số lượng đã bị b thay đổi
    commit tran;
end
go

-- giao tác b:
create procedure sp_kichban3_giaotacb
as
begin
    begin tran;
    update linhkien set soluongton = 9999 where malk = 'MOU001';
    commit tran; -- b ghi thành công ngay lập tức do a ở mức read committed không giữ khóa đọc (s-lock)
end
go

-- 4. kịch bản 4: mức cô lập serializable (sửa lỗi bóng ma dữ liệu)
-- giao tác a:
create procedure sp_kichban4_giaotaca
    @isfixmode bit
as
begin
    if @isfixmode = 1
        -- sửa lỗi bằng mức cô lập serializable
        set transaction isolation level serializable; 
    else
        -- demo lỗi bóng ma dữ liệu( phantom read)
        set transaction isolation level repeatable read; 

    begin tran;
    select   count(*) as tongso from linhkien where maloai = 'MOU'; -- ví dụ ra 5 linh kiện
    
    waitfor delay '00:00:10';
    
    select count(*) as tongso  from linhkien where maloai = 'MOU'; -- đếm lại ra 6 (dòng bóng ma xuất hiện) / hoặc chắc chắn vẫn là 5 (nếu fix)
    commit tran;
end
go

-- giao tác b:
create procedure sp_kichban4_giaotacb
as
begin
    begin tran;
    -- xóa dòng rác để tránh lỗi khóa chính khi test lại nhiều lần
    delete from linhkien where malk = 'MOU099';
    
    -- b thêm thành công 1 linh kiện mới vào nhóm 'mou'
    insert into linhkien(malk, tenlk, maloai,MaNSX) values ('MOU099', N'chuột bluetooth', 'MOU','NSX02');
    commit tran;
end
go

-- 5. kịch bản 5: dùng khóa xlock gây tắc nghẽn (deadlock)
-- giao tác a:
create procedure sp_kichban5_giaotaca
as
begin
    begin tran;
    -- 1. a khóa độc quyền mou001
    update linhkien with (xlock) set dongiaban = 160000 where malk = 'MOU001';
    waitfor delay '00:00:05'; 
    -- 3. a đòi khóa mou002 (nhưng lúc này b đang giữ) -> a bị treo
    update linhkien with (xlock) set dongiaban = 460000 where malk = 'MOU002';
    commit tran;
end
go

-- giao tác b:
create procedure sp_kichban5_giaotacb
    @isfixmode bit
as
begin
    begin tran;

    if @isfixmode = 1
    begin
        -- cách fix: ép giao tác b cũng phải xin khóa mou001 trước
        -- nếu a đang chạy, b sẽ bị treo ngay tại dòng này và kiên nhẫn chờ đợi a xong
        update linhkien with (xlock) set dongiaban = 999999 where malk = 'MOU001';
        waitfor delay '00:00:05';
        -- sau khi a commit và nhả khóa, b mới được đi tiếp xuống đây để khóa mou002
        update linhkien with (xlock) set dongiaban = 888888 where malk = 'MOU002';
    end
    else
    begin
        -- 2. b khóa độc quyền mou002 
        update linhkien with (xlock) set dongiaban = 888888 where malk = 'MOU002';
        waitfor delay '00:00:05';
        -- 4. b đòi khóa mou001 (nhưng lúc này a đang giữ) -> b bị treo
        update linhkien with (xlock) set dongiaban = 999999 where malk = 'MOU001';
    end

    commit tran;
end
go

-- Trịnh Hữu Kiến Quốc - kịch bản 6: xử lý giao tác đồng thời có rollback
-- giao tác a: cập nhật tạm thời, giữ khóa, gặp lỗi và rollback toàn bộ thay đổi
create procedure sp_kichban6_giaotaca_rollback
as
begin
    set xact_abort on;

    declare @tonkho_bandau int;
    declare @tonkho_tam int;

    begin try
        begin tran;

        select @tonkho_bandau = soluongton
        from linhkien with (updlock, holdlock)
        where malk = 'MOU001';

        update linhkien
        set soluongton = @tonkho_bandau - 15
        where malk = 'MOU001';

        select @tonkho_tam = soluongton
        from linhkien
        where malk = 'MOU001';

        waitfor delay '00:00:10';

        -- cố tình phát sinh lỗi nghiệp vụ để chứng minh rollback
        if (@tonkho_tam < @tonkho_bandau)
            throw 50106, N'Lỗi mô phỏng: giao tác T1 bị hủy nên phải rollback về tồn kho ban đầu.', 1;

        commit tran;
    end try
    begin catch
        if @@trancount > 0
            rollback tran;

        select @tonkho_tam = soluongton
        from linhkien
        where malk = 'MOU001';

        select
            N'T1 đã rollback, dữ liệu trở về trạng thái trước khi giao tác chạy' as ThongBao,
            @tonkho_bandau as TonKhoBanDau,
            @tonkho_tam as TonKhoTamThoi;
    end catch
end
go

-- giao tác b: chạy đồng thời để chứng minh không đọc dữ liệu tạm khi T1 đang rollback
create procedure sp_kichban6_giaotacb_docsaorollback
as
begin
    set transaction isolation level read committed;
    begin tran;

    select
        N'T2 đọc sau khi T1 rollback hoặc nhả khóa' as ThongBao,
        soluongton as TonKhoDocDuoc
    from linhkien
    where malk = 'MOU001';

    commit tran;
end
go

-- Hàm tính tổng giá trị tồn kho theo loại linh kiện
create function fn_GiaTriTonKhoTheoLoai(@MaLoai char(3))
returns money
as
begin
    declare @TongGiaTri money;

    select @TongGiaTri = sum(isnull(SoLuongTon, 0) * isnull(DonGiaBan, 0))
    from LinhKien
    where MaLoai = @MaLoai;

    return isnull(@TongGiaTri, 0);
end;
go

-- Hàm tính tổng số lượng đã nhập của một linh kiện
create function fn_TongSoLuongNhapTheoLinhKien(@MaLK char(6))
returns int
as
begin
    declare @TongSoLuongNhap int;

    select @TongSoLuongNhap = sum(isnull(SoLuongNhap, 0))
    from ChiTietPN
    where MaLK = @MaLK;

    return isnull(@TongSoLuongNhap, 0);
end;
go


--/* 1.Kịch bản 1 mức cô lập READ UNCOMMITED( lỗi mất dữ liệu cập nhật)
--Vì lỗi mất dữ liệu cập nhật( Updated Lost) được SQL Sever đều tự động xin khóa độc quyền (X-Lock) và giữ đến cuối giao tác, bất kể mức cô lập nào các thao tác CRUD*/
---- Giao tác A:
--begin tran
--    declare @tonkho int;
--    -- A đọc được số lượng là 50
--    select @tonkho = soluongton from linhkien where malk = 'MOU001'; 
--    waitfor delay '00:00:10';-- Demo thời gian chờ thực tế 
--     -- A tính toán: 50 - 10 = 40 và ghi xuống
--    update linhkien set soluongton = @tonkho - 10 where malk = 'MOU001';
--commit tran;

----Giao tác B
--begin tran
--    declare @tonkho int;
--    -- Do A không giữ khóa đọc, B vẫn đọc được số lượng cũ là 50
--    select @tonkho = soluongton from linhkien where malk = 'MOU001'; 
--    -- B tính toán: 50 - 20 = 30 và ghi xuống kho ngay lập tức
--    update linhkien set soluongton = @tonkho - 20 where malk = 'MOU001';
--commit tran;

--select SoLuongTon 
--from LinhKien
--where malk = 'MOU001'

---- sửa lỗi bằng mức cô lập  READ UNCOMMITED
--/*set transaction isolation level read committed;
--begin tran
--    declare @tonkho int;
--    select @tonkho = soluongton from linhkien where malk = 'MOU001'; 
--    waitfor delay '00:00:10'; 
--    update linhkien set soluongton = @tonkho - 10 where malk = 'MOU001';
--commit tran;

--set transaction isolation level read committed;
--begin tran
--    declare @tonkho int;
--    select @tonkho = soluongton from linhkien where malk = 'MOU001'; 
--    update linhkien set soluongton = @tonkho - 20 where malk = 'MOU001';
--commit tran;*/

--2. Kịch bản 2: Mức READ COMMITTED( lỗi đọc dữ liệu rác)
-- demo lỗi đọc dữ liệu rác( Dirty Read)
--begin tran
--    update linhkien set soluongton = 1000 where malk = 'MOU001';
--    waitfor delay '00:00:10';
--rollback tran;

--set transaction isolation level read uncommitted;
--begin tran
--    select soluongton from linhkien where malk = 'MOU001';
--commit tran;

---- sửa lỗi bằng mức cô lập  READ COMMITED
--set transaction isolation level read committed;
--begin tran
--    select soluongton from linhkien where malk = 'MOU001';
--commit tran;

-- 3. Kịch bản 3:mức cô lập REPEATABLE READ( lỗi không thể đọc lại)
--demo lỗi không thể đọc lại( UNREPEATABLE READ)
--set transaction isolation level read committed;
--begin tran
--    select soluongton from linhkien where malk = 'MOU001'; -- Lần 1: Số lượng gốc
--    waitfor delay '00:00:10';
--    select soluongton from linhkien where malk = 'MOU001'; -- Lần 2: Số lượng đã bị B thay đổi
--commit tran;

--begin tran
--    update linhkien set soluongton = 9999 where malk = 'MOU001';
--commit tran; -- B ghi thành công ngay lập tức do A ở mức Read Committed không giữ khóa đọc (S-Lock)

---- sửa lỗi bằng mức cô lập  REPEATABLE READ
--set transaction isolation level repeatable read;
--begin tran
--    select soluongton from linhkien where malk = 'MOU001'; 
--    waitfor delay '00:00:10';
--    select soluongton from linhkien where malk = 'MOU001'; 
--commit tran;

--4. Kịch bản 4: Mức cô lập  SERIALIZABLE (Sửa lỗi bóng ma dữ liệu)
--demo lỗi bóng ma dữ liệu( Repeatable Read)
--set transaction isolation level repeatable read;
--begin tran
--    select count(*) from linhkien where maloai = 'MOU'; -- Ví dụ ra 5 linh kiện
--    waitfor delay '00:00:10';
--    select count(*) from linhkien where maloai = 'MOU'; -- Đếm lại ra 6 (Dòng bóng ma xuất hiện)
--commit tran;

--begin tran
--     B thêm thành công 1 linh kiện mới vào nhóm 'MOU'
--    insert into linhkien(malk, tenlk, maloai,MaNSX) values ('MOU099', N'chuột bluetooth', 'MOU','NSX02');
--commit tran;

-- sửa lỗi bằng mức cô lập SERIALIZABLE
--set transaction isolation level serializable;
--begin tran
--    select count(*) from linhkien where maloai = 'MOU'; 
--    waitfor delay '00:00:10';
--    select count(*) from linhkien where maloai = 'MOU'; -- Chắc chắn vẫn là 5
--commit tran;

----5. Kịch bản 5: Dùng khóa XLOCK gây Tắc nghẽn (Deadlock)
----Giao tác A:
--begin tran
--    -- 1. A Khóa độc quyền MOU001
--    update linhkien with (xlock) set dongiaban = 160000 where malk = 'MOU001';
--    waitfor delay '00:00:05'; -- 
--    -- 3. A đòi khóa MOU002 (Nhưng lúc này B đang giữ) -> A BỊ TREO
--    update linhkien with (xlock) set dongiaban = 460000 where malk = 'MOU002';
--commit tran;

----Giao tác B:
--begin tran
--    -- 2. B Khóa độc quyền MOU002 
--    update linhkien with (xlock) set dongiaban = 888888 where malk = 'MOU002';
--    waitfor delay '00:00:05';
--    -- 4. B đòi khóa MOU001 (Nhưng lúc này A đang giữ) -> B BỊ TREO
--    update linhkien with (xlock) set dongiaban = 999999 where malk = 'MOU001';
--commit tran;

---- theo sách thì SQL sever sẽ tự hủy 1 giao tác để chạy hoàn tất thì ở đây giao tác A là giao tác bị hủy\

-- -- CÁCH FIX: Ép Giao tác B CŨNG PHẢI xin khóa MOU001 TRƯỚC
--begin tran
--    -- Nếu A đang chạy, B sẽ BỊ TREO ngay tại dòng này và KIÊN NHẪN CHỜ ĐỢI A xong
--    update linhkien with (xlock) set dongiaban = 999999 where malk = 'MOU001';
--    waitfor delay '00:00:05';
--    -- Sau khi A commit và nhả khóa, B mới được đi tiếp xuống đây để khóa MOU002
--    update linhkien with (xlock) set dongiaban = 888888 where malk = 'MOU002';
--commit tran;

--6. Kịch bản 6: xử lý giao tác đồng thời có rollback
--Mục tiêu demo: T1 cập nhật tạm thời và giữ khóa, sau đó gặp lỗi nên rollback.
--T2 chạy đồng thời ở READ COMMITTED nên không đọc dữ liệu tạm, phải chờ T1 rollback/nhả khóa.

---- Giao tác A - chạy ở cửa sổ query thứ nhất
--set xact_abort on;
--begin try
--    begin tran;
--
--    declare @tonkho_bandau int;
--    declare @tonkho_tam int;
--
--    select @tonkho_bandau = soluongton
--    from linhkien with (updlock, holdlock)
--    where malk = 'MOU001';
--
--    update linhkien
--    set soluongton = @tonkho_bandau - 15
--    where malk = 'MOU001';
--
--    select @tonkho_tam = soluongton
--    from linhkien
--    where malk = 'MOU001';
--
--    select
--        N'T1 đã cập nhật tạm thời, đang giữ khóa 10 giây' as ThongBao,
--        @tonkho_bandau as TonKhoBanDau,
--        @tonkho_tam as TonKhoTamThoi;
--
--    waitfor delay '00:00:10';
--
--    throw 50106, N'Lỗi mô phỏng: T1 bị hủy nên rollback toàn bộ thay đổi.', 1;
--
--    commit tran;
--end try
--begin catch
--    if @@trancount > 0
--        rollback tran;
--
--    select
--        N'T1 đã rollback, tồn kho trở về như trước giao tác' as ThongBao,
--        error_message() as LyDoRollback,
--        soluongton as TonKhoSauRollback
--    from linhkien
--    where malk = 'MOU001';
--end catch;

---- Giao tác B - chạy ở cửa sổ query thứ hai trong lúc T1 đang waitfor
--set transaction isolation level read committed;
--begin tran;
--
--select
--    N'T2 đọc sau khi T1 rollback hoặc nhả khóa' as ThongBao,
--    soluongton as TonKhoDocDuoc
--from linhkien
--where malk = 'MOU001';
--
--commit tran;
