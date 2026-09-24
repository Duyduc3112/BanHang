using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System;
using System.Configuration;
using System.Data;
using Npgsql;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace ERP_BanHang
{
    public partial class ChiTietHoaDon : Form
    {
        // Chuỗi kết nối đến CSDL PostgreSQL
        private string connectionString = ConfigurationManager.ConnectionStrings["ERP_Connection"]?.ConnectionString
            ?? ConfigurationManager.ConnectionStrings["ERP_BanHang"]?.ConnectionString;

        // Mã đơn hàng nhận từ Form Quản lý đơn hàng
        private string maDonHangSelected = "DH001";

        // Biến lưu trạng thái thanh toán của hóa đơn/đơn hàng
        private string trangThaiThanhToan = "Chưa thanh toán";

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

            // 5. Kiểm tra quyền xuất file dựa trên trạng thái thanh toán
            CapNhatTrangThaiNutXuat();
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
            // SỬA CHUẨN: Dùng LEFT JOIN hoadon để tránh mất dữ liệu khi đơn hàng chưa tạo bản ghi trong bảng hoadon
            string query = @"
                SELECT 
                    COALESCE(hd.id_hd, 'HD_' || dh.id_dh) AS id_hd,
                    COALESCE(hd.ngaylap, dh.ngaytao) AS ngaylap,
                    COALESCE(hd.trangthai, N'Chưa thanh toán') AS trangthaihd,
                    COALESCE(hd.hinhthuctt, N'Tiền mặt / Chuyển khoản') AS hinhthuctt,
                    COALESCE(hd.tratruoc, 0) AS tratruoc,
                    COALESCE(SUM(ctdh.soluong * ctdh.dongia), 0) AS tongtien,
                    kh.tendoanhnghiep,
                    kh.nguoidaidien,
                    kh.masothue,
                    kh.diachi,
                    kh.sdt,
                    kh.email,
                    nv.tennv AS nhanvienlap
                FROM donhang dh
                INNER JOIN khachhang kh ON dh.id_kh = kh.id_kh
                INNER JOIN nhanvien nv ON dh.id_nv = nv.id_nv
                LEFT JOIN hoadon hd ON dh.id_dh = hd.id_dh
                LEFT JOIN chitietdonhang ctdh ON dh.id_dh = ctdh.id_dh
                WHERE LOWER(dh.id_dh) = LOWER(@ID_DH)
                GROUP BY hd.id_hd, hd.ngaylap, hd.trangthai, hd.hinhthuctt, hd.tratruoc,
                         dh.id_dh, dh.ngaytao, kh.tendoanhnghiep, kh.nguoidaidien, 
                         kh.masothue, kh.diachi, kh.sdt, kh.email, nv.tennv";

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
                                // Đánh dữ liệu lên Label giao diện chính xác theo đơn hàng được chọn
                                if (lblMaHoaDon != null) lblMaHoaDon.Text = reader["id_hd"].ToString();

                                if (lblNgayLap != null && reader["ngaylap"] != DBNull.Value)
                                    lblNgayLap.Text = Convert.ToDateTime(reader["ngaylap"]).ToString("dd/MM/yyyy HH:mm");

                                if (lblTenKhachHang != null) lblTenKhachHang.Text = reader["tendoanhnghiep"]?.ToString() ?? "";
                                if (lblNguoiDaiDien != null) lblNguoiDaiDien.Text = reader["nguoidaidien"]?.ToString() ?? "";
                                if (lblMaSoThue != null) lblMaSoThue.Text = reader["masothue"]?.ToString() ?? "";
                                if (lblDiaChi != null) lblDiaChi.Text = reader["diachi"]?.ToString() ?? "";
                                if (lblSDT != null) lblSDT.Text = reader["sdt"]?.ToString() ?? "";
                                if (lblEmail != null) lblEmail.Text = reader["email"]?.ToString() ?? "";
                                if (lblNhanVienLap != null) lblNhanVienLap.Text = reader["nhanvienlap"]?.ToString() ?? "";

                                // Đọc trạng thái thanh toán chuẩn
                                trangThaiThanhToan = reader["trangthaihd"] != DBNull.Value ? reader["trangthaihd"].ToString() : "Chưa thanh toán";

                                // Tính toán số tiền từ đơn hàng
                                decimal tongTien = Convert.ToDecimal(reader["tongtien"]);
                                decimal traTruoc = Convert.ToDecimal(reader["tratruoc"]);
                                decimal conLai = tongTien - traTruoc;

                                if (lblTongTien != null) lblTongTien.Text = tongTien.ToString("N0") + " đ";
                                if (lblTraTruoc != null) lblTraTruoc.Text = traTruoc.ToString("N0") + " đ";
                                if (lblConLai != null) lblConLai.Text = conLai.ToString("N0") + " đ";
                            }
                            else
                            {
                                MessageBox.Show($"Không tìm thấy thông tin cho mã đơn hàng [{maDonHangSelected}]!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            // SỬA CHUẨN: Lấy chi tiết danh sách sản phẩm trực tiếp từ chitietdonhang ghép với hanghoa
            string query = @"
                SELECT 
                    sp.id_sp,
                    hh.tenhang,
                    hh.donvitinh,
                    ctdh.soluong,
                    ctdh.dongia,
                    (ctdh.soluong * ctdh.dongia) AS thanhtien
                FROM chitietdonhang ctdh
                INNER JOIN sanpham sp ON ctdh.id_sp = sp.id_sp
                INNER JOIN hanghoa hh ON sp.mahang = hh.mahang
                WHERE LOWER(ctdh.id_dh) = LOWER(@ID_DH)";

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

        // Kiểm tra điều kiện để khóa hoặc mở nút "Xuất PDF"
        private void CapNhatTrangThaiNutXuat()
        {
            bool isDaThanhToan = trangThaiThanhToan.Equals("Đã thanh toán", StringComparison.OrdinalIgnoreCase)
                              || trangThaiThanhToan.Equals("Hoàn tất", StringComparison.OrdinalIgnoreCase);

            if (!isDaThanhToan)
            {
                btnXuatPDF.Enabled = false; // Vô hiệu hóa nút xuất
                ToolTip tt = new ToolTip();
                tt.SetToolTip(btnXuatPDF, "Đơn hàng chưa thanh toán! Không thể xuất hóa đơn.");
            }
            else
            {
                btnXuatPDF.Enabled = true;
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

        // Sự kiện xuất file PDF khi bấm nút
        private void btnXuatPDF_Click(object sender, EventArgs e)
        {
            bool isDaThanhToan = trangThaiThanhToan.Equals("Đã thanh toán", StringComparison.OrdinalIgnoreCase)
                              || trangThaiThanhToan.Equals("Hoàn tất", StringComparison.OrdinalIgnoreCase);

            if (!isDaThanhToan)
            {
                MessageBox.Show("Đơn hàng này CHƯA THANH TOÁN!\nBạn không thể xuất file hóa đơn khi chưa hoàn tất thanh toán.",
                                "Cảnh báo xuất hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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