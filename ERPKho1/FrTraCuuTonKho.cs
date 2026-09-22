using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ERPKho1
{
    public partial class FrTraCuuTonKho : Form
    {
        public FrTraCuuTonKho()
        {
            InitializeComponent();
        }

        private void FrTraCuuTonKho_Load(object sender, EventArgs e)
        {
            LoadDuLieuTraCuu(""); 
        }

        private void LoadDuLieuTraCuu(string keyword)
        {
            try
            {
                string sql = @"
                    SELECT 
                        hh.MaHang AS [MaVatTu],
                        hh.TenHang AS [TenVatTu],
                        ISNULL(k.tenKho, N'Chưa phân bổ') AS [KhoLuuTru],
                        ISNULL(hh.TonKho, 0) AS [TonKho],
                        ISNULL(hh.DonViTinh, N'Kg') AS [DVT],
                        ISNULL(vt.maViTri, N'Chưa xếp kệ') AS [ViTri]
                    FROM HangHoa hh
                    LEFT JOIN LoHang lh ON hh.MaHang = lh.MaHang
                    LEFT JOIN TonKho tk ON lh.maLo = tk.maLo
                    LEFT JOIN ViTriLuuTru vt ON tk.maViTri = vt.maViTri
                    LEFT JOIN Kho k ON vt.maKho = k.maKho
                    WHERE hh.MaHang LIKE @Key OR hh.TenHang LIKE @Key OR k.tenKho LIKE @Key";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@Key", "%" + keyword + "%")
                };

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, parameters);
                dgvTraCuu.DataSource = dt; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tra cứu tồn kho: " + ex.Message, "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string keyword = txtKeyword.Text.Trim(); 
            LoadDuLieuTraCuu(keyword);
        }

        private void btnDong_Click(object sender, EventArgs e)
        {
            this.Close(); 
        }
    }
}