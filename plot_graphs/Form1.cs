using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Xml;
using ZedGraph;

namespace plot_graphs
{
    public partial class Form1 : Form
    {
        string DataIn="";
        string temp1="", temp2="";
        double datanumber=0.0, realtime=0.0;
        int tickStart=0;
        int status = 0;
        
        public Form1()
        {
            InitializeComponent();
        }

        // Khi nhấn exit 
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        // Khi nhấn kết nối
        private void btnConnect_Click(object sender, EventArgs e)
        {   // cấu hình lấy dữ liệu từ com và baudrate cho cổng serial, ko có cái này ko chạy dc
            serialPort1.PortName = cboxCom.Text;
            serialPort1.BaudRate = Convert.ToInt32(cboxBaudRate.Text);

            if (serialPort1.IsOpen) return;
            serialPort1.Open();
            btnConnect.Enabled = false;
            btnDisConnect.Enabled = true;
            txtData.Text = temp2;

        }


        // Khi nhấn ngắt kết nối 
        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen == false) return;
            serialPort1.Close();
            btnConnect.Enabled = true;
            btnDisConnect.Enabled = false;
            txtData.Text = "";
        }

        // 
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            cboxCom.Items.AddRange(ports);
            // khoi tao zedgraph
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị dữ liệu theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Dữ Liệu";

            RollingPointPairList list = new RollingPointPairList(60000);
            timer1.Interval = 1;// 1ms gửi lên 1 lần 
            timer1.Start();// chayj timer 1
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Blue, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;

            curve.Line.Width = 3;

            myPane.AxisChange();// gọi hàm xác định cỡ trục
            tickStart = Environment.TickCount;// khởi động timer vể vị trí ban đầu 

            


        }

        private void Draw()
        {
            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;
            // kiểm tra việc khởi tạo các đường curve
            // đưa vể điểm xuất phát 

            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            if (curve == null)
                return;
            // list các điểm 
            IPointListEdit list = curve.Points as IPointListEdit;

            if (list == null)
                return;
            // realtime được tính bắng ms
            realtime = (Environment.TickCount - tickStart) / 1000.0;

            list.Add(realtime, datanumber); // hàm hiển thị dữ liệu trên đồ thị 

            

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (realtime > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = realtime + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }


            // Tự động Scale theo trục y
            if (datanumber > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = datanumber + yScale.MajorStep;
            }
            else if (datanumber < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = datanumber - yScale.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string[] temp = serialPort1.ReadLine().Split('a');// đọc một dòng của serial, cắt chuỗi khi gặp kí tự a
            temp1 = temp[0];// Chuỗi đầu tiên lưu vào temp1
            temp2 = temp[1];// Chuỗi thứ hai lưu vào temp2

            double.TryParse(temp2, out datanumber);// chuyển sang kiểu double;


            status = 1;
            this.Invoke(new EventHandler(ShowData));

        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
       

        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            serialPort1.Open();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            
            datanumber = 0;
            temp2 = "";
            status = 0;
            ClearZedGraph();
            listView1.Items.Clear();
            txtData.Text = "0";

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(!serialPort1.IsOpen)
            {
                progressBar1.Value = 0;
            }
            else if (serialPort1.IsOpen)
            {
                progressBar1.Value = 100;
                Draw();
                Data_Listview();
                status = 0;
            }

        }

        private void ShowData(object sender, EventArgs e)
        {
            txtData.Text = temp2;
            
        }
        // Xóa đồ thị, với ZedGraph thì phải khai báo lại như ở hàm Form1_Load, nếu không sẽ không hiển thị

        private void ClearZedGraph()
        {
            zedGraphControl1.GraphPane.CurveList.Clear(); // Xóa đường
            zedGraphControl1.GraphPane.GraphObjList.Clear(); // Xóa đối tượng

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị dữ liệu theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Dữ liệu";

            RollingPointPairList list = new RollingPointPairList(60000);
            LineItem curve = myPane.AddCurve("Dữ liệu", list, Color.Blue, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;
            myPane.XAxis.Scale.MajorStep = 5;
            myPane.YAxis.Scale.Min = -100;
            myPane.YAxis.Scale.Max = 100;
            curve.Line.Width = 3;

            zedGraphControl1.AxisChange();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            SaveToExcel();
        }

        // Hiển thị dữ liệu trong ListView
        private void Data_Listview()
        {
            if (status == 0)
                return;
            else
            {
                ListViewItem item = new ListViewItem(realtime.ToString()); // Gán biến realtime vào cột đầu tiên của ListView
                item.SubItems.Add(datanumber.ToString());
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView
                                           // Không nên gán string SDatas vì khi xuất dữ liệu sang Excel sẽ là dạng string, không thực hiện các phép toán được

                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiện thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó
            }
        }

        // Hàm lưu ListView sang Excel
private
    void SaveToExcel()
    {
        Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();
        xla.Visible = true;
        Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);
        Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;
 
        // Đặt tên cho hai ô A1. B1 lần lượt là "Thời gian (s)" và "Dữ liệu", sau đó tự động dãn độ rộng
        Microsoft.Office.Interop.Excel.Range rg = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("A1", "B1");
        ws.Cells[1, 1] = "Thời gian (s)";
        ws.Cells[1, 2] = "Dữ liệu";
        rg.Columns.AutoFit();
 
        // Lưu từ ô đầu tiên của dòng thứ 2, tức ô A2
        int i = 2;
        int j = 1;
 
        foreach (ListViewItem comp in listView1.Items) {
            ws.Cells[i, j] = comp.Text.ToString();
            foreach (ListViewItem.ListViewSubItem drv in comp.SubItems) {
                ws.Cells[i, j] = drv.Text.ToString();
                j++;
            }
            j = 1;
            i++;
        }
    }

    }
}
