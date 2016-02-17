
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace arXiv
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private string[] idres;
        private string[] titleres;
        public delegate void CallBackDelegate(string message);

        //回调方法
        private void CallBack(string message)
        {
            //主线程报告信息,可以根据这个信息做判断操作,执行不同逻辑.
            MessageBox.Show(message);
        }

        private bool GetWebContent(string Url,out string strResult)
        {
            //string strResult = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                //声明一个HttpWebRequest请求
                request.Timeout = 30000;
                //设置连接超时时间
                //request.Headers.Set("Pragma", "no-cache");
                request.UserAgent = "Railgun";//这里需要fake一下
                request.Accept = "*/*";
                request.UseDefaultCredentials = true;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();//403没有访问权限
                Stream streamReceive = response.GetResponseStream();
                Encoding encoding = Encoding.GetEncoding("GB2312");
                StreamReader streamReader = new StreamReader(streamReceive, encoding);
                strResult = streamReader.ReadToEnd();
                return true;
            }
            catch
            {
                MessageBox.Show("访问被拒绝");
                strResult = null;
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProcessUrl();
            Thread[] t = new Thread[idres.Length];
            for (int i = 0; i < idres.Length; i++)
            {
                //downloadprocess(i);
                t[i] = new Thread(new ParameterizedThreadStart(downloadprocess));
                t[i].Start(i);
                
            }
        }

        private void ProcessUrl()
        {
            string strResult;
            if (GetWebContent("http://arxiv.org/list/stat.ML/recent", out strResult))
            {
                string idexpr = "<a href=\"" + @"/abs/\S*\.\S*" + "\" title";
                idres = MatchExpr(strResult, idexpr);
                //string abs = GetAbsUrl(idres[0]);
                string titlexpr = "<span class=\"descriptor\">Title:</span> " + @".*";
                titleres = MatchExpr(strResult, titlexpr);
                //string title = GetTitle(titleres[0]);
                
            }
        }

        private void downloadprocess(object obj)
        {
            int i = Convert.ToInt32(obj);
            //把回调的方法给委托变量
            //CallBackDelegate cbd = CallBack;
            //for (int i = 0; i < idres.Length; i++)
            //{
            idres[i] = GetAbsUrl(idres[i]);
            titleres[i] = GetTitle(titleres[i]);
            
                //richTextBox1.Text += "id: " + idres[i] + ",  Title: " + titleres[i] + ";\n";
            if (DownloadPaper("http://arxiv.org/pdf/" + idres[i] + ".pdf", titleres[i]))
            {
                //把传来的参数转换为委托
                //cbd = obj as CallBackDelegate;
                ////执行回调.
                //cbd("这个线程传回的信息");
            }
            //}
        }


        private static string GetAbsUrl(string x)
        {
            int end = x.LastIndexOf("\"");
            return x.Substring(14, end-14);
        }

        private static string GetTitle(string x)
        {
            //int end = x.LastIndexOf("\"");
            return x.Substring(39);
        }

        private bool DownloadPaper(string URL, string FileName)
        {
            try
            {
                WebClient myWebClient = new WebClient();
                myWebClient.UseDefaultCredentials = true;
                myWebClient.Headers.Add("User-Agent: Other"); string cpath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\arXivPaper";
                if (!Directory.Exists(cpath))
                {
                    Directory.CreateDirectory(cpath);
                }
                //if (!File.Exists(cpath + @"\" + FileName + ".pdf"))
                //{
                //    myWebClient.DownloadFile(URL, cpath + @"\" + FileName + ".pdf");
                //}
                if (!File.Exists(cpath + @"\" + FileName + ".pdf"))
                {
                    if (richTextBox1.InvokeRequired)
                    {
                        richTextBox1.Invoke(
                            new MethodInvoker(
                                delegate { richTextBox1.Text += "id: " + URL + ",  Title: " + FileName + ";\n"; }));
                    }
                    myWebClient.DownloadFile(URL, cpath + @"\" + FileName + ".pdf");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
            
        }


        private string[] MatchExpr(string text, string expr)
        {
            MatchCollection mc = Regex.Matches(text, expr);
            string []res=new string[mc.Count];
            int i = 0;
            foreach (Match m in mc)
            {
                res[i] = m.ToString();
                //Console.WriteLine(m);
                //MessageBox.Show(m.ToString());
                i++;
            }
            return res;
        }

        public string SplitName(string longStrName, string name)
        {
            string result = "";
            if (name.Equals("MusicName"))
            {
                if (longStrName.IndexOf("(") == -1)
                    result = longStrName;
                else result = longStrName.Substring(0, longStrName.IndexOf("(") - 1);
            }
            else if (name.Equals("Singer"))
            {
                if (longStrName.IndexOf("(") == -1)
                    result = "";
                else result = longStrName.Substring(longStrName.IndexOf("(") + 1).TrimEnd(new char[] { ')' });
            }
            return result;
        }

        public void AddLine(string id, string music, string sing, DataTable dt)
        {
            //DataTable dt = (DataTable)this.ViewState["dt"];
            DataRow dr = dt.NewRow();
            dr["Id"] = id;
            dr["MusicName"] = music;
            dr["Singer"] = sing;
            dt.Rows.Add(dr);
            dt.AcceptChanges();
            string ConnString = @"Data Source=*;Initial Catalog=*;User ID=sa;pwd=*";
            SqlConnection conn = new SqlConnection(ConnString);
            SqlCommand comm = new SqlCommand();
            comm.Connection = conn;
            conn.Open();
            string sql = "insert into Music(MusicName,Singer) values('" + music + "','" + sing + "')";
            comm.CommandText = sql;
            comm.ExecuteNonQuery();
            conn.Close();
        }

        private void getData()
        {
            //要抓取的URL地址
            //string Url = this.textBox1.Text;            //得到指定Url的源码
            string strResult;
            GetWebContent("http://list.mp3.baidu.com/topso/mp3topsong.html?id=1#top2", out strResult);
            this.richTextBox1.Text = strResult;
            //取出和数据有关的那段源码
            int iBodyStart = strResult.IndexOf("<body", 0);
            int iStart = strResult.IndexOf("歌曲TOP500", iBodyStart);
            int iTableStart = strResult.IndexOf("<table", iStart);
            int iTableEnd = strResult.IndexOf("</table>", iTableStart);
            string strWeb = strResult.Substring(iTableStart, iTableEnd - iTableStart + 8);

            //
            DataTable dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("MusicName");
            dt.Columns.Add("Singer");

            //生成HtmlDocument
            WebBrowser webb = new WebBrowser();
            webb.Navigate("about:blank");
            HtmlDocument htmldoc = webb.Document.OpenNew(true);
            htmldoc.Write(strWeb);
            HtmlElementCollection htmlTR = htmldoc.GetElementsByTagName("TR");
            foreach (HtmlElement tr in htmlTR)
            {
                string strID = tr.GetElementsByTagName("TD")[0].InnerText;
                string strName = SplitName(tr.GetElementsByTagName("TD")[1].InnerText, "MusicName");
                string strSinger = SplitName(tr.GetElementsByTagName("TD")[1].InnerText, "Singer");
                strID = strID.Replace(".", "");
                //插入DataTable
                AddLine(strID, strName, strSinger, dt);

                string strID1 = tr.GetElementsByTagName("TD")[2].InnerText;
                string strName1 = SplitName(tr.GetElementsByTagName("TD")[3].InnerText, "MusicName");
                string strSinger1 = SplitName(tr.GetElementsByTagName("TD")[3].InnerText, "Singer");
                //插入DataTable
                strID1 = strID1.Replace(".", "");
                AddLine(strID1, strName1, strSinger1, dt);
                string strID2 = tr.GetElementsByTagName("TD")[4].InnerText;
                string strName2 = SplitName(tr.GetElementsByTagName("TD")[5].InnerText, "MusicName");
                string strSinger2 = SplitName(tr.GetElementsByTagName("TD")[5].InnerText, "Singer");
                //插入DataTable
                strID2 = strID2.Replace(".", "");
                AddLine(strID2, strName2, strSinger2, dt);
                if (strID2.Equals("498"))
                    break;

            }

            //dataGridView1.DataSource = dt.DefaultView;
            //dataGridView1.Columns[1].Width = 165;
            //dataGridView1.Columns[2].Width = 165;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse)
            {
                if (e.Node.Checked)//若选中了某节点，则设置其所有子节点为选中状态
                {
                    TreeViewOperator.setChildNodeCheckedState(e.Node, true);
                }
                else
                {
                    TreeViewOperator.setChildNodeCheckedState(e.Node, false);//否则设置其所有字节点为没选中状态
                    if (e.Node.Parent != null)
                    {
                        TreeViewOperator.setParentNodeCheckedState(e.Node, false);//如果当前节点有父节点，则取消父节点选中状态
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode tnode = treeView1.TopNode;//得到topnode,也就是treeview的最最最根节点（第一列节点）
            while (tnode.PrevNode != null)
            {
                tnode = tnode.PrevNode;//最最最根节点不止一个吗，一直往上找，找到最左上角那个节点
            }
            TreeViewOperator.getstring(tnode);
        }

    }
}
