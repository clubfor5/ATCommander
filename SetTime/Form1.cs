using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;

namespace SetTime
{
    public partial class Form1 : Form
    {
        SerialPort mySerial = null; //声明串口类
        bool isOpen = false; //打开串口标志
        bool comExistence = true; //端口是否存在
        bool isSetPort = false; //是否设置参数

        public Form1()
        {
            InitializeComponent();
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            initial();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenCloseCom();
        }

        private void button2_Click(object sender, EventArgs e)
        {//时间校准
            string year= ((DateTime.Now.Year)%100).ToString("D2");
            string month = DateTime.Now.Month.ToString("D2");
            string day = DateTime.Now.Day.ToString("D2");
            string week = DateTime.Now.DayOfWeek.ToString();
            string hour = DateTime.Now.Hour.ToString("D2");
            string minute = DateTime.Now.Minute.ToString("D2");
            string second = DateTime.Now.Second.ToString("D2");
            switch(week)
            {
                case "Sunday": week = "0"; break;
                case "Monday": week = "1"; break;
                case "Tuesday": week = "2"; break;
                case "Wednesday": week = "3"; break;
                case "Thursday": week = "4"; break;
                case "Friday": week = "5"; break;
                case "Saturday": week = "6"; break;
            }
            string now = year+month+day+week+hour+minute+second;
            Console.WriteLine("AT+TIME=" + now);
            SendData("AT+TIME=" + now);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == string.Empty || textBox2.Text == string.Empty)
            {
                MessageBox.Show("请输入用户名和密码");
                return;
            }
            Console.WriteLine("AT+USER=" + textBox1.Text + "," + textBox2.Text);
            SendData("AT+USER=" + textBox1.Text + "," + textBox2.Text);
        }

        void initial()
        {
            CheckCOM();
            SetPortProperty(comboBox1.Text, "9600");
            OpenCloseCom();
        }

        public void OpenCom()
        {
            if (!comExistence) //检测串口设置
            {
                Console.WriteLine("Can't find port.");
                return;
            }

            if (!isSetPort)
            {
                SetPortProperty(comboBox1.Text, "9600");
                if (!isSetPort) return; 
            }
            try
            {
                mySerial.Open();
                isOpen = true;
                button1.Text = "断开";
                Console.WriteLine(mySerial.PortName + " Open.");
            }
            catch
            {
                isOpen = false;
                button1.Text = "连接";
                Console.WriteLine(mySerial.PortName + " open failed.");
            }
        }

        public void CloseCom()
        {
            try
            {
                CheckCOM();
                mySerial.Close();
                mySerial.Dispose();
                isOpen = false;
                button1.Text = "连接";
                Console.WriteLine(mySerial.PortName + " Closed.");
            }
            catch
            {
                button1.Text = "断开";
                Console.WriteLine(mySerial.PortName + " close failed.");
            }
        }

        public void OpenCloseCom()
        {//打开或关闭串口
            if (isOpen == false) //尚未打开
            {
                OpenCom();
            }
            else //已打开
            {
                CloseCom();
            }
        }

        public void CheckCOM()
        {//检测端口
            comboBox1.Items.Clear();
            foreach (string comName in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(comName);
                comExistence = true;
            }
            if (comboBox1.Items.Count > 0) comboBox1.Text = (string)comboBox1.Items[0];
            if (comboBox1.Items.Count > 1) comboBox1.Text = (string)comboBox1.Items[1];
        }

        public void SetPortProperty(string comPort = "COM1", string comBaudRate = "9600")
        { //设置串口属性
            try
            {
                mySerial = new SerialPort();
                mySerial.PortName = comPort.Trim(); //端口名
                mySerial.BaudRate = Convert.ToInt32(comBaudRate.Trim()); //波特率
                mySerial.Parity = Parity.None;
                mySerial.DataBits = 8;//数据位
                mySerial.StopBits = StopBits.One;

                mySerial.ReadTimeout = -1;//读取超时时间
                mySerial.RtsEnable = true;
                mySerial.DataReceived += new SerialDataReceivedEventHandler(ReceiveEven);

                isSetPort = true;
            }
            catch
            {
                Console.WriteLine("Setting port failed.");
            }
        }

        public void SendData(string DataToSend)
        {//发送数据
            
            if (isOpen)
            {
                try
                {
                    mySerial.Write(DataToSend);
                    Console.WriteLine("Sent:" + DataToSend);
                    
                }
                catch
                {
                    Console.WriteLine("Sending failed.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Port not open.");
            }
        }

        public void ReceiveEven(Object sender, SerialDataReceivedEventArgs e)
        {//接收数据事件
            Thread.Sleep(400);//等待接收完毕
            this.Invoke((EventHandler)(delegate { ReceiveData(); }));
            //ReceiveData();
        }

        public void ReceiveData()
        { //接收数据
            string DataReceive = string.Empty;
            Byte[] TempReceive = new Byte[mySerial.BytesToRead];
            mySerial.Read(TempReceive, 0, TempReceive.Length);
            foreach (byte temp in TempReceive)
            {
                DataReceive += ((char)temp).ToString();
            }
            mySerial.DiscardInBuffer();
            Console.Write("Received[");
            Console.Write(DataReceive);
            Console.WriteLine("]");
        }

    }
}
