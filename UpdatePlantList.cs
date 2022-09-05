using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class UpdatePlantList : Form
    {
        //DataTable Gdt = null;

        public UpdatePlantList()
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
            //// clear old inputs
            //plantId.Text = "";
            //plantName.Text = "";
            //contactPerson.Text = "";
            //contactInfo.Text = "";

        }

        private void updatePlantBtn_Click(object sender, EventArgs e)
        {
            string PlantNumber;
            string TBL_NAME = "PlantList";

            PlantNumber = plantId.Text.Trim();
            if (PlantNumber.Length < 5) // 너무 짧은 것은 무시
            {
                MessageBox.Show("입력한 발전소 코드를 확인하세요.");
                return;
            }

            //if (getCntPlantDB(PlantNumber) > 0) // 이미 존재  -- 당연히 존재
            //{
            //    MessageBox.Show("발전소 코드가 이미 존재합니다.", "DB 입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return;
            //}

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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
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
