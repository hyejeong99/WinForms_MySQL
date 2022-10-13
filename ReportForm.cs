using RobotControlCenter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Windows.Forms;

namespace RobotCC
{

    // Report 부분은 메인화면과 달리 추후 실행 가능하도록 G. 변수 사용을 최소화 하는 것이 필요
    // Current 변수 역시 리포트할 대상을 위해서 임시 사용해야 한다.
    public partial class ReportForm : Form
    {
        private string CurrentPlantNumber = null;
        private string CurrentPlantName = null;
        private string CurrentContactEmail = "없음";

        // 로봇명-날짜 리스트 구성
        private static Dictionary<string, List<string>> ReportRobotList = new Dictionary<string, List<string>>();
        //private static List<int> WorkingTimeList = new List<int>(); //위의 로봇/날짜 목록별 작업시간
        //private static List<int> WorkingAreaList = new List<int>(); //위의 로봇/날짜 목록별 작업면적

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
   
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            // [1] ReportData 테이블 내용 삭제 - 초기화
            string REPORT_TBL_NAME = "ReportData";
            SqlCommand cmd = new SqlCommand("delete from " + REPORT_TBL_NAME, con);
            cmd.ExecuteReader();
            con.Close();

            // [2] WorkLog에서 자료를 읽어 온다.
            // 필요한 정보를 다시 읽어 들인다.
            string TBL_NAME = "WorkLog";

            DateTime From = DateTime.Parse(dateTimeFrom.Value.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(dateTimeTo.Value.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            con.Open();

            // 날짜 기간 선택을 조건으로 넣는 경우, 다소 불확실한 결과가 나오므로 일단 모두 검색하고, 나중에 걸러냄
            cmd = new SqlCommand("select RId, TimeStamp, PlantNumber, State, LSize, RSize, Counter, Progress, Etc  from " + TBL_NAME + " where PlantNumber = @PlantNumber AND @DT_From <= TimeStamp AND TimeStamp <= @DT_To", con);
            //cmd = new SqlCommand("select RId, TimeStamp, PlantNumber, State, LSize, RSize, Counter, Progress, Etc  from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@DT_From", From.ToShortDateString());
            cmd.Parameters.AddWithValue("@DT_To", To.ToShortDateString());

            SqlDataReader sdr = cmd.ExecuteReader();
            //DataTable dt = new DataTable();
            //dt.Load(sdr);
            //dataGridView1.DataSource = dt;

            // [3] 테이블 내용을 보여주는 대신 <로봇명, 날짜> 및 해당하는 작업시간, 작업면적 목록을 구성한다.
            // [3-1] <로봇명, 날짜> 목록 구성 (로봇명과 날짜가 모두 일치하는 목록은 없어야 한다)
            while (sdr.Read())
            {
                string robotID = sdr.GetString(0);
                DateTime timeStamp = sdr.GetDateTime(1);

                //Console.WriteLine(timeStamp.ToShortDateString() + "  // " + From.ToShortDateString() + "//" + To.ToShortDateString());
                //Console.WriteLine(timeStamp.Date + "  // " + From.Date + "//" + To.Date);

                // 날짜 범위 검사
                //if (timeStamp.Date < From.Date || timeStamp.Date > To.Date) continue;

                if (!ReportRobotList.ContainsKey(robotID)) // 목록에 존재하지 않으면, 추가
                {
                    ReportRobotList.Add(robotID, new List<string>());
                    ReportRobotList[robotID].Add(timeStamp.ToShortDateString());
                    Console.WriteLine("로봇 추가 NAME/DATE = " + robotID + "/" + timeStamp.ToShortDateString());
                }
                else
                {
                    if (!ReportRobotList[robotID].Contains(timeStamp.ToShortDateString())) // 목록에 존재하나 날짜가 다르면 추가
                    {
                        ReportRobotList[robotID].Add(timeStamp.ToShortDateString());
                        Console.WriteLine("날짜 추가 NAME/DATE = " + robotID + "/" + timeStamp.ToShortDateString());
                    }
                }
            }

            con.Close();

            // [3-2] <로봇명, 날짜>가 일치하는 목록에 해당하는 작업시간, 작업면적을 계산하여 리스트로 구성
            if (G.DEBUG) Console.WriteLine("######################################");
            if (G.DEBUG) Console.WriteLine("TEST: ROBOT/DATE List");

            foreach (var s in ReportRobotList)
            {
                string robotID = s.Key;


                foreach (string rdate in ReportRobotList[robotID])
                {
                    if (G.DEBUG) Console.WriteLine("TEST: ROBOT/DATE List -  " + robotID + "/" + DateTime.Parse(rdate));

                    int workingTime = calcWorkingTime(robotID, DateTime.Parse(rdate));
                    double workingArea = calcWorkingArea(robotID, DateTime.Parse(rdate));

                    int Hour = workingTime / 3600;
                    int Minute = (workingTime % 3600) / 60;
                    int Second = (workingTime % 60);

                    //string workingTimeStr = Hour + ":" + Minute + ":" + Second;
                    string workingTimeStr = "" + workingTime ;
                    string workingAreaStr = "" + workingArea;

                    DB.insertReportTable(CurrentPlantNumber, s.Key, DateTime.Parse(rdate).Date, workingTimeStr, workingAreaStr);
                }
            }

            // [3] 생성된 테이블 내용을 보여준다. <로봇명, 날짜> 별로 작업시간, 작업면적을 계산하여 리스트로 구성
            DisplayReportTable();

        }

        // 이상하게 아래 변수는 외부에 선언해야 동작함
        DateTime startTime;
        DateTime endTime;

        // WorkLog 테이블에서 <로봇명, 날짜>에 해당하는 작업시간(누적)을 추출한다.
        private int calcWorkingTime(string robotID, DateTime reportDate)
        {
            DateTime From = DateTime.Parse(dateTimeFrom.Value.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(dateTimeTo.Value.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            // [1] WorkLog에서 자료를 읽어 온다.
            string TBL_NAME = "WorkLog";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            // 날짜 기간 선택을 조건으로 넣는 경우, 다소 불확실한 결과가 나오므로 일단 모두 검색하고, 나중에 걸러냄
            SqlCommand cmd = new SqlCommand("select TimeStamp, State from " + TBL_NAME + " where PlantNumber = @PlantNumber AND RId = @RobotID AND @DT_From <= TimeStamp AND TimeStamp <= @DT_To", con);
            //SqlCommand cmd = new SqlCommand("select TimeStamp, State from " + TBL_NAME + " where PlantNumber = @PlantNumber AND RId = @RobotID", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@RobotID", robotID);
            cmd.Parameters.AddWithValue("@DT_From", From.ToShortDateString());
            cmd.Parameters.AddWithValue("@DT_To", To.ToShortDateString());
            //cmd.Parameters.AddWithValue("@Date", reportDate); // 검색 조건이 정확히 동작하지 않음. ==> 이 조건은 루프안에서 제거

            SqlDataReader sdr = cmd.ExecuteReader();

            int workingTime = 0;
            bool startTimeExist = false;

            while (sdr.Read())
            {
                DateTime timeStamp = sdr.GetDateTime(0);    // DB의 TimeStamp 값
                string state = sdr.GetString(1).ToString(); // DB의 State 값
            
                if (!timeStamp.Date.Equals(reportDate.Date)) // 다른 날짜인 경우, 무시
                    continue;

                // state 종류 중 작동 시작/완료 관련된 것은 RUN_BTN_PRESSED(작동 시작 버튼),  STOP_BTN_PRESSED(정지 버튼),
                // ERROR_STOP(오류로 중단), FINISHED(정상 작업 완료) 등이 있음,
                if (state.Equals(G.RUN_BTN_PRESSED))
                {
                    startTime = timeStamp;
                    startTimeExist = true; //작업 시작시간 세팅
                }
                else if (state.Equals(G.FINISHED) || state.Equals(G.STOP_BTN_PRESSED) || state.Equals(G.ERROR_STOP))
                {
                    // 시작 시간은 없고 종료 시간만 검색되는 경우, 건너띔
                    if (startTimeExist == false) continue;

                    endTime = timeStamp;

                    if (startTime.CompareTo(endTime) >= 0)  // 시작과 끝 시간이 같거나 순서가 뒤바뀐 경우이면 예외적인 경우로 취급하여 무시
                        continue;

                    TimeSpan timeSpan = endTime - startTime;
                    workingTime += (int)timeSpan.TotalSeconds;

                    if (G.DEBUG)
                    {
                        Console.WriteLine("ROBOT ID : " + robotID + " WORKED FROM " + G.TimeStamp(startTime) + " TO " + G.TimeStamp(endTime));
                        Console.WriteLine("TIMESPAN = " + timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);
                        //Console.WriteLine("STATE = " + state);
                    }

                    startTimeExist = false; // 다음 시간 간격 검사를 위해 다시 false로 변경

                }
            }

            con.Close();

            return workingTime;
        }

        // WorkLog 테이블에서 <로봇명, 날짜>에 해당하는 작업 면적(누적)을 추출한다.
        private double calcWorkingArea(string robotID, DateTime reportDate)
        {
            DateTime From = DateTime.Parse(dateTimeFrom.Value.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(dateTimeTo.Value.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            // [1] WorkLog에서 자료를 읽어 온다.
            string TBL_NAME = "WorkLog";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            // 날짜 기간 선택을 조건으로 넣는 경우, 다소 불확실한 결과가 나오므로 일단 모두 검색하고, 나중에 걸러냄
            SqlCommand cmd = new SqlCommand("select TimeStamp, State, LSize, RSize, Counter, Progress from " + TBL_NAME + " where PlantNumber = @PlantNumber AND RId = @RobotID AND @DT_From <= TimeStamp AND TimeStamp <= @DT_To ", con);
            //SqlCommand cmd = new SqlCommand("select TimeStamp, State, LSize, RSize, Counter, Progress from " + TBL_NAME + " where PlantNumber = @PlantNumber AND RId = @RobotID", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@RobotID", robotID);
            cmd.Parameters.AddWithValue("@DT_From", From);
            cmd.Parameters.AddWithValue("@DT_To", To);
            //cmd.Parameters.AddWithValue("@Date", reportDate); // 검색 조건이 정확히 동작하지 않음. ==> 이 조건은 루프안에서 제거

            SqlDataReader sdr = cmd.ExecuteReader();

            double workingArea = 0;

            while (sdr.Read())
            {
                DateTime timeStamp = sdr.GetDateTime(0);        // DB의 TimeStamp 값
                string state = sdr.GetString(1);                // DB의 State 값
                double lsize = (double)sdr.GetSqlDouble(2);    // DB의 LSize 값
                double rsize = (double) sdr.GetSqlDouble(3);    // DB의 RSize 값
                int counter = sdr.GetInt32(4);      // DB의 counter 값
                int progress = sdr.GetInt32(5); ;     // DB의 progress 진도율

                if (!timeStamp.Date.Equals(reportDate.Date)) // 다른 날짜인 경우, 무시
                    continue;

                if (state.Equals(G.FINISHED)) // 작업 정상 종료의 경우
                {
                    progress = 100;
                    workingArea = lsize * rsize;
                    return workingArea;

                }
                else if (state.Equals(G.STOP_BTN_PRESSED) || state.Equals(G.ERROR_STOP)) // 작업 비정상 종료의 경우
                {
                    double newArea = lsize * rsize * progress / 100;
                    if (workingArea < newArea) workingArea = newArea;
                }

            }

            con.Close();

            return workingArea;
        }

        // 조건을 만족하도록 생성된 ReportData 테이블 내용을 보여준다. <로봇명, 날짜> 별로 작업시간, 작업면적을 보여준다
        private void DisplayReportTable()
        {
            string TBL_NAME = "ReportData";

            DateTime From = DateTime.Parse(dateTimeFrom.Value.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(dateTimeTo.Value.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open(); 
            
            SqlCommand cmd = new SqlCommand("select Date, RId, WorkingTime, WorkingArea from " + TBL_NAME + " where PlantNumber = @PlantNumber AND @DT_From <= Date AND Date <= @DT_To ORDER BY Date", con);
            //SqlCommand cmd = new SqlCommand("select RId, Date, WorkingTime, WorkingArea from " + TBL_NAME + " where PlantNumber = @PlantNumber", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@DT_From", From.ToShortDateString());
            cmd.Parameters.AddWithValue("@DT_To", To.ToShortDateString());

            SqlDataReader sdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();


            dt.Load(sdr);

            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["Date"].Width = 800;
            dataGridView1.Columns["RId"].Width = 800;
            dataGridView1.Columns["WorkingTime"].Width = 400;
            dataGridView1.Columns["WorkingArea"].Width = 400;


            dataGridView1.Columns["Date"].HeaderText = "작업 날짜(년월일)";
            dataGridView1.Columns["Date"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["RId"].HeaderText = "작업 로봇 ID";
            dataGridView1.Columns["RId"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["WorkingTime"].HeaderText = "작업 시간(누적, 초)";
            dataGridView1.Columns["WorkingTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["WorkingArea"].HeaderText = "작업 면적(제곱미터)";
            dataGridView1.Columns["WorkingArea"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
          
            return;
        }

    }
}
