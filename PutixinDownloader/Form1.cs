using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;

namespace PutixinDownloader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();
            this.button1.Enabled = false;

            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                HttpResponseMessage response = await client.GetAsync(this.textBox1.Text);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsByteArrayAsync();

                //get string from gbk
                string str = Encoding.GetEncoding("gbk").GetString(responseBody);

                //convert string to utf-8
                byte[] array3 = Encoding.UTF8.GetBytes(str);
                var gbkstr = Encoding.UTF8.GetString(array3);

                parseMunu(gbkstr);
            }
            catch (Exception err)
            {
                //Console.WriteLine("\nException Caught!");
                //Console.WriteLine("Message :{0} ", e.Message);
                MessageBox.Show(err.Message);
            }


        }

        private async void parseMunu(string response) {
            Uri baseAddress = new Uri(this.textBox1.Text);
            Uri directory = new Uri(baseAddress, "."); // "." == current dir, like MS-DOS
            Console.WriteLine(directory.OriginalString);

            System.IO.DirectoryInfo di = new DirectoryInfo(".\\下载");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(response);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//table//table");

            Regex rx = new Regex(@">1</a>|>2</a>|>3</a>");

            foreach (var node in nodes)
            {
                MatchCollection matches = rx.Matches(node.InnerHtml);
                if (matches.Count == 3)
                {
                    var aNodes = node.SelectNodes("//a");
                    this.progressBar1.Maximum = aNodes.Count;
                    foreach (var aNode in aNodes)
                    {
                        Regex numberReg = new Regex(@"\d");
                        if (numberReg.IsMatch(aNode.InnerText))
                        {
                            var href = aNode.GetAttributeValue("href", "");
                            await downloadPage(directory + href, href);
                            this.progressBar1.Value += 1;
                        }
                    }
                    this.button1.Enabled = true;
                    MessageBox.Show("下载完成");
                    this.progressBar1.Value = 0;
                    break;
                }
            }


        }

        private async Task<bool> downloadPage(string url, string filename) {
            HttpClient client = new HttpClient();

            // Call asynchronous network methods in a try/catch block to handle exceptions
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsByteArrayAsync();


                DirectoryInfo di = Directory.CreateDirectory(".\\下载");
                string str = Encoding.GetEncoding("gb2312").GetString(responseBody);
                System.IO.File.WriteAllText($".\\下载\\{filename}", str, Encoding.GetEncoding("gb2312"));
                return true;
            }
            catch (Exception e)
            {
                //Console.WriteLine("\nException Caught!");
                //Console.WriteLine("Message :{0} ", e.Message);
                MessageBox.Show($"下载页面{url}错误");
                return false;
            }

        }
    }
}
