using System;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Npgsql;

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

            // ==========================================
            // BẮT ĐIỀU KIỆN RÀNG BUỘC DỮ LIỆU
            // ==========================================

            // 1. Kiểm tra trường bắt buộc
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
            if(string.IsNullOrEmpty(sdt))
            {
                MessageBox.Show("Vui lòng nhập Số điện thoại!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 1;
                txtSDT.Focus();
                return;
            }
            if(string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập Email!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 1;
                txtEmail.Focus();
                return;
            }
            if (string.IsNullOrEmpty(diaChi))
            {
                MessageBox.Show("Vui lòng nhập Địa chỉ!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 1;
                txtDiaChi.Focus();
                return;
            }
            if (string.IsNullOrEmpty(mst))
            {
                MessageBox.Show("Vui lòng nhập Mã số thuế!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabControlMain.SelectedIndex = 0;
                txtMaSoThue.Focus();
                return;
            }

            // 2. Kiểm tra định dạng Mã số thuế (Nếu có nhập)
            if (!string.IsNullOrEmpty(mst))
            {
                if (!Regex.IsMatch(mst, @"^[0-9]{10,13}$"))
                {
                    MessageBox.Show("Mã số thuế không hợp lệ! MST phải gồm từ 10 đến 13 chữ số.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControlMain.SelectedIndex = 0;
                    txtMaSoThue.Focus();
                    return;
                }
            }

            // 3. Kiểm tra định dạng Số điện thoại (Nếu có nhập)
            if (!string.IsNullOrEmpty(sdt))
            {
                if (!Regex.IsMatch(sdt, @"^0[0-9]{9}$"))
                {
                    MessageBox.Show("Số điện thoại không hợp lệ! SĐT phải bắt đầu bằng số 0 và đúng 10 chữ số.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControlMain.SelectedIndex = 1;
                    txtSDT.Focus();
                    return;
                }
            }

            // 4. Kiểm tra định dạng Email (Nếu có nhập)
            if (!string.IsNullOrEmpty(email))
            {
                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    MessageBox.Show("Địa chỉ Email không đúng định dạng (Ví dụ: contact@domain.com)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabControlMain.SelectedIndex = 1;
                    txtEmail.Focus();
                    return;
                }
            }

            // ==========================================
            // KIỂM TRA TRÙNG LẶP TRONG DATABASE NEON
            // ==========================================
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Kiểm tra Mã KH bị trùng
                    string checkIdQuery = "SELECT COUNT(*) FROM khachhang WHERE LOWER(id_kh) = LOWER(@ID_KH)";
                    using (NpgsqlCommand cmdCheck = new NpgsqlCommand(checkIdQuery, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@ID_KH", idKH);
                        if (Convert.ToInt64(cmdCheck.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show($"Mã khách hàng [{idKH}] đã tồn tại trong hệ thống!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            tabControlMain.SelectedIndex = 0;
                            txtID_KH.Focus();
                            return;
                        }
                    }

                    // Kiểm tra Mã số thuế bị trùng (Nếu có nhập)
                    if (!string.IsNullOrEmpty(mst))
                    {
                        string checkMstQuery = "SELECT COUNT(*) FROM khachhang WHERE masothue = @MaSoThue";
                        using (NpgsqlCommand cmdCheck = new NpgsqlCommand(checkMstQuery, conn))
                        {
                            cmdCheck.Parameters.AddWithValue("@MaSoThue", mst);
                            if (Convert.ToInt64(cmdCheck.ExecuteScalar()) > 0)
                            {
                                MessageBox.Show($"Mã số thuế [{mst}] đã được đăng ký cho một khách hàng khác!", "Trùng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                tabControlMain.SelectedIndex = 0;
                                txtMaSoThue.Focus();
                                return;
                            }
                        }
                    }

                    // ==========================================
                    // THỰC HIỆN THÊM MỚI KHI THỎA MÃN TẤT CẢ
                    // ==========================================
                    string insertQuery = @"
                        INSERT INTO khachhang (id_kh, nguoidaidien, tendoanhnghiep, masothue, diachi, sdt, email)
                        VALUES (@ID_KH, @NguoiDaiDien, @TenDoanhNghiep, @MaSoThue, @DiaChi, @SDT, @Email)";

                    using (NpgsqlCommand cmdInsert = new NpgsqlCommand(insertQuery, conn))
                    {
                        cmdInsert.Parameters.AddWithValue("@ID_KH", idKH);
                        cmdInsert.Parameters.AddWithValue("@NguoiDaiDien", string.IsNullOrEmpty(nguoiDD) ? (object)DBNull.Value : nguoiDD);
                        cmdInsert.Parameters.AddWithValue("@TenDoanhNghiep", tenDN);
                        cmdInsert.Parameters.AddWithValue("@MaSoThue", string.IsNullOrEmpty(mst) ? (object)DBNull.Value : mst);
                        cmdInsert.Parameters.AddWithValue("@DiaChi", string.IsNullOrEmpty(diaChi) ? (object)DBNull.Value : diaChi);
                        cmdInsert.Parameters.AddWithValue("@SDT", string.IsNullOrEmpty(sdt) ? (object)DBNull.Value : sdt);
                        cmdInsert.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);

                        cmdInsert.ExecuteNonQuery();
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