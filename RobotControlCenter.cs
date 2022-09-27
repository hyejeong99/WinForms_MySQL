using RobotControlCenter;
using System;
using System.Collections.Generic;
using System.Data;
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
        // 명령 코드 
        public const string MSG_HEADER = "LX";  // R->C
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
        List<string> recvMsgs = new List<string>();
        List<string> confMsgs = new List<string>();
        List<string> addrMsgs = new List<string>();
        List<string> sendMsgs = new List<string>();

        // 로봇 통신 주소 보관용 리스트 - 이름순 정렬된 구조 - 향후 사용 예정
        SortedDictionary<string, int> RobotSet = new SortedDictionary<string, int> ();

        // 각종 component를 배열로 관리 처리하기 위한 선언
        //// Buttons : R/S/H/OPTION
        public Button[] Btn_RUN = new Button[G.ROBOT_CNT];
        public Button[] Btn_STOP = new Button[G.ROBOT_CNT];
        public Button[] Btn_HOME = new Button[G.ROBOT_CNT];
        public Button[] Btn_OPTION = new Button[G.ROBOT_CNT];

        // TEXT BOXes
        TextBox[] TBox_RobotName = new TextBox[G.ROBOT_CNT];
        RichTextBox[] TBox_Status = new RichTextBox[G.ROBOT_CNT];

        // PROGRESSBARs
        ProgressBar[] Progress = new ProgressBar[G.ROBOT_CNT];
        RichTextBox[] TBox_Progress = new RichTextBox[G.ROBOT_CNT];

        // Battery Infos
        ProgressBar[] BatteryLevel = new ProgressBar[G.ROBOT_CNT];
        RichTextBox[] TBox_Battery = new RichTextBox[G.ROBOT_CNT];

        // Radio Buttons
        RadioButton[] OT_H = new RadioButton[G.ROBOT_CNT];
        RadioButton[] OT_V = new RadioButton[G.ROBOT_CNT];


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
            CheckLoRaPortAsync(false); // 성공시에는 메시지 없음

            // COMBO BOX에 실제 DATA 연결
            LinkComboBoxPlantList();

            //여러개의 동일 컴포넌트를 한번에 처리하기 위한 작업 - 작동 잘되나, 버튼의 경우, 테이블 레이아웃인 경우 미작동 
            LinkArrayComponent();

            // 읽은 로봇 정보 및 설정을 화면에 표시
            DisplayRobotInformation();

            //////////////////// JUST FOR SAMPLE TEST : RICHTEXTBOX, COLOR
            if (G.DEBUG)
            {
                /// ProgressBar 색상 변경을 적용하려면, 메인 program.cs에서 아래 문장을 삭제해야 제대로 동작함
                // Application.EnableVisualStyles(); 윈도우즈 UI 표준화 작업

                Progress[0].Value = 25; TBox_Progress[0].Text = "25";
                Progress[1].Value = 75; TBox_Progress[1].Text = "75";
                Progress[2].Value = 3; TBox_Progress[2].Text = "3";
                Progress[3].Value = 10; TBox_Progress[3].Text = "10";
                Progress[4].Value = 5; TBox_Progress[4].Text = "5";

                BatteryLevel[0].Value = 25; TBox_Battery[0].Text = "25";
                BatteryLevel[1].Value = 75; TBox_Battery[1].Text = "75";
                BatteryLevel[2].Value = 3; TBox_Battery[2].Text = "3";
                BatteryLevel[3].Value = 20; TBox_Battery[3].Text = "20";
                BatteryLevel[4].Value = 5; TBox_Battery[4].Text = "5";

                //status5.SelectionAlignment = HorizontalAlignment.Center;
                //battery5.SelectionAlignment = HorizontalAlignment.Center;

                for (int i = 0; i < G.ROBOT_CNT; i++)
                {
                    if (Progress[i].Value <= 5)
                    {
                        Progress[i].ForeColor = Color.Red;
                        TBox_Progress[i].ForeColor = Color.Red;
                    }
                    if (BatteryLevel[i].Value <= 10)
                    {
                        BatteryLevel[i].ForeColor = Color.Red;
                        TBox_Battery[i].ForeColor = Color.Red;
                    }
                }
            }
        }

        private void DisplayRobotInformation()
        {
            // [1] 저장된 로봇명을 보여줌
            robotName1.Text = G.robotID[0];
            robotName2.Text = G.robotID[1];
            robotName3.Text = G.robotID[2];
            robotName4.Text = G.robotID[3];
            robotName5.Text = G.robotID[4];

            // [2] 기존 OT 설정 - 수평/수직 값을 보여줌
            for (int i = 0; i < G.ROBOT_CNT; i++)
            {
                if (G.OT[i] == G.VERTICAL) OT_V[i].Checked = true;
                else OT_H[i].Checked = true;
            }

            // LSize, RSize는 읽어 오되, 화면에 따로 보여주지는 않음 (세부설정시 보여줌)

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

            // 로봇명/주소 세트를 생성
            for(int i = 0; i < G.ROBOT_CNT; i++)
            {
                RobotSet.Add(G.robotID[i], G.robotAddress[i]);
            }

            // 현재 로봇/주소 목록 출력
            foreach (var s in RobotSet)
            {
                if(G.DEBUG) OutputMesssage("로봇명/주소 = " + s.Key + " / " + s.Value);
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
            for (int i = 0; i < G.ROBOT_CNT; i++)
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
                    if (G.DEBUG) OutputMesssage($"< # {i} : {recvMsgs[i]} >" + " - " + " MESG");
                    if (G.DEBUG) Console.WriteLine($"< {recvMsgs[i]} >" + " : " + " MESG");

                    // 통신용 헤더 정보와 송수신 메시지 분리
                    int index = recvMsgs[i].IndexOf("=");
                    string[] parts = recvMsgs[i].Substring(index + 1).Split(',');

                    //// 수신 메시지/명령 추출 
                    ///// [1] 송신자 주소
                    int senderAddr = int.Parse(parts[0]);
                    int senderIndex = getRobotIndex(senderAddr);

                    //// [2] 전달된 메시지 크기
                    int dataSize = int.Parse(parts[1]);
                    if (dataSize < 2) // 
                    {
                        ErrorMessage(@"비거나 불완전한 메시지를 제거");
                        return;
                    }

                    //// [3] 전달된 메시지 본문 검사
                    ///// Header Check "LX" : All Messages should start with "LX"
                    if (!parts[2].Equals(MSG_HEADER)) continue; // "LX"로 시작하지 않으면 무시

                    //// [4] 명령어에 따른 작업
                    string CmdCode = parts[3]; // 명령어 종류 파악

                    // 명령어 종류에 따라 해당 작업 수행
                    if (CmdCode.Equals(REGISTER_REQ)) //  로봇 등록 명령 처리 R->C
                    {
                        string robotName = parts[4]; // 로봇 등록명 추출

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, REGISTER_CNF);

                        // [2] 로봇명을 추출하여, 관련 업데이트 작업 필요
                        ///메인화면 정보 업데이트 + DB 업데이트
                        G.robotID[senderIndex] = robotName;
                        TBox_RobotName[senderIndex].Text = robotName;
                        TBox_Status[senderIndex].Text = "로봇 등록, 주소 = " + senderAddr;

                        // 작업 버튼 작동 상태로 리셋
                        Btn_RUN[senderIndex].Enabled = true;
                        Btn_STOP[senderIndex].Enabled = true;
                        Btn_HOME[senderIndex].Enabled = true;

                        //if (G.DEBUG) Console.WriteLine("robot index = " + senderIndex + " // Robot Name = " + G.robotID[senderIndex]);

                        if(!RobotSet.ContainsKey(robotName)) // 로봇명 일치하는 것이 없으면, 로봇명과 주소를 신규 등록
                            RobotSet.Add(robotName, senderAddr);
                        else if (RobotSet.ContainsKey(robotName) && RobotSet[robotName] != senderAddr) // 같은 로봇명이지만, 다른 주소이면 나중 것 사용                       {
                        {
                            RobotSet.Remove(robotName); // 기존 정보 삭제
                            RobotSet.Add(robotName, senderAddr);  // 새로운 정보로 대체
                            OutputMesssage("로봇명 "+ robotName + "의 통신 주소가 " + senderAddr + "으로 변경되었습니다.");
                            //MessageBox.Show("예상치 못한 경우 발생");
                        }
                        
                        if(RobotSet.Count > G.ROBOT_CNT)
                        {
                            MessageBox.Show("예상치 못한 경우 발생, 등록 로봇의 개수가 너무 많습니다.");

                        }

                        foreach (var s in RobotSet)
                        {
                            Console.WriteLine("로봇명/주소 = " + s.Key + " / " + s.Value);
                        }

                        recvMsgs.RemoveAt(i);  // 해당 메시지 삭제

                        // 로봇명 변경시 언제든지 자동으로 설정 파일을 저장한다.
                        G.CNFSaveFile(); // 현재 설정을 자동 저장한다.
                    }
                    else if (CmdCode.Equals(ERR_STATUS_REQ))
                    {
                        string errorCodeStr = parts[4]; // 에러 코드를 Status에 보여준다.

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, ERR_STATUS_CNF);

                        // [2] 에러 코드를 Status 란에 보여준다. 추가 액션/DB 관리 가능
                        TBox_Status[senderIndex].Text = "작동오류 : " + errorCodeStr;
                        TBox_Status[senderIndex].ForeColor = Color.Red;

                        recvMsgs.RemoveAt(i);  // 해당 메시지 삭제
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
                            BatteryLevel[senderIndex].ForeColor = Color.Red;
                            TBox_Battery[senderIndex].ForeColor = Color.Red;
                        }
                        else
                        {
                            BatteryLevel[senderIndex].ForeColor = Color.Blue;
                            TBox_Battery[senderIndex].ForeColor = Color.Blue;
                        }
                        recvMsgs.RemoveAt(i); // 해당 메시지 삭제
                    }
                    else if (CmdCode.Equals(CLEAN_COUNT)) // NO CONFIRM
                    {
                        // [1] CONFIRM은 불필요
                        // [2] 전달된 추가적인 정보, 즉 작업 진도량을 계산하여 보여줌
                        ///////////////////////// ProgressBar 값 변경

                        recvMsgs.RemoveAt(i); // 해당 메시지 삭제
                    }
                    else if (CmdCode.Equals(FINISH_STATUS_REQ)) // 작업 종료 보고
                    {
                        // string statusCodeStr = parts[4]; // 작업 종료 사실을 Status에 보여준다.

                        // [1] CONFIRM 메시지를 먼저 보내고
                        SendConfMsgToRobot(senderAddr, FINISH_STATUS_CNF);

                        // [2] 작업 종료 사실을 Status란에 보여준다. 추가 액션/DB 관리 가능
                        TBox_Status[senderIndex].Text = "작업 완료";



                        recvMsgs.RemoveAt(i);  // 해당 메시지 삭제
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
                            confMsgs.Add(recvMsgs[i]);

                        recvMsgs.RemoveAt(i);
                    }
                    else // 이 예외적인 경우는 발생하면 안됨. 코드 사용 잘못
                    {
                        if (G.DEBUG) OutputMesssage(@"잘못된 명령어 수신. 명령어 = " + CmdCode, Color.Red);
                        if (G.DEBUG) MessageBox.Show(@"시스템 오류 - 예상못한 경우가 발생했습니다.", "시스템 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        recvMsgs.RemoveAt(i); // 해당 메시지 삭제
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
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
                            if (G.DEBUG) OutputMesssage("No Confirm Msg. --명령 전송 실패");
                            if (G.DEBUG) Console.WriteLine(@"응답 메시지 없음");
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
            string HEADER_SEND = "AT+SEND=";
            string sendMesg = MSG_HEADER + "," + cmdCode; // "LX"+ "," + 명령어 코드
            String sendPacket = HEADER_SEND + robotAddress + "," + sendMesg.Length + "," + sendMesg;

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
            ////////////////////////////////////////

            bool result = await SendCommand(Address, cmdCode);

            if (result) OutputMesssage(cmdCode + " #" + (robotIndex + 1) + " 명령 전송 완료(응답확인)");
            else OutputMesssage(cmdCode + " #" + (robotIndex + 1) + " 명령 전송 실패(무응답)");

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
                TBox_Status[robotIndex].Text = "작동 중";
                TBox_Status[robotIndex].ForeColor = Color.Blue;

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

        private void LinkComboBoxPlantList()
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand("select PlantNumber, PlantName from " + TBL_NAME, con);
            DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            comboBox1.Items.Clear();
            while (sdr.Read())
            {
                comboBox1.Items.Add(sdr.GetString(0) + " : " + sdr.GetString(1));
            }
            //dt.Load(sdr);
            con.Close();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            //G.CurrentPlantNumber = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            //G.CurrentPlantName = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            //comboBox1.DataSource = dt;
            //comboBox1.DisplayMember = "PlantName";
            //comboBox1.ValueMember = "PlantNumber";
            // Console.WriteLine(G.CurrentPlantNumber + "//" + G.CurrentPlantName + "//");

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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] plantinfo = new string[2];
            plantinfo = comboBox1.Text.ToString().Split(new string[] { " : " }, StringSplitOptions.None);
            G.CurrentPlantNumber = plantinfo[0];
            G.CurrentPlantName = plantinfo[1];

            if (G.DEBUG) Console.WriteLine("<" + G.CurrentPlantNumber + ">,<" + G.CurrentPlantName + ">");
        }

    }
}
