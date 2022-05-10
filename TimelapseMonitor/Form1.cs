using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using AForge;
using AForge.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace TimelapseMonitor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        string time;
        bool canstop = false;
        private FilterInfoCollection videoDevices;

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                label3.Text = "No local capture devices";
                videoDevices = null;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            Visible = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = false;
            Visible = false;
            notifyIcon1.Visible = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            canstop = true;
            button1.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[comboBox1.SelectedIndex].MonikerString);
            //videoSource.DesiredFrameSize = new System.Drawing.Size(1920, 1080);
            //videoSource.DesiredFrameRate = 1;

            videoSourcePlayer1.VideoSource = videoSource;
            videoSourcePlayer1.Start();

            label3.Text = "Camera Connected";
        }

        public void capture(int i)
        {
            this.Invoke(new Action(() =>
            {
                string picName = @"C:\captures\" + time + "\\" + i + ".jpg";
                Bitmap bt = new Bitmap(640,360);
                bt = videoSourcePlayer1.GetCurrentVideoFrame();
                bt.Save(picName, ImageFormat.Jpeg);
                bt.Dispose();
                GC.Collect();
                //BinaryReader br = new BinaryReader(videoSourcePlayer1.GetCurrentVideoFrame());
                /*BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        videoSourcePlayer1.GetCurrentVideoFrame().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                PngBitmapEncoder pE = new PngBitmapEncoder();
                pE.Frames.Add(BitmapFrame.Create(bitmapSource));*/
            }));
        }
        public void doingcapture()
        {
            for (int i=0;!canstop;i++)
            {
                capture(i);
                label3.Text = "NO." + i;
                Thread.Sleep(Convert.ToInt32(numericUpDown1.Value)*1000);
            }
            label3.Text = "Stopped!";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            canstop = false;
            time = DateTime.Now.ToString("yyyyMMddhhmmss");
            Directory.CreateDirectory(@"C:\captures\" + time);
            var task1 = Task.Factory.StartNew(() => doingcapture());
            button1.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            canstop = true;
            button1.Enabled = true;
            label3.Text = "Start to convert";
            Process process = new Process();
            process.StartInfo.FileName = "ffmpeg.exe";
            process.StartInfo.Arguments = $" -f image2 -r {Convert.ToInt32(numericUpDown2.Value)} -i C:\\captures\\{time}\\%d.jpg C:\\Users\\xuanzhi233\\Desktop\\{time}.mp4";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            DirectoryInfo di = new DirectoryInfo($"C:\\captures\\{time}");
            di.Delete(true);
            label3.Text = "Finished!";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            Process.GetCurrentProcess().Kill();
        }
    }
}
