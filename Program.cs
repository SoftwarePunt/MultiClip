using MultiClip.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace MultiClip
{
    internal static class Program
    {
        internal const string PROCESS_MUTEX_NAME = "MSKBBEA1.MultiClip.ProcessMutex";
        internal const string PROCESS_PIPE_NAME = "MSKBBEA1.MultiClip.NamedCommPipe";

        private static Mutex processMutex;
        private static NotifyIcon tray;

        internal static NotifyIcon Tray
        {
            get
            {
                return tray;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main(string[] args)
        {
            var joinedArgs = String.Join(" ", args);

            Debug.WriteLine("MultiClip " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

            if (!String.IsNullOrWhiteSpace(joinedArgs))
            {
                Debug.WriteLine("Invoked with command line parameters: " + joinedArgs);
            }

            // If we can not aquire a mutex lock (i.e. one already exists), that means we need
            // to send the command to the existing process.
            if (!EnsureMainInstance())
            {
                Debug.WriteLine("WARNING: Could not aquire a process mutex lock!");

                if (!String.IsNullOrWhiteSpace(joinedArgs))
                {
                    var client = new PipeClient();
                    client.Open();
                    client.SendAndRead(joinedArgs);
                    client.Close();
                }
                else
                {
                    MessageBox.Show("MultiClip is already active in the background.", "You tried to start MultiClip", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                Debug.WriteLine("This process has completed its work. Farewell, cruel world!");

                Environment.Exit(0);
                return;
            }

            Debug.WriteLine("Successfully aquired a mutex lock for MultiClip (" + PROCESS_MUTEX_NAME + "); this is the main process.");

            // Initialize server
            PipeServer.Start();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize tray
            tray = new NotifyIcon();
            tray.Icon = Icon.FromHandle(Resources.iconWhite.GetHicon());
            tray.Text = "MultiClip";
            tray.Visible = true;
            tray.DoubleClick += tray_DoubleClick;

            // Set up tray context menu
            tray.ContextMenu = new ContextMenu();
            tray.ContextMenu.MenuItems.Add("Clipboard 1", new EventHandler(OnClipClicked));
            tray.ContextMenu.MenuItems.Add("Clipboard 2", new EventHandler(OnClipClicked));
            tray.ContextMenu.MenuItems.Add("Clipboard 3", new EventHandler(OnClipClicked));
            tray.ContextMenu.MenuItems.Add("Clipboard 4", new EventHandler(OnClipClicked));
            tray.ContextMenu.MenuItems.Add("Clipboard 5", new EventHandler(OnClipClicked));
            tray.ContextMenu.MenuItems.Add("-");
            tray.ContextMenu.MenuItems.Add("Exit", new EventHandler(OnExitClicked));

            CommandProcessor.Process(args);

            if (!Clipper.WasConfigured)
            {
                Clipper.SetClipboard(0);
            }

            Application.ApplicationExit += Application_ApplicationExit;
            Application.Run();
        }

        public static void ShowThreadSafeClipNotify(string text)
        {
            var th = new Thread(() =>
            {
                var form = new ClipNotifier(text);
                form.FormClosed += form_FormClosed;
                form.Show();
                Application.Run();
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
        }

        static void form_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.ExitThread(); // Notifier form thread
        }

        public static void HighlightClipboardInTray(int id)
        {
            string expected = "Clipboard " + (id + 1);

            foreach (MenuItem item in tray.ContextMenu.MenuItems)
            {
                if (item.Text.Equals(expected))
                {
                    item.DefaultItem = true;
                }
                else
                {
                    item.DefaultItem = false;
                }
            }
        }

        private static void OnClipClicked(object o, EventArgs e)
        {
            int no = int.Parse(((MenuItem)o).Text.Substring("Clipboard ".Length)); // is this the dirtiest solution in the world? yes it is.
            Clipper.SetClipboard(no - 1);
        }

        private static void OnExitClicked(object o, EventArgs e)
        {
            Application.Exit(); // Main thread
            Application.DoEvents();
            Application_ApplicationExit(null, null);
        }

        private static void Application_ApplicationExit(object sender, EventArgs e)
        {
            tray.Visible = false;
            tray.Dispose();
            tray = null;

            Application.DoEvents();

            try
            {
                PipeServer.Stop();
            }
            catch (Exception) { }

            Application.ExitThread();
            Environment.Exit(0);
        }

        private static void tray_DoubleClick(object sender, EventArgs e)
        {
            new ClipNotifier("MultiClip v" + Assembly.GetExecutingAssembly().GetName().Version.ToString()).Show();
        }

        public static bool EnsureMainInstance()
        {
            try
            {
                Mutex.OpenExisting(PROCESS_MUTEX_NAME);
                return false;
            }
            catch (Exception)
            {
                processMutex = new Mutex(true, PROCESS_MUTEX_NAME);
                return true;
            }
        }
    }
}
