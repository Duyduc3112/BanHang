using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrPheDuyetYeuCau : Form
    {
        private DataRow requestRow;
        private DataTable dtLichSu;

        public FrPheDuyetYeuCau(DataRow row, DataTable lichSuTable)
        {
            InitializeComponent();
            requestRow = row;
            dtLichSu = lichSuTable;
        }

        private void FrPheDuyetYeuCau_Load(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show("Tài khoản của bạn không có quyền phê duyệt yêu cầu!", "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            cboHuongXuLy.Items.Clear();
            cboHuongXuLy.Items.Add("Trả lại Nhà cung cấp (NCC)");
            cboHuongXuLy.Items.Add("Đưa vào danh sách Tiêu hủy tại Kho");
            cboHuongXuLy.Items.Add("Nhập kho lại sản phẩm");
            cboHuongXuLy.SelectedIndex = 0;

            if (requestRow != null)
            {
                txtThongTin.Text = $"Mã phiếu trả: {requestRow["MaYeuCau"]}\r\n" +
                                   $"Bên gửi: {requestRow["BoPhanGui"]}\r\n" +
                                   $"Sản phẩm: {requestRow["MaLoHang"]} - {requestRow["TenSanPham"]}\r\n" +
                                   $"Số lượng: {requestRow["SoLuong"]}\r\n" +
                                   $"Lý do đề xuất: {requestRow["LyDo"]}\r\n" +
                                   $"Ngày trả: {requestRow["NgayGui"]}";
            }
        }

        private bool IsPermittedUser()
        {
            string chucVu = UserSession.ChucVu ?? "";
            return chucVu.Equals("Nhân viên kho", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản lý kho", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản lý", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                   chucVu.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);
        }

        private void btnXacNhan_Click(object sender, EventArgs e)
        {
            if (!IsPermittedUser())
            {
                MessageBox.Show("Bạn không có quyền thực hiện chức năng này!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (requestRow == null) return;

            string maPT = requestRow["MaYeuCau"].ToString();
            string maLoHang = requestRow["MaLoHang"].ToString();
            int soLuong = Convert.ToInt32(requestRow["SoLuong"]);
            string luachon = cboHuongXuLy.SelectedItem.ToString();
            string ghiChu = txtGhiChu.Text.Trim();

            try
            {
                // Nếu chọn "Nhập kho lại sản phẩm" -> Cộng tồn kho / tạo mã sản phẩm tồn kho
                if (luachon == "Nhập kho lại sản phẩm")
                {
                    XuLyNhapLaiKho(maLoHang, soLuong);
                }

                // Cập nhật trạng thái 'Đã xử lý' đồng bộ cho phieutrahang, yeucausaubanhang và donhang liên quan
                string sqlUpdate = @"
                    UPDATE phieutrahang 
                    SET trangthai = 'Đã xử lý' 
                    WHERE id_phieutra = @MaPT;

                    UPDATE yeucausaubanhang 
                    SET trangthai = 'Đã xử lý',
                        mota = COALESCE(mota, '') || @GhiChu
                    WHERE id_yc = @MaPT 
                       OR id_yc = (
                           SELECT ctyc.id_yc 
                           FROM chitietyeucau ctyc 
                           JOIN phieutrahang pt ON ctyc.id_ctyc = pt.id_ctyc 
                           WHERE pt.id_phieutra = @MaPT 
                           LIMIT 1
                       );

                    UPDATE donhang 
                    SET trangthai = 'Đã xử lý' 
                    WHERE id_dh = (
                        SELECT id_dh 
                        FROM yeucausaubanhang yc
                        JOIN chitietyeucau ctyc ON yc.id_yc = ctyc.id_yc
                        JOIN phieutrahang pt ON ctyc.id_ctyc = pt.id_ctyc
                        WHERE pt.id_phieutra = @MaPT AND yc.id_dh IS NOT NULL
                        LIMIT 1
                    );";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@GhiChu", string.IsNullOrWhiteSpace(ghiChu) ? $" | [Hướng xử lý: {luachon}]" : $" | [Hướng xử lý: {luachon}] - " + ghiChu),
                    new SqlParameter("@MaPT", maPT)
                };

                int result = DatabaseHelper.ExecuteNonQuery(sqlUpdate, parameters);

                if (result > 0)
                {
                    MessageBox.Show($"Phê duyệt phiếu trả [{maPT}] thành công!\nTrạng thái chuyển thành: [Đã xử lý]\nHướng xử lý: [{luachon}]",
                                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi CSDL khi xử lý nhập kho/phê duyệt: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void XuLyNhapLaiKho(string maLoHang, int soLuong)
        {
            string sqlCheck = "SELECT COUNT(*) FROM sanpham WHERE id_sp = @ID_SP";
            SqlParameter[] paramCheck = new SqlParameter[] { new SqlParameter("@ID_SP", maLoHang) };

            DataTable dtCheck = DatabaseHelper.ExecuteQuery(sqlCheck, paramCheck);
            int count = (dtCheck != null && dtCheck.Rows.Count > 0) ? Convert.ToInt32(dtCheck.Rows[0][0]) : 0;

            if (count > 0)
            {
                string sqlUpdateTon = @"
                    UPDATE sanpham 
                    SET soluongton = COALESCE(soluongton, 0) + @SoLuong 
                    WHERE id_sp = @ID_SP";

                SqlParameter[] paramUpdate = new SqlParameter[]
                {
                    new SqlParameter("@SoLuong", soLuong),
                    new SqlParameter("@ID_SP", maLoHang)
                };

                DatabaseHelper.ExecuteNonQuery(sqlUpdateTon, paramUpdate);
            }
            else
            {
                string sqlGetMaHang = @"
                    SELECT hh.mahang 
                    FROM chitietyeucau ctyc
                    INNER JOIN phieutrahang pt ON ctyc.id_ctyc = pt.id_ctyc
                    INNER JOIN sanpham sp ON ctyc.id_sp = sp.id_sp
                    INNER JOIN hanghoa hh ON sp.mahang = hh.mahang
                    WHERE pt.id_phieutra = @MaPT
                    LIMIT 1";

                SqlParameter[] paramMaHang = new SqlParameter[] { new SqlParameter("@MaPT", requestRow["MaYeuCau"].ToString()) };
                DataTable dtMaHang = DatabaseHelper.ExecuteQuery(sqlGetMaHang, paramMaHang);

                string maHang = (dtMaHang != null && dtMaHang.Rows.Count > 0) ? dtMaHang.Rows[0]["mahang"].ToString() : "MH001";
                string maLoMoi = "SP" + DateTime.Now.ToString("ddHHmmss");

                string sqlInsertLoMoi = @"
                    INSERT INTO sanpham (id_sp, mahang, soluongton, ngaysanxuat, hansudung)
                    VALUES (@ID_SP, @MaHang, @SoLuong, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP + INTERVAL '1 year')";

                SqlParameter[] paramInsert = new SqlParameter[]
                {
                    new SqlParameter("@ID_SP", maLoMoi),
                    new SqlParameter("@MaHang", maHang),
                    new SqlParameter("@SoLuong", soLuong)
                };

                DatabaseHelper.ExecuteNonQuery(sqlInsertLoMoi, paramInsert);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}