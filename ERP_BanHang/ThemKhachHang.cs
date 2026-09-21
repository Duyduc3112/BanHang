using System;
using System.Configuration;
using Npgsql; // Thay thế cho System.Data.SqlClient
using System.Drawing;
using System.Windows.Forms;

namespace ERP_BanHang
{
    public partial class ThemKhachHang : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;

        public ThemKhachHang()
        {
            InitializeComponent();
        }

        private void btnNextToTab2_Click(object sender, EventArgs e)
        {
            // Chuyển sang Tab 2 (Địa chỉ & Liên hệ)
            tabControlMain.SelectedIndex = 1;
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string idKH = LayGiaTri(txtID_KH, "VD: KH001");
            string tenDN = LayGiaTri(txtTenDoanhNghiep, "VD: Công ty TNHH Phân Phối Thành Đạt");
            string nguoiDD = LayGiaTri(txtNguoiDaiDien, "VD: Nguyễn Văn A");
            string mst = LayGiaTri(txtMaSoThue, "VD: 0314567890");
            string sdt = LayGiaTri(txtSDT, "VD: 0987654321");
            string email = LayGiaTri(txtEmail, "VD: contact@thanhdat.com");
            string diaChi = LayGiaTri(txtDiaChi, "VD: Số 10, Đường Cầu Giấy, Hà Nội");

            // Ràng buộc trường bắt buộc
            if (string.IsNullOrEmpty(idKH))
            {
                MessageBox.Show("Vui lòng nhập Mã khách hàng!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 0;
                txtID_KH.Focus();
                return;
            }

            if (string.IsNullOrEmpty(tenDN))
            {
                MessageBox.Show("Vui lòng nhập Tên doanh nghiệp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 0;
                txtTenDoanhNghiep.Focus();
                return;
            }

            string query = @"
                INSERT INTO KhachHang (ID_KH, NguoiDaiDien, TenDoanhNghiep, MaSoThue, DiaChi, SDT, Email)
                VALUES (@ID_KH, @NguoiDaiDien, @TenDoanhNghiep, @MaSoThue, @DiaChi, @SDT, @Email)";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_KH", idKH);
                        cmd.Parameters.AddWithValue("@NguoiDaiDien", string.IsNullOrEmpty(nguoiDD) ? (object)DBNull.Value : nguoiDD);
                        cmd.Parameters.AddWithValue("@TenDoanhNghiep", tenDN);
                        cmd.Parameters.AddWithValue("@MaSoThue", string.IsNullOrEmpty(mst) ? (object)DBNull.Value : mst);
                        cmd.Parameters.AddWithValue("@DiaChi", string.IsNullOrEmpty(diaChi) ? (object)DBNull.Value : diaChi);
                        cmd.Parameters.AddWithValue("@SDT", string.IsNullOrEmpty(sdt) ? (object)DBNull.Value : sdt);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Thêm khách hàng mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm khách hàng: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string LayGiaTri(TextBox txt, string placeholder)
        {
            string val = txt.Text.Trim();
            return (val == placeholder) ? "" : val;
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ==========================================
        // XỬ LÝ PLACEHOLDER (HƯỚNG DẪN NHẬP)
        // ==========================================
        private void RemovePlaceholder(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && txt.ForeColor == Color.Gray)
            {
                txt.Text = "";
                txt.ForeColor = Color.Black;
            }
        }

        private void SetPlaceholder(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt != null && string.IsNullOrWhiteSpace(txt.Text))
            {
                if (txt == txtID_KH) txt.Text = "VD: KH001";
                else if (txt == txtTenDoanhNghiep) txt.Text = "VD: Công ty TNHH Phân Phối Thành Đạt";
                else if (txt == txtNguoiDaiDien) txt.Text = "VD: Nguyễn Văn A";
                else if (txt == txtMaSoThue) txt.Text = "VD: 0314567890";
                else if (txt == txtSDT) txt.Text = "VD: 0987654321";
                else if (txt == txtEmail) txt.Text = "VD: contact@thanhdat.com";
                else if (txt == txtDiaChi) txt.Text = "VD: Số 10, Đường Cầu Giấy, Hà Nội";

                txt.ForeColor = Color.Gray;
            }
        }
    }
}