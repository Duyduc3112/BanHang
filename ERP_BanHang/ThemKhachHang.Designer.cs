namespace ERP_BanHang
{
    partial class ThemKhachHang
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lblHeaderSub = new System.Windows.Forms.Label();
            this.lblDate = new System.Windows.Forms.Label();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabThongTinCoBan = new System.Windows.Forms.TabPage();
            this.btnNextToTab2 = new System.Windows.Forms.Button();
            this.txtMaSoThue = new System.Windows.Forms.TextBox();
            this.lblMaSoThue = new System.Windows.Forms.Label();
            this.txtNguoiDaiDien = new System.Windows.Forms.TextBox();
            this.lblNguoiDaiDien = new System.Windows.Forms.Label();
            this.txtTenDoanhNghiep = new System.Windows.Forms.TextBox();
            this.lblTenDoanhNghiep = new System.Windows.Forms.Label();
            this.txtID_KH = new System.Windows.Forms.TextBox();
            this.lblID_KH = new System.Windows.Forms.Label();
            this.tabDiaChiLienHe = new System.Windows.Forms.TabPage();
            this.btnLuu = new System.Windows.Forms.Button();
            this.btnHuy = new System.Windows.Forms.Button();
            this.txtDiaChi = new System.Windows.Forms.TextBox();
            this.lblDiaChi = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.lblEmail = new System.Windows.Forms.Label();
            this.txtSDT = new System.Windows.Forms.TextBox();
            this.lblSDT = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabControlMain.SuspendLayout();
            this.tabThongTinCoBan.SuspendLayout();
            this.tabDiaChiLienHe.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(30, 20);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(308, 37);
            this.lblHeaderTitle.TabIndex = 0;
            this.lblHeaderTitle.Text = "Thêm Khách Hàng Mới";
            // 
            // lblHeaderSub
            // 
            this.lblHeaderSub.AutoSize = true;
            this.lblHeaderSub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblHeaderSub.ForeColor = System.Drawing.Color.Transparent;
            this.lblHeaderSub.Location = new System.Drawing.Point(32, 60);
            this.lblHeaderSub.Name = "lblHeaderSub";
            this.lblHeaderSub.Size = new System.Drawing.Size(429, 20);
            this.lblHeaderSub.TabIndex = 1;
            this.lblHeaderSub.Text = "Điền đầy đủ thông tin để đăng ký khách hàng vào hệ thống ERP";
            // 
            // lblDate
            // 
            this.lblDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblDate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDate.ForeColor = System.Drawing.Color.Transparent;
            this.lblDate.Location = new System.Drawing.Point(620, 25);
            this.lblDate.Name = "lblDate";
            this.lblDate.Size = new System.Drawing.Size(120, 30);
            this.lblDate.TabIndex = 2;
            this.lblDate.Text = "📅 16/9/2026";
            this.lblDate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabControlMain
            // 
            this.tabControlMain.Controls.Add(this.tabThongTinCoBan);
            this.tabControlMain.Controls.Add(this.tabDiaChiLienHe);
            this.tabControlMain.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.tabControlMain.ItemSize = new System.Drawing.Size(180, 40);
            this.tabControlMain.Location = new System.Drawing.Point(35, 100);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(705, 380);
            this.tabControlMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControlMain.TabIndex = 3;
            // 
            // tabThongTinCoBan
            // 
            this.tabThongTinCoBan.BackColor = System.Drawing.Color.White;
            this.tabThongTinCoBan.Controls.Add(this.btnNextToTab2);
            this.tabThongTinCoBan.Controls.Add(this.txtMaSoThue);
            this.tabThongTinCoBan.Controls.Add(this.lblMaSoThue);
            this.tabThongTinCoBan.Controls.Add(this.txtNguoiDaiDien);
            this.tabThongTinCoBan.Controls.Add(this.lblNguoiDaiDien);
            this.tabThongTinCoBan.Controls.Add(this.txtTenDoanhNghiep);
            this.tabThongTinCoBan.Controls.Add(this.lblTenDoanhNghiep);
            this.tabThongTinCoBan.Controls.Add(this.txtID_KH);
            this.tabThongTinCoBan.Controls.Add(this.lblID_KH);
            this.tabThongTinCoBan.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabThongTinCoBan.Location = new System.Drawing.Point(4, 44);
            this.tabThongTinCoBan.Name = "tabThongTinCoBan";
            this.tabThongTinCoBan.Padding = new System.Windows.Forms.Padding(20);
            this.tabThongTinCoBan.Size = new System.Drawing.Size(697, 332);
            this.tabThongTinCoBan.TabIndex = 0;
            this.tabThongTinCoBan.Text = "Thông Tin Cơ Bản";
            // 
            // btnNextToTab2
            // 
            this.btnNextToTab2.BackColor = System.Drawing.Color.Black;
            this.btnNextToTab2.FlatAppearance.BorderSize = 0;
            this.btnNextToTab2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNextToTab2.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnNextToTab2.ForeColor = System.Drawing.Color.White;
            this.btnNextToTab2.Location = new System.Drawing.Point(520, 260);
            this.btnNextToTab2.Name = "btnNextToTab2";
            this.btnNextToTab2.Size = new System.Drawing.Size(140, 40);
            this.btnNextToTab2.TabIndex = 8;
            this.btnNextToTab2.Text = "Tiếp Theo →";
            this.btnNextToTab2.UseVisualStyleBackColor = false;
            this.btnNextToTab2.Click += new System.EventHandler(this.btnNextToTab2_Click);
            // 
            // txtMaSoThue
            // 
            this.txtMaSoThue.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtMaSoThue.ForeColor = System.Drawing.Color.Gray;
            this.txtMaSoThue.Location = new System.Drawing.Point(360, 195);
            this.txtMaSoThue.Name = "txtMaSoThue";
            this.txtMaSoThue.Size = new System.Drawing.Size(300, 30);
            this.txtMaSoThue.TabIndex = 7;
            this.txtMaSoThue.Text = "VD: 0314567890";
            this.txtMaSoThue.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtMaSoThue.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblMaSoThue
            // 
            this.lblMaSoThue.AutoSize = true;
            this.lblMaSoThue.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMaSoThue.ForeColor = System.Drawing.Color.DimGray;
            this.lblMaSoThue.Location = new System.Drawing.Point(360, 170);
            this.lblMaSoThue.Name = "lblMaSoThue";
            this.lblMaSoThue.Size = new System.Drawing.Size(111, 20);
            this.lblMaSoThue.TabIndex = 6;
            this.lblMaSoThue.Text = "MÃ SỐ THUẾ *";
            // 
            // txtNguoiDaiDien
            // 
            this.txtNguoiDaiDien.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtNguoiDaiDien.ForeColor = System.Drawing.Color.Gray;
            this.txtNguoiDaiDien.Location = new System.Drawing.Point(30, 195);
            this.txtNguoiDaiDien.Name = "txtNguoiDaiDien";
            this.txtNguoiDaiDien.Size = new System.Drawing.Size(300, 30);
            this.txtNguoiDaiDien.TabIndex = 5;
            this.txtNguoiDaiDien.Text = "VD: Nguyễn Văn A";
            this.txtNguoiDaiDien.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtNguoiDaiDien.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblNguoiDaiDien
            // 
            this.lblNguoiDaiDien.AutoSize = true;
            this.lblNguoiDaiDien.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblNguoiDaiDien.ForeColor = System.Drawing.Color.DimGray;
            this.lblNguoiDaiDien.Location = new System.Drawing.Point(30, 170);
            this.lblNguoiDaiDien.Name = "lblNguoiDaiDien";
            this.lblNguoiDaiDien.Size = new System.Drawing.Size(132, 20);
            this.lblNguoiDaiDien.TabIndex = 4;
            this.lblNguoiDaiDien.Text = "NGƯỜI ĐẠI DIỆN";
            // 
            // txtTenDoanhNghiep
            // 
            this.txtTenDoanhNghiep.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtTenDoanhNghiep.ForeColor = System.Drawing.Color.Gray;
            this.txtTenDoanhNghiep.Location = new System.Drawing.Point(30, 120);
            this.txtTenDoanhNghiep.Name = "txtTenDoanhNghiep";
            this.txtTenDoanhNghiep.Size = new System.Drawing.Size(630, 30);
            this.txtTenDoanhNghiep.TabIndex = 3;
            this.txtTenDoanhNghiep.Text = "VD: Công ty TNHH Phân Phối Thành Đạt";
            this.txtTenDoanhNghiep.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtTenDoanhNghiep.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblTenDoanhNghiep
            // 
            this.lblTenDoanhNghiep.AutoSize = true;
            this.lblTenDoanhNghiep.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblTenDoanhNghiep.ForeColor = System.Drawing.Color.DimGray;
            this.lblTenDoanhNghiep.Location = new System.Drawing.Point(30, 95);
            this.lblTenDoanhNghiep.Name = "lblTenDoanhNghiep";
            this.lblTenDoanhNghiep.Size = new System.Drawing.Size(231, 20);
            this.lblTenDoanhNghiep.TabIndex = 2;
            this.lblTenDoanhNghiep.Text = "TÊN DOANH NGHIỆP / ĐẠI LÝ *";
            // 
            // txtID_KH
            // 
            this.txtID_KH.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtID_KH.ForeColor = System.Drawing.Color.Gray;
            this.txtID_KH.Location = new System.Drawing.Point(30, 45);
            this.txtID_KH.Name = "txtID_KH";
            this.txtID_KH.Size = new System.Drawing.Size(300, 30);
            this.txtID_KH.TabIndex = 1;
            this.txtID_KH.Text = "VD: KH001";
            this.txtID_KH.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtID_KH.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblID_KH
            // 
            this.lblID_KH.AutoSize = true;
            this.lblID_KH.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblID_KH.ForeColor = System.Drawing.Color.DimGray;
            this.lblID_KH.Location = new System.Drawing.Point(30, 20);
            this.lblID_KH.Name = "lblID_KH";
            this.lblID_KH.Size = new System.Drawing.Size(150, 20);
            this.lblID_KH.TabIndex = 0;
            this.lblID_KH.Text = "MÃ KHÁCH HÀNG *";
            // 
            // tabDiaChiLienHe
            // 
            this.tabDiaChiLienHe.BackColor = System.Drawing.Color.White;
            this.tabDiaChiLienHe.Controls.Add(this.btnLuu);
            this.tabDiaChiLienHe.Controls.Add(this.btnHuy);
            this.tabDiaChiLienHe.Controls.Add(this.txtDiaChi);
            this.tabDiaChiLienHe.Controls.Add(this.lblDiaChi);
            this.tabDiaChiLienHe.Controls.Add(this.txtEmail);
            this.tabDiaChiLienHe.Controls.Add(this.lblEmail);
            this.tabDiaChiLienHe.Controls.Add(this.txtSDT);
            this.tabDiaChiLienHe.Controls.Add(this.lblSDT);
            this.tabDiaChiLienHe.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabDiaChiLienHe.Location = new System.Drawing.Point(4, 44);
            this.tabDiaChiLienHe.Name = "tabDiaChiLienHe";
            this.tabDiaChiLienHe.Padding = new System.Windows.Forms.Padding(20);
            this.tabDiaChiLienHe.Size = new System.Drawing.Size(697, 332);
            this.tabDiaChiLienHe.TabIndex = 1;
            this.tabDiaChiLienHe.Text = "Địa Chỉ & Liên Hệ";
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(49)))), ((int)(((byte)(24)))));
            this.btnLuu.FlatAppearance.BorderSize = 0;
            this.btnLuu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLuu.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(520, 260);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(140, 40);
            this.btnLuu.TabIndex = 7;
            this.btnLuu.Text = "Lưu Khách Hàng";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // btnHuy
            // 
            this.btnHuy.BackColor = System.Drawing.Color.Gainsboro;
            this.btnHuy.FlatAppearance.BorderSize = 0;
            this.btnHuy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHuy.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnHuy.ForeColor = System.Drawing.Color.DimGray;
            this.btnHuy.Location = new System.Drawing.Point(400, 260);
            this.btnHuy.Name = "btnHuy";
            this.btnHuy.Size = new System.Drawing.Size(100, 40);
            this.btnHuy.TabIndex = 6;
            this.btnHuy.Text = "Hủy Bỏ";
            this.btnHuy.UseVisualStyleBackColor = false;
            this.btnHuy.Click += new System.EventHandler(this.btnHuy_Click);
            // 
            // txtDiaChi
            // 
            this.txtDiaChi.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtDiaChi.ForeColor = System.Drawing.Color.Gray;
            this.txtDiaChi.Location = new System.Drawing.Point(30, 135);
            this.txtDiaChi.Name = "txtDiaChi";
            this.txtDiaChi.Size = new System.Drawing.Size(630, 30);
            this.txtDiaChi.TabIndex = 5;
            this.txtDiaChi.Text = "VD: Số 10, Đường Cầu Giấy, Hà Nội";
            this.txtDiaChi.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtDiaChi.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblDiaChi
            // 
            this.lblDiaChi.AutoSize = true;
            this.lblDiaChi.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDiaChi.ForeColor = System.Drawing.Color.DimGray;
            this.lblDiaChi.Location = new System.Drawing.Point(30, 110);
            this.lblDiaChi.Name = "lblDiaChi";
            this.lblDiaChi.Size = new System.Drawing.Size(129, 20);
            this.lblDiaChi.TabIndex = 4;
            this.lblDiaChi.Text = "ĐỊA CHỈ CHI TIẾT";
            // 
            // txtEmail
            // 
            this.txtEmail.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtEmail.ForeColor = System.Drawing.Color.Gray;
            this.txtEmail.Location = new System.Drawing.Point(360, 50);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(300, 30);
            this.txtEmail.TabIndex = 3;
            this.txtEmail.Text = "VD: contact@thanhdat.com";
            this.txtEmail.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtEmail.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblEmail
            // 
            this.lblEmail.AutoSize = true;
            this.lblEmail.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblEmail.ForeColor = System.Drawing.Color.DimGray;
            this.lblEmail.Location = new System.Drawing.Point(360, 25);
            this.lblEmail.Name = "lblEmail";
            this.lblEmail.Size = new System.Drawing.Size(55, 20);
            this.lblEmail.TabIndex = 2;
            this.lblEmail.Text = "EMAIL";
            // 
            // txtSDT
            // 
            this.txtSDT.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSDT.ForeColor = System.Drawing.Color.Gray;
            this.txtSDT.Location = new System.Drawing.Point(30, 50);
            this.txtSDT.Name = "txtSDT";
            this.txtSDT.Size = new System.Drawing.Size(300, 30);
            this.txtSDT.TabIndex = 1;
            this.txtSDT.Text = "VD: 0987654321";
            this.txtSDT.Enter += new System.EventHandler(this.RemovePlaceholder);
            this.txtSDT.Leave += new System.EventHandler(this.SetPlaceholder);
            // 
            // lblSDT
            // 
            this.lblSDT.AutoSize = true;
            this.lblSDT.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblSDT.ForeColor = System.Drawing.Color.DimGray;
            this.lblSDT.Location = new System.Drawing.Point(30, 25);
            this.lblSDT.Name = "lblSDT";
            this.lblSDT.Size = new System.Drawing.Size(119, 20);
            this.lblSDT.TabIndex = 0;
            this.lblSDT.Text = "SỐ ĐIỆN THOẠI";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Tomato;
            this.panel1.Controls.Add(this.lblDate);
            this.panel1.Controls.Add(this.lblHeaderSub);
            this.panel1.Controls.Add(this.lblHeaderTitle);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(777, 91);
            this.panel1.TabIndex = 4;
            // 
            // ThemKhachHang
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(775, 500);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabControlMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ThemKhachHang";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ERP Bán Hàng - Thêm Khách Hàng";
            this.tabControlMain.ResumeLayout(false);
            this.tabThongTinCoBan.ResumeLayout(false);
            this.tabThongTinCoBan.PerformLayout();
            this.tabDiaChiLienHe.ResumeLayout(false);
            this.tabDiaChiLienHe.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lblHeaderSub;
        private System.Windows.Forms.Label lblDate;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabThongTinCoBan;
        private System.Windows.Forms.TabPage tabDiaChiLienHe;
        private System.Windows.Forms.Label lblID_KH;
        private System.Windows.Forms.TextBox txtID_KH;
        private System.Windows.Forms.Label lblTenDoanhNghiep;
        private System.Windows.Forms.TextBox txtTenDoanhNghiep;
        private System.Windows.Forms.Label lblNguoiDaiDien;
        private System.Windows.Forms.TextBox txtNguoiDaiDien;
        private System.Windows.Forms.Label lblMaSoThue;
        private System.Windows.Forms.TextBox txtMaSoThue;
        private System.Windows.Forms.Button btnNextToTab2;
        private System.Windows.Forms.Label lblSDT;
        private System.Windows.Forms.TextBox txtSDT;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblDiaChi;
        private System.Windows.Forms.TextBox txtDiaChi;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Button btnHuy;
        private System.Windows.Forms.Panel panel1;
    }
}