using System;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class OptionSetting : Form
    {
        int robotIndex; // 현재 처리중인 로봇 번호(인덱스)

        public OptionSetting()
        {
            InitializeComponent();
        }

        private void OptionFormLoad(object sender, EventArgs e)
        {
            robotIndex = G.CurrentRobotNumer; // 옵션 적용할 로봇 인덱스

            // 기존 설정 값들을 보여줌
            // [1] 로봇명
            robotName.Text = G.robotID[robotIndex];

            // [2] 기존 입럭 값을 보여줌
            LSize.Text = G.LSize[robotIndex].ToString();
            RSize.Text = G.RSize[robotIndex].ToString();

            //// [3] 자동 시작 설정 값을 보여줌
            //if (G.AUTOSTART[robotIndex] == G.AUTO_ON) checkBox1.Checked = true;
            //else checkBox1.Checked = false;

        }

        private void buttonSave_Click(object sender, EventArgs e)  // 저장
        {

            // 숫자가 아닌 입력의 경우, 오류 발생
            LSize.Text = LSize.Text.Trim();
            RSize.Text = RSize.Text.Trim();

            double lsize, rsize;

            if (double.TryParse(LSize.Text.ToString(), out lsize) == false)
            {
                MessageBox.Show("가로 입력 값이 올바르지 않습니다.(숫자만 허용)", "입력값 오류");
            }
            else if (double.TryParse(RSize.Text.ToString(), out rsize) == false)
            {
                MessageBox.Show("세로 입력 값이 올바르지 않습니다.(숫자만 허용)", "입력값 오류");
            }
            else
            {
                G.LSize[robotIndex] = lsize;
                G.RSize[robotIndex] = rsize;
                Console.Write("LSize = " + G.LSize[robotIndex]);
                Console.Write("RSize = " + G.RSize[robotIndex]);

                // AUTOSTART 등 기타 정보는 고칠떄마다 설정 파일 자동 변경
                G.CNFSaveFile(); // 고칠 때 마다 저장
                this.Close();
            }

        }

        private bool isValid(string expression)
        {
            int dot_cnt = 0; // 소수점 개수

            foreach (var ch in expression)
            {
                if (ch >= '0' && ch <= '9') continue; // 숫자는 OK
                else if (ch == '.') // 소수점은 하나만 가능
                {
                    dot_cnt++;
                    continue;
                }
                else return false; // 빈칸 및 다른 부호
            }
            if (dot_cnt < 2) return true; // 소수점 2개 방지
            else return false;
        }

        private void buttonCancel_Click(object sender, EventArgs e) //취소
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if (checkBox1.Checked == true)
            //    G.AUTOSTART[G.CurrentRobotNumer] = G.AUTO_ON;
            //else G.AUTOSTART[G.CurrentRobotNumer] = G.AUTO_OFF;
        }

        private void LSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' || e.KeyChar == '\b') return;
            else if (e.KeyChar >= '0' && e.KeyChar <= '9') return;
            else e.Handled = true; // 입력 제외
        }

        private void RSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' || e.KeyChar == '\b') return;
            else if (e.KeyChar >= '0' && e.KeyChar <= '9') return;
            else e.Handled = true; // 입력 제외
        }

    }
}
