using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class ReportForm : Form
    {
        private string CurrentPlantNumber = null;
        private string CurrentPlantName = null;
        private string CurrentContactEmail = "없음";

        public ReportForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LinkComboBoxPlantList();
        }

        private void LinkComboBoxPlantList()
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand(@"select PlantNumber, PlantName, ContactPerson, ContactEmail, ContactInfo from " + TBL_NAME, con);
            //DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            comboBox1.Items.Clear();
            while (sdr.Read())
            {
                comboBox1.Items.Add(sdr.GetString(0) + " : " + sdr.GetString(1));
                CurrentPlantNumber = sdr.GetString(0);
                CurrentPlantName = sdr.GetString(1);
                CurrentContactEmail = sdr.GetString(3);
            }
            //dt.Load(sdr);
            con.Close();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            emailTBox.Text = CurrentContactEmail.ToString();
        }

        private string getEmailAddress(string plantNumber)
        {
            string TBL_NAME = "PlantList";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select PlantNumber, ContactEmail  from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber);
            SqlDataReader sdr = cmd.ExecuteReader();

            while (sdr.Read())
            {
                if (sdr.GetString(0).Equals(plantNumber))
                {
                    //CurrentPlantName = sdr.GetString(0);
                    CurrentContactEmail = sdr.GetString(1);
                    break;
                }
            }
            con.Close();

            return CurrentContactEmail;
        }

        private void printBtn_Click(object sender, EventArgs e)  // 보고서 인쇄
        {
            MessageBox.Show("보고서를 인쇄합니다.", "인쇄", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
        }

        private void emailBtn_Click(object sender, EventArgs e) //  보고서 이메일 발송
        {
            MessageBox.Show("보고서 내용을 이메일 주소 " + CurrentContactEmail + "로 발송합니다.", "이메일 발송", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] plantinfo = comboBox1.Text.ToString().Split(new string[] { " : " }, StringSplitOptions.None);
            CurrentPlantNumber = plantinfo[0];
            CurrentPlantName = plantinfo[1];
            // PlantNumber에 해당하는 CurrentContactEmail 값을 설정
            CurrentContactEmail = getEmailAddress(CurrentPlantNumber);
            emailTBox.Text = CurrentContactEmail.ToString();

            if (G.DEBUG) Console.WriteLine("<" + CurrentPlantNumber + ">,<" + CurrentPlantName + ">,<" + CurrentContactEmail + ">");
        }

        // 검색 테스트용으로 WorkLog 자료를 보여줌
        private void searchBtn_Click(object sender, EventArgs e)
        {
            string TBL_NAME = "WorkLog";

            DateTime From = dateTimeFrom.Value;
            DateTime To = dateTimeTo.Value;

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select RId, TimeStamp, PlantNumber, State, LSize, RSize, Counter, Etc  from " + TBL_NAME + " where PlantNumber = @PlantNumber AND @DT_From <= TimeStamp AND TimeStamp <= @DT_To", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@DT_From", From);
            cmd.Parameters.AddWithValue("@DT_To", To);

            SqlDataReader sdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();

            //while (sdr.Read())
            //{
            //    if (sdr.GetString(0).Equals(plantNumber))
            //    {
            //        //CurrentPlantName = sdr.GetString(1);
            //        CurrentContactEmail = sdr.GetString(1);
            //        break;
            //    }
            //}
            dt.Load(sdr);
            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["PlantNumber"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["TimeStamp"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["State"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["LSize"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["RSize"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["Counter"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["Etc"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }

        // 다른 버전
        private void searchBtn1_Click(object sender, EventArgs e)
        {
            string TBL_NAME = "ReportData";
            DateTime From = dateTimeFrom.Value;
            DateTime To = dateTimeTo.Value;

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select RId, Date, WorkingTime, WorkingAreas from " + TBL_NAME + " where PlantNumber = @PlantNumber AND @DT_From <= Date AND Date <= @DT_To", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@DT_From", From);
            cmd.Parameters.AddWithValue("@DT_To", To);

            SqlDataReader sdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();

            //while (sdr.Read())
            //{
            //    if (sdr.GetString(0).Equals(plantNumber))
            //    {
            //        //CurrentPlantName = sdr.GetString(1);
            //        CurrentContactEmail = sdr.GetString(1);
            //        break;
            //    }
            //}
            dt.Load(sdr);
            con.Close();

            dataGridView1.DataSource = dt;

        }
    }
}
