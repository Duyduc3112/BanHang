using System;
using System.Data;
using Npgsql; // Đã đổi từ System.Data.SqlClient sang Npgsql
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;

namespace ERP_BanHang
{
    public partial class BaoCaoThongKe : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;

        public BaoCaoThongKe()
        {
            InitializeComponent();
        }

        private void BaoCaoThongKe_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;

            // Đặt thời gian mặc định: Đầu tháng hiện tại -> Hôm nay
            DateTime now = DateTime.Now;
            dtpFromDate.Value = new DateTime(now.Year, now.Month, 1);
            dtpToDate.Value = now;

            if (cboReportType.Items.Count > 0)
            {
                cboReportType.SelectedIndex = 0;
            }

            TaiTongQuanKPI();
            TaiDuLieuBaoCao();
        }

        // ==========================================
        // 1. TÍNH TOÁN CÁC THẺ KPI TỔNG QUAN
        // ==========================================
        private void TaiTongQuanKPI()
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // KPI 1: Tổng doanh thu từ bảng HoaDon (Dùng COALESCE thay ISNULL)
                    string queryDoanhThu = "SELECT COALESCE(SUM(TongTien), 0) FROM HoaDon WHERE TrangThai = N'Đã thanh toán'";
                    using (NpgsqlCommand cmd1 = new NpgsqlCommand(queryDoanhThu, conn))
                    {
                        decimal tongDoanhThu = Convert.ToDecimal(cmd1.ExecuteScalar());
                        lblKPI1Value.Text = string.Format("{0:#,##0} VNĐ", tongDoanhThu);
                    }

                    // KPI 2: Số đơn hàng đã giao thành công từ bảng DonHang & GiaoHang
                    string queryDonHang = "SELECT COUNT(DISTINCT ID_DH) FROM GiaoHang WHERE TrangThaiGiaoHang = N'Đã giao' OR TrangThaiGiaoHang = N'Hoàn thành'";
                    using (NpgsqlCommand cmd2 = new NpgsqlCommand(queryDonHang, conn))
                    {
                        int donHoanThanh = Convert.ToInt32(cmd2.ExecuteScalar());
                        lblKPI2Value.Text = donHoanThanh.ToString("#,##0") + " đơn";
                    }

                    // KPI 3: Số yêu cầu sau bán hàng (bảo hành/đổi trả) từ bảng YeuCauSauBanHang
                    string queryYeuCau = "SELECT COUNT(*) FROM YeuCauSauBanHang";
                    using (NpgsqlCommand cmd3 = new NpgsqlCommand(queryYeuCau, conn))
                    {
                        int soYeuCau = Convert.ToInt32(cmd3.ExecuteScalar());
                        lblKPI3Value.Text = soYeuCau.ToString("#,##0") + " yêu cầu";
                    }
                }
                catch (Exception)
                {
                    lblKPI1Value.Text = "0 VNĐ";
                    lblKPI2Value.Text = "0 đơn";
                    lblKPI3Value.Text = "0 yêu cầu";
                }
            }
        }

        // ==========================================
        // 2. LẤY DỮ LIỆU BÁO CÁO THEO BỘ LỌC
        // ==========================================
        private void TaiDuLieuBaoCao()
        {
            DateTime tuNgay = dtpFromDate.Value.Date;
            DateTime denNgay = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

            dgvData.Columns.Clear();
            dgvData.AutoGenerateColumns = false;

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    DataTable dt = new DataTable();

                    if (cboReportType.SelectedIndex == 0)
                    {
                        // ----- BÁO CÁO 1: DOANH THU ĐƠN HÀNG -----
                        TaoCotBaoCaoDonHang();

                        string query = @"
                            SELECT 
                                DH.ID_DH,
                                DH.NgayTao,
                                KH.TenDoanhNghiep AS TenKhachHang,
                                NV.TenNV AS NguoiLap,
                                COALESCE(HD.TongTien, 0) AS TongTien,
                                DH.TrangThai
                            FROM DonHang DH
                            LEFT JOIN KhachHang KH ON DH.ID_KH = KH.ID_KH
                            LEFT JOIN NhanVien NV ON DH.ID_NV = NV.ID_NV
                            LEFT JOIN HoaDon HD ON DH.ID_DH = HD.ID_DH
                            WHERE DH.NgayTao >= @TuNgay AND DH.NgayTao <= @DenNgay
                            ORDER BY DH.NgayTao DESC";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                            cmd.Parameters.AddWithValue("@DenNgay", denNgay);
                            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                            da.Fill(dt);
                        }
                    }
                    else
                    {
                        // ----- BÁO CÁO 2: YÊU CẦU SAU BÁN HÀNG & LỖI SẢN PHẨM -----
                        TaoCotBaoCaoYeuCau();

                        string query = @"
                            SELECT 
                                YC.ID_YC,
                                YC.ID_DH,
                                KH.TenDoanhNghiep AS TenKhachHang,
                                H.TenHang AS TenSanPham,
                                CTYC.SoLuong,
                                CTYC.TinhTrang,
                                YC.LoaiYeuCau,
                                YC.NgayYeuCau,
                                YC.TrangThai
                            FROM YeuCauSauBanHang YC
                            LEFT JOIN ChiTietYeuCau CTYC ON YC.ID_YC = CTYC.ID_YC
                            LEFT JOIN SanPham SP ON CTYC.ID_SP = SP.ID_SP
                            LEFT JOIN HangHoa H ON SP.MaHang = H.MaHang
                            LEFT JOIN KhachHang KH ON YC.ID_KH = KH.ID_KH
                            WHERE YC.NgayYeuCau >= @TuNgay AND YC.NgayYeuCau <= @DenNgay
                            ORDER BY YC.NgayYeuCau DESC";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@TuNgay", tuNgay);
                            cmd.Parameters.AddWithValue("@DenNgay", denNgay);
                            NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                            da.Fill(dt);
                        }
                    }

                    dgvData.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải dữ liệu báo cáo: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ==========================================
        // 3. TẠO CỘT CHO BẢNG DATAGRIDVIEW
        // ==========================================
        private void TaoCotBaoCaoDonHang()
        {
            DataGridViewTextBoxColumn colMaDH = new DataGridViewTextBoxColumn
            {
                Name = "colID_DH",
                HeaderText = "MÃ ĐƠN HÀNG",
                DataPropertyName = "ID_DH",
                FillWeight = 15,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(13, 110, 253) }
            };
            colMaDH.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colMaDH);

            DataGridViewTextBoxColumn colNgay = new DataGridViewTextBoxColumn
            {
                Name = "colNgayTao",
                HeaderText = "NGÀY TẠO ĐƠN",
                DataPropertyName = "NgayTao",
                FillWeight = 18,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "dd/MM/yyyy HH:mm" }
            };
            colNgay.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colNgay);

            DataGridViewTextBoxColumn colKhach = new DataGridViewTextBoxColumn
            {
                Name = "colTenKhachHang",
                HeaderText = "TÊN DOANH NGHIỆP / KHÁCH HÀNG",
                DataPropertyName = "TenKhachHang",
                FillWeight = 30
            };
            dgvData.Columns.Add(colKhach);

            DataGridViewTextBoxColumn colNV = new DataGridViewTextBoxColumn
            {
                Name = "colNguoiLap",
                HeaderText = "NHÂN VIÊN LẬP",
                DataPropertyName = "NguoiLap",
                FillWeight = 20
            };
            dgvData.Columns.Add(colNV);

            DataGridViewTextBoxColumn colTongTien = new DataGridViewTextBoxColumn
            {
                Name = "colTongTien",
                HeaderText = "TỔNG TIỀN (VNĐ)",
                DataPropertyName = "TongTien",
                FillWeight = 20,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "#,##0", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }
            };
            colTongTien.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvData.Columns.Add(colTongTien);

            DataGridViewTextBoxColumn colTrangThai = new DataGridViewTextBoxColumn
            {
                Name = "colTrangThai",
                HeaderText = "TRẠNG THÁI",
                DataPropertyName = "TrangThai",
                FillWeight = 17,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colTrangThai.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colTrangThai);
        }

        private void TaoCotBaoCaoYeuCau()
        {
            DataGridViewTextBoxColumn colMaYC = new DataGridViewTextBoxColumn
            {
                Name = "colID_YC",
                HeaderText = "MÃ YÊU CẦU",
                DataPropertyName = "ID_YC",
                FillWeight = 12,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.Crimson }
            };
            colMaYC.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colMaYC);

            DataGridViewTextBoxColumn colMaDH = new DataGridViewTextBoxColumn
            {
                Name = "colID_DH",
                HeaderText = "MÃ ĐƠN HÀNG",
                DataPropertyName = "ID_DH",
                FillWeight = 12,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colMaDH.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colMaDH);

            DataGridViewTextBoxColumn colKhach = new DataGridViewTextBoxColumn
            {
                Name = "colTenKhachHang",
                HeaderText = "KHÁCH HÀNG",
                DataPropertyName = "TenKhachHang",
                FillWeight = 22
            };
            dgvData.Columns.Add(colKhach);

            DataGridViewTextBoxColumn colTenSP = new DataGridViewTextBoxColumn
            {
                Name = "colTenSanPham",
                HeaderText = "SẢN PHẨM PHẢN HỒI",
                DataPropertyName = "TenSanPham",
                FillWeight = 22
            };
            dgvData.Columns.Add(colTenSP);

            DataGridViewTextBoxColumn colSL = new DataGridViewTextBoxColumn
            {
                Name = "colSoLuong",
                HeaderText = "SL",
                DataPropertyName = "SoLuong",
                FillWeight = 8,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colSL.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colSL);

            DataGridViewTextBoxColumn colTinhTrang = new DataGridViewTextBoxColumn
            {
                Name = "colTinhTrang",
                HeaderText = "TÌNH TRẠNG / LÝ DO LỖI",
                DataPropertyName = "TinhTrang",
                FillWeight = 24
            };
            dgvData.Columns.Add(colTinhTrang);

            DataGridViewTextBoxColumn colTrangThai = new DataGridViewTextBoxColumn
            {
                Name = "colTrangThai",
                HeaderText = "TRẠNG THÁI",
                DataPropertyName = "TrangThai",
                FillWeight = 15,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            colTrangThai.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvData.Columns.Add(colTrangThai);
        }

        // ==========================================
        // 4. SỰ KIỆN LỌC VÀ ĐIỀU HƯỚNG
        // ==========================================
        private void cboReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            TaiDuLieuBaoCao();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            if (dtpFromDate.Value > dtpToDate.Value)
            {
                MessageBox.Show("Từ ngày không được lớn hơn Đến ngày!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            TaiDuLieuBaoCao();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Đang tiến hành xuất báo cáo ra file Excel...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

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
    }
}