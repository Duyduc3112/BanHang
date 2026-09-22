using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrXacNhanXuatKho : Form
    {
        private readonly DataRow targetRow;

        public FrXacNhanXuatKho(DataRow row)
        {
            InitializeComponent();
            targetRow = row;
        }

        private bool KiemTraQuyenQuanLyHoacAdmin(string tenChucNang)
        {
            string chucVu = UserSession.ChucVu ?? "";
            bool isToanQuyen = chucVu.Equals("Quản lý kho", StringComparison.OrdinalIgnoreCase) ||
                               chucVu.Equals("Quản lý", StringComparison.OrdinalIgnoreCase) ||
                               chucVu.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                               chucVu.Equals("Quản trị viên", StringComparison.OrdinalIgnoreCase);

            if (!isToanQuyen)
            {
                MessageBox.Show($"Tài khoản vai trò ({chucVu}) không có quyền thực hiện [{tenChucNang}]!",
                                "Từ chối truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void FrXacNhanXuatKho_Load(object sender, EventArgs e)
        {
            if (!KiemTraQuyenQuanLyHoacAdmin("Xác nhận xuất kho"))
            {
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            if (targetRow != null)
            {
                string maPX = targetRow.Table.Columns.Contains("MaPhieuXuat") ? targetRow["MaPhieuXuat"]?.ToString() :
                              targetRow.Table.Columns.Contains("maphieuxuat") ? targetRow["maphieuxuat"]?.ToString() : "";
                maPX = maPX?.Trim() ?? "";

                string trangThai = targetRow.Table.Columns.Contains("TrangThai") ? targetRow["TrangThai"]?.ToString() :
                                   targetRow.Table.Columns.Contains("trangthai") ? targetRow["trangthai"]?.ToString() : "";
                trangThai = trangThai?.Trim() ?? "";

                lblPhieu.Text = $"Mã phiếu xuất: {maPX}\nTrạng thái hiện tại: {trangThai}";

                if (trangThai.Equals("Hoàn tất xuất", StringComparison.OrdinalIgnoreCase) ||
                    trangThai.Equals("Đã xuất kho", StringComparison.OrdinalIgnoreCase) ||
                    trangThai.Equals("Đã hủy", StringComparison.OrdinalIgnoreCase))
                {
                    lblPhieu.ForeColor = Color.Red;
                    lblPhieu.Text += $"\n❌ Phiếu đã ở trạng thái '{trangThai}', không thể xuất lại!";
                    btnXacNhanHoanThanh.Enabled = false;
                    btnXacNhanHoanThanh.BackColor = Color.Gray;
                }
                else
                {
                    lblPhieu.ForeColor = Color.Black;
                    btnXacNhanHoanThanh.Enabled = true;
                    btnXacNhanHoanThanh.BackColor = Color.ForestGreen;
                }
            }
        }

        private void btnXacNhanHoanThanh_Click(object sender, EventArgs e)
        {
            if (!KiemTraQuyenQuanLyHoacAdmin("Xác nhận xuất kho")) return;
            if (targetRow == null) return;

            string maPX = targetRow.Table.Columns.Contains("MaPhieuXuat") ? targetRow["MaPhieuXuat"]?.ToString() :
                          targetRow.Table.Columns.Contains("maphieuxuat") ? targetRow["maphieuxuat"]?.ToString() : "";
            maPX = maPX?.Trim() ?? "";

            if (string.IsNullOrEmpty(maPX))
            {
                MessageBox.Show("Không tìm thấy mã phiếu xuất kho!", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                bool hasDeducted = false;

                // -------------------------------------------------------------
                // LUỒNG 1: Đọc chi tiết lấy hàng từ chitietlayhang
                // -------------------------------------------------------------
                string sqlGetItems = @"
                    SELECT 
                        LTRIM(RTRIM(ctlh.maton)) AS maton, 
                        ISNULL(NULLIF(ctlh.soluongthuclay, 0), ISNULL(ctlh.soluongcanlay, 0)) AS soluongxuat
                    FROM chitietlayhang ctlh
                    INNER JOIN danhsachlayhang dslh ON LTRIM(RTRIM(ctlh.madanhsach)) = LTRIM(RTRIM(dslh.madanhsach))
                    WHERE LTRIM(RTRIM(dslh.maphieuxuat)) = LTRIM(RTRIM(@MaPX))";

                DataTable dtItems = DatabaseHelper.ExecuteQuery(sqlGetItems, new SqlParameter[] { new SqlParameter("@MaPX", maPX) });

                if (dtItems != null && dtItems.Rows.Count > 0)
                {
                    foreach (DataRow item in dtItems.Rows)
                    {
                        string maTon = item["maton"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(maTon)) continue;

                        decimal soLuongXuat = Convert.ToDecimal(item["soluongxuat"]);
                        if (soLuongXuat <= 0) continue;

                        // Lấy thông tin Vị trí, Mã Lô, Mã Hàng từ tonkho
                        string sqlGetInfo = @"
                            SELECT LTRIM(RTRIM(tk.mavitri)) AS mavitri, LTRIM(RTRIM(tk.malo)) AS malo, LTRIM(RTRIM(lh.mahang)) AS mahang 
                            FROM tonkho tk
                            INNER JOIN lohang lh ON LTRIM(RTRIM(tk.malo)) = LTRIM(RTRIM(lh.malo))
                            WHERE LTRIM(RTRIM(tk.maton)) = LTRIM(RTRIM(@MaTon))";

                        DataTable dtInfo = DatabaseHelper.ExecuteQuery(sqlGetInfo, new SqlParameter[] { new SqlParameter("@MaTon", maTon) });

                        string maViTri = "";
                        string maLo = "";
                        string maHang = "";

                        if (dtInfo != null && dtInfo.Rows.Count > 0)
                        {
                            maViTri = dtInfo.Rows[0]["mavitri"]?.ToString()?.Trim() ?? "";
                            maLo = dtInfo.Rows[0]["malo"]?.ToString()?.Trim() ?? "";
                            maHang = dtInfo.Rows[0]["mahang"]?.ToString()?.Trim() ?? "";
                        }

                        // 1. Trừ tồn kho tại vị trí (bảng tonkho)
                        string sqlUpTon = @"
                            UPDATE tonkho
                            SET soluongton = CASE WHEN soluongton >= @SL THEN soluongton - @SL ELSE 0 END,
                                soluongkhadung = CASE WHEN soluongkhadung >= @SL THEN soluongkhadung - @SL ELSE 0 END
                            WHERE LTRIM(RTRIM(maton)) = LTRIM(RTRIM(@MaTon))";
                        DatabaseHelper.ExecuteNonQuery(sqlUpTon, new SqlParameter[] {
                            new SqlParameter("@SL", soLuongXuat),
                            new SqlParameter("@MaTon", maTon)
                        });

                        // 2. Trừ số lượng còn của Lô hàng (bảng lohang) & Đổi trạng thái nếu hết
                        if (!string.IsNullOrEmpty(maLo))
                        {
                            string sqlUpLo = @"
                                UPDATE lohang
                                SET soluongcon = CASE WHEN soluongcon >= @SL THEN soluongcon - @SL ELSE 0 END,
                                    trangthai = CASE WHEN (soluongcon - @SL) <= 0 THEN N'Hết hàng' ELSE trangthai END
                                WHERE LTRIM(RTRIM(malo)) = LTRIM(RTRIM(@MaLo))";
                            DatabaseHelper.ExecuteNonQuery(sqlUpLo, new SqlParameter[] {
                                new SqlParameter("@SL", soLuongXuat),
                                new SqlParameter("@MaLo", maLo)
                            });
                        }

                        // 3. Trừ tổng tồn kho hàng hóa (bảng hanghoa)
                        if (!string.IsNullOrEmpty(maHang))
                        {
                            string sqlUpHH = @"
                                UPDATE hanghoa
                                SET tonkho = CASE WHEN tonkho >= @SL THEN tonkho - @SL ELSE 0 END
                                WHERE LTRIM(RTRIM(mahang)) = LTRIM(RTRIM(@MaHang))";
                            DatabaseHelper.ExecuteNonQuery(sqlUpHH, new SqlParameter[] {
                                new SqlParameter("@SL", soLuongXuat),
                                new SqlParameter("@MaHang", maHang)
                            });
                        }

                        // 4. Giải phóng vị trí kệ thành 'Trống' nếu hết hàng
                        if (!string.IsNullOrEmpty(maViTri))
                        {
                            string sqlCheckVT = "SELECT ISNULL(SUM(soluongton), 0) FROM tonkho WHERE LTRIM(RTRIM(mavitri)) = LTRIM(RTRIM(@MaViTri))";
                            DataTable dtVT = DatabaseHelper.ExecuteQuery(sqlCheckVT, new SqlParameter[] { new SqlParameter("@MaViTri", maViTri) });
                            decimal totalTonVT = (dtVT != null && dtVT.Rows.Count > 0 && dtVT.Rows[0][0] != DBNull.Value) ? Convert.ToDecimal(dtVT.Rows[0][0]) : 0;

                            if (totalTonVT <= 0)
                            {
                                string sqlFreeVT = "UPDATE vitriluutru SET trangthai = N'Trống' WHERE LTRIM(RTRIM(mavitri)) = LTRIM(RTRIM(@MaViTri))";
                                DatabaseHelper.ExecuteNonQuery(sqlFreeVT, new SqlParameter[] { new SqlParameter("@MaViTri", maViTri) });
                            }
                        }

                        hasDeducted = true;
                    }
                }

                // -------------------------------------------------------------
                // LUỒNG 2: Nếu chitietlayhang trống, đọc trực tiếp từ chitietphieuxuat
                // -------------------------------------------------------------
                if (!hasDeducted)
                {
                    string sqlGetPXItems = @"
                        SELECT 
                            LTRIM(RTRIM(ctx.mahang)) AS mahang, 
                            ISNULL(ctx.soluongyeucau, 0) AS soluongxuat
                        FROM chitietphieuxuat ctx
                        WHERE LTRIM(RTRIM(ctx.maphieuxuat)) = LTRIM(RTRIM(@MaPX))";

                    DataTable dtPXItems = DatabaseHelper.ExecuteQuery(sqlGetPXItems, new SqlParameter[] { new SqlParameter("@MaPX", maPX) });

                    if (dtPXItems != null && dtPXItems.Rows.Count > 0)
                    {
                        foreach (DataRow pxRow in dtPXItems.Rows)
                        {
                            string maHang = pxRow["mahang"]?.ToString()?.Trim() ?? "";
                            if (string.IsNullOrEmpty(maHang)) continue;

                            decimal qtyNeeded = Convert.ToDecimal(pxRow["soluongxuat"]);
                            if (qtyNeeded <= 0) continue;

                            // Trừ trực tiếp bảng hanghoa
                            string sqlUpHHDirect = @"
                                UPDATE hanghoa
                                SET tonkho = CASE WHEN tonkho >= @SL THEN tonkho - @SL ELSE 0 END
                                WHERE LTRIM(RTRIM(mahang)) = LTRIM(RTRIM(@MaHang))";
                            DatabaseHelper.ExecuteNonQuery(sqlUpHHDirect, new SqlParameter[] {
                                new SqlParameter("@SL", qtyNeeded),
                                new SqlParameter("@MaHang", maHang)
                            });

                            // Tìm các Lô / Vị trí kệ theo FEFO (Hạn sử dụng tăng dần) để trừ
                            string sqlFindTon = @"
                                SELECT LTRIM(RTRIM(tk.maton)) AS maton, LTRIM(RTRIM(tk.malo)) AS malo, LTRIM(RTRIM(tk.mavitri)) AS mavitri, ISNULL(tk.soluongton, 0) AS soluongton
                                FROM tonkho tk
                                INNER JOIN lohang lh ON LTRIM(RTRIM(tk.malo)) = LTRIM(RTRIM(lh.malo))
                                WHERE LTRIM(RTRIM(lh.mahang)) = LTRIM(RTRIM(@MaHang)) AND ISNULL(tk.soluongton, 0) > 0
                                ORDER BY lh.hansudung ASC";

                            DataTable dtTonList = DatabaseHelper.ExecuteQuery(sqlFindTon, new SqlParameter[] { new SqlParameter("@MaHang", maHang) });

                            decimal remain = qtyNeeded;
                            if (dtTonList != null && dtTonList.Rows.Count > 0)
                            {
                                foreach (DataRow tonRow in dtTonList.Rows)
                                {
                                    if (remain <= 0) break;

                                    string maTon = tonRow["maton"]?.ToString()?.Trim() ?? "";
                                    string maLo = tonRow["malo"]?.ToString()?.Trim() ?? "";
                                    string maViTri = tonRow["mavitri"]?.ToString()?.Trim() ?? "";
                                    decimal curTon = Convert.ToDecimal(tonRow["soluongton"]);

                                    decimal deduct = Math.Min(remain, curTon);

                                    // Trừ bảng tonkho
                                    string sqlUpTK = @"
                                        UPDATE tonkho
                                        SET soluongton = CASE WHEN soluongton >= @SL THEN soluongton - @SL ELSE 0 END,
                                            soluongkhadung = CASE WHEN soluongkhadung >= @SL THEN soluongkhadung - @SL ELSE 0 END
                                        WHERE LTRIM(RTRIM(maton)) = LTRIM(RTRIM(@MaTon))";
                                    DatabaseHelper.ExecuteNonQuery(sqlUpTK, new SqlParameter[] {
                                        new SqlParameter("@SL", deduct),
                                        new SqlParameter("@MaTon", maTon)
                                    });

                                    // Trừ bảng lohang
                                    if (!string.IsNullOrEmpty(maLo))
                                    {
                                        string sqlUpLo = @"
                                            UPDATE lohang
                                            SET soluongcon = CASE WHEN soluongcon >= @SL THEN soluongcon - @SL ELSE 0 END,
                                                trangthai = CASE WHEN (soluongcon - @SL) <= 0 THEN N'Hết hàng' ELSE trangthai END
                                            WHERE LTRIM(RTRIM(malo)) = LTRIM(RTRIM(@MaLo))";
                                        DatabaseHelper.ExecuteNonQuery(sqlUpLo, new SqlParameter[] {
                                            new SqlParameter("@SL", deduct),
                                            new SqlParameter("@MaLo", maLo)
                                        });
                                    }

                                    // Kiểm tra giải phóng kệ
                                    if (!string.IsNullOrEmpty(maViTri))
                                    {
                                        string sqlCheckVT = "SELECT ISNULL(SUM(soluongton), 0) FROM tonkho WHERE LTRIM(RTRIM(mavitri)) = LTRIM(RTRIM(@MaViTri))";
                                        DataTable dtVT = DatabaseHelper.ExecuteQuery(sqlCheckVT, new SqlParameter[] { new SqlParameter("@MaViTri", maViTri) });
                                        if (dtVT != null && dtVT.Rows.Count > 0 && Convert.ToDecimal(dtVT.Rows[0][0]) <= 0)
                                        {
                                            string sqlFreeVT = "UPDATE vitriluutru SET trangthai = N'Trống' WHERE LTRIM(RTRIM(mavitri)) = LTRIM(RTRIM(@MaViTri))";
                                            DatabaseHelper.ExecuteNonQuery(sqlFreeVT, new SqlParameter[] { new SqlParameter("@MaViTri", maViTri) });
                                        }
                                    }

                                    remain -= deduct;
                                }
                            }
                        }
                    }
                }

                // Cập nhật trạng thái phiếu xuất (Đã sửa CURRENT_TIMESTAMP -> GETDATE())
                string sqlUpPX = "UPDATE phieuxuat SET trangthai = N'Hoàn tất xuất', ngayxacnhan = GETDATE() WHERE LTRIM(RTRIM(maphieuxuat)) = LTRIM(RTRIM(@MaPX))";
                DatabaseHelper.ExecuteNonQuery(sqlUpPX, new SqlParameter[] { new SqlParameter("@MaPX", maPX) });

                // Cập nhật trạng thái danh sách lấy hàng
                string sqlUpDS = "UPDATE danhsachlayhang SET trangthai = N'Đã hoàn thành' WHERE LTRIM(RTRIM(maphieuxuat)) = LTRIM(RTRIM(@MaPX))";
                DatabaseHelper.ExecuteNonQuery(sqlUpDS, new SqlParameter[] { new SqlParameter("@MaPX", maPX) });

                MessageBox.Show($"Xác nhận xuất kho phiếu [{maPX}] thành công!\n", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thực hiện trừ tồn kho: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}