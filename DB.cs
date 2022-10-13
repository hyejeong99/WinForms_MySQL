using RobotCC;
using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RobotControlCenter
{
    internal class DB
    {

        public static void insertWorkLog(int robotIndex, string action, string etc)
        {
            string TBL_NAME = "WorkLog";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            Console.WriteLine(DateTime.Now);

            SqlCommand cmd = new SqlCommand("insert into " + TBL_NAME + " values (@RobotID, @DateTime, @PlantNumber, @Action, @LSize, @RSize, @Counter, @Progress, @Etc)", con);
            cmd.Parameters.AddWithValue("@RobotID", G.robotID[robotIndex]);
            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@PlantNumber", G.CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@LSize", (float)G.LSize[robotIndex]);
            cmd.Parameters.AddWithValue("@RSize", (float)G.RSize[robotIndex]);
            cmd.Parameters.AddWithValue("@Counter", G.EDGE_CNT[robotIndex]);
            cmd.Parameters.AddWithValue("@Progress", G.WORK_PERCENTAGE[robotIndex]);  // 추가
            cmd.Parameters.AddWithValue("@Etc", etc);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            con.Close();
        }

        public static void insertReportTable(string plantNumber, string robotID, DateTime reportDate, string workingTime, string workingArea)
        {
            string TBL_NAME = "ReportData";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("insert into " + TBL_NAME + " values (@PlantNumber, @RobotID, @ReportDate, @WorkingTime, @WorkingArea)", con);
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber);
            cmd.Parameters.AddWithValue("@RobotID", robotID);
            cmd.Parameters.AddWithValue("@ReportDate", reportDate.ToShortDateString());
            cmd.Parameters.AddWithValue("@WorkingTime", workingTime);
            cmd.Parameters.AddWithValue("@WorkingArea", workingArea);

            Console.WriteLine(cmd);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            con.Close();
        }

    }
}
