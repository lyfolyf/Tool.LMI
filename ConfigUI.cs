using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lead.Tool.XML;

namespace Lead.Tool.LMI
{
    public partial class ConfigUI : UserControl
    {
        LMITool _Proxy = null;
        public ConfigUI(LMITool proxy)
        {
            InitializeComponent();

            _Proxy = proxy;

            this.propertyGrid1.SelectedObject = _Proxy._Config.Sensor;
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var re = XmlSerializerHelper.WriteXML(_Proxy._Config, _Proxy._ConfigPath, typeof(Config));

                if (!re)
                {
                    throw new Exception("保存至文件失败");

                }

                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败:" + ex.Message);
            }
        }
    }
}
