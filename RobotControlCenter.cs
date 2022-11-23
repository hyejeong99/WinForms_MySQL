using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class RobotControlCenter : Form
    {

        public RobotControlCenter()
        {
            InitializeComponent();

            // 출력 창 특성 설정
            output.Select(output.Text.Length, 0);
            output.ScrollToCaret();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            OutputMesssage(G.VERSIONNAME);
            OutputMesssage("[" + G.TimeStamp() + "] " + "SYSTEM START ");
        }

        /*
         * 수신 데이터가 있으면 무조건 List(RecvMesg)에 보관, 발신 주소는 같이 저장하여, CONF 발송 대응
         */
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }

        private void loRa연결확인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void 보고서작성ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReportForm form = new ReportForm();
            form.Show();
        }

        public void OutputMesssage(string line)
        {
            output.AppendText("[" + G.TimeStamp() + "] " + line + Environment.NewLine); // new line 추가

            output.Select(output.Text.Length, 0);// scroll 항상 아래
            output.ScrollToCaret();
            output.SelectionColor = G.DefaultColor; // 원상 복구

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
    }
}
