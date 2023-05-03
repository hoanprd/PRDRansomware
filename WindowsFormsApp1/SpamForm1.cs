using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class SpamForm1 : Form
    {
        public static bool stopSpamPic = false;

        public SpamForm1()
        {
            InitializeComponent();

            Timer tmr = new Timer();
            tmr.Interval = 3000;
            tmr.Tick += Tmr_Tick;
            tmr.Start();
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            if (stopSpamPic == false)
                this.Hide();
            else
            {
                this.Hide();
            }
        }
    }
}
