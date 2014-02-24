using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MultiClip
{
    public static class Clipper
    {
        public static int ActiveClipNumber
        {
            get
            {
                return activeClip;
            }
        }

        public static bool WasConfigured
        {
            get;
            private set;
        }

        private static int activeClip = 0;
        private static Dictionary<int, IDictionary<string, object>> clips = new Dictionary<int, IDictionary<string, object>>()
        {
            {0, null},
            {1, null},
            {2, null},
            {3, null},
            {4, null}
        };

        public static IDictionary<string, object> GetValue(int index)
        {
            return clips[index];
        }

        public static void SetValue(int index, IDictionary<string, object> val)
        {
            clips[index] = val;
        }

        public static IDictionary<string, object> GetClipboardData()
        {
            var dict = new Dictionary<string, object>();
            var dataObject = Clipboard.GetDataObject();
            foreach (var format in dataObject.GetFormats())
            {
                dict.Add(format, dataObject.GetData(format));
            }
            return dict;
        }

        public static void SetClipboardData(IDictionary<string, object> dict)
        {
            if (dict == null || dict.Count == 0)
            {
                return;
            }

            //var dataObject = Clipboard.GetDataObject();

            foreach (var kvp in dict)
            {
                Clipboard.SetData(kvp.Key, kvp.Value);
            }
        }

        public static void SetClipboard(int index)
        {
            var th = new Thread(() =>
            {
                // Store active clip
                SetValue(activeClip, GetClipboardData());
                
                // Clear clipboard
                try
                {
                    Clipboard.Clear();
                }
                catch (Exception) { }

                // Set newly selected clip
                SetClipboardData(GetValue(index));
                activeClip = index;
            });
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            WasConfigured = true;

            Program.ShowThreadSafeClipNotify("Clipboard " + (index + 1));
            Program.Tray.Text = "MultiClip - Clipboard " + (index + 1);
            Program.HighlightClipboardInTray(index);
        }
    }
}
