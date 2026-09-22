using System;
using System.Windows.Forms;

namespace ERP_Khach
{
    public partial class LogInPhanHe : Form
    {
        public LogInPhanHe()
        {
            InitializeComponent();
        }

        private void LogInPhanHe_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            lblThoiGian.Text = DateTime.Now.ToString("dddd, dd 'tháng' MM 'năm' yyyy · HH:mm");
        }

        private void pnlSanXuat_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Mở phân hệ: Sản xuất", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void pnlBanHang_Click(object sender, EventArgs e)
        {
            MoFormDangNhap();
        }

        private void pnlLogistics_Click(object sender, EventArgs e)
        {
            MoFormDangNhap();
        }

        private void pnlKho_Click(object sender, EventArgs e)
        {
            MoFormDangNhap();
        }

        private void pnlNhanSu_Click(object sender, EventArgs e)
        {
            MoFormDangNhap();
        }

        private void pnlTaiChinh_Click(object sender, EventArgs e)
        {
            MoFormDangNhap();
        }

        // Hàm dùng chung để mở Form Đăng Nhập
        private void MoFormDangNhap()
        {
            FormDangNhap login = new FormDangNhap();
            login.ShowDialog(); // Mở Form Đăng Nhập
            this.Show(); 
            this.Close(); 
        }
    }
}