using System;
using System.Configuration;
using System.Data;
using Npgsql; // Đã chuyển đổi từ System.Data.SqlClient sang Npgsql
using System.Drawing;
using System.Windows.Forms;

namespace ERP_BanHang
{
    public partial class TaoDonHang : Form
    {
        // Chuỗi kết nối PostgreSQL chuẩn CSDL ERP_BanHang
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;

        public TaoDonHang()
        {
            InitializeComponent();
        }

        private void TaoDonHang_Load(object sender, EventArgs e)
        {
            KhoiTaoCotBang();
            TaoMaDonHangTuDong();
            LoadDataKhachHang();
            LoadDataNhanVien();
            LoadDataSanPham();
        }

        private void KhoiTaoCotBang()
        {
            dgvChiTietDonHang.Columns.Clear();

            // Khai báo cột DataGridView khớp với bảng ChiTietDonHang
            dgvChiTietDonHang.Columns.Add("colIDSP", "MÃ SP");
            dgvChiTietDonHang.Columns.Add("colTenSP", "TÊN SẢN PHẨM");
            dgvChiTietDonHang.Columns.Add("colSoLuong", "SL");
            dgvChiTietDonHang.Columns.Add("colDonGia", "ĐƠN GIÁ");
            dgvChiTietDonHang.Columns.Add("colThanhTien", "THÀNH TIỀN");

            // Nút bấm Xóa sản phẩm khỏi đơn
            DataGridViewButtonColumn btnXoa = new DataGridViewButtonColumn();
            btnXoa.Name = "colXoa";
            btnXoa.HeaderText = "THAO TÁC";
            btnXoa.Text = "❌ Xóa";
            btnXoa.UseColumnTextForButtonValue = true;
            btnXoa.FlatStyle = FlatStyle.Flat;
            dgvChiTietDonHang.Columns.Add(btnXoa);

            // Cấu hình tỷ lệ hiển thị
            dgvChiTietDonHang.Columns["colIDSP"].FillWeight = 12;
            dgvChiTietDonHang.Columns["colTenSP"].FillWeight = 35;
            dgvChiTietDonHang.Columns["colSoLuong"].FillWeight = 10;
            dgvChiTietDonHang.Columns["colDonGia"].FillWeight = 18;
            dgvChiTietDonHang.Columns["colThanhTien"].FillWeight = 18;
            dgvChiTietDonHang.Columns["colXoa"].FillWeight = 12;

            // Căn lề
            dgvChiTietDonHang.Columns["colIDSP"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTietDonHang.Columns["colSoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTietDonHang.Columns["colDonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTietDonHang.Columns["colThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTietDonHang.Columns["colXoa"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Chỉ cho phép sửa cột Số lượng trên DataGridView
            foreach (DataGridViewColumn col in dgvChiTietDonHang.Columns)
            {
                col.ReadOnly = (col.Name != "colSoLuong");
            }
        }

        // Tự động sinh mã đơn hàng tăng dần (DH013, DH014...)
        private void TaoMaDonHangTuDong()
        {
            // PostgreSQL: dùng COALESCE thay ISNULL và CAST/SUBSTRING chuẩn ANSI SQL
            string query = "SELECT COALESCE(MAX(CAST(SUBSTRING(ID_DH FROM 3 FOR 10) AS INT)), 0) + 1 FROM DonHang";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        int nextID = Convert.ToInt32(result);
                        txtMaDH.Text = "DH" + nextID.ToString("D3");
                    }
                }
                catch
                {
                    txtMaDH.Text = "DH" + DateTime.Now.ToString("HHmmss");
                }
            }
        }

        // Tải danh sách Khách hàng từ PostgreSQL
        private void LoadDataKhachHang()
        {
            string query = "SELECT ID_KH, TenDoanhNghiep, DiaChi FROM KhachHang";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboKhachHang.DataSource = dt;
                    cboKhachHang.DisplayMember = "TenDoanhNghiep";
                    cboKhachHang.ValueMember = "ID_KH";

                    if (dt.Rows.Count > 0)
                        cboKhachHang.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải khách hàng: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Khi chọn khách hàng thì tự động điền địa chỉ giao
        private void cboKhachHang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKhachHang.SelectedItem is DataRowView drv)
            {
                txtDiaChiGiao.Text = drv["DiaChi"].ToString();
            }
        }

        // Tải danh sách Nhân viên kinh doanh/bán hàng
        private void LoadDataNhanVien()
        {
            string query = "SELECT ID_NV, TenNV FROM NhanVien";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboNhanVien.DataSource = dt;
                    cboNhanVien.DisplayMember = "TenNV";
                    cboNhanVien.ValueMember = "ID_NV";

                    if (dt.Rows.Count > 0)
                        cboNhanVien.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải nhân viên: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Tải Sản phẩm (kết hợp bảng SanPham và HangHoa để lấy Tên và Giá bán)
        private void LoadDataSanPham()
        {
            // PostgreSQL dùng TO_CHAR thay cho FORMAT của SQL Server
            string query = @"
                SELECT 
                    SP.ID_SP, 
                    (HH.TenHang || ' - Giá: ' || TO_CHAR(SP.GiaSP, 'FM999,999,999,999') || ' đ') AS TenHienThi, 
                    HH.TenHang, 
                    SP.GiaSP 
                FROM SanPham SP
                INNER JOIN HangHoa HH ON SP.MaHang = HH.MaHang";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboSanPham.DataSource = dt;
                    cboSanPham.DisplayMember = "TenHienThi";
                    cboSanPham.ValueMember = "ID_SP";

                    if (dt.Rows.Count > 0)
                        cboSanPham.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải sản phẩm: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Khi chọn sản phẩm thì cập nhật giá sản phẩm lên TextBox
        private void cboSanPham_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSanPham.SelectedItem is DataRowView drv)
            {
                decimal donGia = Convert.ToDecimal(drv["GiaSP"]);
                txtDonGia.Text = string.Format("{0:N0} đ", donGia);
            }
        }

        // Nút bấm "+ Thêm SP"
        private void btnThemSP_Click(object sender, EventArgs e)
        {
            if (cboSanPham.SelectedItem is DataRowView drv)
            {
                string idSP = drv["ID_SP"].ToString();
                string tenSP = drv["TenHang"].ToString();
                decimal donGia = Convert.ToDecimal(drv["GiaSP"]);
                int soLuong = (int)nudSoLuong.Value;

                // Kiểm tra nếu sản phẩm đã thêm thì cộng dồn số lượng
                bool isExist = false;
                foreach (DataGridViewRow row in dgvChiTietDonHang.Rows)
                {
                    if (row.Cells["colIDSP"].Value?.ToString() == idSP)
                    {
                        int slCu = Convert.ToInt32(row.Cells["colSoLuong"].Value);
                        int slMoi = slCu + soLuong;
                        row.Cells["colSoLuong"].Value = slMoi;
                        row.Cells["colThanhTien"].Value = string.Format("{0:N0} đ", slMoi * donGia);
                        isExist = true;
                        break;
                    }
                }

                if (!isExist)
                {
                    decimal thanhTien = soLuong * donGia;
                    dgvChiTietDonHang.Rows.Add(idSP, tenSP, soLuong, string.Format("{0:N0} đ", donGia), string.Format("{0:N0} đ", thanhTien));
                }

                TinhTongTien();
            }
        }

        // Xóa sản phẩm khỏi bảng
        private void dgvChiTietDonHang_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvChiTietDonHang.Columns[e.ColumnIndex].Name == "colXoa")
            {
                dgvChiTietDonHang.Rows.RemoveAt(e.RowIndex);
                TinhTongTien();
            }
        }

        // Tự động tính lại tiền khi sửa số lượng trực tiếp trong ô DataGridView
        private void dgvChiTietDonHang_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvChiTietDonHang.Columns[e.ColumnIndex].Name == "colSoLuong")
            {
                DataGridViewRow row = dgvChiTietDonHang.Rows[e.RowIndex];
                if (int.TryParse(row.Cells["colSoLuong"].Value?.ToString(), out int sl) && sl > 0)
                {
                    string giaStr = row.Cells["colDonGia"].Value?.ToString().Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                    if (decimal.TryParse(giaStr, out decimal donGia))
                    {
                        row.Cells["colThanhTien"].Value = string.Format("{0:N0} đ", sl * donGia);
                        TinhTongTien();
                    }
                }
            }
        }

        // Tính tổng tiền đơn hàng
        private void TinhTongTien()
        {
            decimal tongTien = 0;

            foreach (DataGridViewRow row in dgvChiTietDonHang.Rows)
            {
                string thanhTienStr = row.Cells["colThanhTien"].Value?.ToString().Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                if (decimal.TryParse(thanhTienStr, out decimal tt))
                {
                    tongTien += tt;
                }
            }

            lblTongTienValue.Text = string.Format("{0:N0} đ", tongTien);
            lblThanhToanValue.Text = string.Format("{0:N0} đ", tongTien);
        }

        // Nút "Lưu Đơn Hàng" vào PostgreSQL dùng NpgsqlTransaction
        private void btnLuuDonHang_Click(object sender, EventArgs e)
        {
            if (dgvChiTietDonHang.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 sản phẩm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string idDH = txtMaDH.Text.Trim();
            string idKH = cboKhachHang.SelectedValue.ToString();
            string idNV = cboNhanVien.SelectedValue.ToString();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                conn.Open();
                NpgsqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. Chèn đơn hàng mới vào bảng DonHang (Dùng CURRENT_TIMESTAMP thay GETDATE())
                    string queryDH = @"INSERT INTO DonHang (ID_DH, ID_KH, ID_NV, NgayTao, TrangThai) 
                                       VALUES (@ID_DH, @ID_KH, @ID_NV, CURRENT_TIMESTAMP, N'Chờ xử lý')";

                    using (NpgsqlCommand cmdDH = new NpgsqlCommand(queryDH, conn, transaction))
                    {
                        cmdDH.Parameters.AddWithValue("@ID_DH", idDH);
                        cmdDH.Parameters.AddWithValue("@ID_KH", idKH);
                        cmdDH.Parameters.AddWithValue("@ID_NV", idNV);
                        cmdDH.ExecuteNonQuery();
                    }

                    // 2. Chèn từng sản phẩm vào bảng ChiTietDonHang
                    int index = 1;
                    foreach (DataGridViewRow row in dgvChiTietDonHang.Rows)
                    {
                        if (row.IsNewRow) continue;

                        string idCTDH = "CT" + idDH + index.ToString("D2");
                        string idSP = row.Cells["colIDSP"].Value.ToString();
                        int soLuong = Convert.ToInt32(row.Cells["colSoLuong"].Value);

                        string donGiaStr = row.Cells["colDonGia"].Value.ToString().Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                        decimal donGia = Convert.ToDecimal(donGiaStr);

                        string queryCT = @"INSERT INTO ChiTietDonHang (ID_CTDH, ID_DH, ID_SP, SoLuong, DonGia) 
                                           VALUES (@ID_CTDH, @ID_DH, @ID_SP, @SoLuong, @DonGia)";

                        using (NpgsqlCommand cmdCT = new NpgsqlCommand(queryCT, conn, transaction))
                        {
                            cmdCT.Parameters.AddWithValue("@ID_CTDH", idCTDH);
                            cmdCT.Parameters.AddWithValue("@ID_DH", idDH);
                            cmdCT.Parameters.AddWithValue("@ID_SP", idSP);
                            cmdCT.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdCT.Parameters.AddWithValue("@DonGia", donGia);
                            cmdCT.ExecuteNonQuery();
                        }

                        index++;
                    }

                    // Xác nhận lưu transaction
                    transaction.Commit();
                    MessageBox.Show($"Tạo đơn hàng {idDH} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Lỗi khi lưu đơn hàng: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}