using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace VideoBackup
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.Run(new UserForm());
            }
            catch (Exception e)
            {
                MessageBox.Show("Unknown error occurred: " + e.Message + "\n\nNOTE:\nNot all files have been copied to your destination folder.\n\n" + e.StackTrace, "Video Backup - Error", MessageBoxButtons.OK);
            }
        }
    }
}
