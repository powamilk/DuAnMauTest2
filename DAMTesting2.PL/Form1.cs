﻿using DAMTesting2.BUS.Implement;
using DAMTesting2.BUS.Utils;
using DAMTesting2.BUS.Utils.Status;
using DAMTesting2.BUS.ViewModel;
using DAMTesting2.BUS.Interface;

namespace DAMTesting2.PL
{
    public partial class Form1 : Form
    {
        List<PhimVM> _phims;
        IPhimService _phimService;
        int _maPhimChon;

        public Form1()
        {
            _phimService = new PhimServices();
            InitializeComponent();
            LoadTheLoaiCheckBoxes();
            LoadDataForm();
            LoadGridData();
        }

        private void LoadDataForm()
        {
            dgv_phim.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv_phim.Columns.Clear();
            dgv_phim.Columns.Add("clm1", "STT");
            dgv_phim.Columns.Add("clm2", "Tên Phim");
            dgv_phim.Columns.Add("clm3", "Thời Gian Phát Hành");
            dgv_phim.Columns.Add("clm4", "Đạo Diễn");
            dgv_phim.Columns.Add("clm5", "Thời Lượng");
            dgv_phim.Columns.Add("clm8", "Mô Tả");
            dgv_phim.Columns.Add("clm6", "Trạng Thái");
            dgv_phim.Columns.Add("clm7", "Thể Loại");


            cb_trangthai.Items.Clear();
            cb_trangthai.Items.Add(StatusEnum.DangPhatHanh.GetStatusName());
            cb_trangthai.Items.Add(StatusEnum.TamDung.GetStatusName());
            cb_trangthai.Items.Add(StatusEnum.DaNgungPhatHanh.GetStatusName());

            cb_trangthai.SelectedIndex = 0;


        }

        private void LoadGridData()
        {
            _phims = _phimService.GetList();
            dgv_phim.Rows.Clear();

            foreach (var phim in _phims)
            {
                string trangThai = GetStatusName(phim.TrangThai ?? 0);

                string theLoais = string.Join(", ", phim.TheLoais);

                dgv_phim.Rows.Add(
                    (_phims.IndexOf(phim) + 1),
                    phim.TenPhim,
                    phim.ThoiGianPhatHanh?.ToShortDateString(),
                    phim.DaoDien,
                    phim.ThoiLuong,
                    phim.MoTa,
                    trangThai,
                    theLoais
                );
            }
        }


        private void LoadTheLoaiCheckBoxes()
        {
            var phimService = new PhimServices();
            var theLoaiList = phimService.GetAllTheLoais();

            foreach (var theLoai in theLoaiList)
            {
                CheckBox checkBox = new CheckBox
                {
                    Text = theLoai.TenTheLoai,
                    Tag = theLoai.TheLoaiId, 
                    AutoSize = true
                };
                flowLayoutPanelTheLoais.Controls.Add(checkBox);
            }
        }

        private string GetStatusName(int trangThai)
        {
            switch (trangThai)
            {
                case 1:
                    return "Đang Phát Hành";
                case 2:
                    return "Tạm Dừng";
                case 3:
                    return "Đã Ngừng Phát Hành";
                default:
                    return "Không xác định";
            }

        }

        private List<int> GetSelectedTheLoaiIds()
        {
            List<int> selectedIds = new List<int>();

            foreach (CheckBox checkBox in flowLayoutPanelTheLoais.Controls)
            {
                if (checkBox.Checked)
                {
                    selectedIds.Add((int)checkBox.Tag);
                }
            }

            return selectedIds;
        }


        private void btn_them_Click(object sender, EventArgs e)
        {
            bool isThoiLuongValid = int.TryParse(txt_thoiluong.Text.Trim(), out int thoiLuong);
            var trangThaiString = cb_trangthai.SelectedItem?.ToString();
            int trangThai = -1;

            if (!string.IsNullOrEmpty(trangThaiString))
            {
                if (trangThaiString.Contains("Đang Phát Hành")) trangThai = (int)StatusEnum.DangPhatHanh;
                else if (trangThaiString.Contains("Tạm Dừng")) trangThai = (int)StatusEnum.TamDung;
                else if (trangThaiString.Contains("Đã Ngừng Phát Hành")) trangThai = (int)StatusEnum.DaNgungPhatHanh;
            }
            if (!isThoiLuongValid || thoiLuong <= 0 || trangThai <= 0)
            {
                MessageBox.Show("Các trường số phải là số nguyên dương.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            bool isThoiGianValid = DateTime.TryParseExact(txt_thoigian.Text.Trim(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime thoiGianPhatHanh);
            if (!isThoiGianValid)
            {
                MessageBox.Show("Thời gian phát hành không hợp lệ. Vui lòng nhập theo định dạng dd/MM/yyyy.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("Bạn có chắc chắn muốn thêm phim này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var selectedTheLoaiIds = GetSelectedTheLoaiIds();

                var phimCreate = new PhimCreateVM
                {
                    TenPhim = txt_ten.Text.Trim(),
                    ThoiGianPhatHanh = DateOnly.FromDateTime(thoiGianPhatHanh),
                    DaoDien = txt_daodien.Text.Trim(),
                    ThoiLuong = thoiLuong,
                    TrangThai = trangThai,
                    MoTa = txt_mota.Text.Trim()
                };

                foreach (var theLoaiId in selectedTheLoaiIds)
                {
                    var theLoai = _phimService.GetTheLoaiById(theLoaiId);
                    if (theLoai != null)
                    {
                        phimCreate.TheLoais.Add(theLoai);
                    }
                }

                var result = _phimService.Create(phimCreate);
                MessageBox.Show(result, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGridData();
            }
        }

        private void btn_sua_Click(object sender, EventArgs e)
        {
            if (_maPhimChon == 0)
            {
                MessageBox.Show("Vui lòng chọn phim cần chỉnh sửa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool isThoiLuongValid = int.TryParse(txt_thoiluong.Text, out int thoiLuong);
            bool isTrangThaiValid = int.TryParse(cb_trangthai.Text, out int trangThai);

            if (!isThoiLuongValid || thoiLuong <= 0 || !isTrangThaiValid || trangThai <= 0)
            {
                MessageBox.Show("Các trường số phải là số nguyên dương.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var selectedTheLoaiIds = GetSelectedTheLoaiIds();

            var phimUpdate = new PhimUpdateVM
            {
                PhimId = _maPhimChon,
                TenPhim = txt_ten.Text,
                ThoiGianPhatHanh = DateOnly.FromDateTime(DateTime.Parse(txt_thoigian.Text)),
                DaoDien = txt_daodien.Text,
                ThoiLuong = thoiLuong,
                TrangThai = trangThai,
                MoTa = txt_mota.Text,
                TheLoaiIds = selectedTheLoaiIds
            };

            var result = _phimService.Update(phimUpdate);
            string msg = result ? "Chỉnh sửa thành công" : "Chỉnh sửa thất bại";
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadGridData();
        }

        private void btn_xoa_Click(object sender, EventArgs e)
        {
            if (_maPhimChon == 0)
            {
                MessageBox.Show("Vui lòng chọn phim cần xóa.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa phim này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var result = _phimService.Delete(_maPhimChon);
                string msg = result ? "Xóa thành công" : "Xóa thất bại";
                MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGridData();
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            txt_ten.Clear();
            txt_thoigian.Clear();
            txt_daodien.Clear();
            txt_thoiluong.Clear();
            txt_mota.Clear();
            cb_trangthai.SelectedIndex = -1;

            foreach (CheckBox checkBox in flowLayoutPanelTheLoais.Controls)
            {
                checkBox.Checked = false;
            }
        }

        private void dgv_phim_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var index = e.RowIndex;

            if (index < 0 || index >= _phims.Count)
            {
                _maPhimChon = 0;
                return;
            }

            var phimChon = _phims.ElementAt(index);

            _maPhimChon = phimChon.PhimId;
            txt_ten.Text = phimChon.TenPhim;
            txt_thoigian.Text = phimChon.ThoiGianPhatHanh?.ToShortDateString();
            txt_daodien.Text = phimChon.DaoDien;
            txt_thoiluong.Text = phimChon.ThoiLuong.ToString();
            txt_mota.Text = phimChon.MoTa;
            cb_trangthai.Text = phimChon.TrangThai.ToString();

            foreach (CheckBox checkBox in flowLayoutPanelTheLoais.Controls)
            {
                checkBox.Checked = phimChon.TheLoais.Contains(checkBox.Text);
            }
        }

        private void txt_timkiem_TextChanged(object sender, EventArgs e)
        {
            _phims = _phimService.Search(txt_timkiem.Text);
            LoadGridData();
        }


    }
}
