using Webapi.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace WindowsFormsGroupSms.Util
{
    public partial class Form_DownLoad : Form
    {
        private string zipTempFile { get; set; } //压缩的文件
        private string unZipTempFile { get; set; } //解压缩的临时目录
        private string tempUpdateDirectory { get; set; } //下载的临时目录
        public Form_DownLoad()
        {
            InitializeComponent();
        }

        private void Form_DownLoad_Load(object sender, EventArgs e)
        { 
    
            this.Text = "下载升级文件";
            Action downLoad = new Action(DownLoadThreadProc);
            downLoad.BeginInvoke(new AsyncCallback(Callback), null);
        }

        /// <summary>
        /// 设置窗体标题
        /// </summary>
        /// <param name="title"></param>
        private void setFormTitle(string title)
        { 

            if (this.InvokeRequired)
            {
                Action<string> actionDelegate = (x) => { this.Text = x.ToString(); };
                // 或者

                this.Invoke(actionDelegate, title);
            }
            else
            {
                this.Text = title;
            }
        }

        /// <summary>
        /// 这里是运行结束后的通知
        /// </summary>
        /// <param name="ar"></param>
        void Callback(IAsyncResult ar)
        {  

            setFormTitle("下载升级文件完成"); 

            setFormTitle("解压升级文件");

            // 解压缩文件
            UnZipFile();
            
            setFormTitle("解压升级文件完成");

            setFormTitle("开始替换升级文件");
            MoveDirFile(unZipTempFile, Directory.GetCurrentDirectory() + "\\");
            setFormTitle("升级完成,正在启动应用程序");
            //启动住应用程序
            string MainAppPath = String.Format("{0}\\{1}", Directory.GetCurrentDirectory(),AppConfig.LOAD_MAIN_APP_NAME);

           // MessageBox.Show(MainAppPath);
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = MainAppPath;
            exep.EnableRaisingEvents = true;
            // exep.Exited += new EventHandler(exep_Exited);
            exep.Start();

            //退出当前程序
            System.Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// 解压缩文件
        /// </summary>
        public void UnZipFile()
        {
            //解压目录
            unZipTempFile = String.Format("{0}\\tempUpdate\\unzip", Directory.GetCurrentDirectory());

            if (!Directory.Exists(unZipTempFile))
            {
                Directory.CreateDirectory(unZipTempFile);
            }
            else
            {//删除文件 
                Directory.Delete(unZipTempFile, true);
                //然后再新建一个空的文件夹
                Directory.CreateDirectory(unZipTempFile);
            }

            //解压文件
            ZipFile.ExtractToDirectory(zipTempFile, unZipTempFile);

          
        }

        /// <summary>   
        /// 移动目录内的文件到另一目录   
        /// </summary>   
        /// <param name="sorDir">源目录，如：Server.MapPath("~/product_image/44/8813/")</param>   
        /// <param name="desDir">目标目录，如：Server.MapPath("~/product_image/141/8813/")</param>   
        public static void MoveDirFile(string sorDir, string desDir)
        {
            if (!Directory.Exists(sorDir))
            {
                return;
            }
            if (!Directory.Exists(desDir))
            {
                Directory.CreateDirectory(desDir);
            }
            //得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
            string[] fileList = Directory.GetFileSystemEntries(sorDir);
            
            //遍历所有的文件和目录
            foreach (string item in fileList)
            {
                try
                {
                    if (Directory.Exists(item))
                    {
                        MoveDirFile(item+"\\", desDir + Path.GetFileName(item)+"\\");
                    }else
                    {
                        FileInfo fi = new FileInfo(item);
                        string tmp = desDir + fi.Name;
                        if (File.Exists(tmp))
                        {
                            File.Delete(tmp);
                        }
                        fi.MoveTo(tmp);
                    }
                    
                }
                catch (Exception)
                {
                    throw;
                }
            }
            Directory.Delete(sorDir, true);
        }

        /// <summary>
        /// 下载线程
        /// </summary>
        public  void DownLoadThreadProc()
        {
            tempUpdateDirectory = String.Format("{0}\\tempUpdate", Directory.GetCurrentDirectory());

            if (!Directory.Exists(tempUpdateDirectory))
            {
                Directory.CreateDirectory(tempUpdateDirectory);
            } 

            zipTempFile = String.Format("{0}\\temp.zip", tempUpdateDirectory); 

            DownloadFile(AppConfig.APP_URL, zipTempFile, progressBar, label_tip);
        }

        delegate void SetValueCallback(int value);
        private void SetProcessBarValue(int value)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (progressBar.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetProcessBarValue);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.progressBar.Value = value;
            }
        }

        private void SetProcessBarMaxValue(int value)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (progressBar.InvokeRequired)
            {
                SetValueCallback d = new SetValueCallback(SetProcessBarMaxValue);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.progressBar.Maximum = value;
            }
        }


        /// <summary>        
        /// c#,.net 下载文件        
        /// </summary>        
        /// <param name="URL">下载文件地址</param>       
        /// 
        /// <param name="Filename">下载后的存放地址</param>        
        /// <param name="Prog">用于显示的进度条</param>        
        /// 
        public   void DownloadFile(string URL, string filename, System.Windows.Forms.ProgressBar prog, System.Windows.Forms.Label label1)
        {
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;

                SetProcessBarMaxValue((int)totalBytes);  

                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize); 

                    SetProcessBarValue((int)totalDownloadedByte); 
                    
                    osize = st.Read(by, 0, (int)by.Length);

                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;

                    string showTip = "当前下载进度" + percent.ToString("f2") + "%";

                    Trace.WriteLine(showTip);


                    if (label1.InvokeRequired)
                    {
                        Action<string> actionDelegate = (x) => { label1.Text = x.ToString(); };
                        // 或者

                        label1.Invoke(actionDelegate, showTip);
                    }
                    else
                    {
                        label1.Text = showTip;
                    }
                     

                    System.Windows.Forms.Application.DoEvents(); //必须加注这句代码，否则label1将因为循环执行太快而来不及显示信息
                }
                so.Close();
                st.Close();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

       
        
    }
}
    
