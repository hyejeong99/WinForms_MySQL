using RobotControlCenter;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Net.Mail;
using System.Collections;
using System.Drawing.Printing;
using Font = System.Drawing.Font;
using Rectangle = System.Drawing.Rectangle;

using MySql.Data.MySqlClient;

namespace RobotCC
{



    // Report 부분은 메인화면과 달리 추후 실행 가능하도록 G. 변수 사용을 최소화 하는 것이 필요
    // Current 변수 역시 리포트할 대상을 위해서 임시 사용해야 한다.
    public partial class ReportForm : Form
    {
        //pdf 파일 이름 위한 변수
        public int pdfNum = 0;

        private string CurrentPlantNumber = null;
        private string CurrentPlantName = null;
        private string CurrentContactEmail = "없음";

        // 로봇명-날짜 리스트 구성 - 비교시 표준화를 위해 날짜는 string(yyyy-MM-dd)으로 저장
        private Dictionary<string, List<string>> ReportRobotList = new Dictionary<string, List<string>>();

        // 발전소별 E-Mail 값 저장용 
        private Dictionary<string, string> PlantEmailList = new Dictionary<string, string>();

        // 로봇당 해당 날짜의 작업 시간 및 면적
        private int workingTime;
        private double workingArea;

        public ReportForm()
        {
            InitializeComponent();
            //크리스탈뷰 안보이도록하기
            //crystalReportViewer1.Visible = false;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LinkComboBoxPlantList();

            //sql 연결 됐는지 확인
            MySqlConnection connection = new MySqlConnection("datasource=localhost;port=3306;username=root;password=tkfkdgo09013;");
            connection.Open();
            if (connection.State == ConnectionState.Open)
            {
                label7.Text = "Connected";
                //label7.ForeColor = Color.ControlText;
            }
            else
            {
                label7.Text = "DisConnected";
                //label6.ForeColor = Control.ForeColor.Red;
            }
        }

        private void savePDF()//PDF로 저장
        {
            pdfNum++;
            //PDF로 저장
            if (dataGridView1.Rows.Count > 0)
            {
                String pdfName = "Output"+ pdfNum + ".pdf";
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF (*.pdf)|*.pdf";
                //sfd.FileName = "Output.pdf";
                sfd.FileName = pdfName;
                bool fileError = false;
                /*if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            PdfPTable pdfTable = new PdfPTable(dataGridView1.Columns.Count);
                            pdfTable.DefaultCell.Padding = 3;
                            pdfTable.WidthPercentage = 100;
                            pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                            foreach (DataGridViewColumn column in dataGridView1.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                                pdfTable.AddCell(cell);
                            }

                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    pdfTable.AddCell(cell.Value.ToString());
                                }
                            }

                            using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                            {
                                Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                                PdfWriter.GetInstance(pdfDoc, stream);
                                pdfDoc.Open();
                                pdfDoc.Add(pdfTable);
                                pdfDoc.Close();
                                stream.Close();
                            }

                            MessageBox.Show("PDF로 저장에 성공했습니다.", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }*/
                try
                {
                    PdfPTable pdfTable = new PdfPTable(dataGridView1.Columns.Count);
                    pdfTable.DefaultCell.Padding = 3;
                    pdfTable.WidthPercentage = 100;
                    pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText));
                        pdfTable.AddCell(cell);
                    }

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            pdfTable.AddCell(cell.Value.ToString());
                        }
                    }

                    using (FileStream stream = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4, 10f, 20f, 20f, 10f);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();
                        pdfDoc.Add(pdfTable);
                        pdfDoc.Close();
                        stream.Close();
                    }

                    MessageBox.Show("PDF로 저장에 성공했습니다.", "Info");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error :" + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("PDF로 저장할 레코드가 없습니다.", "Info");
            }
        }

        private void LinkComboBoxPlantList()
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "PlantList";
            SqlCommand cmd = new SqlCommand(@"select PlantNumber, PlantName, ContactPerson, ContactEmail, ContactInfo from " + TBL_NAME, con);
            //DataTable dt = new DataTable();
            SqlDataReader sdr = cmd.ExecuteReader();

            PlantEmailList.Clear();
            comboBox1.Items.Clear();
            while (sdr.Read())
            {
                string plantNumber = sdr.GetString(0);
                string plantName = sdr.GetString(1);
                string plantContactPerson = sdr.GetString(2);
                string plantEmail = sdr.GetString(3);

                comboBox1.Items.Add(plantNumber + " : " + plantName);
                // 해당 발전소에 대한 이메일 정보를 목록에 별도 저장하고 나중에 사용
                PlantEmailList.Add(plantNumber, plantEmail);
            }
            //dt.Load(sdr);
            con.Close();
            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;  // 항상 최신 것을 가리키도록 조정
        }


        private void printBtn_Click(object sender, EventArgs e)  // 보고서 인쇄
        {
            MessageBox.Show("보고서를 인쇄합니다.", "인쇄", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);

            //크리스탈 리포트 보이기
            crystalReportViewer1.Visible = true;

            //크리스탈 리포트 보고서 출력
            CrystalReport3 report = new CrystalReport3();

            //이름 상관없이 순서대로
            DataTable table = new DataSet2.DataTable1DataTable();
            //DataTable sumTable = new DataSet2.DataTableSummaryDataTable();

            //날짜 받아오기(년-월-일)
            string dt1 = dateTimeFrom.Value.ToString("yyyy-MM-dd");
            string dt2 = dateTimeTo.Value.ToString("yyyy-MM-dd");

            //총 면적&시간 
            int totalArea = 0;
            int totalPeriod = 0;

            //크리스탈 레포트에 해당 정보 저장
            //데이터그리드뷰 데이터 입력
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                DataRow row = table.NewRow();

                row["DateFrom"] = dt1;
                row["DateTo"] = dt2;
                row["RobotID"] = dataGridView1.Rows[i].Cells[1].Value;
                row["CleanPeriod"] = dataGridView1.Rows[i].Cells[2].Value;
                row["CleanArea"] = dataGridView1.Rows[i].Cells[3].Value;
                totalArea+= Int32.Parse(dataGridView1.Rows[i].Cells[3].Value.ToString());
                totalPeriod+= Int32.Parse(dataGridView1.Rows[i].Cells[2].Value.ToString());
                table.Rows.Add(row);
            }
            //추가 기록사항 입력
            DataRow sumRow = table.NewRow();
            sumRow["Summury"] = remarkTextBox.Text.ToString();

            //총 면적&시간
            sumRow["TotalArea"] = totalArea;
            sumRow["TotalPeriod"] = totalPeriod;

            //발전소 이름 가져오기
            string comboS = comboBox1.Text.ToString();
            string plantName = comboS.Substring(7,6);
            string[] plantinfo = comboBox1.Text.ToString().Split(new string[] { " : " }, StringSplitOptions.None);
            //CurrentPlantName = plantinfo[1];
            sumRow["PlantInfo"] = plantName.ToString();

            table.Rows.Add(sumRow);
            //table.Rows.Add(nameRow);

            //레포트에 정보 추가 끝

            report.SetDataSource(table);
            //report.SetDataSource(sumTable);

            crystalReportViewer1.ReportSource = report;

            /*FormPrint form = new FormPrint();
            form.setReportSource(report);
            form.ShowDialog();
            form.Dispose();*/
        }

        private void emailBtn_Click(object sender, EventArgs e) //  보고서 이메일 발송
        {
            //crystalReportViewer1.Visible = false;
            MessageBox.Show("보고서 내용을 이메일 주소 " + CurrentContactEmail + "로 발송합니다.", "이메일 발송", MessageBoxButtons.OKCancel, MessageBoxIcon.Hand);
            //이메일 전송에 사용할 pdf파일 저장
            savePDF();
            //이메일 전송
            MailMessage mail = new MailMessage();
            string emailS = emailTBox.Text.ToString();
            try
            {   // 보내는 사람 메일, 이름, 인코딩(UTF-8)      
                mail.From = new MailAddress(emailS, "에스와이테크", System.Text.Encoding.UTF8);
                // 받는 사람 메일      
                mail.To.Add(emailS);
                // 참조 사람 메일      
                mail.CC.Add(emailS);
                // 비공개 참조 사람 메일      
                mail.Bcc.Add(emailS);
                // 메일 제목      
                mail.Subject = dateTimeFrom.Value.ToString("yyyy-MM-dd") + "부터 "+dateTimeTo.Value.ToString("yyyy-MM-dd") + " 작업상황";
                // 본문 내용      
                mail.Body = "<html><body>"+ remarkTextBox+ "</body></html>";
                // 본문 내용 포멧의 타입 (true의 경우 Html 포멧으로)      
                mail.IsBodyHtml = true;

                // 메일 제목과 본문의 인코딩 타입(UTF-8)      
                mail.SubjectEncoding = System.Text.Encoding.UTF8;
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                // 첨부 파일 (Stream과 파일 이름)      
                //String fileStream = "C:\Users\82104\source\repos\RobotControlCenter_PUBLISH\Output" + pdfNum + ".pdf";
                if (dataGridView1.Columns.Count != 0)
                {
                    mail.Attachments.Add(new Attachment(new FileStream(@"C:\Users\82104\source\repos\RobotControlCenter_PUBLISH\RobotControlCenter_PUBLISH\bin\Debug\Output" + pdfNum + ".pdf", FileMode.Open, FileAccess.Read), "작업상황.pdf"));
                }
                //mail.Attachments.Add(new Attachment(new FileStream(@"D:\test2.zip", FileMode.Open, FileAccess.Read), "test2.zip"));

                // smtp 서버 주소      
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                // smtp 포트      
                SmtpServer.Port = 587;
                // smtp 인증      
                SmtpServer.Credentials = new System.Net.NetworkCredential("gg2060gg", "vwsfdgymtyfqqeuu");
                // SSL 사용 여부      
                SmtpServer.EnableSsl = true;
                // 발송      
                SmtpServer.Send(mail);
                MessageBox.Show("이메일 전송에 성공했습니다.", "Info");
            }
            finally
            {
                // 첨부 파일 Stream 닫기      
                foreach (var attach in mail.Attachments)
                {
                    attach.ContentStream.Close();
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string[] plantinfo = comboBox1.Text.ToString().Split(new string[] { " : " }, StringSplitOptions.None);
            CurrentPlantNumber = plantinfo[0];
            CurrentPlantName = plantinfo[1];
            CurrentContactEmail = PlantEmailList[CurrentPlantNumber];
            emailTBox.Text = CurrentContactEmail.ToString();

            if (G.DEBUG) Console.WriteLine("<" + CurrentPlantNumber + ">,<" + CurrentPlantName + ">,<" + CurrentContactEmail + ">");
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            //crystalReportViewer1.Visible = false;

            // [1] ReportData 테이블 내용 초기화
            string REPORT_TBL_NAME = "ReportData";
            SqlCommand initcmd = new SqlCommand("delete from " + REPORT_TBL_NAME, con);
            initcmd.ExecuteReader();
            con.Close();

            // [2] WorkLog에서 조건에 맞는 자료를 읽어 온다.
            // [2-1] 기간 검색 조건
            string TBL_NAME = "WorkLog";
            DateTime From = DateTime.Parse(dateTimeFrom.Value.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(dateTimeTo.Value.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            // [2-2] 검색 실시
            con.Open();
            SqlCommand cmd = new SqlCommand("select RId, TimeStamp, PlantNumber, State, LSize, RSize, Counter, Progress, Etc  from " + TBL_NAME + " where PlantNumber = @PlantNumber AND @DT_From <= TimeStamp AND TimeStamp < @DT_To", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@DT_From", From.ToShortDateString());  // 날짜 양식 표준화
            cmd.Parameters.AddWithValue("@DT_To", To.ToShortDateString());      // 날짜 양식 표준화
            SqlDataReader sdr = cmd.ExecuteReader();

            // [3] 테이블 내용을 보여주는 대신 <로봇명, 날짜> 및 해당하는 작업시간, 작업면적 목록을 구성한다.
            // [3-1]  로봇-날짜 목록 초기화 - 중요
            ReportRobotList.Clear();
            // [3-2] <로봇명, 날짜> 목록 구성 (로봇명과 날짜가 모두 일치하는 목록은 없어야 한다)
            while (sdr.Read())
            {
                string robotID = sdr.GetString(0);
                DateTime timeStamp = sdr.GetDateTime(1);

                if (!ReportRobotList.ContainsKey(robotID)) // 목록에 존재하지 않으면, 추가
                {
                    ReportRobotList.Add(robotID, new List<string>());
                    ReportRobotList[robotID].Add(timeStamp.ToShortDateString()); // 날짜 양식 표준화 
                    //Console.WriteLine("로봇 추가 NAME/DATE = " + robotID + "/" + timeStamp.ToShortDateString());
                }
                else
                {
                    if (!ReportRobotList[robotID].Contains(timeStamp.ToShortDateString())) // 목록에 존재하나 날짜가 다르면 날짜 항목 추가
                    {
                        ReportRobotList[robotID].Add(timeStamp.ToShortDateString()); // 날짜 양식 표준화
                        //Console.WriteLine("날짜 추가 NAME/DATE = " + robotID + "/" + timeStamp.ToShortDateString());
                    }
                }
            }

            con.Close();

            // [3-2] <로봇명, 날짜>가 일치하는 목록에 해당하는 작업시간, 작업면적을 계산하여 저장
            foreach (var s in ReportRobotList)
            {
                string robotID = s.Key;

                foreach (string rdate in ReportRobotList[robotID])
                {
                    if (G.DEBUG) Console.WriteLine("############## : ROBOT/DATE List -  " + robotID + " / " + DateTime.Parse(rdate).ToShortDateString());

                    // 로봇의 당일 workingTime, workingArea 값을 계산
                    calcWorkingTimeAndArea(robotID, DateTime.Parse(rdate).Date);

                    ////// 시간 계산 및 표시 방법 개선 필요 - 현재는 누적 초 단위 정보 제공
                    ////int Hour = workingTime / 3600; int Minute = (workingTime % 3600) / 60; int Second = (workingTime % 60);
                    ////string workingTimeStr = Hour + ":" + Minute + ":" + Second;

                    // 작업시간 및 작업면적을 string으로 변환하여 DB에 저장
                    string workingTimeStr = "" + workingTime;
                    string workingAreaStr = "" + workingArea;

                    DB.insertReportTable(CurrentPlantNumber, robotID, DateTime.Parse(rdate).Date, workingTimeStr, workingAreaStr);
                }
            }
            
            // [3] 생성된 테이블 내용을 보여준다. <로봇명, 날짜> 별로 작업시간, 작업면적을 계산하여 리스트로 구성
            DisplayReportTable();
            MessageBox.Show("조회되었습니다.");

            //DB연결 후 데이터 전송//혜정 추가
            using (MySqlConnection conn = new MySqlConnection("Server=localhost;Port=3306;Database=login_dataset;Uid=root;Pwd=tkfkdgo09013"))
            {
                conn.Open();
                //mysql에 원래 있던 데이터는 전부 지워주기
                string sqlD = string.Format("DELETE FROM infotable");
                try
                {
                    MySqlCommand commandD = new MySqlCommand(sqlD, conn);
                    commandD.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                //발전소 이름 받아오기
                String comboS = comboBox1.Text.ToString();
                String plantId = comboS.Substring(7, 6);
                //각 행의 정보를 반복문으로 불러온다
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    String date = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    String Rid = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    String period = dataGridView1.Rows[i].Cells[2].Value.ToString();
                    String area = dataGridView1.Rows[i].Cells[3].Value.ToString();
                    //데이터 열람 기한 
                    String startT = dateTimeFrom.Value.ToString("yyyy-MM-dd");
                    String endT = dateTimeTo.Value.ToString("yyyy-MM-dd");
                    DateTime nowDate = DateTime.Now;
                    String insertTime = nowDate.ToString("yyyy-MM-dd");


                    //INSERT INTO 쿼리문으로 받아온 정보를 DB에 전송한다. 
                    string sql = string.Format("INSERT INTO infotable(plantId, startT, endT,Rid,period,area, insertTime) VALUES  ('{0}','{1}','{2}','{3}', '{4}', '{5}', '{6}')", @plantId, @startT, @endT, @Rid, @period, @area, @insertTime);

                    //DB전송을 진행하고 실패시 에러메세지 출력
                    try
                    {
                        MySqlCommand command = new MySqlCommand(sql, conn);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        // WorkLog 테이블에서 <로봇명, 날짜>에 해당하는 작업시간/작업면적(누적)을 계산한다.
        private void calcWorkingTimeAndArea(string robotID, DateTime reportDate)
        {
            // [1] WorkLog에서 RobotID, PlantNUmber, 작업날짜 등의 조건에 맞는 자료를 시간 순으로 읽어 온다.
            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            string TBL_NAME = "WorkLog";
            DateTime From = DateTime.Parse(reportDate.Date.ToShortDateString() + " 오전 00:00:00");
            DateTime To = DateTime.Parse(reportDate.AddDays(1).Date.ToShortDateString() + " 오전 00:00:00");

            SqlCommand cmd = new SqlCommand("select TimeStamp, State, LSize, RSize, Counter, Progress from " + TBL_NAME + " where PlantNumber = @PlantNumber AND RId = @RobotID AND @DT_From <= TimeStamp AND TimeStamp < @DT_To ", con);
            cmd.Parameters.AddWithValue("@PlantNumber", CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@RobotID", robotID);
            cmd.Parameters.AddWithValue("@DT_From", From.ToShortDateString());
            cmd.Parameters.AddWithValue("@DT_To", To.ToShortDateString());
            //주의 :  날짜 일치 검색 조건이 정확히 동작하지 않음. ==> 일단 범위에 드는 것 중 아래 조건은 while 루프안에서 제거
            //cmd.Parameters.AddWithValue("@Date", reportDate);
            SqlDataReader sdr = cmd.ExecuteReader();

            // 매번 변수 초기화 후 계산
            workingTime = 0;
            workingArea = 0;

            // 작업시각, 작업진행률초기값 설정
            DateTime startTime = From;
            int startProgress = 0;
            bool startFound = false;

            while (sdr.Read())
            {
                DateTime timeStamp = sdr.GetDateTime(0);        // DB의 TimeStamp 값
                string state = sdr.GetString(1);                // DB의 State 값
                double lsize = (double)sdr.GetSqlDouble(2);     // DB의 LSize 값
                double rsize = (double)sdr.GetSqlDouble(3);     // DB의 RSize 값
                //int counter = sdr.GetInt32(4);                // DB의 edge counter 
                int progress = sdr.GetInt32(5);                 // DB의 progress 진도율

                // state 종류 중 작동 시작 관련된 것은 RUN_BTN_PRESSED(작동 시작 버튼),
                if (state.Equals(G.RUN_BTN_PRESSED))  // 작업 시작 시각
                {
                    startTime = timeStamp;
                    startProgress = progress;

                    startFound = true; //작업 시작시간 세팅
                }
                // state 종류 중 작동 완료 관련된 것은 STOP_BTN_PRESSED(정지 버튼), ERROR_STOP(오류로 중단), FINISHED(정상 작업 완료) 등이 있음,
                else if (state.Equals(G.FINISHED) || state.Equals(G.STOP_BTN_PRESSED) || state.Equals(G.ERROR_STOP)) // 작업 중단/완료 시각
                {
                    //// 시작 없이 종료만 검색되는 경우, 건너띔???
                    if (!startFound) continue;

                    ////if (startTime.CompareTo(timeStamp) >= 0)  // 시작과 끝 시간이 같거나 순서가 뒤바뀐 경우이면 예외적인 경우로 취급하여 무시
                    ////    continue;

                    TimeSpan timeSpan = timeStamp - startTime;  // 경과 시간
                    workingTime += (int)timeSpan.TotalSeconds;
                    workingArea += lsize * rsize * (progress - startProgress) / 100; // 작업 진행률 변경

                    if (G.DEBUG)
                    {
                        if (workingTime != 0 || workingArea != 0)
                        {
                            Console.WriteLine(robotID + " WORKED FROM " + G.TimeStamp(startTime) + " TO " + G.TimeStamp(timeStamp)
                                + " ##TIMESPAN = " + timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);
                            Console.WriteLine(robotID + "의 작업 진행률 " + startProgress + " ==> " + progress);
                        }
                    }

                    startFound = false; // 다음 검사를 위해 다시 false로 변경
                }
            }



                con.Close();
        }

        // ReportData 테이블은 조건에 딱 맞는 것만 찾아낸 것이므로 테이블 내용 전체를 보여준다.-  <로봇명, 날짜> 별로 작업시간, 작업면적을 보여준다
        private void DisplayReportTable()
        {
            string TBL_NAME = "ReportData";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            // ReportDate 테이블은 검색 조건(기간/날짜, 발전소명 등)이 일치하는 것만 모아둔 것이므로 그냥 보여줘도 됨, 단, 순서만 조정
            SqlCommand cmd = new SqlCommand("select Date, RId, WorkingTime, WorkingArea from " + TBL_NAME + " ORDER BY Date, RId", con);
            SqlDataReader sdr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(sdr);

            con.Close();

            dataGridView1.DataSource = dt;
            dataGridView1.Columns["Date"].Width = 1000;
            dataGridView1.Columns["RId"].Width = 1500;
            dataGridView1.Columns["WorkingTime"].Width = 800;
            dataGridView1.Columns["WorkingArea"].Width = 800;

            dataGridView1.Columns["Date"].HeaderText = "작업날짜(년월일)";
            dataGridView1.Columns["Date"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["RId"].HeaderText = "작업로봇 ID";
            dataGridView1.Columns["RId"].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            dataGridView1.Columns["WorkingTime"].HeaderText = "작업시간(누적, 초)";
            dataGridView1.Columns["WorkingTime"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridView1.Columns["WorkingArea"].HeaderText = "작업면적(제곱미터)";
            dataGridView1.Columns["WorkingArea"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
    }

}
