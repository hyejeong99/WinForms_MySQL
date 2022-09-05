using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RobotCC
{
    public partial class RobotControlCenter : Form
    {
        // 명령 코드 
        public const byte REGISTER_REQ = 0x41; // A : R->C
        public const byte REGISTER_CONF = 0x42; // B : C->R
        public const byte REPORT_REQ = 0x43; // C : R->C (NO CONFIRM)
        public const byte CONTROL_REQ = 0x44; // D : C->R
        public const byte CONTROL_CONF = 0x45; // E : R->C

        public const byte OPTION_CODE_STATUS = 0x46; // F : R->C
        public const byte OPTION_CODE_BATTERY = 0x47; // G : R->C

        // 수신 메시지 보관용 리스트
        List<string> recvMsgs = new List<string>();
        List<string> confMsgs = new List<string>();
        List<string> addrMsgs = new List<string>();
        List<string> sendMsgs = new List<string>();

        // 각종 component를 배열로 관리 처리하기 위한 선언
        //// Buttons : R/S/H/OPTION
        public Button[] Btn_RUN = new Button[G.ROBOT_CNT];
        public Button[] Btn_STOP = new Button[G.ROBOT_CNT];
        public Button[] Btn_HOME = new Button[G.ROBOT_CNT];
        public Button[] Btn_OPTION = new Button[G.ROBOT_CNT];

        // TEXT BOXes
        TextBox[] TBox_RobotName = new TextBox[G.ROBOT_CNT];
        RichTextBox[] TBox_Status = new RichTextBox[G.ROBOT_CNT];
        RichTextBox[] TBox_Battery = new RichTextBox[G.ROBOT_CNT];

        // PROGRESSBARs
        ProgressBar[] Progress = new ProgressBar[G.ROBOT_CNT];
        TextBox[] TBox_Progress = new TextBox[G.ROBOT_CNT];


        public RobotControlCenter()
        {
            InitializeComponent();

            // 초기화
            recvMsgs.Clear();
            addrMsgs.Clear();
            confMsgs.Clear();
            sendMsgs.Clear();

            // 출력 창 설정
            output.Select(output.Text.Length, 0);
            output.ScrollToCaret();

        }

        private void Form_Load(object sender, EventArgs e)
        {
            OutputMesssage(G.VERSIONNAME);
            OutputMesssage(DateTime.Now.ToString() + " : " + "SYSTEM START ");

            OutputMesssage("## [1] 시스템 폴더 검사 ##", Color.Blue);
            // System Folder 확인 및 생성
            G.CheckAndMakeFolder();

            OutputMesssage("## [2] DB 파일 검사 ##", Color.Blue);
            // DB File 검사
            CheckDBFile();

            OutputMesssage("## [3] 설정 파일 업로드 ##", Color.Blue);
            // 설정 파일 읽어 오기
            G.CNFLoadFile();

            // 저장된 로봇명을 보여줌
            robotName1.Text = G.robotID[0];
            robotName2.Text = G.robotID[1];
            robotName3.Text = G.robotID[2];
            robotName4.Text = G.robotID[3];
            robotName5.Text = G.robotID[4];
            // LSize, RSize, OT는 읽어 오되, 화면에 따로 보여주지는 않음 (세부설정시 보여줌)

            //통신 포트 검사
            OutputMesssage("## [4] 통신 포트 검사 ##", Color.Blue);
            CheckSerialPortAtStart();

            // LoRa 송수신 테스트
            OutputMesssage("## [5] LoRa 통신 테스트 ##", Color.Blue);
            CheckLoRaPortAsync(false); // 성공시에는 메시지 없음

            // COMBO BOX에 실제 DATA 연결
            LinkComboBoxPlantList();

            //여러개의 동일 컴포넌트를 한번에 처리하기 위한 작업 - 작동 잘되나, 버튼의 경우, 테이블 레이아웃인 경우 미작동 
            LinkArrayComponent();


            //////////////////// JUST FOR TEST : RICHTEXTBOX, COLOR
            if (G.DEBUG)
            {
                /// ProgressBar 색상 변경을 적용하려면, 메인 program.cs에서 아래 문장을 삭제해야 제대로 동작함
                // Application.EnableVisualStyles(); 윈도우즈 UI 표준화 작업
                // progressText5.SelectionColor = Color.Red;
                progressText5.ForeColor = Color.Red;
                //progressBar5.ForeColor = Color.Red;

                progressBar1.Value = 25; progressText1.Text = "25";
                progressBar2.Value = 75; progressText2.Text = "75";
                progressBar3.Value = 50; progressText3.Text = "50";
                progressBar3.Value = 100; progressText4.Text = "100";
                progressBar4.Value = 5; progressText5.Text = "50";
                //robotName1.ForeColor = Color.Red;
                status5.SelectionAlignment = HorizontalAlignment.Center;
                battery5.SelectionAlignment = HorizontalAlignment.Center;

            }
        }

        private void CheckSerialPortAtStart()
        {
            try
            {
                //통신 포트 관리
                serialPort1.PortName = G.SelectedSerialPortName;
                serialPort1.BaudRate = 115200;
                serialPort1.Parity = Parity.None; serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Open();
                Console.WriteLine("통신포트 : " + serialPort1.PortName + " 연결");
            }
            catch  // 포트 오류시 추가 시도 #1
            {
                MessageBox.Show(@"통신포트 : " + serialPort1.PortName + " 오류" + Environment.NewLine +
                    "올바른 통신포트를 선택하세요.", "통신포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                SerialPortSetting form = new SerialPortSetting();
                form.ShowDialog();

                try // try once more
                {
                    serialPort1.PortName = G.SelectedSerialPortName;
                    serialPort1.BaudRate = 115200;
                    serialPort1.Parity = Parity.None; serialPort1.DataBits = 8;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.Open();
                    Console.WriteLine("통신포트 : " + serialPort1.PortName + " 연결");
                }
                catch // 포트 오류시 추가 시도 #2
                {
                    MessageBox.Show(@"통신포트 : " + serialPort1.PortName + " 오류" + Environment.NewLine + "올바른 통신포트를 다시 선택하세요.",
                        "통신포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    SerialPortSetting form1 = new SerialPortSetting();
                    form1.ShowDialog();

                    try // try once more
                    {
                        serialPort1.PortName = G.SelectedSerialPortName;
                        serialPort1.BaudRate = 115200;
                        serialPort1.Parity = Parity.None; serialPort1.DataBits = 8;
                        serialPort1.StopBits = StopBits.One;
                        serialPort1.Open();
                        Console.WriteLine("통신포트 : " + serialPort1.PortName + " 연결");
                    }
                    catch // 세번째 
                    {
                        MessageBox.Show(@"통신포트 : " + serialPort1.PortName + " 오류" + Environment.NewLine + "올바른 통신포트를 선택하세요. 시스템을 다시 시작합니다.",
                            "통신 포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                    }
                }

            }
            if (serialPort1.IsOpen)
            {
                comInfo.ForeColor = Color.Black;
                comInfo.Text = serialPort1.PortName;
                OutputMesssage("통신포트 : " + serialPort1.PortName + " 연결");
            }

        }


        private async void CheckLoRaPortAsync(bool option)
        {
            if (!serialPort1.IsOpen) return;

            bool respResult = false;
            OutputMesssage(@"Lora 통신 포트 테스트 중 ......");

            // 선택된 통신 포트가 Lora 포트인지 점검
            string sendMesg = "AT+ADDRESS?";

            addrMsgs.Clear();
            serialPort1.Write(sendMesg + Environment.NewLine);

            await Task.Delay(G.LORA_TEST_DELAY);

            foreach (string s in addrMsgs)
            {
                //Console.WriteLine(s);
                if (!s.Contains("+ADDRESS=")) continue;
                string[] strings = s.Split('=');
                G.MyLoRaAddress = strings[1];
                respResult = true;
            }

            if (respResult)
            {
                comInfo.Text = serialPort1.PortName + "," + G.MyLoRaAddress; //comInfo.ForeColor = Color.Blue;

                string msg = "Lora 통신 테스트 성공.(" + serialPort1.PortName + ", 주소 = " + G.MyLoRaAddress + ")";
                OutputMesssage(msg);
                if (option)
                    MessageBox.Show(msg, "LoRa 통신 테스트", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                OutputMesssage("## 시스템 준비 완료 ##", Color.Blue);

            }
            else
            {
                comInfo.Text = "LoRa설정 오류 : " + serialPort1.PortName; comInfo.ForeColor = Color.Red;

                string msg = "Lora 통신 테스트 실패(" + serialPort1.PortName + ")";
                OutputMesssage(msg, Color.Red);

                DialogResult result = MessageBox.Show(msg + Environment.NewLine + "통신포트 설정화면으로 이동하시겠습니까?", "LoRa 통신 테스트", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    ChangeSerialPort();
                    //CheckLoRaPortAsync(false);
                }
            }

        }

        private void CheckDBFile()
        {

            FileInfo fi = new FileInfo(G.DBFileName);

            if (!fi.Exists)
            {
                MessageBox.Show("DB 파일이 없습니다.", "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
        }

        private void LinkArrayComponent()
        {
            // 각종 버튼의 제어를 배열로 관리하기 위한 작업   
            // 잘 동작하지만 **문제 발생 -- 테이블 안에 버튼 안들어감 - 실행시 위치 변경
            // [1] RUN 버튼 연결
            Btn_RUN[0] = runBtn1; Btn_RUN[1] = runBtn2; Btn_RUN[2] = runBtn3;
            Btn_RUN[3] = runBtn4; Btn_RUN[4] = runBtn5;

            //// [2] STOP 버튼 연결
            Btn_STOP[0] = stopBtn1; Btn_STOP[1] = stopBtn2; Btn_STOP[2] = stopBtn3;
            Btn_STOP[3] = stopBtn4; Btn_STOP[4] = stopBtn5;

            //// [3] HOME 버튼 연결
            Btn_HOME[0] = homeBtn1; Btn_HOME[1] = homeBtn2; Btn_HOME[2] = homeBtn3;
            Btn_HOME[3] = homeBtn4; Btn_HOME[4] = homeBtn5;

            ////// [4] OPTION 버튼 연결
            Btn_OPTION[0] = optionBtn1; Btn_OPTION[1] = optionBtn2; Btn_OPTION[2] = optionBtn3;
            Btn_OPTION[3] = optionBtn4; Btn_OPTION[4] = optionBtn5;

            ///// [5] 버튼 클릭 이벤트 공동 연결
            /// 디자인 파일에 직접 설정변경하면 디자인 화면 (경고) 오류 발생 ==> 여기서 연결
            for (int i = 0; i < G.ROBOT_CNT; i++)
            {
                int index = i; // 주의 : 반드시 내부 변수를 별도로 사용해야 잘 동작
                this.Btn_RUN[i].Click += (sender, ex) => this.runActionAsync(index);
                this.Btn_STOP[i].Click += (sender, ex) => this.stopActionAsync(index);
                this.Btn_HOME[i].Click += (sender, ex) => this.homeActionAsync(index);

                this.Btn_OPTION[i].Click += (sender, ex) => this.optionAction(index);
            }
            ///
            // 테이블 내 각 버튼 추가 - 하지않음.
            //this.tableLayoutPanel2.Controls.Add(this.Btn_OPTION[0], 9, 0);
            //this.tableLayoutPanel4.Controls.Add(this.Btn_OPTION[1], 9, 0);
            //this.tableLayoutPanel5.Controls.Add(this.Btn_OPTION[2], 9, 0);
            //this.tableLayoutPanel6.Controls.Add(this.Btn_OPTION[3], 9, 0);
            //this.tableLayoutPanel7.Controls.Add(this.Btn_OPTION[4], 9, 0);
            // 버튼 클릭 이벤트 처리

            //// [5] 로봇 이름
            TBox_RobotName[0] = robotName1; TBox_RobotName[1] = robotName2; TBox_RobotName[2] = robotName3;
            TBox_RobotName[3] = robotName4; TBox_RobotName[4] = robotName5;

            //// [6] 배터리 상태
            TBox_Battery[0] = battery1; TBox_Battery[1] = battery2; TBox_Battery[2] = battery3;
            TBox_Battery[3] = battery4; TBox_Battery[4] = battery5;

            //// [7] 로봇 상태 정보
            TBox_Status[0] = status1; TBox_Status[1] = status2; TBox_Status[2] = status3;
            TBox_Status[3] = status4; TBox_Status[4] = status5;

            //// [8] 진행률
            Progress[0] = progressBar1; Progress[1] = progressBar2; Progress[2] = progressBar3;
            Progress[3] = progressBar4; Progress[4] = progressBar5;

            //// [9] 진행률 숫자 정보
            TBox_Progress[0] = progressText1; TBox_Progress[1] = progressText2; TBox_Progress[2] = progressText3;
            TBox_Progress[3] = progressText4; TBox_Progress[4] = progressText5;

        }

        /*
         * 
         * 수신 데이터가 있으면 무조건 List(RecvMesg)에 보관, 발신 주소는 같이 저장하여, CONF 발송 대응
         * 
         */
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string receiveStr = serialPort1.ReadLine();

            receiveStr = receiveStr.Replace("\r", "");

            if (receiveStr == null) return;
            /////////////////  +ADDRESS 메시지 처리 ///////////////////
            else if (receiveStr.Contains("+ADDRESS=")) // LoRa 포트 주소 확인 전용
            {
                addrMsgs.Add(receiveStr);
            }
            /////////////////  +RCV 메시지 처리 ///////////////////
            else if (receiveStr.Contains("+RCV")) // +RECV 이외  +OK 등의 응답은 무시
            {
                recvMsgs.Add(receiveStr);  // 수신 목록에 저장

                BeginInvoke(new EventHandler(ManageRecvMessages)); // 아래와 동일하나, 비동기 강화 ?
                //this.Invoke(new EventHandler(ManageRecvMessages)); //
            }
            return;
        }

        /*
         * 수신된 메시지(Command) 종류에 따라 정해진 작업 및 CONFIRM 처리, 처리된 메시지는 리스트에서 삭제
         */
        private void ManageRecvMessages(object o, EventArgs e) // CMD 종류에 따라 처리 - WorkLog 업데이트 작업도 해야 함
        {
            ////////////////////////  수신 메시지 처리 ///////////////////////////
            for (int i = 0; i < recvMsgs.Count; i++)
            {
                try
                {
                    if (G.DEBUG) OutputMesssage($"< # {i} : {recvMsgs[i]} >" + " - " + " MESG");
                    if (G.DEBUG) Console.WriteLine($"< {recvMsgs[i]} >" + " : " + " MESG");

                    // 고정 길이 메시지를 가정하고 처리 방법 변경 2020.08.24
                    // 통신용 정보와 송수신 메시지 분리
                    int index = recvMsgs[i].IndexOf("=");
                    string[] parts = recvMsgs[i].Substring(index + 1).Split(',');

                    //// 순수 전달 메시지/명령 추출 및 byte[] 변환
                    ///// [1] 송신자 주소
                    int senderAddr = int.Parse(parts[0]);
                    int senderIndex = getRobotIndex(senderAddr);

                    //// [2] 전달된 메시지 크기
                    int dataSize = int.Parse(parts[1]);
                    if (dataSize == 0)
                    {
                        ErrorMessage(@"Null Packet Found - 제거");
                        return;
                    }

                    //// [3] 전달된 메시지 본문
                    string Cmd = parts[2];
                    byte[] buffs = Encoding.ASCII.GetBytes(Cmd);

                    if (dataSize != Cmd.Length)
                    {  // 전송 메시지 본문 길이 검사 - 실패시 ??? 
                        ErrorMessage(@"Packet Size 불일치 - 제거");
                        return;
                    }

                    ///// [4] 명령 코드 추출 및 코드별 작업 -- DB 업데이트 작업 추가 필요
                    byte cmdCode = buffs[0];


                    switch (cmdCode)
                    {
                        case REGISTER_REQ: /////////  로봇 등록 명령 처리 R->C

                            string robotName = Cmd.Substring(1, 9); // 사용 길이에 따라 재조정

                            //if (G.DEBUG) OutputMesssage($" < { recvMsgs[i]} >" + " : " + " REGISTER_REQ");
                            //if (G.DEBUG) OutputMesssage("ROBOT_NAME : " + robotName);
                            if (G.DEBUG) Console.WriteLine(senderAddr + " : " + Cmd + "  REGISTER_REQ");

                            ///////////////////////////////////
                            // [1] CONFIRM 메시지를 먼저 보내고
                            SendRegisterConfMsg(senderAddr);  // REGISTER_CONF 답신을 보낸다.

                            ///////////////////////////////
                            // [2] 로봇명을 추출하여, 관련 업데이트 작업 필요

                            G.robotID[senderIndex] = robotName;

                            ///메인화면 정보 업데이트 + DB 업데이트
                            if (senderIndex == 0) robotName1.Text = robotName;
                            else if (senderIndex == 1) robotName2.Text = robotName;
                            else if (senderIndex == 2) robotName3.Text = robotName;
                            else if (senderIndex == 3) robotName4.Text = robotName;
                            else if (senderIndex == 4) robotName5.Text = robotName;

                            //Console.WriteLine("robot index = " + senderIndex + " // Robot Name = " + robotName);

                            //robotID[robotIndex] = ????
                            /////////////////////////////  메인화면 

                            recvMsgs.RemoveAt(i);  // 해당 메시지 삭제
                            break;

                        case REPORT_REQ://  상태 보고 메시지 처리 R->C

                            // [1] CONFIRM은 불필요
                            // [2] 전달된 추가적인 정보, 즉 상태정보, 배터리정보, 카운터 등의 정보를 추출하여, 관련 업데이트 작업 필요
                            //if (G.DEBUG) OutputMesssage(senderAddr + " : " + Cmd + " REPORT_INFO");
                            if (G.DEBUG) Console.WriteLine(senderAddr + " : " + Cmd + "  REPORT_INFO");

                            byte optionCode = buffs[1];

                            ///////////////////////////////////
                            // [2] 로봇 번호 및 명령어 종류에 따라 작업
                            string robotInfo = Cmd.Substring(2, 9); // 사용 길이에 따라 재조정

                            if (optionCode == OPTION_CODE_STATUS)
                            {
                                TBox_Status[senderIndex].Text = robotInfo;

                            }
                            else if (optionCode == OPTION_CODE_BATTERY)
                            {
                                TBox_Battery[senderIndex].Text = robotInfo;

                            }
                            //else 다른 경우는 무시

                            recvMsgs.RemoveAt(i); // 해당 메시지 삭제
                            break;

                        case CONTROL_CONF: // CONFIRM 메시지의 경우, 

                            // CONFIRM 메시지 수신 모드이면, 메시지 그대로, 아닌 경우에만, 해당 메시지 삭제,
                            // CONFIRM은 다른 곳에서 따로 처리하므로 삭제하면 안됨
                            //if (G.confirmWaitingMode == false) 
                            //    recvMsgs.RemoveAt(i); // 해당 메시지 삭제
                            // Conf 메시지의 경우, Conf 전용 큐로 이동 후 삭제
                            if (G.confirmWaitingMode == true)
                                confMsgs.Add(recvMsgs[i]);
                            recvMsgs.RemoveAt(i);

                            break;

                        default:  // 잘못된 명령어, 삭제

                            if (G.DEBUG) OutputMesssage(@"잘못된 명령어 수신. 명령어 = " + Cmd, Color.Red);
                            //MessageBox.Show(@"시스템 오류 - 예상못한 경우가 발생했습니다.", "시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            recvMsgs.RemoveAt(i); // 해당 메시지 삭제

                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "???" +i);
                }
            }
        }

        // Address로부터 기기 순서/번호를 알아낸다. 1~5

        int getRobotIndex(int Address)
        {
            for (int i = 0; i < G.ROBOT_CNT; i++)
                if (G.robotAddress[i] == Address) return i;
            return -1;  // 이 경우는 발생하면 안됨
        }

        /*
         * 본 함수는 주어진 주소(robotAddress)로 주어진 메시지(sendMsg)를 보내고, 응답을 기다린다.
         * sendMsg는 호출시 제공되어야 한다.
         */

        private async Task<bool> SendCommand(int robotAddress, string sendMsg)  // 사실 CONTROL_REQ 명령만 처리하는 함수
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    //if (sendMsg.Length < 1) return 0;
                    // 로봇으로 메시지 보내기전, 대상 로봇에서 이전에 온 메시지를 버퍼에서 삭제
                    // 
                    //await clearReceivedMessages(robotAddress);

                    byte[] buffs = Encoding.ASCII.GetBytes(sendMsg);

                    LoRaWrite(robotAddress, buffs);

                    byte cmdCode = buffs[0];

                    ////////////////////////////////////////////////
                    if (cmdCode == CONTROL_REQ)  // 오직 이 경우에만 CONFIRM을 기다림
                    {
                        G.confirmWaitingMode = true;  ////// 중요
                        bool result = await CheckAsyncControlConfmMsg(robotAddress);
                        G.confirmWaitingMode = false;

                        if (result == true)
                        {
                            //if (DEBUG) OutputMesssage("명령 전송 성공");
                            Console.WriteLine(@"응답 메시지 수신 완료");
                            ////////////////////////////////////////////
                            /// 여기에 명령에 따른 WorkLog DB 업데이트 작업 추가



                            return true;
                        }
                        else
                        {
                            // if (DEBUG) OutputMesssage( "No Confirm Msg. --명령 전송 실패");
                            Console.WriteLine(@"응답 메시지 없음");
                            return false;
                        }
                    }
                    else
                    {
                        MessageBox.Show(@"예상치 못한 경우 발생", @"프로그램 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    OutputMesssage(@"통신포트 : " + serialPort1.PortName + " 오류");
                    MessageBox.Show(@"통신포트 " + serialPort1.PortName + "닫힘", @"통신포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"메시지 전송 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        private void LoRaWrite(int robotAddress, byte[] sendData)
        {
            string HEADER_SEND = "AT+SEND=";
            string sendMesg = Encoding.ASCII.GetString(sendData);
            String sendPacket = HEADER_SEND + robotAddress + "," + sendData.Length + "," + sendMesg;

            serialPort1.Write(sendPacket + Environment.NewLine);

            if (G.DEBUG) OutputMesssage(sendPacket);

        }

        async Task<bool> CheckAsyncControlConfmMsg(int robotAddr) // CONTROL_REQ 명령에 대해서는 CONF 필요
        {
            // CONTROL_REQ 명령의 경우, 로봇의 CONFIRM 필요
            if (G.confirmWaitingMode != true)
                MessageBox.Show(@"예상하지 않는 경우 발생", @"시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 응답 메시지는 senderAddr과 일치, cmdCode는 respCode여야 한다. 
            byte respCode = CONTROL_CONF;

            // 첫번째 대기
            await Task.Delay(G.CONF_WAIT_DURATION_1);

            //for (int i = 0; i < recvMsgs.Count; i++)
            for (int i = 0; i < confMsgs.Count; i++)
            {
                int index = confMsgs[i].IndexOf("=");
                string[] parts = confMsgs[i].Substring(index + 1).Split(',');
                byte[] buffs = Encoding.ASCII.GetBytes(parts[2]);
                byte cmdCode = buffs[0];

                if (robotAddr == int.Parse(parts[0]) && respCode == cmdCode)
                {
                    confMsgs.RemoveAt(i); // 해당 메시지 삭제
                    return true;
                }
            }

            // 두번째 대기 
            await Task.Delay(G.CONF_WAIT_DURATION_2); // 반복 2회

            for (int i = 0; i < confMsgs.Count; i++)
            {
                int index = confMsgs[i].IndexOf("=");
                string[] parts = confMsgs[i].Substring(index + 1).Split(',');
                byte[] buffs = Encoding.ASCII.GetBytes(parts[2]);
                byte cmdCode = buffs[0];

                if (robotAddr == int.Parse(parts[0]) && respCode == cmdCode)
                {
                    confMsgs.RemoveAt(i); // 해당 메시지 삭제
                    return true;
                }
            }

            return false;
    
        }
     
        private void SendRegisterConfMsg(int robotAddress) // 오직 REGISTER_CONF 전달을 위한 함수
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    string sendMsg = "_CONF_TEST";
                    byte[] buffs = Encoding.ASCII.GetBytes(sendMsg);
                    buffs[0] = REGISTER_CONF;

                    // 단순 REGISTER_CONF 메시지 발송, CONFIRM은 이 경우밖에 없음
                    {
                        // [1] CONFIRM 메시지만 보낸다. 추가적인 작업 불필요
                        string sendMesg = Encoding.ASCII.GetString(buffs); // !!!!! 수정 작성 필요

                        LoRaWrite(robotAddress, buffs);

                        return;
                    }
                }
                else
                {
                    OutputMesssage("통신포트 : " + serialPort1.PortName + " 오류");
                    MessageBox.Show(@"통신포트 : " + serialPort1.PortName + "닫힘", @"통신포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, @"메시지 전송 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
      
        // 버튼 명령 통합 처리 함수
        private async Task<bool> SendControlReqMesgAndCheck(int robotIndex, string cmdString)
        {
            //TEST - CONF 신호 응답을 받으면 true, 아니면 false
            int Address = G.robotAddress[robotIndex];
            Address = 20;
            ////////////////////////////////////////
            // TEST를 위해 address=20으로 고정
            bool result = await SendCommand(Address, "D_RUN/STOP/HOME/V/H");

            if (result) OutputMesssage(cmdString + " #" + (robotIndex + 1) + " 버튼 전송 완료(응답확인)");
            else OutputMesssage(cmdString + " #" + (robotIndex + 1) + " 버튼 전송 실패(무응답)");

            return result;
        }

        /*  
         *  버튼 동작 처리 부분
         */
        private async void runActionAsync(int robotIndex)
        {
            OutputMesssage("RUN #" + (robotIndex + 1) + " 버튼 실행");

            bool result = await SendControlReqMesgAndCheck(robotIndex, "RUN");

            if (result) // Toggle Image
            {
                Btn_RUN[robotIndex].Enabled = false;
                //if (robotIndex == 0) runBtn1.Enabled = false;
                //else if (robotIndex == 1) runBtn2.Enabled = false;
                //else if (robotIndex == 2) runBtn3.Enabled = false;
                //else if (robotIndex == 3) runBtn4.Enabled = false;
                //else if (robotIndex == 4) runBtn5.Enabled = false;
            }
        }

        private async void stopActionAsync(int robotIndex)
        {
            OutputMesssage("STOP #" + (robotIndex + 1) + " 버튼 실행");

            bool result = await SendControlReqMesgAndCheck(robotIndex, "STOP");

            if (result) // Toggle Image
            {
                Btn_STOP[robotIndex].Enabled = false;
                //if (robotIndex == 0) stopBtn1.Enabled = false;
                //else if (robotIndex == 1) stopBtn2.Enabled = false;
                //else if (robotIndex == 2) stopBtn3.Enabled = false;
                //else if (robotIndex == 3) stopBtn4.Enabled = false;
                //else if (robotIndex == 4) stopBtn5.Enabled = false;
            }
        }

        private async void homeActionAsync(int robotIndex)
        {
            OutputMesssage("HOME #" + (robotIndex + 1) + " 버튼 실행");

            bool result = await SendControlReqMesgAndCheck(robotIndex, "HOME");

            if (result) // Toggle Image
            {
                Btn_HOME[robotIndex].Enabled = false;
                //if (robotIndex == 0) homeBtn1.Enabled = false;
                //else if (robotIndex == 1) homeBtn2.Enabled = false;
                //else if (robotIndex == 2) homeBtn3.Enabled = false;
                //else if (robotIndex == 3) homeBtn4.Enabled = false;
                //else if (robotIndex == 4) homeBtn5.Enabled = false;
            }
        }

        private void optionAction(int robotIndex)
        {
            OutputMesssage("OPTION #" + (robotIndex + 1) + " 버튼 실행");

            G.CurrentRobotNumer = robotIndex;
            OptionSetting form = new OptionSetting();
            form.ShowDialog();
        }

        private void LinkComboBoxPlantList()
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            // Another Try
            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand("select PlantId, PlantName from " + TBL_NAME, con);
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

            //comboBox1.DataSource = dt;
            //comboBox1.DisplayMember = "PlantName";
            //comboBox1.ValueMember = "PlantId";

        }

        private void reportBtn_Click(object sender, EventArgs e)  // 보고서 작성 화면으로 이동
        {
            ReportForm form = new ReportForm();
            form.Show();
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();

        }

        private void 로그파일저장ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            G.SaveLogFile(output.Text.Trim()); // 로그 정보 파일 저장
            MessageBox.Show(@"출력창 내용을 로그파일로 저장했습니다.", @"로그파일 저장", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void 시스템종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exitBtn_Click(sender, e);
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) serialPort1.Close();

            DialogResult result = MessageBox.Show(@"작업을 종료합니다.", @"작업 종료", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                G.CNFSaveFile(); // 현재 설정을 자동 저장한다.
                G.SaveLogFile(output.Text.Trim()); // 로그 정보 파일 저장

                this.Close();
            }
        }

        private void 보고서작성ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            reportBtn_Click(sender, e);

        }

        private void ChangeSerialPort()
        {
            SerialPortSetting form = new SerialPortSetting();
            form.ShowDialog();

            CheckNewSerialPort();
        }

        private void CheckNewSerialPort()
        {
            if (!G.oldSelectedSerialPortName.Equals(G.SelectedSerialPortName))
            {
                Console.WriteLine(G.oldSelectedSerialPortName + " ==> " + G.SelectedSerialPortName);
                try
                {
                    if (serialPort1.IsOpen) serialPort1.Close();
                    serialPort1.PortName = G.SelectedSerialPortName;

                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        comInfo.ForeColor = Color.Black;
                        comInfo.Text = serialPort1.PortName;
                        OutputMesssage("통신포트 변경 : " + serialPort1.PortName + " 연결");
                        // 포트 변경 및 오픈 성공시에만 추가적으로 검사
                        CheckLoRaPortAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    comInfo.ForeColor = Color.Red;
                    comInfo.Text = "통신포트 오류 : " + serialPort1.PortName;
                    OutputMesssage("통신포트 오류 : " + serialPort1.PortName);
                    MessageBox.Show(ex.Message, "통신포트 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    //Application.Restart();
                }
            }
            else Console.WriteLine("NO change : " + G.SelectedSerialPortName);
        }

        private void 통신포트변경ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeSerialPort();
        }

        //string LoRaAddress;

        private void loRa연결확인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckLoRaPortAsync(true);
        }

        private void 신규등록ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddPlantList form = new AddPlantList();
            form.ShowDialog();

            // refresh the combobox
            LinkComboBoxPlantList();
        }

        public void OutputMesssage(string line)
        {
            output.AppendText(Environment.NewLine + line); // new line 추가

            output.Select(output.Text.Length, 0);// scroll 항상 아래
            output.ScrollToCaret();
            output.SelectionColor = Color.Black; // 원상 복구

        }

        public void OutputMesssage(string line, Color color)
        {
            Color oldColor = output.SelectionColor;

            output.SelectionColor = color; // 색상 변경
            // 색상이 있는 경우, 굵게 쓰기 
            //output.SelectionFont = new Font(output.Font, FontStyle.Bold);
            OutputMesssage(line);
            output.SelectionColor = oldColor; // 색상 복원

        }

        public void ErrorMessage(string line)
        {
            OutputMesssage(line, Color.Red);
        }

        private void 항목삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeletePlantList form = new DeletePlantList();
            form.ShowDialog();

            // refresh the combobox
            LinkComboBoxPlantList();
        }

        private void 정보변경ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdatePlantList form = new UpdatePlantList();
            form.ShowDialog();

            // refresh the combobox
            LinkComboBoxPlantList();
        }
    }
}
