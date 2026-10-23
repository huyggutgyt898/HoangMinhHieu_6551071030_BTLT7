using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SunriseHomestayApp.Models;

namespace SunriseHomestayApp
{
    public partial class FormLoaiPhong : Form
    {
        private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");

        private SunriseHomestayDbContext? _context;
        private int? _selectedRoomTypeId;

        private readonly DataGridView dgvLoaiPhong = new();
        private readonly TextBox txtTimKiem = new();
        private readonly TextBox txtTenLoai = new();
        private readonly TextBox txtGia = new();
        private readonly TextBox txtMoTa = new();
        private readonly Button btnTimKiem = new();
        private readonly Button btnThem = new();
        private readonly Button btnSua = new();
        private readonly Button btnXoa = new();
        private readonly Button btnLamMoi = new();

        private sealed class RoomTypeGridItem
        {
            public int MaLoai { get; init; }
            public string TenLoai { get; init; } = string.Empty;
            public string GiaMoiDem { get; init; } = string.Empty;
            public decimal? GiaChinhXac { get; init; }
            public string MoTa { get; init; } = string.Empty;
        }

        public FormLoaiPhong()
        {
            InitializeComponent();
            BuildUI();
            _context = DatabaseConfiguration.CreateContext();
            FormClosed += (_, _) => _context?.Dispose();
            LoadData();
        }

        private void BuildUI()
        {
            Text = "Quản Lý Loại Phòng - Sunrise Homestay";
            ClientSize = new System.Drawing.Size(760, 500);
            MinimumSize = new System.Drawing.Size(780, 560);
            StartPosition = FormStartPosition.CenterParent;

            var lblTenLoai = new Label { Text = "Tên loại:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            txtTenLoai.SetBounds(105, 18, 220, 25);
            txtTenLoai.MaxLength = 100;

            var lblGia = new Label { Text = "Giá/đêm:", Location = new System.Drawing.Point(350, 20), AutoSize = true };
            txtGia.SetBounds(420, 18, 180, 25);

            var lblMoTa = new Label { Text = "Mô tả:", Location = new System.Drawing.Point(20, 58), AutoSize = true };
            txtMoTa.SetBounds(105, 55, 495, 50);
            txtMoTa.MaxLength = 255;
            txtMoTa.Multiline = true;
            txtMoTa.ScrollBars = ScrollBars.Vertical;

            btnThem.Text = "Thêm";
            btnThem.SetBounds(20, 120, 80, 30);
            btnThem.Click += BtnThem_Click;

            btnSua.Text = "Sửa";
            btnSua.SetBounds(110, 120, 80, 30);
            btnSua.Click += BtnSua_Click;

            btnXoa.Text = "Xóa";
            btnXoa.SetBounds(200, 120, 80, 30);
            btnXoa.Click += BtnXoa_Click;

            btnLamMoi.Text = "Làm mới";
            btnLamMoi.SetBounds(290, 120, 85, 30);
            btnLamMoi.Click += (_, _) =>
            {
                ClearInputs();
                txtTimKiem.Clear();
                LoadData();
            };

            txtTimKiem.SetBounds(20, 170, 235, 26);
            txtTimKiem.PlaceholderText = "Tìm theo tên loại phòng";
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    LoadData();
                }
            };

            btnTimKiem.Text = "Tìm kiếm";
            btnTimKiem.SetBounds(265, 169, 90, 28);
            btnTimKiem.Click += (_, _) => LoadData();

            dgvLoaiPhong.SetBounds(20, 210, 720, 270);
            dgvLoaiPhong.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvLoaiPhong.AutoGenerateColumns = false;
            dgvLoaiPhong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLoaiPhong.AllowUserToAddRows = false;
            dgvLoaiPhong.ReadOnly = true;
            dgvLoaiPhong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLoaiPhong.MultiSelect = false;
            dgvLoaiPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaLoai",
                DataPropertyName = nameof(RoomTypeGridItem.MaLoai),
                HeaderText = "Mã loại",
                FillWeight = 65
            });
            dgvLoaiPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TenLoai",
                DataPropertyName = nameof(RoomTypeGridItem.TenLoai),
                HeaderText = "Tên loại phòng",
                FillWeight = 120
            });
            dgvLoaiPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GiaMoiDem",
                DataPropertyName = nameof(RoomTypeGridItem.GiaMoiDem),
                HeaderText = "Giá/đêm",
                FillWeight = 85
            });
            dgvLoaiPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MoTa",
                DataPropertyName = nameof(RoomTypeGridItem.MoTa),
                HeaderText = "Mô tả",
                FillWeight = 180
            });
            dgvLoaiPhong.CellClick += DgvLoaiPhong_CellClick;

            Controls.Clear();
            Controls.AddRange(new Control[]
            {
                lblTenLoai, txtTenLoai, lblGia, txtGia, lblMoTa, txtMoTa,
                btnThem, btnSua, btnXoa, btnLamMoi,
                txtTimKiem, btnTimKiem, dgvLoaiPhong
            });
        }

        private void LoadData()
        {
            if (_context == null) return;
            ClearInputs();

            try
            {
                var searchText = txtTimKiem.Text.Trim();
                var query = _context.LoaiPhongs.AsNoTracking().AsQueryable();
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(type => type.TenLoai.Contains(searchText));
                }

                var rows = query
                    .OrderBy(type => type.MaLoai)
                    .ToList()
                    .Select(type => new RoomTypeGridItem
                    {
                        MaLoai = type.MaLoai,
                        TenLoai = type.TenLoai ?? string.Empty,
                        GiaMoiDem = type.GiaMoiDem.HasValue
                            ? type.GiaMoiDem.Value.ToString("N0", VietnameseCulture)
                            : string.Empty,
                        GiaChinhXac = type.GiaMoiDem,
                        MoTa = type.MoTa ?? string.Empty
                    })
                    .ToList();
                dgvLoaiPhong.DataSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể nạp dữ liệu loại phòng. Hãy kiểm tra SQL Server (instance .\\SQL2025) " +
                    "và database SunriseHomestayDb.\n\nChi tiết: " + ex.Message,
                    "Lỗi kết nối CSDL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (_context == null || !ValidateInput(out var price)) return;

            try
            {
                _context.LoaiPhongs.Add(new LoaiPhong
                {
                    TenLoai = txtTenLoai.Text.Trim(),
                    GiaMoiDem = price,
                    MoTa = NullIfEmpty(txtMoTa.Text)
                });
                _context.SaveChanges();
                ClearInputs();
                LoadData();
                MessageBox.Show("Thêm loại phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Thêm loại phòng thất bại.\n\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_context == null || !ValidateInput(out var price)) return;
            if (!_selectedRoomTypeId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn loại phòng cần sửa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var roomType = _context.LoaiPhongs.FirstOrDefault(type => type.MaLoai == _selectedRoomTypeId.Value);
                if (roomType == null)
                {
                    MessageBox.Show("Không tìm thấy loại phòng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData();
                    return;
                }

                roomType.TenLoai = txtTenLoai.Text.Trim();
                roomType.GiaMoiDem = price;
                roomType.MoTa = NullIfEmpty(txtMoTa.Text);
                _context.SaveChanges();
                LoadData();
                MessageBox.Show("Cập nhật loại phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Cập nhật loại phòng thất bại.\n\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_context == null) return;
            if (!_selectedRoomTypeId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn loại phòng cần xóa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa loại phòng {_selectedRoomTypeId.Value}?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                var roomType = _context.LoaiPhongs.FirstOrDefault(type => type.MaLoai == _selectedRoomTypeId.Value);
                if (roomType == null)
                {
                    MessageBox.Show("Không tìm thấy loại phòng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadData();
                    return;
                }

                _context.LoaiPhongs.Remove(roomType);
                _context.SaveChanges();
                ClearInputs();
                LoadData();
                MessageBox.Show("Xóa loại phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show(
                    "Không thể xóa loại phòng. Nếu loại phòng đang được phòng sử dụng, hãy chuyển hoặc xóa các phòng đó trước.\n\nChi tiết: " + ex.Message,
                    "Xóa thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput(out decimal? price)
        {
            price = null;
            if (string.IsNullOrWhiteSpace(txtTenLoai.Text))
            {
                MessageBox.Show("Vui lòng nhập tên loại phòng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenLoai.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtGia.Text))
            {
                if (!decimal.TryParse(txtGia.Text.Trim(), NumberStyles.Number, VietnameseCulture, out var parsedPrice) || parsedPrice < 0)
                {
                    MessageBox.Show("Giá/đêm phải là số không âm. Ví dụ: 35000 hoặc 35.000.", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGia.Focus();
                    return false;
                }

                price = parsedPrice;
            }

            return true;
        }

        private void DgvLoaiPhong_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvLoaiPhong.Rows[e.RowIndex].DataBoundItem is not RoomTypeGridItem row) return;

            _selectedRoomTypeId = row.MaLoai;
            txtTenLoai.Text = row.TenLoai;
            txtGia.Text = row.GiaChinhXac?.ToString("0.##", VietnameseCulture) ?? string.Empty;
            txtMoTa.Text = row.MoTa;
        }

        private static string? NullIfEmpty(string value)
        {
            var trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private void ClearInputs()
        {
            _selectedRoomTypeId = null;
            txtTenLoai.Clear();
            txtGia.Clear();
            txtMoTa.Clear();
            dgvLoaiPhong.ClearSelection();
        }
    }
}
