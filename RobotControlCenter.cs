using RobotControlCenter;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobotCC
{
    public partial class RobotControlCenter : Form
    {
        // 메시지 HEADER
        public const string MSG_HEADER = "LX";  // R->C

        // 명령 코드 
        public const string REGISTER_REQ = "10";  // R->C
        public const string REGISTER_CNF = "11"; // C->R

        // ERROR 발생
        public const string ERR_STATUS_REQ = "20";   // R->C
        public const string ERR_STATUS_CNF = "21";   // C->R

        // 배터리 정보
        public const string BATTERY_STATUS_REQ = "30";    // R->C (NO CONFIRM)

        // 에지 도달 카운트
        public const string CLEAN_COUNT = "32";    // R->C (NO CONFIRM)

        // 작업 종료
        public const string FINISH_STATUS_REQ = "40";  // R->C
        public const string FINISH_STATUS_CNF = "41";  // C->R

        // 센터 명령 / 버튼
        public const string RUN_REQ = "50";   // C->R
        public const string RUN_CNF = "51";   // R->C

        public const string STOP_REQ = "60";   // C->R
        public const string STOP_CNF = "61";   // R->C

        public const string HOME_REQ = "70";   // C->R
        public const string HOME_CNF = "71";   // R->C

        // 센터 방향 지시
        public const string OT_V_REQ = "80";   // C->R  수직 방향 지시
        public const string OT_V_CNF = "81";   // R->C // 

        public const string OT_H_REQ = "90";   // C->R 수평 방향 지시
        public const string OT_H_CNF = "91";   // R->C


        // 수신 메시지 보관용 리스트
        List<string> recvMsgs = new List<string>();  // 수신 메시지 전체 보관
        List<string> confMsgs = new List<string>();  // CONF 메시지 분리 보관
        List<string> addrMsgs = new List<string>();  // 주소 관련 메시지 분리 보관


        // 각종 component를 배열로 관리 처리하기 위한 선언
        //// Buttons : R/S/H/OPTION
        public Button[] Btn_RUN = new Button[G.ROBOT_MAX_CNT];
        public Button[] Btn_STOP = new Button[G.ROBOT_MAX_CNT];
        public Button[] Btn_HOME = new Button[G.ROBOT_MAX_CNT];
        public Button[] Btn_OPTION = new Button[G.ROBOT_MAX_CNT];

        // TEXT BOXes
        TextBox[] TBox_RobotName = new TextBox[G.ROBOT_MAX_CNT];
        RichTextBox[] TBox_Status = new RichTextBox[G.ROBOT_MAX_CNT];

        // PROGRESSBARs
        ProgressBar[] Progress = new ProgressBar[G.ROBOT_MAX_CNT];
        RichTextBox[] TBox_Progress = new RichTextBox[G.ROBOT_MAX_CNT];

        // Battery Infos
        ProgressBar[] BatteryLevel = new ProgressBar[G.ROBOT_MAX_CNT];
        RichTextBox[] TBox_Battery = new RichTextBox[G.ROBOT_MAX_CNT];

        // Radio Buttons
        RadioButton[] OT_H = new RadioButton[G.ROBOT_MAX_CNT];
        RadioButton[] OT_V = new RadioButton[G.ROBOT_MAX_CNT];




        public RobotControlCenter()
        {
            InitializeComponent();

            // 리스트 목록 초기화
            recvMsgs.Clear();
            addrMsgs.Clear();
            confMsgs.Clear();

            // 출력 창 특성 설정
            output.Select(output.Text.Length, 0);
            output.ScrollToCaret();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            OutputMesssage(G.VERSIONNAME);
            //OutputMesssage(DateTime.Now.ToString() + " : " + "SYSTEM START ");
            OutputMesssage("[" + G.TimeStamp() + "] " + "SYSTEM START ");


            OutputMesssage("## [단계 1] 시스템 폴더 검사 ##", Color.Blue);
            // System Folder 확인 및 생성
            G.CheckAndMakeFolder();

            OutputMesssage("## [단계 2] DB 파일 검사 ##", Color.Blue);
            // DB File 검사
            CheckDBFile();

            OutputMesssage("## [단계 3] 설정 파일 업로드 ##", Color.Blue);
            // 설정 파일 읽어 오기
            G.CNFLoadFile();

            //통신 포트 검사
            OutputMesssage("## [단계 4] 통신 포트 검사 ##", Color.Blue);
            CheckSerialPortAtStart();

            // LoRa 송수신 테스트
            OutputMesssage("## [단계 5] LoRa 통신 테스트 ##", Color.Blue);
            CheckLoRaPortAsync(false); // 초기 단계의 경우, 성공시 메시지 없도록 조정

            // COMBO BOX에 실제 DATA 연결
            LinkComboBoxPlantList();

            //여러개의 동일 컴포넌트를 한번에 처리하기 위한 연결 작업
            LinkArrayComponent();

            // 읽은 로봇 정보 및 설정을 화면에 표시
            DisplayRobotInformation();

            //////////////////// JUST FOR SAMPLE TEST : RICHTEXTBOX, COLOR
            if (G.DEBUG)
            {
                /// ProgressBar 색상 변경을 적용하려면, 메인 program.cs에서 아래 문장을 삭제해야 제대로 동작함
                // Application.EnableVisualStyles(); 윈도우즈 UI 표준화 작업

                //Progress[0].Value = 25; TBox_Progress[0].Text = "25";
                //Progress[1].Value = 75; TBox_Progress[1].Text = "75";
                //Progress[2].Value = 3; TBox_Progress[2].Text = "3";
                //Progress[3].Value = 10; TBox_Progress[3].Text = "10";

                //BatteryLevel[0].Value = 25; TBox_Battery[0].Text = "25";
                //BatteryLevel[1].Value = 75; TBox_Battery[1].Text = "75";
                //BatteryLevel[2].Value = 3; TBox_Battery[2].Text = "3";
                //BatteryLevel[3].Value = 20; TBox_Battery[3].Text = "20";

                //status5.SelectionAlignment = HorizontalAlignment.Center;
                //battery5.SelectionAlignment = HorizontalAlignment.Center;


            }
        }

        private void DisableRow(int i)
        {
            // 각종 컴포넌트 비활성화
            Btn_RUN[i].Enabled = false; Btn_STOP[i].Enabled = false;
            Btn_HOME[i].Enabled = false; Btn_OPTION[i].Enabled = false;
            OT_V[i].Enabled = false; OT_H[i].Enabled = false;

            TBox_RobotName[i].Enabled = false; TBox_Status[i].Enabled = false;

            Progress[i].Visible = false; TBox_Progress[i].Enabled = false;
            BatteryLevel[i].Visible = false; TBox_Battery[i].Enabled = false;
        }

        private void EnableRow(int i)
        {
            // 각종 컴포넌트 활성화
            Btn_RUN[i].Enabled = true; Btn_STOP[i].Enabled = true;
            Btn_HOME[i].Enabled = true; Btn_OPTION[i].Enabled = true;
            OT_V[i].Enabled = true; OT_H[i].Enabled = true;

            TBox_RobotName[i].Enabled = true; TBox_Status[i].Enabled = true;

            Progress[i].Visible = true; TBox_Progress[i].Enabled = true;
            BatteryLevel[i].Visible = true; TBox_Battery[i].Enabled = true;

            // 일부 내용은 화면에 반영
            TBox_RobotName[i].Text = G.robotID[i];
            BatteryLevel[i].Value = 0; Progress[i].Value = 0;
            TBox_Battery[i].Text = "0"; TBox_Progress[i].Text = "0";

            // 방향 설정, 자동 실행 정보의 경우,  이전에 설정된 정보를 적용
            if (G.ExistingRobotNameAndAddress.ContainsKey(G.robotID[i]))
            {
                G.OT[i] = G.ExistingRobotNameAndOT[G.robotID[i]];
                G.AUTOSTART[i] = G.ExistingRobotNameAndAutoStart[G.robotID[i]];

                if (G.OT[i] == G.VERTICAL) OT_V[i].Checked = true;
                else OT_H[i].Checked = true;
            }

        }

        private void DisplayRobotInformation()
        {
            // [1] 저장된 로봇명, 동작 방향 등을 보여줌
            // LSize, RSize는 읽어 오되, 화면에 따로 보여주지는 않음 (세부설정시 보여줌)
            for (int i = 0; i < G.ROBOT_REG_CNT; i++)
            {
                TBox_RobotName[i].Text = G.robotID[i];

                if (G.OT[i] == G.VERTICAL) OT_V[i].Checked = true;
                else OT_H[i].Checked = true;

                TBox_Status[i].Text = "로봇 전원 OFF, 주소 = " + G.robotAddress[i];
                BatteryLevel[i].Value = 0; Progress[i].Value = 0;

                DisableRow(i); // 전원 ON될 때까지 각종 컴포넌트 일시 비활성화
            }

            //// [2] 미사용 기기에 대한 버튼 비활성화
            for (int i = G.ROBOT_REG_CNT; i < G.ROBOT_MAX_CNT; i++)
            {
                DisableRow(i); // 각종 컴포넌트 비활성화
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
                comInfo.ForeColor = Color.Blue;
                comInfo.Text = serialPort1.PortName;
                OutputMesssage("통신포트 : " + serialPort1.PortName + " 연결");
            }
        }

        private async void CheckLoRaPortAsync(bool option)
        {
            if (!serialPort1.IsOpen) return;

            bool respResult = false;
            OutputMesssage(@"Lora 통신 테스트 중 ......");

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

        private void LinkComboBoxPlantList()
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand("select PlantNumber, PlantName from " + TBL_NAME, con);
            //DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            comboBox1.Items.Clear();
            while (sdr.Read())
            {
                comboBox1.Items.Add(sdr.GetString(0) + " : " + sdr.GetString(1));
            }
            //dt.Load(sdr);
            con.Close();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void LinkArrayComponent()
        {
            // 각종 버튼의 제어를 배열로 관리하기 위한 작업   
            // [1] RUN 버튼 연결
            Btn_RUN[0] = runBtn1; Btn_RUN[1] = runBtn2; Btn_RUN[2] = runBtn3;
            Btn_RUN[3] = runBtn4; Btn_RUN[4] = runBtn5;

            //// [2] STOP 버튼 연결
            Btn_STOP[0] = stopBtn1; Btn_STOP[1] = stopBtn2; Btn_STOP[2] = stopBtn3;
            Btn_STOP[3] = stopBtn4; Btn_STOP[4] = stopBtn5;

            //// [3] HOME 버튼 연결
            Btn_HOME[0] = homeBtn1; Btn_HOME[1] = homeBtn2; Btn_HOME[2] = homeBtn3;
            Btn_HOME[3] = homeBtn4; Btn_HOME[4] = homeBtn5;

            //// [4] 로봇 이름
            TBox_RobotName[0] = robotName1; TBox_RobotName[1] = robotName2; TBox_RobotName[2] = robotName3;
            TBox_RobotName[3] = robotName4; TBox_RobotName[4] = robotName5;

            //// [5] 로봇 상태 정보
            TBox_Status[0] = status1; TBox_Status[1] = status2; TBox_Status[2] = status3;
            TBox_Status[3] = status4; TBox_Status[4] = status5;

            //// [6] 배터리 잔량 (ProgressBar 형식)
            BatteryLevel[0] = batteryBar1; BatteryLevel[1] = batteryBar2; BatteryLevel[2] = batteryBar3;
            BatteryLevel[3] = batteryBar4; BatteryLevel[4] = batteryBar5;

            //// [7] 배터리 정보
            TBox_Battery[0] = batteryText1; TBox_Battery[1] = batteryText2; TBox_Battery[2] = batteryText3;
            TBox_Battery[3] = batteryText4; TBox_Battery[4] = batteryText5;

            //// [8] 작업 진행률 (ProgressBar 형식)
            Progress[0] = progressBar1; Progress[1] = progressBar2; Progress[2] = progressBar3;
            Progress[3] = progressBar4; Progress[4] = progressBar5;

            //// [9] 진행률 숫자 정보
            TBox_Progress[0] = progressText1; TBox_Progress[1] = progressText2; TBox_Progress[2] = progressText3;
            TBox_Progress[3] = progressText4; TBox_Progress[4] = progressText5;

            ////// [10] OT 선택 버튼 연결
            OT_H[0] = radioH1; OT_H[1] = radioH2; OT_H[2] = radioH3; OT_H[3] = radioH4; OT_H[4] = radioH5;
            OT_V[0] = radioV1; OT_V[1] = radioV2; OT_V[2] = radioV3; OT_V[3] = radioV4; OT_V[4] = radioV5;

            //// [11] OPTION 버튼 연결
            Btn_OPTION[0] = optionBtn1; Btn_OPTION[1] = optionBtn2; Btn_OPTION[2] = optionBtn3;
            Btn_OPTION[3] = optionBtn4; Btn_OPTION[4] = optionBtn5;

            //// [12] 각 버튼 클릭 이벤트 공동 연결
            //// 디자인 파일에 직접 설정변경하면 디자인 화면 (경고) 오류 발생 ==> 여기서 연결
            for (int i = 0; i < G.ROBOT_MAX_CNT; i++)
            {
                int index = i; // 주의 : 반드시 내부 변수를 별도로 사용해야 잘 동작
                this.Btn_RUN[i].Click += (sender, ex) => this.runActionAsync(index);
                this.Btn_STOP[i].Click += (sender, ex) => this.stopActionAsync(index);
                this.Btn_HOME[i].Click += (sender, ex) => this.homeActionAsync(index);

                this.OT_H[i].Click += (sender, ex) => this.OT_HActionAsync(index);
                this.OT_V[i].Click += (sender, ex) => this.OT_VActionAsync(index);

                this.Btn_OPTION[i].Click += (sender, ex) => this.optionAction(index);
            }
        }

        // Address로부터 기기 순서/번호를 알아낸다. 1~5
        int getRobotIndex(int Address)
        {
            for (int i = 0; i < G.ROBOT_REG_CNT; i++)
                if (G.robotAddress[i] == Address) return i;
            return -1;  // 이 경우는 발생하면 안됨, 그러나 주소 변경이나 신규 로봇이 추가되는 경우에 대한 대비
        }

        private int RefExist(string name, int address) // 설정 저장 목록 중 name/address 일치 항목의 인덱스 반환
        {
            // 등록 요청한 로봇에 대한 설정 파일 정보가 있으면 최대한 활용
            int count = 0;
            foreach (var s in G.ExistingRobotNameAndAddress)
            {
                count++;
                if (s.Key.Equals(name) && s.Value == (address)) // 기존 정보와 정확하게 일치하면
                    return count;
            }
            return 0;
        }

        /*
         * 수신 데이터가 있으면 무조건 List(RecvMesg)에 보관, 발신 주소는 같이 저장하여, CONF 발송 대응
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
         * 수신된 명령 (Command) 종류에 따라 정해진 작업 및 CONFIRM 처리, 처리된 메시지는 리스트에서 삭제
         */
        private void ManageRecvMessages(object o, EventArgs e) // CMD 종류에 따라 처리 - WorkLog 업데이트 작업도 해야 함
        {
            ////////////////////////  수신 메시지 처리 ///////////////////////////

            for (int i = 0; i < recvMsgs.Count; i++)
            {
                try
                {
                    // 해당 메시지 백업 후, 목록에서 미리 삭제
                    string lastMsg = recvMsgs[i];
                    recvMsgs.RemoveAt(i);

                    if (G.DEBUG) OutputMesssage($"수신 메시지 : < # {i} : {lastMsg} >");
                    if (G.DEBUG) Console.WriteLine($"수신 메시지 : < {lastMsg} >");

                    //// 통신용 헤더 정보와 송수신 메시지 분리
                    int index = lastMsg.IndexOf("=");
                    string[] parts = lastMsg.Substring(index + 1).Split(',');

                    //// 추출된 수신 메시지/명령 분석 

                    ///// [1] 송신자 주소
                    int senderAddr = int.Parse(parts[0]);
                    ////새로운 기기 사용으로 senderAddress가 새로운 주소이면, getRobotIndex() 제대로 작동 않음
                    /// 다른 명령은 이럴수가 없으나 REG_REQ에서는 발생 가능 ==> REG_REQ에서는 별도로 index 계산
                    int senderIndex = getRobotIndex(senderAddr);

                    //// [2] 전달된 메시지 크기
                    int dataSize = int.Parse(parts[1]);
                    if (dataSize < 2) // 
                    {
                        ErrorMessage(@"비거나 불완전한 메시지를 제거");
                        return;
                    }

                    //// [3] 전달된 메시지 본문 검사
                    //// Header Check "LX" : All messages should start with "LX"
                    if (!parts[2].Equals(MSG_HEADER)) continue; // "LX"로 시작하지 않으면 무시

                    //// [4] 명령어에 따른 작업
                    string CmdCode = parts[3]; // 명령어 종류 파악

                    // 특수 경우 검사
                    if (!CmdCode.Equals(REGISTER_REQ) && senderIndex == -1)
                    {
                        Console.WriteLine("오류 : 미등록된 로봇으로부터 로봇 등록 이외의 메시지 수신 - 무시.");
                        continue;
                    }

                    // 명령어 종류에 따라 해당 작업 수행
                    if (CmdCode.Equals(REGISTER_REQ)) //  로봇 등록 명령 처리 R->C
                    {
                        // 이 경우는 senderIndex 새로 계산 필요 - 새로운 기기 등록 가능성 때문
                        string robotName = parts[4]; // 로봇 등록명 추출

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, REGISTER_CNF);

                        // [2] 로봇명을 추출하여, 관련 업데이트 작업 필요
                        // [2-1] 기존에 존재하는 목록인 경우, 활성화 +초기값 설정
                        // [2-2] 동일 로봇명의 주소만 바뀐 경우, 새로운 주소로 변경 - 통신 모듈 변경, 활성화 
                        // [2-3] 동일 주소의 로봇명이 바뀐 경우, 로봇명 변경 - 로봇 대체 + 기존 통신 모듈 사용, 활성화
                        // [2-4] 기존 목록에 없는 경우, 새로운 기기 추가, 사용 기기수 증가, 활성화
                        ///메인화면 정보 업데이트 + DB 업데이트

                        // 아래에서 senderIndex 재설정 필요
                        bool found = false;

                        for (int r = 0; r < G.ROBOT_REG_CNT; r++)
                        {
                            if (G.robotID[r].Equals(robotName) && G.robotAddress[r] == senderAddr)  // 2-1 경우
                            {
                                EnableRow(r);
                                TBox_RobotName[r].Text = robotName;
                                TBox_Status[r].Text = "로봇 재등록"; TBox_Status[r].ForeColor = G.DefaultColor;

                                OutputMesssage("기존 로봇 : " + robotName + " 재등록, 주소(동일) = " + senderAddr);
                                senderIndex = r;
                                DB.insertWorkLog(senderIndex, G.REGISTER, "");
                                found = true;

                                break;
                            }
                            else if (G.robotID[r].Equals(robotName) && G.robotAddress[r] != senderAddr)  // 2-2 경우
                            {
                                G.robotAddress[r] = senderAddr;
                                EnableRow(r);
                                TBox_RobotName[r].Text = robotName;
                                TBox_Status[r].Text = "로봇 등록, 주소(변경)"; TBox_Status[r].ForeColor = G.DefaultColor;
                                OutputMesssage("등록 로봇 : " + robotName + ", 주소(변경) = " + senderAddr);
                                senderIndex = r;
                                DB.insertWorkLog(senderIndex, G.REGISTER, "");

                                found = true;

                                break;
                            }
                            else if (!G.robotID[r].Equals(robotName) && G.robotAddress[r] == senderAddr)  // 2-3 경우
                            {
                                G.robotID[r] = robotName;
                                EnableRow(r);
                                TBox_RobotName[r].Text = robotName;
                                TBox_Status[r].Text = "신규 등록, 주소(동일)";
                                OutputMesssage("로봇 변경 : " + robotName + ", 주소(동일) = " + senderAddr);
                                senderIndex = r;
                                DB.insertWorkLog(senderIndex, G.REGISTER, "");

                                found = true;

                                break;
                            }
                        }

                        // 일치하는 목록이 없으면, 신규 추가
                        if (!found)
                        {
                            if (G.ROBOT_REG_CNT == G.ROBOT_MAX_CNT)
                            {
                                MessageBox.Show("로봇은 최대 " + G.ROBOT_MAX_CNT + "대까지만 등록 가능합니다.",
                                    "로봇 등록 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            else
                            {
                                // 동일한 값이 전혀 없는 경우 - 2-4 경우
                                // 새로운 기기 추가
                                G.robotID[G.ROBOT_REG_CNT] = robotName;
                                G.robotAddress[G.ROBOT_REG_CNT] = senderAddr;
                                EnableRow(G.ROBOT_REG_CNT);
                                TBox_RobotName[G.ROBOT_REG_CNT].Text = robotName;

                                // 설정 파일에 저장되어 있던 로봇/주소 조합인 경우, 이를 표시
                                int refindex = RefExist(robotName, senderAddr);


                                if (refindex != 0)
                                {
                                    TBox_Status[G.ROBOT_REG_CNT].Text = "로봇 등록. 기존 설정값 사용 ";
                                    TBox_Status[G.ROBOT_REG_CNT].ForeColor = G.DefaultColor;

                                    OutputMesssage("로봇 재등록 : " + robotName + ", 주소 = " + senderAddr + ", 기존 설정값 사용");
                                }
                                else
                                {
                                    TBox_Status[G.ROBOT_REG_CNT].Text = "신규 등록, 주소(추가)";
                                    TBox_Status[G.ROBOT_REG_CNT].ForeColor = G.DefaultColor;

                                    OutputMesssage("신규 로봇 등록 : " + robotName + ", 주소 = " + senderAddr);
                                }

                                // senderIndex를 재설정 - 로봇 대수 추가
                                senderIndex = G.ROBOT_REG_CNT;

                                // 로봇 등록시 작업 진행률 관련 수치를 모두 0으로 초기화
                                Progress[senderIndex].Value = 0;
                                TBox_Progress[senderIndex].Text = Progress[senderIndex].Value.ToString();
                                G.WORK_PERCENTAGE[senderIndex] = 0; //전체 진행률
                                G.EDGE_CNT[senderIndex] = 0; //모서리 카운트 초기화

                                // 로봇 등록 사항을 DB에 기록
                                DB.insertWorkLog(senderIndex, G.REGISTER, "");

                                G.ROBOT_REG_CNT++;
                            }
                        }

                        SoundBeep(0, 300); //도

                        G.CNFSaveFile(); // 현재 설정을 자동 저장한다.

                    }
                    else if (CmdCode.Equals(ERR_STATUS_REQ))
                    {
                        string errorCodeStr = parts[4]; // 에러 코드를 Status에 보여준다.

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, ERR_STATUS_CNF);

                        // [2] 에러 코드를 Status 란에 보여준다. 추가 액션/DB 관리 가능
                        TBox_Status[senderIndex].ForeColor = Color.Red;
                        TBox_Status[senderIndex].Text = "작동오류, 중지 : " + errorCodeStr;
                        Refresh();// 화면 출력 후 소리 재생
                        SoundBeep(7, 3000); SoundBeep(3, 3000); SoundBeep(7, 3000);

                        // [3] 작업 상태(ERROR_STOP)를 DB에 저장한다. 카운트는 현재 값으로 
                        DB.insertWorkLog(senderIndex, G.ERROR_STOP, errorCodeStr);

                        // [4] 작업 버튼을 다시 활성화시킨다.
                        Btn_RUN[senderIndex].Enabled = true;
                    }
                    else if (CmdCode.Equals(BATTERY_STATUS_REQ)) // NO CONFIRM
                    {
                        // [1] CONFIRM은 불필요
                        // [2] 전달된 추가적인 정보, 즉 배터리 수준을 보여줌
                        // 추후 숫자가 아니라, progressbar 형식으로 출력
                        if (G.DEBUG) OutputMesssage(senderAddr + " : " + CmdCode + " BATTERY INFO");

                        TBox_Battery[senderIndex].Text = parts[4];
                        BatteryLevel[senderIndex].Value = int.Parse(parts[4]);

                        if (BatteryLevel[senderIndex].Value <= 10)
                        {
                            TBox_Status[senderIndex].ForeColor = Color.Red;
                            TBox_Status[senderIndex].Text = "배터리 충전 필요";

                            BatteryLevel[senderIndex].ForeColor = Color.Red;
                            TBox_Battery[senderIndex].ForeColor = Color.Red;
                            Refresh();// 화면 출력 후 소리 재생

                            SoundBeep(1, 100); SoundBeep(3, 500);
                        }
                        else
                        {
                            TBox_Status[senderIndex].ForeColor = G.DefaultColor;
                            TBox_Status[senderIndex].Text = "배터리 정보 수집";
                            BatteryLevel[senderIndex].ForeColor = ProgressBar.DefaultForeColor;
                            TBox_Battery[senderIndex].ForeColor = G.DefaultColor;
                        }
                    }
                    else if (CmdCode.Equals(CLEAN_COUNT)) // NO CONFIRM
                    {
                        // [1] CONFIRM은 불필요
                        // [2] 전달된 추가적인 정보, 즉 작업 진도량을 계산하여 보여줌
                        int Counter = int.Parse(parts[4]); // 카운터 정보를 저장한다.
                        ///////////////////////// ProgressBar 값 변경 등 
                        ///
                        G.EDGE_CNT[senderIndex] = Counter; // 전달받은 카운터를 저장

                        // 테스트 차원에서 PROGRESS를 10 더함, 실제로는 자세한 계산 필요
                        Progress[senderIndex].Value += 10; // 구체적인 계산 필요.
                        TBox_Progress[senderIndex].Text = Progress[senderIndex].Value.ToString();
                        G.WORK_PERCENTAGE[senderIndex] = Progress[senderIndex].Value; //전체 진행률

                        // 작업 상태를 DB에 저장한다. 
                        // DB 저장은 굳이 하지 않아도 될 듯하나 - 일단 저장
                        DB.insertWorkLog(senderIndex, G.REPORT_CLEAN_COUNTER, "");

                        if (G.DEBUG) Console.WriteLine("EDGE COUNTER : " + Counter);
                    }
                    else if (CmdCode.Equals(FINISH_STATUS_REQ)) // 작업 종료 보고
                    {
                        // string statusCodeStr = parts[4]; // 작업 종료 사실을 Status에 보여준다.

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, FINISH_STATUS_CNF);

                        // [2] 작업 종료 사실을 Status란에 보여준다. 추가 액션/DB 관리 가능

                        TBox_Status[senderIndex].Text = "작업 완료!";
                        TBox_Status[senderIndex].ForeColor = Color.Blue;
                        Refresh();// 화면 출력 후 소리 재생

                        SoundBeep(7, 500); //도
                        SoundBeep(7, 500); //도

                        // [3] 작업 진행률을 100%로 수정
                        Progress[senderIndex].Value = 100;
                        TBox_Progress[senderIndex].Text = Progress[senderIndex].Value.ToString();
                        G.WORK_PERCENTAGE[senderIndex] = Progress[senderIndex].Value; //전체 진행률

                        // [4] 작업 상태를 DB에 저장한다. 카운트는 최신 값으로 
                        DB.insertWorkLog(senderIndex, G.FINISHED, "");

                        // [5] 작업 버튼을 다시 활성화시킨다.
                        Btn_RUN[senderIndex].Enabled = true;
                    }
                    // 수신 메시지가 명령어가 아닌 단순 CONFIRM 메시지인 경우, 
                    else if (CmdCode.Equals(RUN_CNF) || CmdCode.Equals(STOP_CNF) || CmdCode.Equals(HOME_CNF)
                        || CmdCode.Equals(OT_V_CNF) || CmdCode.Equals(OT_H_CNF)) // 그외 정보 처리 (CONFIRM 종류) 
                    {
                        /// OSY : 좀더 신중한 처리가 필요 ////////
                        // CONFIRM 메시지 수신 모드이면, 메시지 그대로, 아닌 경우에만, 해당 메시지 삭제,
                        // CONFIRM은 다른 곳에서 따로 처리하므로 삭제하면 안됨
                        // Conf 메시지의 경우, Conf 전용 큐로 이동 후 삭제
                        if (G.CONF_WAIT_MODE == true)
                            confMsgs.Add(lastMsg);
                    }
                    else // 이 예외적인 경우는 발생하면 안됨. 코드 사용 잘못
                    {
                        if (G.DEBUG) OutputMesssage(@"잘못된 명령어 수신. 명령어 = " + CmdCode, Color.Red);
                        if (G.DEBUG) MessageBox.Show(@"시스템 오류 - 예상못한 경우가 발생했습니다.", "시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /*
         * 본 함수는 주어진 로봇 주소(robotAddress)로 명령 실행(버튼 동작) 메시지를 보내고, 응답을 기다린다.
         */
        private async Task<bool> SendCommand(int robotAddress, string cmdCode)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    LoRaWrite(robotAddress, cmdCode);

                    ////////////////////////////////////////////////
                    if (cmdCode.Equals(RUN_REQ) || cmdCode.Equals(STOP_REQ) || cmdCode.Equals(HOME_REQ)
                        || cmdCode.Equals(OT_V_REQ) || cmdCode.Equals(OT_H_REQ))  // 해당 명령을 송신하고, 수신 여부를 확인함
                    {
                        string respCode = getConfCode(cmdCode); // 각 명령 코드에 해당하는 CONF 코드 
                        G.CONF_WAIT_MODE = true;  ////// 중요
                        bool result = await WaitAndCheckCONF(robotAddress, respCode);
                        G.CONF_WAIT_MODE = false;

                        if (result == true) // CONF 신호가 확인되면, 
                        {
                            if (G.DEBUG) OutputMesssage("명령 전송 및 응답 확인 성공");
                            if (G.DEBUG) Console.WriteLine(@"응답 메시지 수신 완료");
                            ////////////////////////////////////////////
                            /// 여기에 명령에 따른 WorkLog DB 업데이트 작업 추가



                            return true;
                        }
                        else
                        {
                            if (G.DEBUG) OutputMesssage("응답 확인 없음");
                            if (G.DEBUG) Console.WriteLine(@"응답 확인 메시지 없음");
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

        private string getConfCode(string CmdCode)  // C->R 명령에 대해, 대기할 CONF 신호의 종류를 알려준다.
        {
            if (CmdCode.Equals(RUN_REQ)) return RUN_CNF;
            else if (CmdCode.Equals(STOP_REQ)) return STOP_CNF;
            else if (CmdCode.Equals(HOME_REQ)) return HOME_CNF;
            else if (CmdCode.Equals(OT_V_REQ)) return OT_V_CNF;
            else if (CmdCode.Equals(OT_H_REQ)) return OT_H_CNF;
            return ""; // 오류 발생
        }

        private void LoRaWrite(int robotAddress, string cmdCode)
        {
            string sendMesg = MSG_HEADER + "," + cmdCode; // "LX"+ "," + 명령어 코드
            String sendPacket = "AT+SEND=" + robotAddress + "," + sendMesg.Length + "," + sendMesg;

            serialPort1.Write(sendPacket + Environment.NewLine);

            if (G.DEBUG) OutputMesssage(sendPacket);

        }

        // C->R(실행) 명령에 대해서는 로봇으로부터의 CONF 확인 필요
        private async Task<bool> WaitAndCheckCONF(int robotAddr, string respCode)
        {
            // CONTROL_REQ 명령의 경우, 로봇의 CONFIRM 필요
            if (G.CONF_WAIT_MODE != true)
                MessageBox.Show(@"예상하지 않는 경우 발생", @"시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // 응답 확인할 메시지는 메시지 발송 주소는 senderAddr과 일치, cmdCode는 respCode와 동일해야 한다. 

            // 첫번째 대기
            await Task.Delay(G.CONF_WAIT_DURATION_1);

            for (int i = 0; i < confMsgs.Count; i++)
            {
                int index = confMsgs[i].IndexOf("=");
                string[] parts = confMsgs[i].Substring(index + 1).Split(',');
                // parts[0] = Address, parts[1] = msg length, parts[2] == LX, parts[3] = cmdCode

                // 응답 메시지는 senderAddr과 일치, cmdCode는 confCode여야 한다. 
                if (robotAddr == int.Parse(parts[0]) && respCode.Equals(parts[3]))
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
                //parts[0] = Address, parts[1] = msg length, parts[2] == LX, parts[3] = cmdCode

                // 응답 메시지는 senderAddr과 일치, cmdCode는 respCode여야 한다. 
                if (robotAddr == int.Parse(parts[0]) && respCode.Equals(parts[3]))
                {
                    confMsgs.RemoveAt(i); // 해당 메시지 삭제
                    return true;
                }
            }

            return false; // CONF 응답 확인 실패
        }

        // 로봇으로부터 받은 REQ 메시지에 대한 CONF 신호를 보내 수신 여부를 확인해준다. R->C CONFIRM
        private void SendConfMsgToRobot(int robotAddress, string respCode)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    // [1] 단순히 수신 명령에 대한 CONF 메시지(respCode)를 단순 발송한다. 추가적인 작업 불필요
                    LoRaWrite(robotAddress, respCode);

                    return;
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
        private async Task<bool> SendCommandAndCheckCONF(int robotIndex, string cmdCode) // 특정 로봇에게 명령을 전송하고, CONF 신호를 기다림
        {
            int Address = G.robotAddress[robotIndex];

            bool result = await SendCommand(Address, cmdCode); // cmdCode 명령 전송

            if (result)
            {
                //TBox_Status[robotIndex].Text = "명령 전송 완료";
                //TBox_Status[robotIndex].ForeColor = Color.Blue;
                //Refresh();

                OutputMesssage(cmdCode + " #" + (robotIndex + 1) + " 명령 전송 완료(응답확인)");
            }
            else
            {
                TBox_Status[robotIndex].Text = "명령 전송 실패(무응답)";
                TBox_Status[robotIndex].ForeColor = Color.Red;
                Refresh();// 화면 출력 후 소리 재생
                SoundBeep(5, 1000);

                OutputMesssage(cmdCode + " #" + (robotIndex + 1) + " 명령 전송 실패(무응답)");
            }
            return result;
        }

        ///////////////////////////
        /// 버튼 동작 처리 부분 /// - CONFIRM 답신 필요
        ///////////////////////////
        private async void runActionAsync(int robotIndex)
        {
            OutputMesssage("[작동 #" + (robotIndex + 1) + "] 버튼 실행");

            // 해당 로봇에게 작동 개시 명령을 전송
            bool result = await SendCommandAndCheckCONF(robotIndex, RUN_REQ);

            if (result) // Toggle Image
            {
                Btn_RUN[robotIndex].Enabled = false;
                Btn_STOP[robotIndex].Enabled = true; Btn_HOME[robotIndex].Enabled = true;

                //Btn_RUN[robotIndex].BackColor = Color.Blue;
                TBox_Status[robotIndex].Text = "작동중";
                TBox_Status[robotIndex].ForeColor = Color.Blue;

                // [ ] 작업 상태를 DB에 저장한다. 
                DB.insertWorkLog(robotIndex, G.RUN_BTN_PRESSED, "");
            }
            else MessageBox.Show(@"작동 개시 명령 전달 실패", @"로봇 무응답", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private async void stopActionAsync(int robotIndex)
        {
            OutputMesssage("[중지 #" + (robotIndex + 1) + "] 버튼 실행");

            // 해당 로봇에게 작업 중단 명령을 전송
            bool result = await SendCommandAndCheckCONF(robotIndex, STOP_REQ);

            if (result) // Toggle Image
            {
                Btn_STOP[robotIndex].Enabled = false;
                Btn_RUN[robotIndex].Enabled = true; Btn_HOME[robotIndex].Enabled = true;
                TBox_Status[robotIndex].Text = "작동 중지";
                TBox_Status[robotIndex].ForeColor = Color.Red;

                // [ ] 작업 상태를 DB에 저장한다.  
                DB.insertWorkLog(robotIndex, G.STOP_BTN_PRESSED, "");
            }
            else MessageBox.Show(@"작업 중단 명령 전달 실패", @"로봇 무응답", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private async void homeActionAsync(int robotIndex)
        {
            OutputMesssage("[홈 #" + (robotIndex + 1) + "] 버튼 실행");

            // 해당 로봇에게 홈 복귀 명령을 전송
            bool result = await SendCommandAndCheckCONF(robotIndex, HOME_REQ);

            if (result) // Toggle Image
            {
                Btn_HOME[robotIndex].Enabled = false;
                Btn_RUN[robotIndex].Enabled = true; Btn_STOP[robotIndex].Enabled = true;

                TBox_Status[robotIndex].Text = "홈 복귀중";
                TBox_Status[robotIndex].ForeColor = Color.Red;

                DB.insertWorkLog(robotIndex, G.HOME_BTN_PRESSED, "");

            }
            else MessageBox.Show(@"홈 복귀 명령 전달 실패", @"로봇 무응답", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private async void OT_VActionAsync(int robotIndex)
        {
            OutputMesssage("[수직방향 #" + (robotIndex + 1) + "] 버튼 선택");

            // 해당 로봇에게 방향 설정 명령을 전송
            bool result = await SendCommandAndCheckCONF(robotIndex, OT_V_REQ);

            if (result)
            {
                OT_V[robotIndex].Checked = true;
                G.OT[robotIndex] = G.VERTICAL;
                //OutputMesssage("OT 명령 : 수직 설정 완료");
                TBox_Status[robotIndex].Text = "수직 방향 동작 설정 완료";
                TBox_Status[robotIndex].ForeColor = Color.Blue;

                DB.insertWorkLog(robotIndex, G.OT_V_BTN_PRESSED, "");

            }
            else MessageBox.Show(@"수직 방향 설정 오류", @"로봇 무응답", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }

        private async void OT_HActionAsync(int robotIndex)
        {
            OutputMesssage("[수평방향 #" + (robotIndex + 1) + "] 버튼 선택");

            // 해당 로봇에게 방향 설정 명령을 전송
            bool result = await SendCommandAndCheckCONF(robotIndex, OT_H_REQ);

            if (result)
            {
                OT_H[robotIndex].Checked = true;
                G.OT[robotIndex] = G.HORIZONTAL;
                //OutputMesssage("OT 명령 : 수평 설정 완료");
                TBox_Status[robotIndex].Text = "수평 방향 동작 설정 완료";
                TBox_Status[robotIndex].ForeColor = Color.Blue;

                DB.insertWorkLog(robotIndex, G.OT_H_BTN_PRESSED, "");

            }
            else MessageBox.Show(@"수평 방향 설정 오류", @"로봇 무응답 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void optionAction(int robotIndex)
        {
            //// 세부설정 화면으로 이동

            OutputMesssage("세부설정 #" + (robotIndex + 1) + " 버튼 실행");

            G.CurrentRobotNumer = robotIndex;
            OptionSetting form = new OptionSetting();
            form.ShowDialog();
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
                        comInfo.ForeColor = Color.Blue;
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

        private void loRa연결확인ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CheckLoRaPortAsync(true);
        }

        private void 발전소목록관리ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PlantListManagement form = new PlantListManagement();
            form.ShowDialog();

            // refresh the combobox
            LinkComboBoxPlantList();
        }

        private void plantListManagementBtn_Click(object sender, EventArgs e)
        {
            PlantListManagement form = new PlantListManagement();
            form.ShowDialog();

            // refresh the combobox
            LinkComboBoxPlantList();
        }

        private void 보고서작성ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReportForm form = new ReportForm();
            form.Show();
        }

        public void OutputMesssage(string line)
        {
            output.AppendText(line + Environment.NewLine); // new line 추가

            output.Select(output.Text.Length, 0);// scroll 항상 아래
            output.ScrollToCaret();
            output.SelectionColor = G.DefaultColor; // 원상 복구

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] plantinfo = new string[2];
            plantinfo = comboBox1.Text.ToString().Split(new string[] { " : " }, StringSplitOptions.None);
            G.CurrentPlantNumber = plantinfo[0];
            G.CurrentPlantName = plantinfo[1];

            //if (G.DEBUG) Console.WriteLine("Selected Plant : <" + G.CurrentPlantNumber + ">,<" + G.CurrentPlantName + ">");
        }

        private void SoundBeep(int cord, int time)
        {
            if (cord == 0) Console.Beep(262, time); //도 
            else if (cord == 1) Console.Beep(294, time); //레
            else if (cord == 2) Console.Beep(330, time); //미 
            else if (cord == 3) Console.Beep(349, time); //파
            else if (cord == 4) Console.Beep(392, time); //솔 
            else if (cord == 5) Console.Beep(440, time); //라
            else if (cord == 6) Console.Beep(494, time); //시
            else Console.Beep(523, time); //도
        }

    }
}
