using RobotCC;
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
            SqlCommand cmd = new SqlCommand(@"select PlantId, PlantName, ContactPerson, ContactInfo from " + TBL_NAME, con);
            DataTable dt = new DataTable();

            SqlDataReader sdr = cmd.ExecuteReader();
            dt.Load(sdr);
            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["PlantId"].HeaderText = "빌전소 코드";
            dataGridView1.Columns["PlantId"].Width = 100;
            dataGridView1.Columns["PlantName"].HeaderText = "발전소 이름";
            dataGridView1.Columns["PlantName"].Width = 120;
            dataGridView1.Columns["ContactPerson"].HeaderText = "담당자명";
            dataGridView1.Columns["ContactPerson"].Width = 120;
            dataGridView1.Columns["ContactInfo"].HeaderText = "연락처";
            dataGridView1.Columns["ContactInfo"].Width = 150;

            // plantCnt = dt.Rows.Count;
            // if (plantId.Text == "") plantId.Text = "P" + string.Format("{0:D4}", (plantCnt + 1));
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

        private void insertBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber;
            string TBL_NAME = "PlantList";

            PlantNumber = plantId.Text.Trim();
            if (PlantNumber.Length < 5) // 너무 짧은 것은 무시
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            if (getCntPlantDB(PlantNumber) > 0) // 이미 존재
            {
                MessageBox.Show("발전소 코드가 이미 존재합니다.", "DB 입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("insert into " + TBL_NAME + " values (@PlantId, @PlantName, @ContactPerson, @ContactInfo)", con);
            cmd.Parameters.AddWithValue("@PlantId", plantId.Text);
            cmd.Parameters.AddWithValue("@PlantName", plantName.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactPerson", contactPerson.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactInfo", contactInfo.Text.Trim());
            string newPlantName = plantName.Text.Trim();

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

            string msg = @"새로운 발전소가 신규 등록되었습니다." + Environment.NewLine +
                "# 발전소 코드 : " + PlantNumber + Environment.NewLine +
                "# 발전소 이름 : " + newPlantName;
            MessageBox.Show(msg, "DB등록완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }


        private void updateBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber;
            string TBL_NAME = "PlantList";

            PlantNumber = plantId.Text.Trim();
            if (PlantNumber.Length < 5) // 너무 짧은 것은 무시
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(@"update " + TBL_NAME + " set PlantId = @PlantId, PlantName = @PlantName, ContactPerson = @ContactPerson, ContactInfo = @ContactInfo where PlantId = @PlantId", con);
            cmd.Parameters.AddWithValue("@PlantId", plantId.Text);
            cmd.Parameters.AddWithValue("@PlantName", plantName.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactPerson", contactPerson.Text.Trim());
            cmd.Parameters.AddWithValue("@ContactInfo", contactInfo.Text.Trim());
            string newPlantName = plantName.Text.Trim();

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

            string msg = "발전소(" + PlantNumber + ":" + newPlantName + ") 정보가 수정되었습니다.";
            MessageBox.Show(msg, "DB수정완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

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

        private void deleteBtn_Click(object sender, EventArgs e)
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

            string msg = @"선택한 발전소가 삭제되었습니다." + Environment.NewLine +
                "# 발전소 코드 : " + deletePlantNumber + Environment.NewLine +
                "# 발전소 이름 : " + deletePlantName;
            MessageBox.Show(msg, "DB삭제완료", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // Clear all deleted info
            plantId.Text = "";
            plantName.Text = "";
            contactPerson.Text = "";
            contactInfo.Text = "";

        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            //DataGridViewRow row = dataGridView1.SelectedRows[0];
            // 데이터 테이블의 순서가 정렬될 경우도 가정하여 코딩 작업 필요....
            //Console.WriteLine(dataGridView1.CurrentRow.Index);           
            plantId.Text = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            plantName.Text = dataGridView1.SelectedRows[0].Cells[1].Value.ToString();
            contactPerson.Text = dataGridView1.SelectedRows[0].Cells[2].Value.ToString();
            contactInfo.Text = dataGridView1.SelectedRows[0].Cells[3].Value.ToString();
            //plantId.Text = dataGridView1.SelectedRows.Count.ToString();
            //plantId.Text = Gdt.Rows[dataGridView1.CurrentRow.Index][0].ToString();
            //plantName.Text = Gdt.Rows[dataGridView1.CurrentRow.Index][1].ToString();
            //contactPerson.Text = Gdt.Rows[dataGridView1.CurrentRow.Index][2].ToString();
            //contactInfo.Text = Gdt.Rows[dataGridView1.CurrentRow.Index][3].ToString();
            //plantId.Text = Gdt.Rows[e.RowIndex][0].ToString();
            //plantName.Text = Gdt.Rows[e.RowIndex][1].ToString();
            //contactPerson.Text = Gdt.Rows[e.RowIndex][2].ToString();
            //contactInfo.Text = Gdt.Rows[e.RowIndex][3].ToString();
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
