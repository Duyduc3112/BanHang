using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Configuration;
using System.Data;
using Npgsql; // Đã đổi từ System.Data.SqlClient sang Npgsql
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ERP_BanHang
{
    public partial class ChiTietHoaDon : Form
    {
        // Chuỗi kết nối đến CSDL PostgreSQL
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"].ConnectionString;

        // Mã đơn hàng nhận từ Form Quản lý đơn hàng
        private string maDonHangSelected = "DH001";

        public ChiTietHoaDon()
        {
            InitializeComponent();
        }

        // Constructor nhận mã đơn hàng từ Form QlyDonHang
        public ChiTietHoaDon(string maDH)
        {
            InitializeComponent();
            this.maDonHangSelected = maDH;
        }

        private void ChiTietHoaDon_Load(object sender, EventArgs e)
        {
            // 1. Cấu hình các cột cho DataGridView
            KhoiTaoCotBang();

            // 2. Tải thông tin chung Hóa đơn / Khách hàng / Thanh toán
            LoadThongTinChungHoaDon();

            // 3. Đổ dữ liệu chi tiết sản phẩm từ CSDL
            LoadDataChiTietHoaDon();

            // 4. Tự động tính toán lại chiều cao giao diện
            TuDongCapNhatChieuCaoBang();
        }

        private void KhoiTaoCotBang()
        {
            dgvChiTiet.Columns.Clear();
            dgvChiTiet.AutoGenerateColumns = false;

            dgvChiTiet.Columns.Add("colMaSP", "MÃ SP");
            dgvChiTiet.Columns.Add("colMoTa", "TÊN SẢN PHẨM / HÀNG HÓA");
            dgvChiTiet.Columns.Add("colDVT", "ĐVT");
            dgvChiTiet.Columns.Add("colSoLuong", "SỐ LƯỢNG");
            dgvChiTiet.Columns.Add("colDonGia", "ĐƠN GIÁ");
            dgvChiTiet.Columns.Add("colThanhTien", "THÀNH TIỀN");

            // Mapping DataPropertyName với SQL
            dgvChiTiet.Columns["colMaSP"].DataPropertyName = "ID_SP";
            dgvChiTiet.Columns["colMoTa"].DataPropertyName = "TenHang";
            dgvChiTiet.Columns["colDVT"].DataPropertyName = "DonViTinh";
            dgvChiTiet.Columns["colSoLuong"].DataPropertyName = "SoLuong";
            dgvChiTiet.Columns["colDonGia"].DataPropertyName = "DonGia";
            dgvChiTiet.Columns["colThanhTien"].DataPropertyName = "ThanhTien";

            // Tỷ lệ co giãn các cột
            dgvChiTiet.Columns["colMaSP"].FillWeight = 12;
            dgvChiTiet.Columns["colMoTa"].FillWeight = 42;
            dgvChiTiet.Columns["colDVT"].FillWeight = 10;
            dgvChiTiet.Columns["colSoLuong"].FillWeight = 10;
            dgvChiTiet.Columns["colDonGia"].FillWeight = 13;
            dgvChiTiet.Columns["colThanhTien"].FillWeight = 13;

            // Định dạng căn lề và tiền tệ
            dgvChiTiet.Columns["colSoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTiet.Columns["colDonGia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvChiTiet.Columns["colThanhTien"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dgvChiTiet.Columns["colDonGia"].DefaultCellStyle.Format = "N0";
            dgvChiTiet.Columns["colThanhTien"].DefaultCellStyle.Format = "N0";

            dgvChiTiet.Columns["colThanhTien"].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvChiTiet.Columns["colMaSP"].DefaultCellStyle.ForeColor = Color.FromArgb(165, 42, 42);
            dgvChiTiet.Columns["colMaSP"].DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        }

        private void LoadThongTinChungHoaDon()
        {
            string query = @"
                SELECT 
                    HD.ID_HD,
                    HD.NgayLap,
                    HD.TrangThai AS TrangThaiHD,
                    HD.HinhThucTT,
                    HD.TraTruoc,
                    HD.TongTien,
                    KH.TenDoanhNghiep,
                    KH.NguoiDaiDien,
                    KH.MaSoThue,
                    KH.DiaChi,
                    KH.SDT,
                    KH.Email,
                    NV.TenNV AS NhanVienLap
                FROM HoaDon HD
                INNER JOIN DonHang DH ON HD.ID_DH = DH.ID_DH
                INNER JOIN KhachHang KH ON DH.ID_KH = KH.ID_KH
                INNER JOIN NhanVien NV ON HD.ID_NV = NV.ID_NV
                WHERE DH.ID_DH = @ID_DH";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_DH", maDonHangSelected);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Đán dữ liệu thông tin Hóa đơn & Khách hàng lên các Label giao diện
                                if (lblMaHoaDon != null) lblMaHoaDon.Text = reader["ID_HD"].ToString();
                                if (lblNgayLap != null) lblNgayLap.Text = Convert.ToDateTime(reader["NgayLap"]).ToString("dd/MM/yyyy HH:mm");
                                if (lblTenKhachHang != null) lblTenKhachHang.Text = reader["TenDoanhNghiep"].ToString();
                                if (lblNguoiDaiDien != null) lblNguoiDaiDien.Text = reader["NguoiDaiDien"].ToString();
                                if (lblMaSoThue != null) lblMaSoThue.Text = reader["MaSoThue"].ToString();
                                if (lblDiaChi != null) lblDiaChi.Text = reader["DiaChi"].ToString();
                                if (lblSDT != null) lblSDT.Text = reader["SDT"].ToString();
                                if (lblEmail != null) lblEmail.Text = reader["Email"].ToString();
                                if (lblNhanVienLap != null) lblNhanVienLap.Text = reader["NhanVienLap"].ToString();

                                // Tính toán hiển thị tổng số tiền
                                decimal tongTien = Convert.ToDecimal(reader["TongTien"]);
                                decimal traTruoc = Convert.ToDecimal(reader["TraTruoc"]);
                                decimal conLai = tongTien - traTruoc;

                                if (lblTongTien != null) lblTongTien.Text = tongTien.ToString("N0") + " đ";
                                if (lblTraTruoc != null) lblTraTruoc.Text = traTruoc.ToString("N0") + " đ";
                                if (lblConLai != null) lblConLai.Text = conLai.ToString("N0") + " đ";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi lấy thông tin hóa đơn: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadDataChiTietHoaDon()
        {
            // Query lấy danh sách sản phẩm thuộc Hóa đơn/Đơn hàng được chọn
            string query = @"
                SELECT 
                    SP.ID_SP,
                    HH.TenHang,
                    HH.DonViTinh,
                    CTHD.SoLuong,
                    CTHD.DonGia,
                    CTHD.ThanhTien
                FROM ChiTietHoaDon CTHD
                INNER JOIN HoaDon HD ON CTHD.ID_HD = HD.ID_HD
                INNER JOIN SanPham SP ON CTHD.ID_SP = SP.ID_SP
                INNER JOIN HangHoa HH ON SP.MaHang = HH.MaHang
                WHERE HD.ID_DH = @ID_DH";

            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID_DH", maDonHangSelected);

                        NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd);
                        DataTable dtChiTiet = new DataTable();
                        da.Fill(dtChiTiet);

                        dgvChiTiet.DataSource = dtChiTiet;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải chi tiết sản phẩm hóa đơn: " + ex.Message, "Lỗi PostgreSQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Tự động tính toán chiều cao DataGridView & Form
        private void TuDongCapNhatChieuCaoBang()
        {
            dgvChiTiet.ScrollBars = ScrollBars.None;

            int tongChieuCaoDgv = dgvChiTiet.ColumnHeadersHeight + dgvChiTiet.Rows.GetRowsHeight(DataGridViewElementStates.Visible);

            if (pnlGridContainer != null)
                pnlGridContainer.Height = tongChieuCaoDgv + 30;

            if (pnlMainCard != null && pnlHeader != null && pnlCustomerBank != null && pnlTotalSummary != null && pnlFooter != null)
            {
                int tongChieuCaoNoiDung = pnlHeader.Height + pnlCustomerBank.Height + pnlGridContainer.Height + pnlTotalSummary.Height + pnlFooter.Height + 80;
                pnlMainCard.Height = tongChieuCaoNoiDung;
            }

            this.AutoScroll = true;
        }

        // Sự kiện xuất file PDF
        private void btnXuatPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
            saveFileDialog.FileName = $"HoaDon_{maDonHangSelected}.pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    btnXuatPDF.Visible = false;

                    XuatFormRaPDF(pnlMainCard, saveFileDialog.FileName);

                    btnXuatPDF.Visible = true;

                    MessageBox.Show("Xuất hóa đơn PDF thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    btnXuatPDF.Visible = true;
                    MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void XuatFormRaPDF(Panel targetPanel, string filePath)
        {
            Bitmap bmp = new Bitmap(targetPanel.Width, targetPanel.Height);
            targetPanel.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, targetPanel.Width, targetPanel.Height));

            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                byte[] imgBytes = ms.ToArray();

                using (PdfWriter writer = new PdfWriter(filePath))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(writer))
                    {
                        PageSize pageSize = new PageSize(targetPanel.Width, targetPanel.Height);
                        Document doc = new Document(pdfDoc, pageSize);
                        doc.SetMargins(0, 0, 0, 0);

                        ImageData imageData = ImageDataFactory.Create(imgBytes);
                        iText.Layout.Element.Image pdfImg = new iText.Layout.Element.Image(imageData);
                        pdfImg.ScaleToFit(pageSize.GetWidth(), pageSize.GetHeight());

                        doc.Add(pdfImg);
                        doc.Close();
                    }
                }
            }
        }
    }
}