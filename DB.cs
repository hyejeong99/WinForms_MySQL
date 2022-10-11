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

            SqlCommand cmd = new SqlCommand("insert into " + TBL_NAME + " values (@RobotID, @DateTime, @PlantNumber, @Action, @LSize, @RSize, @Counter, @Etc)", con);
            cmd.Parameters.AddWithValue("@RobotID", G.robotID[robotIndex]);
            cmd.Parameters.AddWithValue("@DateTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@PlantNumber", G.CurrentPlantNumber);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@LSize", (float)G.LSize[robotIndex]);
            cmd.Parameters.AddWithValue("@RSize", (float)G.RSize[robotIndex]);
            cmd.Parameters.AddWithValue("@Counter", G.EDGE_CNT[robotIndex]);
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

        ///  작성 중 ,.,....
        public static void selectWorkLog(string robotID, DateTime from, DateTime to, string plantNumber)
        {
            string TBL_NAME = "WorkLog";

            SqlConnection con = new SqlConnection(G.connectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand("select * from " + TBL_NAME + " where RId = @RobotID, PlantNumber = @PlantNumber, dt >= @from, dt <= @to", con);

            cmd.Parameters.AddWithValue("@RobotID", robotID); // 로봇 기기 선택
            cmd.Parameters.AddWithValue("@frome", from); // 기간 선택
            cmd.Parameters.AddWithValue("@to", to); // 기간 선택
            cmd.Parameters.AddWithValue("@PlantNumber", plantNumber); // 발전소 선택
                                                                      //cmd.Parameters.AddWithValue("@State", state); // 상태 선택

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "DB선택오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            con.Close();

        }

    }
}
