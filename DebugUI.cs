using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Tool.LMI
{
    public partial class DebugUI : UserControl
    {
        LMITool _Proxy = null;
        public DebugUI(LMITool proxy)
        {
            InitializeComponent();

            _Proxy = proxy;
        }
    }
}
