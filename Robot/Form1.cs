using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using static Robot.Form1;

namespace Robot
{
    public partial class Form1 : Form
    {
        RobotUDP robot;
        RobotUDP robotSend;
        RobotState robotState;
        //等比例缩放的原始点，控件位置调整
        private float X;
        private float Y;
        public float X1 { get => X; set => X = value; }
        public float Y1 { get => Y; set => Y = value; }
        private int curentPostion = 0;

        private const int testingMachinePosition = 0;//试验机位置
        private const int MeasurementStationPosition = 45;//测量台架子
        private const int ezcad = 90;//打标机架子
        private const int Sample1 = 120; //1-3试样架子
        private const int Sample4 = 180;//4-6试样架子
        private const int WasteRackLocation = -90;//废料架子

        private int currentImageIndex = 1;
        // 定义一个状态对象
        public class RobotState
        {
            public bool State0 { get; set; }//试样达到打标位置
            public bool State1 { get; set; }//试样打标中
            public bool State2 { get; set; }//上断样回收完成
            public bool State3 { get; set; }//下断样回收完成
            public bool State4 { get; set; }//试样退出打标位置
            public bool State5 { get; set; }//机械手去上夹具取料中
            public bool State6 { get; set; }//机械手去下夹具取料中
            public bool State7 { get; set; }//试样尺寸测量中
            public bool State8 { get; set; }//机械手试样架取样中
            public bool State9 { get; set; }//机械手去打标机放样中
            public bool State10 { get; set; }//机械手去打标机取样中
            public bool State11 { get; set; }//机械手去测量台送样中
            public bool State12 { get; set; }//机械手去测量台取样中
            public bool State13 { get; set; }//机械手去主机送样中
            public bool State14 { get; set; }//机械手去回收架卸料中
            public bool State15 { get; set; }//试样打标中
        }
        public Form1()
        {
            InitializeComponent();
            robotState = new RobotState();
            //=======================窗体缩放===============================================================
            X1 = this.Width;
            Y1 = this.Height;
            //在窗体加载时候  解决闪烁问题
            //将图像绘制到缓冲区 减少闪烁
            this.DoubleBuffered = true;//设置本窗体
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);// 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
             //=======================窗体缩放完===============================================================
            timerA.Interval = 200;
            timerA.Start();
        }
        private void SetControls(float newx, float newy, Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                if (con.Tag != null)
                {
                    string nameTag = con.Tag.ToString();
                    if (nameTag.Contains(":"))
                    {
                        string[] mytag = nameTag.Split(':');
                        float a = Convert.ToSingle(mytag[0]) * newx;
                        con.Width = (int)a;
                        a = Convert.ToSingle(mytag[1]) * newy;
                        con.Height = (int)(a);
                        a = Convert.ToSingle(mytag[2]) * newx;
                        con.Left = (int)(a);
                        a = Convert.ToSingle(mytag[3]) * newy;
                        con.Top = (int)(a);
                        Single currentSize = Convert.ToSingle(mytag[4]) * Math.Min(newx, newy);
                        con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                        if (con.Controls.Count > 0)
                        {
                            SetControls(newx, newy, con);
                        }
                    }
                }
            }
        }
        private void SetTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    SetTag(con);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //=======================窗体缩放===============================================================
            //按键等比例缩放，注册到窗口Resize事件中
            this.Resize += new EventHandler(MainForm_Resize);
            SetTag(this);
            //=======================窗体缩放完==============================================================
        }
        private void MainForm_Resize(object sender, EventArgs e)
        {
            //=======================窗体缩放===============================================================
            float newx = (this.Width) / X1;
            float newy = this.Height / Y1;
            SetControls(newx, newy, this);
            //=======================窗体缩放===============================================================
        }
      
  
        private void button1_Click(object sender, EventArgs e)
        {
            robot = new RobotUDP(8011);
            robotSend = new RobotUDP(8000);
            // 将事件处理方法注册到 MessageReceived 事件上
            robot.MessageReceived += robot_MessageReceived;//订阅
            robot.StartListening();
        }
        private void robot_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // 处理接收到的消息
            // 在这里可以更新 UI 或执行其他逻辑
            MessageBox.Show($"Received message: {e.Message} from {e.RemoteEndPoint}");
            //robotState = JsonConvert.DeserializeObject<RobotState>(e.Message);
            //MessageBox.Show(robotState.State0.ToString());
           
        }
        private void button3_Click(object sender, EventArgs e)
        {
            rotationAngle = 0;
            RotateImage(rotationAngle);
            if(robot != null)
            {
                robot.Close();
            }
            timerA.Stop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (robotSend != null)
            {
                robotSend.SendMessage("深圳三思纵横科技股份有限公司SUNS", "127.0.0.1");
            }     
            
        }
      
        private void button4_Click(object sender, EventArgs e)
        {
            //  将对象序列化为JSON字符串
            /* RobotState person = new RobotState { State0 = false, State1 = true, State2 = true, State3 = true, State4 = true, State5 = true, State6 = true, State7 = true, State8 = true, State9 = true, State10 = true, State11 = true, State12 = true, State13 = true, State14 = true, State15 = true};
            string json = JsonConvert.SerializeObject(person);

             // 将JSON字符串反序列化为对象
             string json = "{\"Name\":1,\"Age\":0}";
             RobotState person2 = new RobotState();
             person2 = JsonConvert.DeserializeObject<RobotState>(json);
             MessageBox.Show(person2.State0.ToString());*/
            rotationAngle = 180;
            RotateImage(rotationAngle);
            robotState.State13 = true;
           

        }

      

        private int rotationAngle = 0;
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
          
        }
        private void RotateImage(float angle)
        {
            string imageName = $"rob.png";
            Image image = Image.FromFile(imageName);
            Bitmap bitmap = new Bitmap(image);
            image.Dispose();

            Bitmap rotatedBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            rotatedBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

            using (Graphics g = Graphics.FromImage(rotatedBitmap))
            {
                g.TranslateTransform((float)rotatedBitmap.Width / 2, (float)rotatedBitmap.Height / 2); // 将坐标原点移到图片中心
                g.RotateTransform(angle); // 旋转角度
                g.TranslateTransform(-(float)rotatedBitmap.Width / 2, -(float)rotatedBitmap.Height / 2); // 将坐标原点移回左上角
                g.DrawImage(bitmap, new Point(0, 0));
            }
            pictureBox2.Image = rotatedBitmap;
        }

        private void TimeA_time(object sender, EventArgs e)
        {
            //机械手当前位置 初始化时在 试验机位置
            if (robotState.State8 && rotationAngle != Sample1)//机械手试样架取样中
            {
                if(rotationAngle < Sample1)
                {
                    rotationAngle += 5;
                }else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }else if(robotState.State9 && rotationAngle != ezcad)//机械手送样到打标机
            {
                if (rotationAngle < ezcad)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }else if (robotState.State11 && rotationAngle != MeasurementStationPosition)//机械手送样到测量台
            {
                if (rotationAngle < MeasurementStationPosition)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }else if (robotState.State13 && rotationAngle != testingMachinePosition)//机械手送样到主机
            {
                if (rotationAngle < testingMachinePosition)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }
            else if (robotState.State5 && rotationAngle != testingMachinePosition)//机械手取上断样
            {
                if (rotationAngle < testingMachinePosition)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }
            else if (robotState.State6 && rotationAngle != testingMachinePosition)//机械手取下断样
            {
                if (rotationAngle < testingMachinePosition)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }
            else if (robotState.State14 && rotationAngle != WasteRackLocation)//机械手将上断样放入回收架
            {
                if (rotationAngle < WasteRackLocation)
                {
                    rotationAngle += 5;
                }
                else
                {
                    rotationAngle -= 5;
                }
                RotateImage(rotationAngle);
            }
        }
    }
}
