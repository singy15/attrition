/*
 * Attrition Test Evidence Utility
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;

public class ATEUForm : Form
{
    private ATEUService svc;
    private ATEUHotKeyUtil hku;

    public ATEUForm()
    {
        this.Load += new EventHandler(OnLoad);
        this.FormClosing += new FormClosingEventHandler(OnClose);
    }

    private void OnLoad(object sender, EventArgs e)
    {
        svc = new ATEUService();

        hku = new ATEUHotKeyUtil(
                ATEUHotKeyUtil.MOD.ALT | ATEUHotKeyUtil.MOD.SHIFT,
                Keys.C);
        hku.OnHotKeyPush += new EventHandler(OnHotKey);
    }

    private void OnHotKey(object sender, EventArgs e)
    {
        Console.WriteLine("capture");

        SendKeys.SendWait("%{PRTSC}");
        IDataObject d = Clipboard.GetDataObject();

        if (d != null)
        {
            Image img = (Image)d.GetData(DataFormats.Bitmap);
            if (img != null)
            {
                img.Save(@"C:\root\tmp\a.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }

    private void OnClose(object sender, FormClosingEventArgs e)
    {
        hku.Dispose();
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new ATEUForm());
    }
}

public class ATEUService
{
}

public class ATEUHotKeyUtil :IDisposable
{
    HotKeyForm form;

    public event EventHandler OnHotKeyPush;

    public ATEUHotKeyUtil(MOD modKey, Keys key)
    {
        form = new HotKeyForm(modKey, key, raiseHotKeyPush);
    }

    private void raiseHotKeyPush()
    {
        if(OnHotKeyPush != null)
        {
            OnHotKeyPush(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        form.Dispose();
    }

    private class HotKeyForm : Form
    {
        [DllImport("user32.dll")]
        extern static int RegisterHotKey(IntPtr HWnd, int id, MOD mod, Keys key);

        [DllImport("user32.dll")]
        extern static int UnregisterHotKey(IntPtr HWnd, int id);

        int id;
        ThreadStart proc;

        public HotKeyForm(MOD modKey, Keys key, ThreadStart proc)
        {
            this.proc = proc;
            for(int i = 0x0000; i <= 0xbfff; i++)
            {
                if(RegisterHotKey(this.Handle, i, modKey, key) != 0)
                {
                    id = i;
                    break;
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if(m.Msg == /*WM_HOTKEY*/0x0312)
            {
                if((int)m.WParam == id) proc();
            }
        }

        protected override void Dispose(bool disposing)
        {
            UnregisterHotKey(this.Handle, id);
            base.Dispose(disposing);
        }
    }

    public enum MOD : int
    {
        ALT = 0x0001,
        CTRL = 0x0002,
        SHIFT = 0x0004,
    }
}


