using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Entity;

namespace De01
{
    public partial class frmSinhvien : Form
    {
        private string connectionString = "your_connection_string_here";
        public frmSinhvien()
        {
            InitializeComponent();
        }
        private void FrmSinhvien_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigureDataGridView();

                // Load student data into DataGridView
                LoadStudentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadStudentData()
        {
            using (var context = new StudentModel())
            {
                // Lấy danh sách sinh viên từ cơ sở dữ liệu với thông tin khoa
                List<Sinhvien> listSinhviens = context.Sinhviens
                    .Include(s => s.Lop) // Sử dụng Include để lấy thông tin Faculty
                    .ToList();

                // Gán dữ liệu vào DataGridView
                BindGrid(listSinhviens);
            }
        }
        private void BindGrid(List<Sinhvien> listSinhviens)
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = null; // Xóa nguồn dữ liệu hiện tại
            dataGridView1.DataSource = listSinhviens; // Gán danh sách sinh viên vào DataGridView
        }
        private void ConfigureDataGridView()
        {
            dataGridView1.Columns.Clear();

            // Cột StudentID
            DataGridViewTextBoxColumn colStudentID = new DataGridViewTextBoxColumn();
            colStudentID.Name = "StudentID";
            colStudentID.HeaderText = "MSSV";
            colStudentID.DataPropertyName = "StudentID";
            dataGridView1.Columns.Add(colStudentID);

            // Cột FullName
            DataGridViewTextBoxColumn colFullName = new DataGridViewTextBoxColumn();
            colFullName.Name = "FullName";
            colFullName.HeaderText = "Họ Tên";
            colFullName.DataPropertyName = "FullName";
            dataGridView1.Columns.Add(colFullName);

            // Cột Faculty (hiển thị FacultyName)
            DataGridViewTextBoxColumn colFaculty = new DataGridViewTextBoxColumn();
            colFaculty.Name = "FacultyName";
            colFaculty.HeaderText = "Khoa";
            colFaculty.DataPropertyName = "Faculty.FacultyName"; // Sử dụng FacultyName từ đối tượng Faculty
            dataGridView1.Columns.Add(colFaculty);

            // Cột AverageScore
            DataGridViewTextBoxColumn colAverageScore = new DataGridViewTextBoxColumn();
            colAverageScore.Name = "AverageScore";
            colAverageScore.HeaderText = "Điểm TB";
            colAverageScore.DataPropertyName = "AverageScore";
            dataGridView1.Columns.Add(colAverageScore);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                // Gán giá trị của sinh viên vào các control nhập liệu
                txtMaSV.Text = dataGridView1.CurrentRow.Cells["MaSV"].Value.ToString();
                txtHotenSV.Text = dataGridView1.CurrentRow.Cells["HotenSV"].Value.ToString();
                dtNgaysinh.Value = Convert.ToDateTime(dataGridView1.CurrentRow.Cells["Ngaysinh"].Value);
                cboLop.Text = dataGridView1.CurrentRow.Cells["Lop"].Value.ToString();
            }


        }

        private void btThem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var context = new StudentModel())
                {
                    Lop selectedLop = cboLop.SelectedItem as Lop;
                    // Tạo đối tượng sinh viên mới
                    Sinhvien newSinhvien = new Sinhvien
                    {
                        MaSV = txtMaSV.Text,
                        HotenSV = txtHotenSV.Text,
                        NgaySinh = dtNgaysinh.Value,
                        Lop = selectedLop
                    };

                    // Thêm sinh viên vào database
                    context.Sinhviens.Add(newSinhvien);
                    context.SaveChanges();

                    // Refresh DataGridView
                    LoadStudentData();
                    MessageBox.Show("Thêm sinh viên thành công!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btSua_Click(object sender, EventArgs e)
        {
            try
            {
                using (var context = new StudentModel())
                {
                    // Lấy mã sinh viên từ ô nhập
                    string maSV = txtMaSV.Text;

                    // Tìm sinh viên trong cơ sở dữ liệu
                    var existingSinhvien = context.Sinhviens.FirstOrDefault(sv => sv.MaSV == maSV);
                    if (existingSinhvien == null)
                    {
                        MessageBox.Show("Không tìm thấy sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Cập nhật thông tin sinh viên
                    existingSinhvien.HotenSV = txtHotenSV.Text;
                    existingSinhvien.NgaySinh = dtNgaysinh.Value;

                    // Lấy lớp được chọn
                    Lop selectedLop = cboLop.SelectedItem as Lop;
                    if (selectedLop == null)
                    {
                        MessageBox.Show("Vui lòng chọn lớp!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    existingSinhvien.Lop = selectedLop;

                    // Lưu thay đổi
                    context.SaveChanges();

                    // Refresh DataGridView
                    LoadStudentData();
                    MessageBox.Show("Sửa thông tin sinh viên thành công!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btXoa_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                return; // Người dùng chọn không xóa
            }

            try
            {
                using (var context = new StudentModel())
                {
                    // Lấy mã sinh viên từ ô nhập
                    string maSV = txtMaSV.Text;

                    // Tìm sinh viên trong cơ sở dữ liệu
                    var sinhvienToDelete = context.Sinhviens.FirstOrDefault(sv => sv.MaSV == maSV);
                    if (sinhvienToDelete == null)
                    {
                        MessageBox.Show("Không tìm thấy sinh viên để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Xóa sinh viên
                    context.Sinhviens.Remove(sinhvienToDelete);
                    context.SaveChanges();

                    // Refresh DataGridView
                    LoadStudentData();
                    MessageBox.Show("Xóa sinh viên thành công!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Bạn có chắc chắn muốn thoát?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit(); // Thoát ứng dụng
            }
        }
    }
}
