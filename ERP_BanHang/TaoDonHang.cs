using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Npgsql;

namespace ERP_BanHang
{
    public partial class TaoDonHang : Form
    {
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

            dgvChiTietDonHang.Columns.Add("colIDSP", "MÃ SP");
            dgvChiTietDonHang.Columns.Add("colTenSP", "TÊN SẢN PHẨM");
            dgvChiTietDonHang.Columns.Add("colSoLuong", "SL");
            dgvChiTietDonHang.Columns.Add("colDonGia", "ĐƠN GIÁ");
            dgvChiTietDonHang.Columns.Add("colThanhTien", "THÀNH TIỀN");

            DataGridViewButtonColumn btnXoa = new DataGridViewButtonColumn();
            btnXoa.Name = "colXoa";
            btnXoa.HeaderText = "THAO TÁC";
            btnXoa.Text = "Xóa";
            btnXoa.UseColumnTextForButtonValue = true;
            btnXoa.FlatStyle = FlatStyle.Flat;
            dgvChiTietDonHang.Columns.Add(btnXoa);

            dgvChiTietDonHang.Columns["colIDSP"].FillWeight = 12;
            dgvChiTietDonHang.Columns["colTenSP"].FillWeight = 35;
            dgvChiTietDonHang.Columns["colSoLuong"].FillWeight = 10;
            dgvChiTietDonHang.Columns["colDonGia"].FillWeight = 18;
            dgvChiTietDonHang.Columns["colThanhTien"].FillWeight = 18;
            dgvChiTietDonHang.Columns["colXoa"].FillWeight = 12;

            dgvChiTietDonHang.Columns["colIDSP"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTietDonHang.Columns["colSoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTietDonHang.Columns["colDonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTietDonHang.Columns["colThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTietDonHang.Columns["colXoa"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn col in dgvChiTietDonHang.Columns)
            {
                col.ReadOnly = (col.Name != "colSoLuong");
            }
        }

        private void TaoMaDonHangTuDong()
        {
            string query = "SELECT COALESCE(MAX(CAST(SUBSTRING(id_dh FROM 3 FOR 10) AS INT)), 0) + 1 FROM donhang";

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

        private void LoadDataKhachHang()
        {
            string query = "SELECT id_kh, tendoanhnghiep, diachi FROM khachhang ORDER BY tendoanhnghiep ASC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboKhachHang.DataSource = dt;
                    cboKhachHang.DisplayMember = "tendoanhnghiep";
                    cboKhachHang.ValueMember = "id_kh";

                    if (dt.Rows.Count > 0)
                        cboKhachHang.SelectedIndex = 0;
                    else
                        cboKhachHang.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải khách hàng: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cboKhachHang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKhachHang.SelectedItem is DataRowView drv)
            {
                txtDiaChiGiao.Text = drv["diachi"]?.ToString();
            }
            else
            {
                txtDiaChiGiao.Clear();
            }
        }

        private void LoadDataNhanVien()
        {
            string query = "SELECT id_nv, tennv FROM nhanvien ORDER BY tennv ASC";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboNhanVien.DataSource = dt;
                    cboNhanVien.DisplayMember = "tennv";
                    cboNhanVien.ValueMember = "id_nv";

                    if (dt.Rows.Count > 0)
                        cboNhanVien.SelectedIndex = 0;
                    else
                        cboNhanVien.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải nhân viên: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDataSanPham()
        {
            string query = @"
                SELECT 
                    sp.id_sp, 
                    (hh.tenhang || ' - Giá: ' || TO_CHAR(sp.giasp, 'FM999,999,999,999') || ' đ (Tồn: ' || hh.tonkho || ')') AS tenhienthi, 
                    hh.tenhang, 
                    sp.giasp,
                    hh.tonkho
                FROM sanpham sp
                INNER JOIN hanghoa hh ON sp.mahang = hh.mahang";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlDataAdapter da = new NpgsqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    cboSanPham.DataSource = dt;
                    cboSanPham.DisplayMember = "tenhienthi";
                    cboSanPham.ValueMember = "id_sp";

                    if (dt.Rows.Count > 0)
                        cboSanPham.SelectedIndex = 0;
                    else
                        cboSanPham.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải sản phẩm: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cboSanPham_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboSanPham.SelectedItem is DataRowView drv)
            {
                decimal donGia = Convert.ToDecimal(drv["giasp"]);
                int tonKho = Convert.ToInt32(drv["tonkho"]);

                txtDonGia.Text = string.Format("{0:N0} đ", donGia);

                // Giới hạn số lượng tối đa chọn được bằng đúng số tồn kho
                nudSoLuong.Maximum = tonKho > 0 ? tonKho : 1;
                nudSoLuong.Value = tonKho > 0 ? 1 : 0;
            }
            else
            {
                txtDonGia.Clear();
            }
        }

        // ==========================================
        // THÊM SẢN PHẨM VÀO GIỎ HÀNG
        // ==========================================
        private void btnThemSP_Click(object sender, EventArgs e)
        {
            if (cboSanPham.SelectedIndex == -1 || cboSanPham.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn sản phẩm muốn thêm!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboSanPham.Focus();
                return;
            }

            int soLuongThem = (int)nudSoLuong.Value;
            if (soLuongThem <= 0)
            {
                MessageBox.Show("Số lượng mua phải lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nudSoLuong.Focus();
                return;
            }

            DataRowView drv = cboSanPham.SelectedItem as DataRowView;
            string idSP = drv["id_sp"].ToString();
            string tenSP = drv["tenhang"].ToString();
            decimal donGia = Convert.ToDecimal(drv["giasp"]);
            int tonKho = Convert.ToInt32(drv["tonkho"]);

            // Kiểm tra số lượng đã chọn trong DataGridView
            int slHienTaiTrongGrid = 0;
            DataGridViewRow rowSua = null;

            foreach (DataGridViewRow row in dgvChiTietDonHang.Rows)
            {
                if (row.Cells["colIDSP"].Value?.ToString() == idSP)
                {
                    slHienTaiTrongGrid = Convert.ToInt32(row.Cells["colSoLuong"].Value);
                    rowSua = row;
                    break;
                }
            }

            int tongSoLuongMua = slHienTaiTrongGrid + soLuongThem;

            // KIỂM TRA TỒN KHO TỔNG CỘNG
            if (tongSoLuongMua > tonKho)
            {
                MessageBox.Show($"Sản phẩm [{tenSP}] trong kho chỉ còn lại {tonKho} sản phẩm!\n(Bạn đã chọn trong đơn: {slHienTaiTrongGrid}, thêm mới: {soLuongThem})",
                                "Cảnh báo tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cập nhật hoặc thêm dòng mới
            if (rowSua != null)
            {
                rowSua.Cells["colSoLuong"].Value = tongSoLuongMua;
                rowSua.Cells["colThanhTien"].Value = string.Format("{0:N0} đ", tongSoLuongMua * donGia);
            }
            else
            {
                decimal thanhTien = soLuongThem * donGia;
                dgvChiTietDonHang.Rows.Add(idSP, tenSP, soLuongThem, string.Format("{0:N0} đ", donGia), string.Format("{0:N0} đ", thanhTien));
            }

            TinhTongTien();
        }

        private void dgvChiTietDonHang_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvChiTietDonHang.Columns[e.ColumnIndex].Name == "colXoa")
            {
                dgvChiTietDonHang.Rows.RemoveAt(e.RowIndex);
                TinhTongTien();
            }
        }

        // ==========================================
        // SỬA SỐ LƯỢNG TRỰC TIẾP TRÊN BẢNG
        // ==========================================
        private void dgvChiTietDonHang_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvChiTietDonHang.Columns[e.ColumnIndex].Name == "colSoLuong")
            {
                DataGridViewRow row = dgvChiTietDonHang.Rows[e.RowIndex];
                string slInput = row.Cells["colSoLuong"].Value?.ToString();
                string idSP = row.Cells["colIDSP"].Value?.ToString();

                if (!int.TryParse(slInput, out int sl) || sl <= 0)
                {
                    MessageBox.Show("Số lượng nhập phải là số nguyên dương hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    row.Cells["colSoLuong"].Value = 1;
                    sl = 1;
                }

                // Kiểm tra lại với tồn kho khi người dùng gõ phím sửa trực tiếp
                if (cboSanPham.DataSource is DataTable dt)
                {
                    DataRow[] foundRows = dt.Select($"id_sp = '{idSP}'");
                    if (foundRows.Length > 0)
                    {
                        int tonKho = Convert.ToInt32(foundRows[0]["tonkho"]);
                        if (sl > tonKho)
                        {
                            MessageBox.Show($"Số lượng vượt quá tồn kho (Kho còn {tonKho} SP)!", "Cảnh báo tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            row.Cells["colSoLuong"].Value = tonKho;
                            sl = tonKho;
                        }
                    }
                }

                string giaStr = row.Cells["colDonGia"].Value?.ToString().Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                if (decimal.TryParse(giaStr, out decimal donGia))
                {
                    row.Cells["colThanhTien"].Value = string.Format("{0:N0} đ", sl * donGia);
                    TinhTongTien();
                }
            }
        }

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

        // ==========================================
        // LƯU ĐƠN HÀNG VÀ TRỪ TỒN KHO THỰC TẾ
        // ==========================================
        private void btnLuuDonHang_Click(object sender, EventArgs e)
        {
            string idDH = txtMaDH.Text.Trim();

            // 1. Kiểm tra Mã đơn hàng
            if (string.IsNullOrEmpty(idDH))
            {
                MessageBox.Show("Mã đơn hàng không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMaDH.Focus();
                return;
            }

            // 2. Kiểm tra chọn Khách hàng
            if (cboKhachHang.SelectedIndex == -1 || cboKhachHang.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Khách hàng mua hàng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboKhachHang.Focus();
                return;
            }

            // 3. Kiểm tra chọn Nhân viên
            if (cboNhanVien.SelectedIndex == -1 || cboNhanVien.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn Nhân viên phụ trách đơn hàng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboNhanVien.Focus();
                return;
            }

            // 4. Kiểm tra giỏ hàng
            if (dgvChiTietDonHang.Rows.Count == 0 || (dgvChiTietDonHang.Rows.Count == 1 && dgvChiTietDonHang.Rows[0].IsNewRow))
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 sản phẩm vào đơn hàng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboSanPham.Focus();
                return;
            }

            string idKH = cboKhachHang.SelectedValue.ToString();
            string idNV = cboNhanVien.SelectedValue.ToString();

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 5. Kiểm tra trùng mã đơn hàng
                    string checkExistQuery = "SELECT COUNT(*) FROM donhang WHERE id_dh = @ID_DH";
                    using (NpgsqlCommand cmdCheck = new NpgsqlCommand(checkExistQuery, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@ID_DH", idDH);
                        if (Convert.ToInt64(cmdCheck.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show($"Mã đơn hàng [{idDH}] đã tồn tại! Hệ thống sẽ tạo mã mới.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            TaoMaDonHangTuDong();
                            idDH = txtMaDH.Text.Trim();
                        }
                    }

                    // 6. THỰC HIỆN TRANSACTION LƯU ĐƠN & TRỪ TỒN KHO
                    using (NpgsqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 6.1. Lưu đơn hàng
                            string queryDH = @"INSERT INTO donhang (id_dh, id_kh, id_nv, ngaytao, trangthai) 
                                               VALUES (@ID_DH, @ID_KH, @ID_NV, CURRENT_TIMESTAMP, N'Chờ xử lý')";

                            using (NpgsqlCommand cmdDH = new NpgsqlCommand(queryDH, conn, transaction))
                            {
                                cmdDH.Parameters.AddWithValue("@ID_DH", idDH);
                                cmdDH.Parameters.AddWithValue("@ID_KH", idKH);
                                cmdDH.Parameters.AddWithValue("@ID_NV", idNV);
                                cmdDH.ExecuteNonQuery();
                            }

                            // 6.2. Lưu chi tiết đơn hàng & Cập nhật trừ tồn kho
                            int index = 1;
                            foreach (DataGridViewRow row in dgvChiTietDonHang.Rows)
                            {
                                if (row.IsNewRow) continue;

                                string idCTDH = "CT" + idDH + index.ToString("D2");
                                string idSP = row.Cells["colIDSP"].Value.ToString();
                                int soLuong = Convert.ToInt32(row.Cells["colSoLuong"].Value);

                                string donGiaStr = row.Cells["colDonGia"].Value.ToString().Replace("đ", "").Replace(".", "").Replace(",", "").Trim();
                                decimal donGia = Convert.ToDecimal(donGiaStr);

                                // A. Chèn chi tiết đơn hàng
                                string queryCT = @"INSERT INTO chitietdonhang (id_ctdh, id_dh, id_sp, soluong, dongia) 
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

                                // B. Trừ tồn kho trong bảng hanghoa
                                string queryUpdateKho = @"
                                    UPDATE hanghoa 
                                    SET tonkho = tonkho - @SoLuong 
                                    WHERE mahang = (SELECT mahang FROM sanpham WHERE id_sp = @ID_SP)
                                      AND tonkho >= @SoLuong";

                                using (NpgsqlCommand cmdKho = new NpgsqlCommand(queryUpdateKho, conn, transaction))
                                {
                                    cmdKho.Parameters.AddWithValue("@SoLuong", soLuong);
                                    cmdKho.Parameters.AddWithValue("@ID_SP", idSP);

                                    int rowsUpdated = cmdKho.ExecuteNonQuery();

                                    if (rowsUpdated == 0)
                                    {
                                        throw new Exception($"Sản phẩm mã [{idSP}] không đủ số lượng tồn kho để hoàn tất giao dịch!");
                                    }
                                }

                                index++;
                            }

                            transaction.Commit();
                            MessageBox.Show($"Tạo đơn hàng {idDH} thành công và đã cập nhật trừ tồn kho!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        catch (Exception exTx)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Lỗi khi lưu đơn hàng và trừ kho: " + exTx.Message, "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}