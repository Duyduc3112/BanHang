using ERP_BanHang; // Tham chiếu sang Project Bán Hàng
using ERP;
using ERPKho1;
using Npgsql;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace ERP_Khach 
{
    public partial class FormDangNhap : Form
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;

        // Biến lưu Phân hệ do Form LoginPhanHe truyền sang
        private string phanHeDaChon = "";

        // Constructor nhận tên Phân hệ
        public FormDangNhap(string phanHe)
        {
            InitializeComponent();
            this.phanHeDaChon = phanHe;
        }

        public FormDangNhap()
        {
            InitializeComponent();
        }

        private void FormDangNhap_Load(object sender, EventArgs e)
        {
            // Hiển thị tên Phân hệ lên tiêu đề hoặc Label trên Form nếu muốn
            this.Text = $"Đăng nhập - {phanHeDaChon}";
            txtTaiKhoan.Focus();
        }

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string tenDangNhap = txtTaiKhoan.Text.Trim();
            string matKhau = txtMatKhau.Text.Trim();

            if (string.IsNullOrEmpty(tenDangNhap))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return;
            }

            if (string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }

            KiemTraVaChuyenForm(tenDangNhap, matKhau);
        }

        private void KiemTraVaChuyenForm(string tenDangNhap, string matKhau)
        {
            string query = @"
                SELECT 
                    h.id_nv,
                    h.vaitro,
                    n.tennv,
                    n.chucvu,
                    n.phongban
                FROM hethong h
                INNER JOIN nhanvien n ON h.id_nv = n.id_nv
                WHERE LOWER(h.tendangnhap) = LOWER(@TenDangNhap) 
                  AND h.matkhau = @MatKhau";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand cmd = new NpgsqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TenDangNhap", tenDangNhap);
                    cmd.Parameters.AddWithValue("@MatKhau", matKhau);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string idNV = reader["id_nv"]?.ToString();
                            string tenNV = reader["tennv"]?.ToString();
                            string chucVu = reader["chucvu"]?.ToString();
                            string vaiTro = reader["vaitro"]?.ToString();

                            // XÁC THỰC THEO PHÂN HỆ ĐÃ CHỌN TỪ BÊN NGOÀI
                            if (phanHeDaChon == "Phân hệ Kho")
                            {
                                if (KiemTraQuyenKho(chucVu, vaiTro))
                                {
                                    MessageBox.Show($"Đăng nhập thành công!\nNghề nghiệp/Chức vụ: {chucVu}\nChào mừng vào Phân hệ Kho.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Hide();
                                    FrMain frmKho = new FrMain();
                                    frmKho.ShowDialog();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show($"Tài khoản của nhân viên [{tenNV}] (Chức vụ: {chucVu}) KHÔNG CÓ QUYỀN truy cập vào Phân hệ Kho!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                }
                            }
                            else if (phanHeDaChon == "Phân hệ Bán Hàng")
                            {
                                if (KiemTraQuyenBanHang(chucVu, vaiTro))
                                {
                                    MessageBox.Show($"Đăng nhập thành công!\nNghề nghiệp/Chức vụ: {chucVu}\nChào mừng vào Phân hệ Bán Hàng.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Hide();
                                    QlyDonHang frmBanHang = new QlyDonHang();
                                    frmBanHang.ShowDialog();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show($"Tài khoản của nhân viên [{tenNV}] (Chức vụ: {chucVu}) KHÔNG CÓ QUYỀN truy cập vào Phân hệ Bán Hàng!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                }
                            }
                            else if (phanHeDaChon == "Phân hệ Logistics")
                            {
                                if (KiemTraQuyenLogistics(chucVu, vaiTro))
                                {
                                    MessageBox.Show($"Đăng nhập thành công!\nNghề nghiệp/Chức vụ: {chucVu}\nChào mừng vào Phân hệ Logistics.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    this.Hide();
                                    QuanLyNhaCungCap logistics = new QuanLyNhaCungCap();
                                    logistics.ShowDialog();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show($"Tài khoản của nhân viên [{tenNV}] (Chức vụ: {chucVu}) KHÔNG CÓ QUYỀN truy cập vào Phân hệ Logistics!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không chính xác!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtMatKhau.Clear();
                            txtMatKhau.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool KiemTraQuyenBanHang(string chucVu, string vaiTro)
        {
            if (string.IsNullOrEmpty(chucVu)) chucVu = "";
            if (string.IsNullOrEmpty(vaiTro)) vaiTro = "";

            string cvLower = chucVu.Trim().ToLower();
            string vtLower = vaiTro.Trim().ToLower();

            if (cvLower.Contains("kho") || cvLower.Contains("kế toán") || cvLower.Contains("nhân sự") || cvLower.Contains("giao hàng"))
                return false;

            bool laNhanVienBanHang = cvLower.Equals("nhân viên bán hàng") || cvLower.Equals("nhân viên kinh doanh") || cvLower.Equals("quản lý bán hàng") || cvLower.Equals("trưởng phòng bán hàng");
            bool laAdmin = cvLower.Contains("admin") || vtLower.Contains("quản trị") || vtLower.Contains("admin");

            return laNhanVienBanHang || laAdmin;
        }
        private bool KiemTraQuyenLogistics(string chucVu, string vaiTro)
        {
            if (string.IsNullOrEmpty(chucVu)) chucVu = "";
            if (string.IsNullOrEmpty(vaiTro)) vaiTro = "";

            string cvLower = chucVu.Trim().ToLower();
            string vtLower = vaiTro.Trim().ToLower();

            if (cvLower.Contains("kho") || cvLower.Contains("kế toán") || cvLower.Contains("nhân sự") || cvLower.Contains("bán hàng"))
                return false;

            bool laNhanVienLogistics = cvLower.Equals("nhân viên logistics") || cvLower.Equals("quản lý logistics") || cvLower.Equals("trưởng phòng logistics");
            bool laAdmin = cvLower.Contains("admin") || vtLower.Contains("quản trị") || vtLower.Contains("admin");

            return laNhanVienLogistics || laAdmin;
        }

        private bool KiemTraQuyenKho(string chucVu, string vaiTro)
        {
            if (string.IsNullOrEmpty(chucVu)) chucVu = "";
            if (string.IsNullOrEmpty(vaiTro)) vaiTro = "";

            string cvLower = chucVu.Trim().ToLower();
            string vtLower = vaiTro.Trim().ToLower();

            if (cvLower.Contains("sản xuất") || cvLower.Contains("kế toán") || cvLower.Contains("nhân sự") || cvLower.Contains("giao hàng"))
                return false;

            bool laNhanVienKho = cvLower.Equals("nhân viên kho") || cvLower.Equals("quản lý kho") || cvLower.Equals("trưởng phòng kho");
            bool laAdmin = cvLower.Contains("admin") || vtLower.Contains("quản trị") || vtLower.Contains("admin");

            return laNhanVienKho || laAdmin;
        }

        private void chkHienThiMatKhau_CheckedChanged(object sender, EventArgs e)
        {
            txtMatKhau.UseSystemPasswordChar = !chkHienThiMatKhau.Checked;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}