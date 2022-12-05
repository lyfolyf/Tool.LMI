using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lead.Tool.Interface;
using Lead.Tool.Resources;

namespace Lead.Tool.LMI
{
    public class LMICreat: ICreat
    {       
        public ITool GetInstance(string Name, string Path)
        {
            return new LMITool(Name, Path);
        }

        public System.Drawing.Image Icon
        {
            get
            {
                return (Image)ImageManager.GetImage("LMI");
            }
        }

        public string Name
        {
            get
            {
                return "LMI";
            }
        }
    }
}
