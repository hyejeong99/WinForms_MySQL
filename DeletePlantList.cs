using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class DeletePlantList : Form
    {
        // DataTable Gdt = null;

        public DeletePlantList()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            RefreshPlantDB();
        }



        private void RefreshPlantDB()
        {
            // 기존 목록을 보여줌
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand(@"select PlantId, PlantName, ContactPerson, ContactInfo from " + TBL_NAME, con);
            DataTable dt = new DataTable(); //Gdt = dt;

            SqlDataReader sdr = cmd.ExecuteReader();
            dt.Load(sdr);
            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["PlantId"].HeaderText = "발전소 코드";
            dataGridView1.Columns["PlantId"].Width = 100;
            dataGridView1.Columns["PlantName"].HeaderText = "발전소 이름";
            dataGridView1.Columns["PlantName"].Width = 120;
            dataGridView1.Columns["ContactPerson"].HeaderText = "담당자명";
            dataGridView1.Columns["ContactPerson"].Width = 120;
            dataGridView1.Columns["ContactInfo"].HeaderText = "연락처";
            dataGridView1.Columns["ContactInfo"].Width = 150;

        }

        private void deletePlantBtn_Click(object sender, EventArgs e)
        {
            string deletePlantNumber = plantId.Text.Trim();

            if (deletePlantNumber.Length < 5) // 너무 짧은 경우 확인
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            if (getCntPlantDB(deletePlantNumber) == 0) // 존재하지 않는 행
            {
                MessageBox.Show("해당되는 발전소 코드가 없습니다.", "DB 입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //// 지우기 전에 삭제할 발전소명을 먼저 읽어 저장한다. // 사용자 편의
            string deletePlantName = getPlantName(deletePlantNumber);

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd2 = new SqlCommand("delete from " + TBL_NAME + " where PlantID = @PlantId", con);
            cmd2.Parameters.AddWithValue("@PlantId", deletePlantNumber);

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

            //string msg = "발전소(" + deletePlantNumber + ":" + deletePlantName + @")가 삭제되었습니다.";
            //DialogResult result = MessageBox.Show(msg + Environment.NewLine +"메인화면으로 돌아가시겠습니까?", "삭제완료", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
            //if(result == DialogResult.OK)
            //{
            //    this.Close();
            //}
            string msg = @"선택한 발전소가 삭제되었습니다." + Environment.NewLine +
                "# 발전소 코드 : " + deletePlantNumber + Environment.NewLine +
                "# 발전소 이름 : " + deletePlantName;
            MessageBox.Show(msg, "DB삭제완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        private int getCntPlantDB(string plantNumber)
        {
            string TBL_NAME = "PlantList";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(@"select * from " + TBL_NAME + " where PlantID = @PlantId", con);
            cmd.Parameters.AddWithValue("@PlantId", plantId.Text);
            DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            dt.Load(sdr);

            con.Close();
            // 동일 ID를 갖는 행 개수 반환 사실 0 또는 1
            return (dt.Rows.Count);

        }

        string deletePlantName;

        private string getPlantName(string plantNumber)
        {
            string TBL_NAME = "PlantList";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select PlantId, PlantName from " + TBL_NAME + " where PlantID = @PlantId", con);
            cmd.Parameters.AddWithValue("@PlantId", plantId.Text);
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

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            plantId.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            plantName.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            contactPerson.Text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            contactInfo.Text = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
        }
    }
}
