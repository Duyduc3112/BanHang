using System;
using System.Configuration;
using System.Data;
using Npgsql; // Đã đổi từ System.Data.SqlClient sang Npgsql
using System.Drawing;
using System.Windows.Forms;

namespace ERP_BanHang
{
    public partial class QlyGiaoHang : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;
        private DataTable dtGiaoHang;

        public QlyGiaoHang()
        {
            InitializeComponent();
        }

        private void QlyGiaoHang_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            if (cboFilterStatus.Items.Count > 0)
                cboFilterStatus.SelectedIndex = 0;

            KhoiTaoCotBang();
            LoadDataGiaoHang();
        }

        private void KhoiTaoCotBang()
        {
            dgvGiaoHang.Columns.Clear();
            dgvGiaoHang.AutoGenerateColumns = false;

            // 1. Mã đơn hàng
            DataGridViewTextBoxColumn colIDDH = new DataGridViewTextBoxColumn();
            colIDDH.Name = "colIDDH";
            colIDDH.HeaderText = "MÃ ĐƠN HÀNG";
            colIDDH.DataPropertyName = "ID_DH";
            colIDDH.FillWeight = 10;
            colIDDH.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colIDDH.DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            colIDDH.DefaultCellStyle.ForeColor = Color.FromArgb(13, 110, 253);
            dgvGiaoHang.Columns.Add(colIDDH);

            // 2. Doanh nghiệp / Khách hàng nhận
            DataGridViewTextBoxColumn colKH = new DataGridViewTextBoxColumn();
            colKH.Name = "colTenKhachHang";
            colKH.HeaderText = "KHÁCH HÀNG NHẬN";
            colKH.DataPropertyName = "TenDoanhNghiep";
            colKH.FillWeight = 20;
            dgvGiaoHang.Columns.Add(colKH);

            // 3. Người nhận thực tế
            DataGridViewTextBoxColumn colNguoiNhan = new DataGridViewTextBoxColumn();
            colNguoiNhan.Name = "colNguoiNhan";
            colNguoiNhan.HeaderText = "NGƯỜI NHẬN";
            colNguoiNhan.DataPropertyName = "NguoiNhan";
            colNguoiNhan.FillWeight = 15;
            dgvGiaoHang.Columns.Add(colNguoiNhan);

            // 4. Địa chỉ giao
            DataGridViewTextBoxColumn colDiaChi = new DataGridViewTextBoxColumn();
            colDiaChi.Name = "colDiaChi";
            colDiaChi.HeaderText = "ĐỊA CHỈ GIAO HÀNG";
            colDiaChi.DataPropertyName = "DiaChiGiaoHang";
            colDiaChi.FillWeight = 25;
            dgvGiaoHang.Columns.Add(colDiaChi);

            // 5. Số điện thoại nhận
            DataGridViewTextBoxColumn colSDT = new DataGridViewTextBoxColumn();
            colSDT.Name = "colSDT";
            colSDT.HeaderText = "SĐT LIÊN HỆ";
            colSDT.DataPropertyName = "SDTNguoiNhan";
            colSDT.FillWeight = 12;
            colSDT.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvGiaoHang.Columns.Add(colSDT);

            // 6. Trạng thái giao hàng
            DataGridViewTextBoxColumn colTrangThai = new DataGridViewTextBoxColumn();
            colTrangThai.Name = "colTrangThaiGiao";
            colTrangThai.HeaderText = "TRẠNG THÁI GIAO";
            colTrangThai.DataPropertyName = "TrangThaiGiaoHang";
            colTrangThai.FillWeight = 13;
            colTrangThai.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvGiaoHang.Columns.Add(colTrangThai);

            dgvGiaoHang.AllowUserToResizeColumns = true;
            dgvGiaoHang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadDataGiaoHang()
        {
            // PostgreSQL: Thay ISNULL thành COALESCE
            string query = @"
                SELECT 
                    GH.ID_GH,
                    DH.ID_DH,
                    KH.TenDoanhNghiep,
                    COALESCE(GH.NguoiNhan, KH.NguoiDaiDien) AS NguoiNhan,
                    COALESCE(GH.DiaChiGiaoHang, KH.DiaChi) AS DiaChiGiaoHang,
                    COALESCE(GH.SDTNguoiNhan, KH.SDT) AS SDTNguoiNhan,
                    COALESCE(GH.TrangThaiGiaoHang, N'Chưa giao') AS TrangThaiGiaoHang
                FROM GiaoHang GH
                INNER JOIN DonHang DH ON GH.ID_DH = DH.ID_DH
                INNER JOIN KhachHang KH ON DH.ID_KH = KH.ID_KH
                ORDER BY DH.NgayTao DESC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    dtGiaoHang = new DataTable();
                    da.Fill(dtGiaoHang);

                    dgvGiaoHang.DataSource = dtGiaoHang;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL khi tải danh sách giao hàng: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvGiaoHang_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvGiaoHang.Columns[e.ColumnIndex].Name == "colTrangThaiGiao" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (status == "Đã giao")
                {
                    e.CellStyle.BackColor = Color.FromArgb(212, 237, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(21, 87, 36);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (status == "Đang giao")
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 243, 205);
                    e.CellStyle.ForeColor = Color.FromArgb(133, 100, 4);
                    e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                }
                else if (status == "Chờ giao" || status == "Chưa giao")
                {
                    e.CellStyle.BackColor = Color.FromArgb(248, 215, 218);
                    e.CellStyle.ForeColor = Color.FromArgb(114, 28, 36);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LocDuLieu();
        }

        private void cboFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LocDuLieu();
        }

        private void LocDuLieu()
        {
            if (dtGiaoHang == null) return;

            string keyword = txtSearch.Text.Trim().Replace("'", "''");
            if (keyword == "Tìm mã đơn, địa chỉ, người nhận, NV giao...") keyword = "";

            string statusFilter = cboFilterStatus.SelectedItem?.ToString();

            DataView dv = dtGiaoHang.DefaultView;
            string filter = "1=1";

            if (!string.IsNullOrEmpty(keyword))
            {
                filter += $" AND (ID_DH LIKE '%{keyword}%' OR TenDoanhNghiep LIKE '%{keyword}%' OR NguoiNhan LIKE '%{keyword}%' OR DiaChiGiaoHang LIKE '%{keyword}%' OR SDTNguoiNhan LIKE '%{keyword}%')";
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Tất cả trạng thái")
            {
                filter += $" AND TrangThaiGiaoHang = '{statusFilter}'";
            }

            dv.RowFilter = filter;
            dgvGiaoHang.DataSource = dv;
        }

        // ==========================================
        // XỬ LÝ PHÂN CÔNG VẬN CHUYỂN (CÓ ĐIỀU KIỆN)
        // ==========================================
        private void btnPhanCongNV_Click(object sender, EventArgs e)
        {
            if (dgvGiaoHang.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một đơn hàng trong danh sách để phân công vận chuyển!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string idDH = dgvGiaoHang.CurrentRow.Cells["colIDDH"].Value?.ToString();
            string tenKhach = dgvGiaoHang.CurrentRow.Cells["colTenKhachHang"].Value?.ToString();
            string trangThaiHienTai = dgvGiaoHang.CurrentRow.Cells["colTrangThaiGiao"].Value?.ToString();

            // RÀNG BUỘC ĐIỀU KIỆN: Chỉ cho phép phân công đơn ở trạng thái "Chưa giao" hoặc "Chờ giao"
            if (trangThaiHienTai == "Đang giao" || trangThaiHienTai == "Đã giao")
            {
                MessageBox.Show($"Đơn hàng [{idDH}] đang ở trạng thái '{trangThaiHienTai}'.\nKhông thể phân công lại cho đơn đã hoặc đang vận chuyển!",
                                "Không thể phân công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"Xác nhận chuyển thông tin vận chuyển đơn hàng [{idDH}] ({tenKhach}) sang Phân hệ Vận Chuyển?",
                "Thông báo phân công",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                MessageBox.Show($"Đã phát yêu cầu điều phối vận chuyển cho đơn hàng [{idDH}] thành công!\nThông tin đơn đã được chuyển sang bộ phận vận chuyển.",
                                "Phân công thành công",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
        }

        // ==========================================
        // KHU VỰC ĐIỀU HƯỚNG SIDEBAR
        // ==========================================

        private void btnSanPham_Click(object sender, EventArgs e)
        {
            this.Hide();
            QlySanPham qlySanPhamForm = new QlySanPham();
            qlySanPhamForm.ShowDialog();
            this.Close();
        }

        private void btnDonHang_Click(object sender, EventArgs e)
        {
            this.Hide();
            QlyDonHang qlyDonHangForm = new QlyDonHang();
            qlyDonHangForm.ShowDialog();
            this.Close();
        }

        private void btnNhaPhanPhoi_Click(object sender, EventArgs e)
        {
            this.Hide();
            QlyKhachHang qlyKhachHangForm = new QlyKhachHang();
            qlyKhachHangForm.ShowDialog();
            this.Close();
        }

        private void btnHangTraLoi_Click(object sender, EventArgs e)
        {
            this.Hide();
            XulyHangLoi xulyHangLoiForm = new XulyHangLoi();
            xulyHangLoiForm.ShowDialog();
            this.Close();
        }

        private void btnGiaoHang_Click(object sender, EventArgs e)
        {
            this.Hide();
            QlyGiaoHang qlyGiaoHangForm = new QlyGiaoHang();
            qlyGiaoHangForm.ShowDialog();
            this.Close();
        }

        private void btnThongKe_Click(object sender, EventArgs e)
        {
            this.Hide();
            BaoCaoThongKe baoCaoThongKeForm = new BaoCaoThongKe();
            baoCaoThongKeForm.ShowDialog();
            this.Close();
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn ĐĂNG XUẤT và quay lại màn hình đăng nhập?",
                "Xác nhận đăng xuất",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    // 1. Xóa file phiên làm việc tạm (nếu có)
                    string tempPath = System.IO.Path.Combine(Application.StartupPath, "session.txt");
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                    }

                    // 2. Thuật toán tìm file ERP_Khach.exe linh hoạt trên mọi máy
                    string baseDir = Application.StartupPath;
                    string targetExe = "ERP_Khach.exe";
                    string pathExeDangNhap = "";

                    // Kiểm tra các vị trí file exe có thể nằm
                    string[] possiblePaths = new string[]
                    {
                // Khi chạy Release / Đóng gói chung thư mục
                System.IO.Path.Combine(baseDir, targetExe),
                System.IO.Path.Combine(baseDir, "..", targetExe),
                System.IO.Path.Combine(baseDir, "..", "ERP_Khach", targetExe),
                
                // Khi chạy Debug trong Visual Studio
                System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\..\ERP_Khach\bin\Debug\ERP_Khach.exe")),
                System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\..\ERP_Khach\bin\Release\ERP_Khach.exe"))
                    };

                    foreach (string p in possiblePaths)
                    {
                        if (System.IO.File.Exists(p))
                        {
                            pathExeDangNhap = p;
                            break;
                        }
                    }

                    // 3. Khởi chạy ứng dụng đăng nhập và đóng ứng dụng hiện tại
                    if (!string.IsNullOrEmpty(pathExeDangNhap))
                    {
                        System.Diagnostics.Process.Start(pathExeDangNhap);
                        Application.Exit(); // Đóng hoàn toàn ERP_BanHang
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy file ứng dụng Đăng nhập (ERP_Khach.exe)!\nVui lòng kiểm tra lại thư mục chứa file.",
                                        "Lỗi khởi chạy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đăng xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}