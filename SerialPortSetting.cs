using System;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class SerialPortSetting : Form
    {
        private string selectedPortName = G.SelectedSerialPortName;

        public SerialPortSetting()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            // 변경 여부 판단을 위해 작업전 이전 값을 저장
            G.oldSelectedSerialPortName = G.SelectedSerialPortName;

            cBoxPort.Items.Clear();
            cBoxPort.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());

            for (int i = 0; i < cBoxPort.Items.Count; i++)
                if (cBoxPort.Items[i].Equals(selectedPortName))
                {
                    cBoxPort.SelectedIndex = i;
                    break;
                }
        }

        private void cBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPortName = cBoxPort.SelectedItem.ToString();

            Console.WriteLine("CBOX : " + selectedPortName + " selected");
        }

        private void saveAndExitBtn_Click(object sender, EventArgs e)
        {
            // 통신 포트 선택 내용을 반영
            G.SelectedSerialPortName = selectedPortName;

            // 이 경우에도 전체 설정 저장 - 시리얼 포트 변경 등
            G.CNFSaveFile();

            // 단순 종료
            this.Close();
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
