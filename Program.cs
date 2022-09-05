using System;
using System.Windows.Forms;

namespace RobotCC
{
    static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 아래 문장은 progressBar 색상 변경을 위해 잠정 미실행
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new RobotControlCenter());
        }
    }
}
