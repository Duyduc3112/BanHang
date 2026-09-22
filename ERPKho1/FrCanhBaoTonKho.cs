using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrCanhBaoTonKho : Form
    {
        public FrCanhBaoTonKho()
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

        private void FrCanhBaoTonKho_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền truy cập cảnh báo tồn kho!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            LoadDuLieuCanhBao();
        }

        private void LoadDuLieuCanhBao()
        {
            try
            {
                string sql = @"
                        SELECT 
                            hh.MaHang AS MaVatTu,
                            hh.TenHang AS TenVatTu,
                            k.tenKho AS TenKho,
                            vt.maViTri AS ViTri,
                            ISNULL(tk.soLuongTon, 0) AS SoLuongTon,
                            ISNULL(hh.DonViTinh, N'Kg') AS DonViTinh,
                            CASE 
                                WHEN hh.MaHang LIKE 'HH%' THEN 500
                                WHEN hh.MaHang LIKE 'NL%' THEN 100
                                ELSE 200
                            END AS NguongToiThieu,
                            N'Cảnh báo thiếu' AS TrangThaiTon
                        FROM HangHoa hh
                        INNER JOIN LoHang lh ON hh.MaHang = lh.MaHang
                        INNER JOIN TonKho tk ON lh.maLo = tk.maLo
                        INNER JOIN ViTriLuuTru vt ON tk.maViTri = vt.maViTri
                        INNER JOIN Kho k ON vt.maKho = k.maKho
                        WHERE ISNULL(tk.soLuongTon, 0) < (
                            CASE 
                                WHEN hh.MaHang LIKE 'HH%' THEN 500
                                WHEN hh.MaHang LIKE 'NL%' THEN 100
                                ELSE 200
                            END
                        )";

                DataTable dt = DatabaseHelper.ExecuteQuery(sql);
                dgvCanhBao.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách cảnh báo: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGuiCanhBao_Click(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show($"Tài khoản vai trò ({UserSession.ChucVu}) không có quyền gửi cảnh báo tồn kho!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Đã gửi thông báo cảnh báo tồn kho thấp đến bộ phận mua hàng!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}