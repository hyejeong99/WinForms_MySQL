using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace RobotCC
{
    public static class G  // 공통 변수 모음
    {
        // debug mode 설정
        public static bool DEBUG = true;

        // CNF 파일, LOG 파일은 현재 디렉토리에 저장
        public static string MySystemFolder = @"C:\RobotCC";
        public static string CNFFileName = MySystemFolder + @"\rcc.cnf";
        public static string LogFile = MySystemFolder + @"\rcc_log_";
        public static string DBFileName = MySystemFolder + @"\RobotDB.mdf";

        // VERSION
        public static string VERSIONNAME = Application.ProductName + " Ver. " + Application.ProductVersion.ToString();
        public static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MySystemFolder + @"\RobotDB.mdf;Integrated Security=True;Connect Timeout=30";


        // 상수 정의 
        public const int ROBOT_MAX_CNT = 10; // 10 Robots maximum
        public const int CONF_WAIT_DURATION_1 = 1000; // 1초 응답 대기 1차
        public const int CONF_WAIT_DURATION_2 = 2000; // 2초 응답 대기 2차
        public const int LORA_TEST_DELAY = 2000; // 2초 응답 대기 
        public const double DEFAULT_L_SIZE = 0; // 가로 길이 m
        public const double DEFAULT_R_SIZE = 0; // 세로 길이 m
        public const double DEFAULT_WIDTH = 0.5;  // 로봇 청소 작업 기본 폭(너비)

        public const int UNDEFINED = -1;
        public const int VERTICAL = 0;
        public const int HORIZONTAL = 1;
        public const int DEFAULT_OT = VERTICAL; // 상하 동작

        public const int AUTO_OFF = 0;
        public const int AUTO_ON = 1;
        public const int DEFAULT_AUTO = AUTO_OFF; // 수동
        public const string DEFAULT_SERIALPORT_NAME = "Com1"; // 초기값

        public static bool CONF_WAIT_MODE = false;

        public static int CurrentRobotNumer = -1;
        public static string SelectedSerialPortName = DEFAULT_SERIALPORT_NAME;
        public static string oldSelectedSerialPortName = DEFAULT_SERIALPORT_NAME;


        // WORK_LOG DB에 사용되는 상태 정보 종류 
        public const string REGISTER = "REGISTER";
        public const string FINISHED = "FINISHED";
        public const string ERROR_STOP = "ERROR_STOP";
        public const string REPORT_CLEAN_COUNTER = "EDGE_CNT";

        public const string RUN_BTN_PRESSED = "RUN_BTN";
        public const string STOP_BTN_PRESSED = "STOP_BTN";
        public const string HOME_BTN_PRESSED = "HOME_BTN";
        public const string OT_V_BTN_PRESSED = "OT_VERTICAL_BTN";
        public const string OT_H_BTN_PRESSED = "OT_HORIZONTAL_BTN";



        // Robot 정보 저장용 : 현재 ROBOT_MAX_CNT 개수만큼 설정
        public static int ROBOT_REG_CNT = 0;    // 현재 사용 중인 로봇의 대수
        public static int SAVED_ROBOT_CNT = 0;     // 설정 저장된 로봇의 대수 - 기존의 정보 최대한 유지하는 로봇의 대수
        public static int[] robotAddress = new int[ROBOT_MAX_CNT]; // 로봇별 통신 주소 - 기본값, 실제값은 rcc.cnf에서 추출
        public static string[] robotID = new string[ROBOT_MAX_CNT]; // 로봇 ID(Name) 저장용
        public static double[] LSize = new double[ROBOT_MAX_CNT]; // 로봇별 청소할 가로 길이
        public static double[] RSize = new double[ROBOT_MAX_CNT]; // 로봇별 청소할 세로 길이
        public static int[] OT = new int[ROBOT_MAX_CNT]; // 로봇별 청소할 방향 
        //public static int[] AUTOSTART = new int[ROBOT_MAX_CNT]; // 자동 시작 여부 ON/OFF

        // 추후 수정....
        // OSY EDGE_CNT 보다는 WORK_PERCENTAGE를 DB에 저장하는 것이 효과적일 듯
        // 왜냐면, EDGE_CNT는 L, R 값이외에 OT값까지 알아야 청소 면적 계산 가능함. 
        // 반면, WORK_PERCENTAGE는 L, R 값만 알면 청소 면적 계산 가능.
        public static int[] EDGE_CNT = new int[ROBOT_MAX_CNT]; // 로봇별 에지 도달 횟수
        public static int[] WORK_PERCENTAGE = new int[ROBOT_MAX_CNT]; // 로봇별 작업 진행률 


        //// 설정 파일에 정보가 있는 목록 구성
        ////public static Dictionary<string, int> ExistingRobotNameAndAddress = new Dictionary<string, int>();
        ////public static Dictionary<string, int> ExistingRobotNameAndOT = new Dictionary<string, int>();
        ////public static Dictionary<string, int> ExistingRobotNameAndAutoStart = new Dictionary<string, int>();

        // Control 센터의 LoRa Address 정보
        public static string MyLoRaAddress; // 1000?

        // 현재 작업 중인 발전소 정보
        public static string CurrentPlantNumber;
        public static string CurrentPlantName;

        // 로봇 통신 주소 보관용 리스트 - 입력순으로 목록 관리 - 향후 사용 예정
        //public static Dictionary<string, int> RobotSet = new Dictionary<string, int>();

        // 기본 색상
        public static Color DefaultColor = Color.Black;

        public static void CheckAndMakeFolder()
        {
            // C:\RobotCC 폴더 생성
            if (!Directory.Exists(MySystemFolder)) Directory.CreateDirectory(MySystemFolder);
        }

        public static void CNFLoadFile()
        {
            FileInfo fi = new FileInfo(G.CNFFileName);

            G.SelectedSerialPortName = G.DEFAULT_SERIALPORT_NAME;

            // 로봇 개수가 가변이므로 일단, 모든 배열값의 기본은 초기화하고 시작
            //for (int i = 0; i < G.ROBOT_MAX_CNT; i++)
            //{
            //    G.robotID[i] = "";
            //    G.LSize[i] = G.DEFAULT_L_SIZE;
            //    G.RSize[i] = G.DEFAULT_R_SIZE;
            //    G.OT[i] = G.DEFAULT_OT;
            //    //G.AUTOSTART[i] = G.DEFAULT_AUTO;
            //    G.robotAddress[i] = 0; // 초기값, 0

            //    //if (G.DEBUG) Console.WriteLine("R#" + i + " " + G.robotID[i] + ":" + G.LSize[i] + "/" + G.RSize[i] + "/" + G.OT[i] + "/" + G.AUTOSTART[i] + "/" + G.robotAddress[i]);
            //    if (G.DEBUG) Console.WriteLine("설정 정보 #" + i + " " + G.robotID[i] + G.OT[i]);
            //}

            // 설정 추출 로봇 정보 초기화
            //ExistingRobotNameAndAddress.Clear();
            //ExistingRobotNameAndOT.Clear();
            //ExistingRobotNameAndAutoStart.Clear();

            if (fi.Exists) // rcc 설정파일 존재시
            {
                string[] lines = File.ReadAllLines(G.CNFFileName);

                // rcc 설정 파일 내용 중 PortName만 활용
                G.SelectedSerialPortName = lines[0];

                // 로봇별 정보 추출 - 라인수만큼만 추출 - 사용자 편의상 일부 값은 기존 세팅 활용
                //for (int i = 0; i < lines.Length - 1; i++)
                //{
                //    if (!lines[i + 1].Contains(",")) continue; // 내용이 빈 라인의 경우, 무시

                //    string[] parts = lines[i + 1].Split(',');
                //    //if (parts.Length != 6) continue; // 잘못된 경우

                //    //G.robotID[i] = parts[0];
                //    //G.LSize[i] = double.Parse(parts[1]);
                //    //G.RSize[i] = double.Parse(parts[2]);
                //    //G.OT[i] = int.Parse(parts[3]);
                //    //G.AUTOSTART[i] = int.Parse(parts[4]);
                //    //G.robotAddress[i] = int.Parse(parts[5]);
                //    G.robotID[i] = parts[0];
                //    G.OT[i] = int.Parse(parts[1]);

                //    // 설정 정보 목록을 일단 저장하여 default 값으로 활용
                //    //ExistingRobotNameAndAddress.Add(G.robotID[i], G.robotAddress[i]);
                //    //ExistingRobotNameAndOT.Add(G.robotID[i], G.OT[i]);
                //    //ExistingRobotNameAndAutoStart.Add(G.robotID[i], G.AUTOSTART[i]);

                //    //if (G.DEBUG) Console.WriteLine("R#" + i + " " + G.robotID[i] + ":" + G.LSize[i] + "/" + G.RSize[i] + "/" + G.OT[i] + "/" + G.AUTOSTART[i] + "/" + G.robotAddress[i]);
                //    if (G.DEBUG) Console.WriteLine("설정 정보 #" + i + " " + G.robotID[i] + "," + G.OT[i]);
                //}
            }

            G.ROBOT_REG_CNT = 0; // 일단 항상 0으로 초기화 
            //G.SAVED_ROBOT_CNT = G.ExistingRobotNameAndAddress.Count; // 기존 설정값을 가진 로봇의 수
            //G.SAVED_ROBOT_CNT = G.ExistingRobotNameAndOT.Count; // 기존 설정값을 가진 로봇의 수

        }

        public static void CNFSaveFile()
        {
            string lines = "";

            lines += G.SelectedSerialPortName + Environment.NewLine;

            //for (int i = 0; i < G.ROBOT_REG_CNT; i++)
            //{
            //    //lines += G.robotID[i] + "," + G.LSize[i] + "," + G.RSize[i] + "," + G.OT[i] + "," +
            //    //         G.AUTOSTART[i] + "," + G.robotAddress[i] + Environment.NewLine;
            //    lines += G.robotID[i] + "," + G.OT[i] + Environment.NewLine;
            //}
            File.WriteAllText(G.CNFFileName, lines);

            Console.WriteLine(@"설정 파일 저장 완료");
            Console.WriteLine(lines);
        }

        public static void SaveLogFile(string logData)
        {
            DateTime now = DateTime.Now;
            string logFile = G.LogFile + now.ToString("yyyyMMddHHmm") + ".log";
            //string logFile = G.LogFile + now.Year + now.Month + now.Day + now.Hour + now.Minute + ".log";

            File.WriteAllText(logFile, logData);

            Console.WriteLine(@"로그 파일 저장 완료");
        }

        public static string TimeStamp()
        {
            DateTime now = DateTime.Now;
            string date = now.ToShortDateString();
            string time = string.Format("{0:d2}", now.Hour) + ":" + string.Format("{0:d2}", now.Minute) + ":" + string.Format("{0:d2}", now.Second);
            //string CurrentTimeStamp = now.ToString(format: "yyyy-MM-dd HH:mm:tt"); // 여기에 오전/오후가 출력됨
            //return CurrentTimeStamp;
            return (date + " " + time);
        }

        public static string TimeStamp(DateTime datetime)
        {
            string dateStr = datetime.ToShortDateString();
            string timeStr = string.Format("{0:d2}", datetime.Hour) + ":" + string.Format("{0:d2}", datetime.Minute) + ":" + string.Format("{0:d2}", datetime.Second);
            //string CurrentTimeStamp = now.ToString(format: "yyyy-MM-dd HH:mm:tt"); // 여기에 오전/오후가 출력됨
            //return CurrentTimeStamp;
            return (dateStr + " " + timeStr);
        }

    }

}
