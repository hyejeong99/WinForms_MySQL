using RobotCC;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotControlCenter
{
    public partial class PlantListManagement : Form
    {
        public PlantListManagement()
        {
            InitializeComponent();
        }

        private void PlantListManagement_Load(object sender, EventArgs e)
        {
            RefreshPlantDB();
        }

        private void RefreshPlantDB()
        {
            // 기존 목록을 보여줌
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand(@"select PlantNumber, PlantName, ContactPerson, ContactEmail, ContactInfo from " + TBL_NAME, con);
            DataTable dt = new DataTable();

            SqlDataReader sdr = cmd.ExecuteReader();
            dt.Load(sdr);
            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["PlantNumber"].HeaderText = "발전소 코드";
            dataGridView1.Columns["PlantNumber"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["PlantName"].HeaderText = "발전소 이름";
            dataGridView1.Columns["PlantName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["ContactPerson"].HeaderText = "담당자명";
            dataGridView1.Columns["ContactPerson"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["ContactEmail"].HeaderText = "담당자 이메일";
            dataGridView1.Columns["ContactEmail"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["ContactInfo"].HeaderText = "연락처 및 주소";
            dataGridView1.Columns["ContactInfo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

        }

        private int getCntPlantDB(string PlantNumber)
        {
            string TBL_NAME = "PlantList";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(@"select * from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", PlantNumber);
            DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            dt.Load(sdr);

            con.Close();
            // 동일 ID를 갖는 행 개수 반환 사실 0 또는 1
            return (dt.Rows.Count);

        }

        private void insertBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber;
            string TBL_NAME = "PlantList";

            PlantNumber = plantNumber.Text.Trim();
            if (PlantNumber.Length < 5) // 너무 짧은 것은 무시
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            if (getCntPlantDB(PlantNumber) > 0) // 이미 존재
            {
                MessageBox.Show("입력한 발전소 코드가 이미 존재합니다.", "DB 입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("insert into " + TBL_NAME + " values (@PlantNumber, @PlantName, @AgencyCode, @ContactPerson, @ContactEmail, @ContactInfo)", con);
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber.Text);
            cmd.Parameters.AddWithValue("@PlantName", plantName.Text.Trim());
            cmd.Parameters.AddWithValue("@AgencyCode", "");
            cmd.Parameters.AddWithValue("@ContactPerson", contactPerson.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactEmail", contactEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactInfo", contactInfo.Text.Trim());
            string PlantName = plantName.Text.Trim();
            string ContactPerson = contactPerson.Text.Trim();
            string ContactEmail = contactEmail.Text.Trim();
            string ContactInfo = contactInfo.Text.Trim();

            try
            {
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB추가오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            con.Close();

            RefreshPlantDB();

            string msg = @"새로운 발전소가 신규 등록되었습니다." + Environment.NewLine + Environment.NewLine +
                "# 발전소 코드 : " + PlantNumber + Environment.NewLine +
                "# 발전소 이름 : " + PlantName + Environment.NewLine +
                "# 담당자명 : " + ContactPerson + Environment.NewLine +
                "# 담당자 이메일 : " + ContactEmail + Environment.NewLine +
                "# 연락처, 주소 : " + ContactInfo;
            MessageBox.Show(msg, "DB등록완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber;
            string TBL_NAME = "PlantList";

            PlantNumber = plantNumber.Text.Trim();
            if (PlantNumber.Length < 5) // 너무 짧은 것은 무시
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(@"update " + TBL_NAME + " set PlantNumber = @PlantNumber, PlantName = @PlantName, ContactPerson = @ContactPerson, ContactEmail = @ContactEmail, ContactInfo = @ContactInfo where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber.Text);
            cmd.Parameters.AddWithValue("@PlantName", plantName.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactPerson", contactPerson.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactEmail", contactEmail.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactInfo", contactInfo.Text.Trim());
            string PlantName = plantName.Text.Trim();
            string ContactPerson = contactPerson.Text.Trim();
            string ContactEmail = contactEmail.Text.Trim();
            string ContactInfo = contactInfo.Text.Trim();

            try
            {
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB수정오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            con.Close();

            RefreshPlantDB();

            string msg = @"선택한 발전소 정보가 수정되었습니다." + Environment.NewLine + Environment.NewLine +
                "# 발전소 코드 : " + PlantNumber + Environment.NewLine +
                "# 발전소 이름 : " + PlantName + Environment.NewLine +
                "# 담당자명 : " + ContactPerson + Environment.NewLine +
                "# 담당자 이메일 : " + ContactEmail + Environment.NewLine +
                "# 연락처, 주소 : " + ContactInfo;
            MessageBox.Show(msg, "DB수정완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        string deletePlantName;

        private string getPlantName(string plantNumber)
        {
            string TBL_NAME = "PlantList";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select PlantNumber, PlantName from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber);
            SqlDataReader sdr = cmd.ExecuteReader();

            while (sdr.Read())
            {
                if (sdr.GetString(0).Equals(plantNumber))
                {
                    // 삭제할 발전소명 추출
                    deletePlantName = sdr.GetString(1);
                    break;
                }
            }
            con.Close();

            return deletePlantName;

        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber = plantNumber.Text.Trim();

            if (PlantNumber.Length < 5) // 너무 짧은 경우 확인
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            if (getCntPlantDB(PlantNumber) == 0) // 존재하지 않는 행
            {
                MessageBox.Show("해당되는 발전소 코드가 없습니다.", "DB 입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //// 지우기 전에 삭제할 발전소명을 먼저 읽어 저장하고, 다시 한번 확인한다. // 사용자 편의
            string PlantName = getPlantName(PlantNumber);

            string del_msg = @"다음 발전소를 DB에서 삭제할까요?" + Environment.NewLine + Environment.NewLine +
               "# 발전소 코드 : " + PlantNumber + Environment.NewLine +
               "# 발전소 이름 : " + PlantName;
            if (MessageBox.Show(del_msg, "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return; // 취소

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd2 = new SqlCommand("delete from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd2.Parameters.AddWithValue("@PlantNumber", PlantNumber);

            try
            {
                cmd2.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB삭제오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            con.Close();

            RefreshPlantDB();

            //string msg = @"선택한 발전소가 삭제되었습니다." + Environment.NewLine +
            //    "# 발전소 코드 : " + PlantNumber + Environment.NewLine +
            //    "# 발전소 이름 : " + PlantName;
            //MessageBox.Show(msg, "DB삭제완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // Clear all deleted info
            plantNumber.Clear();
            plantName.Clear();
            contactPerson.Clear();
            contactInfo.Clear();

        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            // 데이터 테이블의 순서가 정렬될 경우도 가정하여 코딩 작업 필요....
            plantNumber.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            plantName.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            contactPerson.Text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            contactEmail.Text = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            contactInfo.Text = dataGridView1.SelectedRows[0].Cells[4].Value.ToString();
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
