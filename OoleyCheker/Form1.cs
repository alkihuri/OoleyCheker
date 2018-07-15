using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FirebirdSql;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Net;
using System.Web; 

namespace OoleyCheker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            latest_id.Add(0);
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false; 
        }

        List<int> latest_id = new List<int>();


        public void  GetData ()
        { 
            File.WriteAllText("lastid", latest_id.Max().ToString() );

            //так проверять состояние соединения (активно или не активно)
            if (fb.State == ConnectionState.Closed)
                fb.Open();

            FbTransaction fbt = fb.BeginTransaction(); //стартуем транзакцию; стартовать транзакцию можно только для открытой базы (т.е. мутод Open() уже был вызван ранее, иначе ошибка)

            FbCommand SelectSQL = new FbCommand("select * from FB_EVN FULL OUTER JOIN FB_USR ON FB_EVN.USR = FB_USR.ID; ", fb); //задаем запрос на выборку

            SelectSQL.Transaction = fbt; //необходимо проинициализить транзакцию для объекта SelectSQL
            FbDataReader reader = SelectSQL.ExecuteReader(); //для запросов, которые возвращают результат в виде набора данных надо использоваться метод ExecuteReader()

            string select_result = ""; //в эту переменную будем складывать результат запроса Select

            try
            {
                while (reader.Read()) //пока не прочли все данные выполняем...
                {

                    ///

                    latest_id.Add(reader.GetInt32(0)); 
                    select_result = 
                      //  reader.GetString(0) + "\r\n" 
                        reader.GetString(1) + "\r\n" 
                        +reader.GetString(29) + "\r\n<"
                        + reader.GetString(7) + "<\r\n"
                        + reader.GetString(2) + "\r\n" +
                       // reader.GetString(33) + "\r\n"



                         "\n";

                    select_result = select_result.Replace("000B3A001B7B", " 5этаж");
                    select_result = select_result.Replace("000B3A001B7A", " 6этаж");
                    if (reader.GetInt32(0) > Convert.ToInt32(File.ReadAllText("lastid")))
                    {
                        label1.Text = "last id = " + latest_id.Max();
                        richTextBox1.Text = select_result;
                        if(checkBox1.Checked)
                        {
                           if(reader.GetInt32(3)==2)
                            sendteleg(select_result);
                        }
                        File.WriteAllText("lastid", latest_id.Max().ToString());
                    }
                    
                    
                }
            }
            finally
            {
                //всегда необходимо вызывать метод Close(), когда чтение данных завершено
                reader.Close();
                fb.Close(); //закрываем соединение, т.к. оно нам больше не нужно
            }
           // MessageBox.Show(select_result); //выводим результат запроса
            SelectSQL.Dispose(); //в д
            

        }
        /// <summary>
        /// 000B3A001B7B - 5
        /// 000B3A001B7A - 6 
        /// 1518566023167 - камиль 
        /// </summary>

        FbConnection fb;

        /// <summary>
        /// 1518623161148
        /// </summary>
        /// <param name="line"></param>
        public void sendteleg(string line)
        {

            List<string> cheker = GetReq("http://alkihuri.ru/cheker/ooley.txt");         
            
            
                if(!line.Contains("1518623161148")&&cheker[0].Contains("ON"))
                {
                line = line.Split('<')[0] + line.Split('<')[2]; 
                line = Uri.EscapeDataString(line);
                try
                {
                    GetReq("https://api.telegram.org/bot479709612:AAHgac6lvxOV_lSKllOHsZdlBTj4EdISZOE/sendMessage?chat_id=-286482601&text="
                    + line);
                }
                catch (Exception dd)
                {
                    GetReq("https://api.telegram.org/bot479709612:AAHgac6lvxOV_lSKllOHsZdlBTj4EdISZOE/sendMessage?chat_id=-286482601&text=ПользовательПрошел!");
                    File.WriteAllText("data.txt", line + Environment.NewLine);

                }                
            }           
        }

        public List<string> GetReq(string url)
        {

            List<string> resp = new List<string>();
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(url);
            wrGETURL.Proxy = WebProxy.GetDefaultProxy();
            Stream objStream;
            objStream = wrGETURL.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);
            string sLine = "";
            int i = 0;
            while (sLine != null)
            {
                i++;
                sLine = objReader.ReadLine();
                if (sLine != null)
                    resp.Add(sLine);
            }
            return resp;
        }


        public void connectDB()
        {
            FbConnectionStringBuilder fb_con = new FbConnectionStringBuilder();
            fb_con.Charset = "UNICODE_FSS"; //используемая кодировка
            fb_con.UserID = "SYSDBA";//логин
            fb_con.Password = "masterkey"; //пароль
            ///CBASE.FDB GBASE.FDB 
            fb_con.Database = "C:\\Program Files (x86)\\ENT\\Server\\DB\\CBASE.FDB "; //путь к файлу базы данных
            fb_con.ServerType = 0; //указываем тип сервера (0 - "полноценный Firebird" (classic или super server), 1 - встроенный (embedded))

            //создаем подключение
            fb = new FbConnection(fb_con.ToString()); //передаем нашу строку подключения объекту класса FbConnection

            fb.Open(); //открываем БД
            FbDatabaseInfo fb_inf = new FbDatabaseInfo(fb);
            this.Text = ("Info: " + fb_inf.ServerClass + "; " + fb_inf.ServerVersion); //выводим тип и версию используемого сервера Firebird

           //информация о БД

        }
        private void button1_Click(object sender, EventArgs e)
        {
            connectDB();
            //пока у объекта БД не был вызван метод Open() - никакой информации о БД не получить, будет только ошибка
            // MessageBox.Show("Info: " + fb_inf.ServerClass + "; " + fb_inf.ServerVersion   ); //выводим тип и версию используемого сервера Firebird

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           
                while(1==1)
                    {
                        GetData();
                    }
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(); 
        }
    }

 
}
