using System;
using System.IO;
using System.Windows.Forms;


namespace RobotCC
{
    public static class G  // 공통 변수 모음
    {
        // debug mode 설정
        public static bool DEBUG = true;


        // CNF 파일, LOG 파일은 현재 디렉토리에 저장
        //public static string CNFFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\rcc.cnf";
        //public static string LogFileName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\robotcc.log";
        public static string MySystemFolder = @"C:\RobotCC";
        public static string CNFFileName = MySystemFolder + @"\rcc.cnf";
        public static string LogFile = MySystemFolder + @"\rcc_log_";
        public static string DBFileName = MySystemFolder + @"\RobotDB.mdf";

        // VERSION
        public static string VERSIONNAME = Application.ProductName + " Ver. " + Application.ProductVersion.ToString();
        public static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + MySystemFolder + @"\RobotDB.mdf;Integrated Security=True;Connect Timeout=30";


        // 상수 정의 
        public const int ROBOT_CNT = 5; // 5 Robots maximum
        public const int CONF_WAIT_DURATION_1 = 1000; // 1초 응답 대기 1차
        public const int CONF_WAIT_DURATION_2 = 3000; // 3초 응답 대기 2차
        public const int LORA_TEST_DELAY = 3000; // 3초 응답 대기 
        public const double DEFAULT_L_SIZE = 4.0; // LoRA 가로
        public const double DEFAULT_R_SIZE = 2.5; // 1m 세로

        public const int VERTICAL = 0;
        public const int HORIZONTAL = 1;
        public const int AUTO_OFF = 0;
        public const int AUTO_ON = 1;
        public const int DEFAULT_OT = VERTICAL; // 수직 방향
        public const int DEFAULT_AUTO = AUTO_OFF; // 수동
        public const string DEFAULT_SERIALPORT_NAME = "Com1"; // 초기값

        public static bool confirmWaitingMode = false;

        public static int CurrentRobotNumer = -1;
        public static string SelectedSerialPortName = DEFAULT_SERIALPORT_NAME;
        public static string oldSelectedSerialPortName = DEFAULT_SERIALPORT_NAME;

        // Robot 정보 저장용 : 현재 ROBOT_CNT 개수만큼 설정
        public static int[] robotAddress = new int[ROBOT_CNT] { 10, 20, 30, 40, 50 }; // 로봇별 통신 주소
        public static string[] robotID = new string[ROBOT_CNT]; // 로봇 ID(Name) 저장용
        public static double[] LSize = new double[ROBOT_CNT]; // 로봇별 청소할 가로 길이
        public static double[] RSize = new double[ROBOT_CNT]; // 로봇별 청소할 세로 길이
        public static int[] OT = new int[ROBOT_CNT]; // 로봇별 청소할 방향 
        public static int[] AUTOSTART = new int[ROBOT_CNT]; // 자동 시작 여부 ON/OFF

        // Control 센터의 LoRa Address 정보
        public static string MyLoRaAddress;

        // 현재 작업 중인 발전소 정보
        public static string CurrentPlantNumber;
        public static string CurrentPlantName;


        public static void CheckAndMakeFolder()
        {
            // C:\RobotCC 폴더 생성
            if (!Directory.Exists(MySystemFolder)) Directory.CreateDirectory(MySystemFolder);
        }

        public static void CNFLoadFile()
        {
            FileInfo fi = new FileInfo(G.CNFFileName);

            if (fi.Exists)
            {
                string[] lines = File.ReadAllLines(G.CNFFileName);

                // PortName 읽기
                G.SelectedSerialPortName = lines[0];

                // 로봇별 정보 추출
                for (int i = 0; i < G.ROBOT_CNT; i++)
                {
                    string[] parts = lines[i + 1].Split(',');

                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (j == 0) G.robotID[i] = parts[j];
                        else if (j == 1) G.LSize[i] = double.Parse(parts[j]);
                        else if (j == 2) G.RSize[i] = double.Parse(parts[j]);
                        else if (j == 3) G.OT[i] = int.Parse(parts[j]);
                        else if (j == 4) G.AUTOSTART[i] = int.Parse(parts[j]);

                    }

                    if (G.DEBUG) Console.WriteLine("" + i + " " + G.robotID[i] + ":" + G.LSize[i] + "/" + G.RSize[i] + "/" + G.OT[i]);
                }
            }
            else // 설정 파일이 없는 경우, 초기값 설정
            {
                G.SelectedSerialPortName = G.DEFAULT_SERIALPORT_NAME;

                for (int i = 0; i < G.ROBOT_CNT; i++)
                {
                    G.robotID[i] = "UnNamed";
                    G.LSize[i] = G.DEFAULT_L_SIZE;
                    G.RSize[i] = G.DEFAULT_R_SIZE;
                    G.OT[i] = G.DEFAULT_OT;
                    G.AUTOSTART[i] = G.DEFAULT_AUTO;
                }
            }
            //else MessageBox.Show(G.CNFFileName, "FILE ERROR");

            Console.WriteLine("CNF 설정 파일 적용");

        }


        public static bool CheckDBFile()
        {
            FileInfo fi = new FileInfo(G.DBFileName);

            if (fi.Exists)
            {
                return true;
            }
            else return false;

        }

        public static void CNFSaveFile()
        {

            string lines = "";

            lines += G.SelectedSerialPortName + Environment.NewLine;

            for (int i = 0; i < G.ROBOT_CNT; i++)
            {
                lines += robotID[i] + "," + LSize[i] + "," + RSize[i] + "," + OT[i] + "," + AUTOSTART[i] + Environment.NewLine;
            }
            File.WriteAllText(G.CNFFileName, lines);

            Console.WriteLine(@"설정 파일 저장 완료");
            Console.WriteLine(lines);
        }

        public static void SaveLogFile(string logData)
        {
            DateTime now = DateTime.Now;
            string logFile = G.LogFile + now.Year + now.Day + now.Hour + now.Minute + ".log";

            File.WriteAllText(logFile, logData);

            Console.WriteLine(@"로그 파일 저장 완료");
        }


    }


}
