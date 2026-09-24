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

            // Mapping DataPropertyName tương ứng với alias trong SQL
            dgvChiTiet.Columns["colMaSP"].DataPropertyName = "id_sp";
            dgvChiTiet.Columns["colMoTa"].DataPropertyName = "tenhang";
            dgvChiTiet.Columns["colDVT"].DataPropertyName = "donvitinh";
            dgvChiTiet.Columns["colSoLuong"].DataPropertyName = "soluong";
            dgvChiTiet.Columns["colDonGia"].DataPropertyName = "dongia";
            dgvChiTiet.Columns["colThanhTien"].DataPropertyName = "thanhtien";

            // Tỷ lệ co giãn các cột
            dgvChiTiet.Columns["colMaSP"].FillWeight = 12;
            dgvChiTiet.Columns["colMoTa"].FillWeight = 42;
            dgvChiTiet.Columns["colDVT"].FillWeight = 10;
            dgvChiTiet.Columns["colSoLuong"].FillWeight = 10;
            dgvChiTiet.Columns["colDonGia"].FillWeight = 13;
            dgvChiTiet.Columns["colThanhTien"].FillWeight = 13;

            // Định dạng căn lề và tiền tệ
            dgvChiTiet.Columns["colSoLuong"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvChiTiet.Columns["colDVT"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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
            // Query lấy thông tin hóa đơn, khách hàng, nhân viên (Hỗ trợ Postgres)
            string query = @"
                SELECT 
                    hd.id_hd,
                    hd.ngaylap,
                    hd.trangthai AS trangthaihd,
                    hd.hinhthuctt,
                    COALESCE(hd.tratruoc, 0) AS tratruoc,
                    COALESCE(hd.tongtien, 0) AS tongtien,
                    kh.tendoanhnghiep,
                    kh.nguoidaidien,
                    kh.masothue,
                    kh.diachi,
                    kh.sdt,
                    kh.email,
                    nv.tennv AS nhanvienlap
                FROM hoadon hd
                INNER JOIN donhang dh ON hd.id_dh = dh.id_dh
                INNER JOIN khachhang kh ON dh.id_kh = kh.id_kh
                INNER JOIN nhanvien nv ON hd.id_nv = nv.id_nv
                WHERE dh.id_dh = @ID_DH";

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
                                if (lblMaHoaDon != null) lblMaHoaDon.Text = reader["id_hd"].ToString();
                                if (lblNgayLap != null && reader["ngaylap"] != DBNull.Value)
                                    lblNgayLap.Text = Convert.ToDateTime(reader["ngaylap"]).ToString("dd/MM/yyyy HH:mm");
                                if (lblTenKhachHang != null) lblTenKhachHang.Text = reader["tendoanhnghiep"].ToString();
                                if (lblNguoiDaiDien != null) lblNguoiDaiDien.Text = reader["nguoidaidien"].ToString();
                                if (lblMaSoThue != null) lblMaSoThue.Text = reader["masothue"].ToString();
                                if (lblDiaChi != null) lblDiaChi.Text = reader["diachi"].ToString();
                                if (lblSDT != null) lblSDT.Text = reader["sdt"].ToString();
                                if (lblEmail != null) lblEmail.Text = reader["email"].ToString();
                                if (lblNhanVienLap != null) lblNhanVienLap.Text = reader["nhanvienlap"].ToString();

                                // Tính toán hiển thị tổng số tiền
                                decimal tongTien = Convert.ToDecimal(reader["tongtien"]);
                                decimal traTruoc = Convert.ToDecimal(reader["tratruoc"]);
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
            // Query lấy danh sách sản phẩm thuộc Hóa đơn/Đơn hàng kết hợp bảng HangHoa để lấy Đơn vị tính (donvitinh)
            string query = @"
                SELECT 
                    sp.id_sp,
                    hh.tenhang,
                    hh.donvitinh,
                    cthd.soluong,
                    cthd.dongia,
                    (cthd.soluong * cthd.dongia) AS thanhtien
                FROM chitiethoadon cthd
                INNER JOIN hoadon hd ON cthd.id_hd = hd.id_hd
                INNER JOIN sanpham sp ON cthd.id_sp = sp.id_sp
                INNER JOIN hanghoa hh ON sp.mahang = hh.mahang
                WHERE hd.id_dh = @ID_DH";

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