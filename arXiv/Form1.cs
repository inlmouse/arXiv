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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string[] idres;
        private string[] titleres;

        private delegate void SetPos(int ipos, string vinfo);
        public void RefreshPrg(int ipos, string vinfo)
        {
            if (this.InvokeRequired)
            {
                SetPos setpos = new SetPos(RefreshPrg);
                this.Invoke(setpos, new object[] { ipos, vinfo });
            }
            else
            {
                this.label1.Text = ipos.ToString() + "%" + vinfo;
                this.progressBar1.Value = Convert.ToInt32(ipos);
            }
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
                request.UserAgent = "Movepoint";//这里需要fake一下
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
            process();
        }

        private void process()
        {
            string strResult;
            //RefreshPrg(0, "正在获取该领域最新的论文");
            label1.Text = "正在获取该领域最新的论文";
            if (GetWebContent("http://arxiv.org/list/stat.ML/recent", out strResult))
            {
                //this.richTextBox1.Text = strResult;
                //RefreshPrg(10, "正在匹配信息");
                if (label1.InvokeRequired)
                {
                    label1.Invoke(
                            new MethodInvoker(delegate { label1.Text = "正在匹配信息"; }));
                }
                string idexpr = "<a href=\"" + @"/abs/\S*\.\S*" + "\" title";
                idres = MatchExpr(strResult, idexpr);
                string abs = GetAbsUrl(idres[0]);
                string titlexpr = "<span class=\"descriptor\">Title:</span> " + @".*";
                titleres = MatchExpr(strResult, titlexpr);
                string title = GetTitle(titleres[0]);
                for (int i = 0; i < idres.Length; i++)
                {
                    idres[i] = GetAbsUrl(idres[i]);
                    titleres[i] = GetTitle(titleres[i]);
                    richTextBox1.Text += "id: " + idres[i] + ",  Title: " + titleres[i] + ";\n";
                    //RefreshPrg(10 + 90 / idres.Length, "正在下载论文" + titleres[i]);
                    label1.Text = "正在下载论文" + titleres[i];
                    if (!DownloadPaper("http://arxiv.org/pdf/" + idres[i] + ".pdf", titleres[i]))
                    {
                        RefreshPrg(10 + 90/idres.Length, titleres[i] + "下载失败");
                    }
                }
                RefreshPrg(100, "下载完毕");
            }
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
                myWebClient.Headers.Add("User-Agent: Other");
                if (!File.Exists(@"C:\Users\asus\Desktop\test\" + FileName + ".pdf"))
                {
                    myWebClient.DownloadFile(URL, @"C:\Users\asus\Desktop\test\" + FileName + ".pdf");
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
            string ConnString = @"Data Source=182.92.236.197;Initial Catalog=SACS;User ID=sa;pwd=Glass6";
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

            dataGridView1.DataSource = dt.DefaultView;
            dataGridView1.Columns[1].Width = 165;
            dataGridView1.Columns[2].Width = 165;
        }

    }
}
