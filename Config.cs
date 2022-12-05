using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lead.Tool.LMI
{
    public class Config
    {        
        public string Name { get; set; }
        public LMIconfig Sensor { get; set; } = new LMIconfig();
    }
    public class LMIconfig
    {
        public string CameraId { get; set; }//相机序号

        public int index { get; set; }//相机索引

        public string IP { get; set; } = "192.168.1.10";//地址

        public string JobName { get; set; } = "dsf.job";

    }

}
