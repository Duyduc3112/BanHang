using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrTaoPhieuKiemKe : Form
    {
        public FrTaoPhieuKiemKe()
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

        private void FrTaoPhieuKiemKe_Load(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền tạo phiếu kiểm kê!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            txtMaPhieu.Text = "PKK-" + DateTime.Now.ToString("yyyyMMdd-HHmm");
            LoadDanhSachKho();
        }

        private void LoadDanhSachKho()
        {
            try
            {
                string sql = "SELECT maKho, tenKho FROM Kho";
                DataTable dt = DatabaseHelper.ExecuteQuery(sql);

                cboKho.DataSource = dt;
                cboKho.DisplayMember = "tenKho";
                cboKho.ValueMember = "maKho";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách kho: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTaoPhieu_Click(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền tạo phiếu kiểm kê!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maPhieu = txtMaPhieu.Text.Trim();
            string maKho = cboKho.SelectedValue?.ToString();

            if (string.IsNullOrEmpty(maPhieu) || string.IsNullOrEmpty(maKho))
            {
                MessageBox.Show("Vui lòng chọn nhà kho hợp lệ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string sqlInsertPhieu = @"
                    INSERT INTO PhieuKiemKe (maPhieuKK, maKho, ngayKiemKe, nguoiLap, trangThai)
                    VALUES (@MaPhieu, @MaKho, GETDATE(), N'ND_ADMIN', N'Chờ duyệt')";

                SqlParameter[] p1 = new SqlParameter[]
                {
                    new SqlParameter("@MaPhieu", maPhieu),
                    new SqlParameter("@MaKho", maKho)
                };

                DatabaseHelper.ExecuteNonQuery(sqlInsertPhieu, p1);

                string sqlInsertChiTiet = @"
                    INSERT INTO ChiTietKiemKe (maCTKK, maPhieuKK, maTon, soLuongHeThong, soLuongThucTe, chenhLech)
                    SELECT 
                        'CTKK-' + LEFT(REPLACE(NEWID(), '-', ''), 10),
                        @MaPhieu,
                        tk.maTon,
                        ISNULL(tk.soLuongTon, 0),
                        ISNULL(tk.soLuongTon, 0),
                        0
                    FROM TonKho tk
                    INNER JOIN ViTriLuuTru vt ON tk.maViTri = vt.maViTri
                    WHERE vt.maKho = @MaKho";

                SqlParameter[] p2 = new SqlParameter[]
                {
                    new SqlParameter("@MaPhieu", maPhieu),
                    new SqlParameter("@MaKho", maKho)
                };

                DatabaseHelper.ExecuteNonQuery(sqlInsertChiTiet, p2);

                MessageBox.Show($"Đã tạo thành công phiếu kiểm kê [{maPhieu}]!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo phiếu kiểm kê: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboKhoKiemKe_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}