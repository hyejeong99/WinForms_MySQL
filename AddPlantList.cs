using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotCC
{

    public partial class AddPlantList : Form
    {
        private int plantCnt;

        public AddPlantList()
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

        private void insertPlantBtn_Click(object sender, EventArgs e)
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

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
