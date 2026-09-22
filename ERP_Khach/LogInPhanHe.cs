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

        // ==========================================
        // SỰ KIỆN CLICK CÁC PANEL CHỌN PHÂN HỆ
        // ==========================================
        private void pnlSanXuat_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Sản xuất");
        }

        private void pnlBanHang_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Bán Hàng");
        }

        private void pnlLogistics_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Logistics");
        }

        private void pnlKho_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Kho");
        }

        private void pnlNhanSu_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Nhân sự");
        }

        private void pnlTaiChinh_Click(object sender, EventArgs e)
        {
            MoFormDangNhap("Phân hệ Tài chính");
        }

        // ==========================================
        // HÀM MỞ FORM ĐĂNG NHẬP VÀ TRUYỀN PHÂN HỆ ĐÃ CHỌN
        // ==========================================
        private void MoFormDangNhap(string tenPhanHe)
        {
 
            // Khởi tạo FormDangNhap và truyền Tên phân hệ sang Constructor
            using (FormDangNhap login = new FormDangNhap(tenPhanHe))
            {
                login.ShowDialog(); // Mở Form Đăng nhập dưới dạng Dialog
            }

            // Đóng hoàn toàn Form này sau khi làm việc xong
            this.Close();
        }
    }
}