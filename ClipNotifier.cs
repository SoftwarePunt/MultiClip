using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MultiClip
{
    public partial class ClipNotifier : Form
    {
        private static ClipNotifier notifier = null;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect, // x-coordinate of upper-left corner
            int nTopRect, // y-coordinate of upper-left corner
            int nRightRect, // x-coordinate of lower-right corner
            int nBottomRect, // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
         );

        private Timer tt;

        public ClipNotifier(string text)
        {
            InitializeComponent();

            try
            {
                if (notifier != null)
                    notifier.Invoke(new Action(() => { notifier.Close(); }));
            }
            catch (InvalidOperationException) { }

            notifier = this;

            Application.DoEvents();

            this.Text = string.Empty;
            this.ControlBox = false;
            this.label1.Text = text;

            pictureBox1.Left = 20;
            label1.Left += pictureBox1.Width + 15;

            int sidePadding = 15;
            int iconPadding = 10;
            int totalWidth = pictureBox1.Width + iconPadding + label1.Width + (sidePadding * 2);

            pictureBox1.Left = sidePadding;
            label1.Left = pictureBox1.Right + iconPadding;

            this.Width = totalWidth;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.Opacity = 0.95;

            this.Top = Screen.PrimaryScreen.WorkingArea.Height - 150;
            this.Left = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (this.Width / 2);
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 15, 15));

            tt = new Timer();
            tt.Tick += tt_Tick;
            tt.Interval = 3000;
            tt.Enabled = true;

            this.Load += ClipNotifier_Load;
        }

        private void ClipNotifier_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.TopMost = true;
        }

        protected override bool ShowWithoutActivation // stops the window from stealing focus
        {
            get { return true; }
        }

        private void tt_Tick(object sender, System.EventArgs e)
        {
            if (this.Opacity <= 0)
            {
                tt.Enabled = false;
                tt.Dispose();

                this.Close();

                return;
            }

            if (tt.Interval != 10)
            {
                tt.Interval = 10;
            }
            else
            {
                double newOpacity = this.Opacity - 0.05;

                if (newOpacity <= 0)
                {
                    this.Opacity = 0;
                }
                else
                {
                    this.Opacity = newOpacity;
                }
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)0x08000000L; // WS_EX_NOACTIVATE
                return cp;
            }
        }

        protected override void WndProc(ref Message message)
        {
            const int WM_NCHITTEST = 0x0084;

            if (message.Msg == WM_NCHITTEST)
                return;

            base.WndProc(ref message);
        }
    }
}
