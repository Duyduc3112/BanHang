using System;
using System.Configuration;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace ERP_BanHang
{
    public partial class SuaKhachHang : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;
        private string maKH;

        public SuaKhachHang(string idKhachHang)
        {
            InitializeComponent();
            this.maKH = idKhachHang;
        }

        private void SuaKhachHang_Load(object sender, EventArgs e)
        {
            txtMaKH.Text = maKH;
            LoadThongTinKhachHang();
        }

        private void LoadThongTinKhachHang()
        {
            string query = @"
                SELECT 
                    TenDoanhNghiep, 
                    NguoiDaiDien, 
                    SDT, 
                    DiaChi, 
                    MaSoThue 
                FROM KhachHang 
                WHERE ID_KH = @ID_KH";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_KH", maKH);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtTenDN.Text = reader["TenDoanhNghiep"] != DBNull.Value ? reader["TenDoanhNghiep"].ToString() : "";
                                txtNguoiDaiDien.Text = reader["NguoiDaiDien"] != DBNull.Value ? reader["NguoiDaiDien"].ToString() : "";
                                txtSDT.Text = reader["SDT"] != DBNull.Value ? reader["SDT"].ToString() : "";
                                txtDiaChi.Text = reader["DiaChi"] != DBNull.Value ? reader["DiaChi"].ToString() : "";
                                txtMST.Text = reader["MaSoThue"] != DBNull.Value ? reader["MaSoThue"].ToString() : "";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải thông tin khách hàng từ CSDL: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            // Kiểm tra ràng buộc dữ liệu bắt buộc
            if (string.IsNullOrWhiteSpace(txtTenDN.Text))
            {
                MessageBox.Show("Vui lòng nhập Tên doanh nghiệp / Đại lý!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTenDN.Focus();
                return;
            }

            string updateQuery = @"
                UPDATE KhachHang 
                SET TenDoanhNghiep = @TenDN, 
                    NguoiDaiDien = @NguoiDaiDien, 
                    SDT = @SDT, 
                    DiaChi = @DiaChi, 
                    MaSoThue = @MST 
                WHERE ID_KH = @ID_KH";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TenDN", txtTenDN.Text.Trim());
                        cmd.Parameters.AddWithValue("@NguoiDaiDien", string.IsNullOrWhiteSpace(txtNguoiDaiDien.Text) ? (object)DBNull.Value : txtNguoiDaiDien.Text.Trim());
                        cmd.Parameters.AddWithValue("@SDT", string.IsNullOrWhiteSpace(txtSDT.Text) ? (object)DBNull.Value : txtSDT.Text.Trim());
                        cmd.Parameters.AddWithValue("@DiaChi", string.IsNullOrWhiteSpace(txtDiaChi.Text) ? (object)DBNull.Value : txtDiaChi.Text.Trim());
                        cmd.Parameters.AddWithValue("@MST", string.IsNullOrWhiteSpace(txtMST.Text) ? (object)DBNull.Value : txtMST.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID_KH", maKH);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Cập nhật thành công thông tin khách hàng [{maKH}]!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu dữ liệu: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}