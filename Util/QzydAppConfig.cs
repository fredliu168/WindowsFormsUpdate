using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webapi.Util
{
    class AppConfig
    {
        public static string APP_URL = "http://www.qzcool.com/xxx/xxxx.zip";//应用下载地址
        public static string LOAD_MAIN_APP_NAME = "Explore.exe";//升级成功后加载的主程序名称 
       
    }
}
