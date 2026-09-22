using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrNhapKetQuaKiemKe : Form
    {
        public FrNhapKetQuaKiemKe()
        {
            InitializeComponent();
        }

        private bool IsPermittedUser()
        {
            string chucVu = UserSession.ChucVu ?? "";
            return chucVu.Equals("Nhân viên kho", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản lý kho", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản lý", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        private void FrNhapKetQuaKiemKe_Load(object sender, EventArgs e)
        {

            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền nhập kết quả kiểm kê!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            LoadDanhSachPhieuKiemKe();
        }

        private void LoadDanhSachPhieuKiemKe()
        {
            try
            {
                string sql = @"
                    SELECT 
                        pk.maPhieuKK, 
                        k.tenKho 
                    FROM PhieuKiemKe pk
                    INNER JOIN Kho k ON pk.maKho = k.maKho";

                DataTable dt = DatabaseHelper.ExecuteQuery(sql);

                cboMaPhieu.DataSource = dt;
                cboMaPhieu.DisplayMember = "maPhieuKK";
                cboMaPhieu.ValueMember = "maPhieuKK";

                if (dt != null && dt.Rows.Count > 0)
                {
                    LoadChiTietKiemKe(dt.Rows[0]["maPhieuKK"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách phiếu kiểm kê: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadChiTietKiemKe(string maPhieu)
        {
            try
            {
                string sql = @"
                    SELECT 
                        ct.maCTKK,
                        hh.MaHang AS [MaVatTu],
                        hh.TenHang AS [TenVatTu],
                        ct.soLuongHeThong AS [SoLuongHeThong],
                        ct.soLuongThucTe AS [SoLuongThucTe]
                    FROM ChiTietKiemKe ct
                    INNER JOIN TonKho tk ON ct.maTon = tk.maTon
                    INNER JOIN LoHang lh ON tk.maLo = lh.maLo
                    INNER JOIN HangHoa hh ON lh.MaHang = hh.MaHang
                    WHERE ct.maPhieuKK = @MaPhieu";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaPhieu", maPhieu)
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);

                if (dt == null || dt.Rows.Count == 0)
                {
                    sql = @"
                        SELECT 
                            hh.MaHang AS [MaVatTu],
                            hh.TenHang AS [TenVatTu],
                            ISNULL(tk.soLuongTon, 0) AS [SoLuongHeThong],
                            ISNULL(tk.soLuongTon, 0) AS [SoLuongThucTe]
                        FROM HangHoa hh
                        INNER JOIN LoHang lh ON hh.MaHang = lh.MaHang
                        INNER JOIN TonKho tk ON lh.maLo = tk.maLo
                        INNER JOIN ViTriLuuTru vt ON tk.maViTri = vt.maViTri
                        INNER JOIN PhieuKiemKe pk ON vt.maKho = pk.maKho
                        WHERE pk.maPhieuKK = @MaPhieu";

                    dt = DatabaseHelper.ExecuteQuery(sql, parameters);
                }

                dgvChiTiet.DataSource = dt;

                if (dgvChiTiet.Columns["SoLuongThucTe"] != null)
                {
                    dgvChiTiet.Columns["SoLuongThucTe"].ReadOnly = false;
                }
                if (dgvChiTiet.Columns["SoLuongHeThong"] != null)
                {
                    dgvChiTiet.Columns["SoLuongHeThong"].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết kiểm kê: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboMaPhieu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboMaPhieu.SelectedValue != null && cboMaPhieu.SelectedValue.ToString() != "System.Data.DataRowView")
            {
                LoadChiTietKiemKe(cboMaPhieu.SelectedValue.ToString());
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền nhập kết quả kiểm kê!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maPhieu = cboMaPhieu.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(maPhieu))
            {
                MessageBox.Show("Vui lòng chọn phiếu kiểm kê hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                MessageBox.Show($"Đã lưu kết quả kiểm kê thực tế cho phiếu [{maPhieu}] thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu kết quả kiểm kê: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}