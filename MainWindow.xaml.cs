using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Management;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Media;
using System.Drawing.Imaging;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace WPFBatteryAndTouchscreenTool
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon _icon = new NotifyIcon();
        private Timer _timer = new Timer();
        private Font _font = new Font("Calibri", 17f, System.Drawing.FontStyle.Bold);
        private StringFormat _format = new StringFormat();
        private int _percentage = -1;
        private int _remaining = -1;
        private bool _touchscreenEnabled = true;
        private bool _noTouchScreen = false;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            string status = "undefined";
            try
            {
                PowerShell s = PowerShell.Create();
                s.AddScript("Get-PnpDevice | Where-Object {$_.FriendlyName -like '*HID-konformer Touchscreen*'} | Select-Object Status -first 1");
                Collection<PSObject> c = s.Invoke();
                if (c.Count > 0)
                    status = c[0].Properties["Status"].Value.ToString();
                s.Dispose();
            }
            catch(Exception)
            {
                status = "undefined";
            }

            if (status == "undefined")
            {
                _noTouchScreen = true;
            }
            else
            {
                if (status.ToLower() == "error")
                    _touchscreenEnabled = false;
                else
                {
                    _touchscreenEnabled = true;
                }
                _noTouchScreen = false;
            }

            _format.Alignment = StringAlignment.Center;
            _format.LineAlignment = StringAlignment.Center;

            _icon.Visible = true;
            _icon.Click += _icon_Click;

            _timer.Interval = 10000;
            _timer.Tick += _timer_Tick;
            _timer.Start();

            UpdateIcon();
        }

        private void _icon_Click(object sender, EventArgs e)
        {
            if (!_noTouchScreen)
            {
                PowerShell s = PowerShell.Create();
                if (_touchscreenEnabled)
                    s.AddScript("Get-PnpDevice | Where-Object {$_.FriendlyName -like '*HID-konformer Touchscreen*'} | Disable-PnpDevice -Confirm:$false");
                else
                    s.AddScript("Get-PnpDevice | Where-Object {$_.FriendlyName -like '*HID-konformer Touchscreen*'} | Enable-PnpDevice -Confirm:$false");

                s.Invoke();

                s.Dispose();
                _touchscreenEnabled = !_touchscreenEnabled;
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            int percentage = -1;
            int remaining = -1;

            try
            {
                ObjectQuery query = new ObjectQuery("Select EstimatedChargeRemaining, EstimatedRunTime FROM Win32_Battery");
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);



                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject mo in collection)
                {

                    percentage = Convert.ToInt32(mo.Properties["EstimatedChargeRemaining"].Value.ToString());
                    remaining = Convert.ToInt32(mo.Properties["EstimatedRunTime"].Value.ToString());

                    break;
                }
            }
            catch (InvalidCastException)
            {
                
            }
        
            _percentage = percentage >= 0 ? percentage : 100;
            _remaining = remaining;
            
            Bitmap newTrayImage = PaintIcon(percentage, remaining);
            _icon.Icon = System.Drawing.Icon.FromHandle(newTrayImage.GetHicon());
            
            //TestImage.Source = CreateBitmapSourceFromGdiBitmap(newTrayImage);
        }

        private Bitmap PaintIcon(int percentage, int remaining)
        {
            Bitmap icon = new Bitmap(32, 32);
            Graphics g = Graphics.FromImage(icon);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 32, 32);
            g.FillRectangle(System.Drawing.Brushes.Transparent, rect);

            g.DrawString(percentage < 100 ? percentage + "" : "OK" , _font, System.Drawing.Brushes.White, rect, _format);

            return icon;
        }

        private static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
