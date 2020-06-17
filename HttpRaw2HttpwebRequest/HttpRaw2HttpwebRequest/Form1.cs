using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
namespace HttpRaw2HttpwebRequest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            String host = "";
            String[] lines = richTextBox1.Text.Replace("\r", "").Split('\n');
            string[] mime = lines[0].Split(' ');
            string method = mime[0];
            string path = mime[1];
            HttpWebRequestBuilder builder = new HttpWebRequestBuilder(method, path);
            for (int i = 1; i < lines.Length; i++)
            {
                if ((method.ToLower() == "post" ? (i != lines.Length - 1) : true) && !String.IsNullOrWhiteSpace(lines[i]) && (lines[i].Contains(":")))
                {
                    string[] header = lines[i].Split(new char[] { ':' }, 2);
                    switch (header[0])
                    {
                        case "User-Agent":
                            builder.Append($"req.UserAgent = \"{header[1].Remove(0, 1)}\";");
                            break;
                        case "Accept":
                            builder.Append($"req.Accept = \"{header[1].Remove(0, 1)}\";");
                            break;
                        case "Referer":
                            builder.Append($"req.Referer = \"{header[1].Remove(0, 1)}\";");
                            break;
                        case "Connection":
                            if (header[1].Remove(0, 1) == "keep-alive")
                                builder.Append("req.KeepAlive = true;");
                            break;
                        case "Host":
                            host = header[1].Remove(0, 1);
                            builder.Append($"req.Host = \"{header[1].Remove(0, 1)}\";");
                            break;
                        case "Content-Length":
                            break;
                        case "Content-Type":
                            builder.Append($"req.ContentType = \"{header[1].Remove(0, 1)}\";");
                        break;
                        default:
                            builder.Append($"req.Headers.Add(\"{header[0]}\", \"{header[1].Remove(0, 1)}\");");
                            break;
                    }
                }
                else if (!String.IsNullOrWhiteSpace(lines[i]))
                {
                    builder.Append($"Byte[] data = Encoding.UTF8.GetBytes(\"{lines[i]}\");");
                    builder.Append($"req.ContentLength = data.Length;");
                    builder.Append("Stream stream = req.GetRequestStream();");
                    builder.Append("stream.Write(data, 0, data.Length);");
                    builder.Append(" stream.Flush();");
                }
            }
            builder.Append("HttpWebResponse res = (HttpWebResponse)req.GetResponse();");
            builder.Append("String content = new StreamReader(res.GetResponseStream()).ReadToEnd();");
            builder.Append("Console.WriteLine(content);");
            richTextBox1.Text = builder.ToString().Replace("string host = \"\"", $"string host = \"https://{host}\"");
        }
    }
    public class HttpWebRequestBuilder
    {

        private StringBuilder builder { get; set; }
        public HttpWebRequestBuilder(String method, String path)
        {
            this.builder = new StringBuilder();
            this.Append("string host = \"\";");
            this.Append("CookieContainer cookies = new CookieContainer();");
            this.Append("HttpWebRequest req = (HttpWebRequest)WebRequest.Create($\"{host}" + path + "\");");
            this.Append($"req.Method = \"{method}\";");
            this.Append("req.CookieContainer = cookies;");
        }
        public void Append(String data)
        {
            this.builder.Append(data + Environment.NewLine);
        }
        public new String ToString()
        {
            return builder.ToString();
        }
    }
}
