using System;
using System.Drawing;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrMain : Form
    {
        private Button activeNavButton;

        public FrMain()
        {
            InitializeComponent();
        }

        private void FrMain_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            CapNhatThongTinNguoiDung();
            KiemTraPhanQuyenMain();

            // Mở sẵn giao diện Quản lý nhập kho mặc định cho tất cả nhân viên khi đăng nhập
            OpenChildForm(new FrQLNhapKho(), btnQLNhapKho);
        }

        private void CapNhatThongTinNguoiDung()
        {
            lblUserName.Text = !string.IsNullOrEmpty(UserSession.TenNguoiDung) ? UserSession.TenNguoiDung : "Chưa đăng nhập";
            lblRole.Text = !string.IsNullOrEmpty(UserSession.ChucVu) ? UserSession.ChucVu : "N/A";
        }

        private void KiemTraPhanQuyenMain()
        {
            // Mở tất cả các nút trên Sidebar để mọi nhân viên đều truy cập và xem được giao diện
            btnQLNhapKho.Enabled = true;
            btnQLXuatKho.Enabled = true;
            btnQLLuutru.Enabled = true;
            btnQLTonKho.Enabled = true;
            btnQLLoHang.Enabled = true;
            btnQLTraHang.Enabled = true;
        }

        private void OpenChildForm(Form childForm, Button clickedButton)
        {
            if (activeNavButton != null)
            {
                activeNavButton.BackColor = System.Drawing.Color.Crimson;
                activeNavButton.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            }
            activeNavButton = clickedButton;
            activeNavButton.BackColor = System.Drawing.Color.FromArgb(13, 110, 253);
            activeNavButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            pnlMainContent.Controls.Clear();
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            pnlMainContent.Controls.Add(childForm);
            childForm.Show();
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            // Cho phép tất cả người dùng mở giao diện các form con từ Sidebar,
            // Các nút thao tác nghiệp vụ bên trong (Thêm/Sửa/Xóa/Duyệt) sẽ do từng Form con tự chặn theo vai trò.
            if (btn == btnQLNhapKho)
            {
                OpenChildForm(new FrQLNhapKho(), btn);
            }
            else if (btn == btnQLLuutru)
            {
                OpenChildForm(new FrQLLuutruvavitri(), btn);
            }
            else if (btn == btnQLTonKho)
            {
                OpenChildForm(new FrQLTonkho(), btn);
            }
            else if (btn == btnQLLoHang)
            {
                OpenChildForm(new FrQLLohangHSD(), btn);
            }
            else if (btn == btnQLXuatKho)
            {
                OpenChildForm(new FrQLXuatkhovaLayhang(), btn);
            }
            else if (btn == btnQLTraHang)
            {
                OpenChildForm(new FrQLTraHangVaTieuHuy(), btn);
            }
        }

        private void btnDangXuat_Click(object sender, EventArgs e)
        {
            {
                DialogResult confirm = MessageBox.Show(
                    "Bạn có chắc chắn muốn ĐĂNG XUẤT và quay lại màn hình đăng nhập?",
                    "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        // 1. Xóa file phiên làm việc tạm (nếu có)
                        string tempPath = System.IO.Path.Combine(Application.StartupPath, "session.txt");
                        if (System.IO.File.Exists(tempPath))
                        {
                            System.IO.File.Delete(tempPath);
                        }

                        // 2. Thuật toán tìm file ERP_Khach.exe linh hoạt trên mọi máy
                        string baseDir = Application.StartupPath;
                        string targetExe = "ERP_Khach.exe";
                        string pathExeDangNhap = "";

                        // Kiểm tra các vị trí file exe có thể nằm
                        string[] possiblePaths = new string[]
                        {
                // Khi chạy Release / Đóng gói chung thư mục
                System.IO.Path.Combine(baseDir, targetExe),
                System.IO.Path.Combine(baseDir, "..", targetExe),
                System.IO.Path.Combine(baseDir, "..", "ERP_Khach", targetExe),
                
                // Khi chạy Debug trong Visual Studio
                System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\..\ERP_Khach\bin\Debug\ERP_Khach.exe")),
                System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, @"..\..\..\..\ERP_Khach\bin\Release\ERP_Khach.exe"))
                        };

                        foreach (string p in possiblePaths)
                        {
                            if (System.IO.File.Exists(p))
                            {
                                pathExeDangNhap = p;
                                break;
                            }
                        }

                        // 3. Khởi chạy ứng dụng đăng nhập và đóng ứng dụng hiện tại
                        if (!string.IsNullOrEmpty(pathExeDangNhap))
                        {
                            System.Diagnostics.Process.Start(pathExeDangNhap);
                            Application.Exit(); // Đóng hoàn toàn ERP_BanHang
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy file ứng dụng Đăng nhập (ERP_Khach.exe)!\nVui lòng kiểm tra lại thư mục chứa file.",
                                            "Lỗi khởi chạy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi đăng xuất: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}