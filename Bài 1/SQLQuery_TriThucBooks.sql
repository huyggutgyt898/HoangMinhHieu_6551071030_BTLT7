create database TriThuBooks_db;
go

USE TriThuBooks_db;
go

create table TheLoaiSach (
	MaTL int identity(1, 1) primary key,
	TenTheLoai nvarchar(100) not null unique,
	MoTa nvarchar(255),
	SoLuongSach int default 0,
	NgayTao datetime default GETDATE()
);