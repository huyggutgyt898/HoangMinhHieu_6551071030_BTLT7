CREATE DATABASE SunriseHomestayDb;
GO

USE SunriseHomestayDb;
GO

-- 1. Bảng LoaiPhong
CREATE TABLE LoaiPhong (
    MaLoai INT IDENTITY(1,1) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL,
    GiaMoiDem DECIMAL(18,2),
    MoTa NVARCHAR(255)
);

-- 2. Bảng Phong
CREATE TABLE Phong (
    MaPhong INT IDENTITY(1,1) PRIMARY KEY,
    SoPhong VARCHAR(10) NOT NULL,
    TangSo INT,
    TinhTrang NVARCHAR(20),
    HinhAnh NVARCHAR(255),
    MaLoai INT FOREIGN KEY REFERENCES LoaiPhong(MaLoai)
);
GO

-- Thêm dữ liệu mẫu vào bảng LoaiPhong (Đã sửa từ khóa VALUES)
INSERT INTO LoaiPhong (TenLoai, GiaMoiDem, MoTa) 
VALUES 
(N'Phòng Đơn', 35000, N'Phòng 1 giường single'),
(N'Phòng Đôi', 20000, N'Phòng 1 giường double'),
(N'Phòng VIP', 30000, N'Phòng VIP sang trọng');

-- Thêm dữ liệu mẫu vào bảng Phong
INSERT INTO Phong (SoPhong, TangSo, TinhTrang, HinhAnh, MaLoai) 
VALUES 
('1', 1, N'Trống', '1.jpg', 1),
('2', 1, N'Đang ở', '2.jpg', 2),
('3', 2, N'Đang dọn', '3.jpg', 1);