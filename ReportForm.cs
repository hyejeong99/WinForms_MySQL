using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class ReportForm : Form
    {
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

            // Another Try
            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand("select PlantNumber, PlantName from " + TBL_NAME, con);
            DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            comboBox1.Items.Clear();
            while (sdr.Read())
            {
                comboBox1.Items.Add(" " + sdr.GetString(0) + "  " + sdr.GetString(1));
            }
            //dt.Load(sdr);
            con.Close();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void printBtn_Click(object sender, EventArgs e)  // 보고서 인쇄
        {

        }

        private void emailBtn_Click(object sender, EventArgs e) //  보고서 이메일 발송
        {

        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            this.Close();

        }



    }
}
