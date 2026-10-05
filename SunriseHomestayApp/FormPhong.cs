using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SunriseHomestayApp.Models;

namespace SunriseHomestayApp
{
    public partial class FormPhong : Form
    {
        private const string AllRoomTypes = "-- Tất cả loại phòng --";
        private const string AllStatuses = "-- Tất cả tình trạng --";
        private static readonly string[] RoomStatuses = { "Trống", "Đang ở", "Đang dọn" };

        private SunriseHomestayDbContext? _context;
        private int? _selectedRoomId;
        private string? _selectedImageFileName;

        private readonly TextBox txtSoPhong = new();
        private readonly NumericUpDown nudTangSo = new();
        private readonly ComboBox cbLoaiPhong = new();
        private readonly ComboBox cbTinhTrang = new();
        private readonly PictureBox picAnhPhong = new();
        private readonly Button btnChonAnh = new();
        private readonly Button btnQuanLyLoaiPhong = new();
        private readonly Button btnThem = new();
        private readonly Button btnSua = new();
        private readonly Button btnXoa = new();
        private readonly Button btnLamMoi = new();
        private readonly ComboBox cbLocLoaiPhong = new();
        private readonly ComboBox cbLocTinhTrang = new();
        private readonly Button btnTimKiem = new();
        private readonly DataGridView dgvPhong = new();

        private sealed class RoomTypeFilterOption
        {
            public int? MaLoai { get; init; }
            public string TenLoai { get; init; } = string.Empty;
        }

        private sealed class RoomGridItem
        {
            public int MaPhong { get; init; }
            public Image? Anh { get; init; }
            public string SoPhong { get; init; } = string.Empty;
            public int TangSo { get; init; }
            public string LoaiPhong { get; init; } = string.Empty;
            public string GiaDem { get; init; } = string.Empty;
            public string TinhTrang { get; init; } = string.Empty;
        }

        public FormPhong()
        {
            InitializeComponent();
            BuildUI();
            _context = DatabaseConfiguration.CreateContext();
            FormClosed += (_, _) => DisposeResources();
            if (LoadRoomTypes())
            {
                LoadRooms();
            }
        }

        private void BuildUI()
        {
            Text = "Quản Lý Phòng - Sunrise Homestay";
            ClientSize = new Size(970, 550);
            MinimumSize = new Size(990, 590);
            StartPosition = FormStartPosition.CenterScreen;

            var lblSoPhong = new Label { Text = "Số phòng", Location = new Point(20, 15), AutoSize = true };
            txtSoPhong.SetBounds(20, 35, 110, 25);
            txtSoPhong.MaxLength = 10;

            var lblTangSo = new Label { Text = "Tầng số", Location = new Point(150, 15), AutoSize = true };
            nudTangSo.SetBounds(150, 35, 75, 25);
            nudTangSo.Minimum = 0;
            nudTangSo.Maximum = 50;

            var lblLoaiPhong = new Label { Text = "Loại phòng", Location = new Point(245, 15), AutoSize = true };
            cbLoaiPhong.SetBounds(245, 35, 115, 25);
            cbLoaiPhong.DropDownStyle = ComboBoxStyle.DropDownList;

            var lblTinhTrang = new Label { Text = "Tình trạng", Location = new Point(375, 15), AutoSize = true };
            cbTinhTrang.SetBounds(375, 35, 105, 25);
            cbTinhTrang.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTinhTrang.Items.AddRange(RoomStatuses);
            cbTinhTrang.SelectedIndex = 0;

            picAnhPhong.SetBounds(495, 15, 90, 70);
            picAnhPhong.BorderStyle = BorderStyle.FixedSingle;
            picAnhPhong.SizeMode = PictureBoxSizeMode.Zoom;

            btnChonAnh.Text = "Chọn ảnh...";
            btnChonAnh.SetBounds(495, 90, 90, 26);
            btnChonAnh.Click += BtnChonAnh_Click;

            btnQuanLyLoaiPhong.Text = "Loại phòng";
            btnQuanLyLoaiPhong.SetBounds(600, 20, 90, 32);
            btnQuanLyLoaiPhong.Click += BtnQuanLyLoaiPhong_Click;

            btnThem.Text = "Thêm";
            btnThem.SetBounds(700, 20, 58, 32);
            btnThem.Click += BtnThem_Click;

            btnSua.Text = "Sửa";
            btnSua.SetBounds(765, 20, 58, 32);
            btnSua.Click += BtnSua_Click;

            btnXoa.Text = "Xóa";
            btnXoa.SetBounds(830, 20, 58, 32);
            btnXoa.Click += BtnXoa_Click;

            btnLamMoi.Text = "Làm mới";
            btnLamMoi.SetBounds(895, 20, 65, 32);
            btnLamMoi.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLamMoi.Click += (_, _) =>
            {
                cbLocLoaiPhong.SelectedIndex = 0;
                cbLocTinhTrang.SelectedIndex = 0;
                if (LoadRoomTypes())
                {
                    LoadRooms();
                }
            };

            cbLocLoaiPhong.SetBounds(20, 130, 165, 26);
            cbLocLoaiPhong.DropDownStyle = ComboBoxStyle.DropDownList;

            cbLocTinhTrang.SetBounds(195, 130, 165, 26);
            cbLocTinhTrang.DropDownStyle = ComboBoxStyle.DropDownList;
            cbLocTinhTrang.Items.AddRange(new object[] { AllStatuses, "Trống", "Đang ở", "Đang dọn" });
            cbLocTinhTrang.SelectedIndex = 0;

            btnTimKiem.Text = "Tìm kiếm";
            btnTimKiem.SetBounds(370, 128, 90, 28);
            btnTimKiem.Click += (_, _) => LoadRooms();

            ConfigureGrid();
            dgvPhong.SetBounds(20, 170, 930, 345);
            dgvPhong.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Controls.Clear();
            Controls.AddRange(new Control[]
            {
                lblSoPhong, txtSoPhong, lblTangSo, nudTangSo,
                lblLoaiPhong, cbLoaiPhong, lblTinhTrang, cbTinhTrang,
                picAnhPhong, btnChonAnh,
                btnQuanLyLoaiPhong, btnThem, btnSua, btnXoa, btnLamMoi,
                cbLocLoaiPhong, cbLocTinhTrang, btnTimKiem, dgvPhong
            });
        }

        private void ConfigureGrid()
        {
            dgvPhong.AutoGenerateColumns = false;
            dgvPhong.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPhong.AllowUserToAddRows = false;
            dgvPhong.ReadOnly = true;
            dgvPhong.RowTemplate.Height = 52;
            dgvPhong.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPhong.MultiSelect = false;
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MaPhong",
                DataPropertyName = nameof(RoomGridItem.MaPhong),
                HeaderText = "Mã phòng",
                FillWeight = 75
            });
            dgvPhong.Columns.Add(new DataGridViewImageColumn
            {
                Name = "Anh",
                DataPropertyName = nameof(RoomGridItem.Anh),
                HeaderText = "Ảnh",
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                FillWeight = 75
            });
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "SoPhong",
                DataPropertyName = nameof(RoomGridItem.SoPhong),
                HeaderText = "Số phòng",
                FillWeight = 85
            });
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TangSo",
                DataPropertyName = nameof(RoomGridItem.TangSo),
                HeaderText = "Tầng",
                FillWeight = 65
            });
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LoaiPhong",
                DataPropertyName = nameof(RoomGridItem.LoaiPhong),
                HeaderText = "Loại phòng",
                FillWeight = 110
            });
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "GiaDem",
                DataPropertyName = nameof(RoomGridItem.GiaDem),
                HeaderText = "Giá/đêm",
                FillWeight = 85
            });
            dgvPhong.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TinhTrang",
                DataPropertyName = nameof(RoomGridItem.TinhTrang),
                HeaderText = "Tình trạng",
                FillWeight = 100
            });
            dgvPhong.CellClick += DgvPhong_CellClick;
        }

        private bool LoadRoomTypes()
        {
            if (_context == null) return false;

            try
            {
                var previousFilter = (cbLocLoaiPhong.SelectedItem as RoomTypeFilterOption)?.MaLoai;
                var roomTypes = _context.LoaiPhongs
                    .AsNoTracking()
                    .OrderBy(type => type.TenLoai)
                    .ToList();

                cbLoaiPhong.DataSource = roomTypes;
                cbLoaiPhong.DisplayMember = nameof(LoaiPhong.TenLoai);
                cbLoaiPhong.ValueMember = nameof(LoaiPhong.MaLoai);

                var filterOptions = new List<RoomTypeFilterOption>
                {
                    new() { MaLoai = null, TenLoai = AllRoomTypes }
                };
                filterOptions.AddRange(roomTypes.Select(type => new RoomTypeFilterOption
                {
                    MaLoai = type.MaLoai,
                    TenLoai = type.TenLoai
                }));
                cbLocLoaiPhong.DataSource = filterOptions;
                cbLocLoaiPhong.DisplayMember = nameof(RoomTypeFilterOption.TenLoai);
                cbLocLoaiPhong.ValueMember = nameof(RoomTypeFilterOption.MaLoai);
                cbLocLoaiPhong.SelectedItem = filterOptions.FirstOrDefault(option => option.MaLoai == previousFilter)
                    ?? filterOptions[0];
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể nạp danh sách loại phòng. Hãy kiểm tra SQL Server (instance .\\SQL2025) " +
                    "và database SunriseHomestayDb.\n\nChi tiết: " + ex.Message,
                    "Lỗi kết nối CSDL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        private void LoadRooms()
        {
            if (_context == null) return;
            ClearInputs();

            try
            {
                var selectedTypeId = (cbLocLoaiPhong.SelectedItem as RoomTypeFilterOption)?.MaLoai;
                var selectedStatus = cbLocTinhTrang.SelectedItem as string;
                var query = _context.Phongs
                    .AsNoTracking()
                    .Include(room => room.MaLoaiNavigation)
                    .AsQueryable();

                if (selectedTypeId.HasValue)
                {
                    query = query.Where(room => room.MaLoai == selectedTypeId.Value);
                }

                if (!string.IsNullOrWhiteSpace(selectedStatus) && selectedStatus != AllStatuses)
                {
                    query = query.Where(room => room.TinhTrang == selectedStatus);
                }

                var rows = query
                    .OrderBy(room => room.MaPhong)
                    .ToList()
                    .Select(room => new RoomGridItem
                    {
                        MaPhong = room.MaPhong,
                        Anh = LoadRoomImage(room.HinhAnh),
                        SoPhong = room.SoPhong ?? string.Empty,
                        TangSo = room.TangSo ?? 0,
                        LoaiPhong = room.MaLoaiNavigation?.TenLoai ?? string.Empty,
                        GiaDem = (room.MaLoaiNavigation?.GiaMoiDem ?? 0)
                            .ToString("N0", CultureInfo.GetCultureInfo("vi-VN")),
                        TinhTrang = room.TinhTrang ?? string.Empty
                    })
                    .ToList();

                DisposeGridImages();
                dgvPhong.DataSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể nạp dữ liệu phòng. Hãy kiểm tra SQL Server (instance .\\SQL2025), " +
                    "database SunriseHomestayDb và cấu hình kết nối.\n\nChi tiết: " + ex.Message,
                    "Lỗi kết nối CSDL",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static Image? LoadRoomImage(string? storedImageName)
        {
            if (string.IsNullOrWhiteSpace(storedImageName)) return null;

            var safeFileName = Path.GetFileName(storedImageName);
            var imagePath = Path.Combine(AppContext.BaseDirectory, "Images", safeFileName);
            if (!File.Exists(imagePath)) return null;

            using var stream = File.OpenRead(imagePath);
            using var sourceImage = Image.FromStream(stream);
            return new Bitmap(sourceImage);
        }

        private static string CopyImageToApplicationFolder(string sourcePath)
        {
            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension is not ".jpg" and not ".jpeg" and not ".png" and not ".bmp")
            {
                throw new InvalidDataException("Chỉ hỗ trợ ảnh JPG, JPEG, PNG hoặc BMP.");
            }

            using (Image.FromFile(sourcePath))
            {
            }

            var imageDirectory = Path.Combine(AppContext.BaseDirectory, "Images");
            Directory.CreateDirectory(imageDirectory);
            var fileName = Guid.NewGuid().ToString("N") + extension;
            File.Copy(sourcePath, Path.Combine(imageDirectory, fileName));
            return fileName;
        }

        private void BtnChonAnh_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Tệp ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Chọn ảnh phòng"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                _selectedImageFileName = CopyImageToApplicationFolder(dialog.FileName);
                SetPictureBoxImage(LoadRoomImage(_selectedImageFileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể sao chép hoặc mở ảnh đã chọn.\n\nChi tiết: " + ex.Message,
                    "Lỗi ảnh",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnThem_Click(object? sender, EventArgs e)
        {
            if (_context == null || !ValidateRoomInput()) return;

            try
            {
                var room = new Phong
                {
                    SoPhong = txtSoPhong.Text.Trim(),
                    TangSo = (int)nudTangSo.Value,
                    MaLoai = Convert.ToInt32(cbLoaiPhong.SelectedValue),
                    TinhTrang = cbTinhTrang.SelectedItem?.ToString() ?? RoomStatuses[0],
                    HinhAnh = _selectedImageFileName
                };

                _context.Phongs.Add(room);
                _context.SaveChanges();
                ClearInputs();
                LoadRooms();
                MessageBox.Show("Thêm phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Thêm phòng thất bại.\n\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSua_Click(object? sender, EventArgs e)
        {
            if (_context == null || !ValidateRoomInput()) return;
            if (!_selectedRoomId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn phòng cần sửa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var room = _context.Phongs.FirstOrDefault(item => item.MaPhong == _selectedRoomId.Value);
                if (room == null)
                {
                    MessageBox.Show("Không tìm thấy phòng cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadRooms();
                    return;
                }

                room.SoPhong = txtSoPhong.Text.Trim();
                room.TangSo = (int)nudTangSo.Value;
                room.MaLoai = Convert.ToInt32(cbLoaiPhong.SelectedValue);
                room.TinhTrang = cbTinhTrang.SelectedItem?.ToString() ?? RoomStatuses[0];
                room.HinhAnh = _selectedImageFileName;

                _context.SaveChanges();
                LoadRooms();
                MessageBox.Show("Cập nhật phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Cập nhật phòng thất bại.\n\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnXoa_Click(object? sender, EventArgs e)
        {
            if (_context == null) return;
            if (!_selectedRoomId.HasValue)
            {
                MessageBox.Show("Vui lòng chọn phòng cần xóa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Bạn có chắc muốn xóa phòng {_selectedRoomId.Value}?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            try
            {
                var room = _context.Phongs.FirstOrDefault(item => item.MaPhong == _selectedRoomId.Value);
                if (room == null)
                {
                    MessageBox.Show("Không tìm thấy phòng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadRooms();
                    return;
                }

                _context.Phongs.Remove(room);
                _context.SaveChanges();
                ClearInputs();
                LoadRooms();
                MessageBox.Show("Xóa phòng thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _context.ChangeTracker.Clear();
                MessageBox.Show("Xóa phòng thất bại.\n\nChi tiết: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateRoomInput()
        {
            if (string.IsNullOrWhiteSpace(txtSoPhong.Text))
            {
                MessageBox.Show("Vui lòng nhập số phòng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoPhong.Focus();
                return false;
            }

            if (cbLoaiPhong.SelectedValue == null)
            {
                MessageBox.Show("Chưa có loại phòng. Hãy thêm loại phòng trước.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cbTinhTrang.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn tình trạng phòng.", "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void DgvPhong_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvPhong.Rows[e.RowIndex].DataBoundItem is not RoomGridItem row) return;

            _selectedRoomId = row.MaPhong;
            if (_context == null) return;

            var room = _context.Phongs
                .AsNoTracking()
                .FirstOrDefault(item => item.MaPhong == row.MaPhong);
            if (room == null) return;

            txtSoPhong.Text = room.SoPhong ?? string.Empty;
            nudTangSo.Value = Math.Min(nudTangSo.Maximum, Math.Max(nudTangSo.Minimum, room.TangSo ?? 0));
            if (room.MaLoai.HasValue)
            {
                cbLoaiPhong.SelectedValue = room.MaLoai.Value;
            }
            var status = room.TinhTrang ?? string.Empty;
            cbTinhTrang.SelectedItem = RoomStatuses.Contains(status)
                ? status
                : RoomStatuses[0];
            _selectedImageFileName = string.IsNullOrWhiteSpace(room.HinhAnh)
                ? null
                : Path.GetFileName(room.HinhAnh);
            SetPictureBoxImage(LoadRoomImage(room.HinhAnh));
        }

        private void BtnQuanLyLoaiPhong_Click(object? sender, EventArgs e)
        {
            using var form = new FormLoaiPhong();
            form.ShowDialog(this);
            if (LoadRoomTypes())
            {
                LoadRooms();
            }
        }

        private void ClearInputs()
        {
            _selectedRoomId = null;
            txtSoPhong.Clear();
            nudTangSo.Value = 0;
            if (cbLoaiPhong.Items.Count > 0) cbLoaiPhong.SelectedIndex = 0;
            if (cbTinhTrang.Items.Count > 0) cbTinhTrang.SelectedIndex = 0;
            _selectedImageFileName = null;
            SetPictureBoxImage(null);
            dgvPhong.ClearSelection();
        }

        private void SetPictureBoxImage(Image? image)
        {
            var previousImage = picAnhPhong.Image;
            picAnhPhong.Image = image;
            previousImage?.Dispose();
        }

        private void DisposeGridImages()
        {
            foreach (DataGridViewRow row in dgvPhong.Rows)
            {
                if (row.DataBoundItem is RoomGridItem item)
                {
                    item.Anh?.Dispose();
                }
            }
        }

        private void DisposeResources()
        {
            DisposeGridImages();
            SetPictureBoxImage(null);
            _context?.Dispose();
            _context = null;
        }

        private void FormPhong_Load(object sender, EventArgs e)
        {
        }
    }
}
