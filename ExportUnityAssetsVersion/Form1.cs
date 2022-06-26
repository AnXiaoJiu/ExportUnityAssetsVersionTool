using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace ExportUnityAssetsVersion
{
    public partial class Form1 : Form
    {
        [DataContract]
        public class Configinfo
        {
            [DataMember]
            public float vid;
            [DataMember]
            public Info[] infos;
            [DataMember]
            public string vMD5;
        }
         [DataContract]
        public class Info
        {
            [DataMember]
            public string id;
             [DataMember]
            public string path;
             [DataMember]
            public string version;
            [DataMember]
            public string md5;
        }


        private string root = string.Empty;
        private Queue<string> pathQueue;
        private Configinfo configinfo;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //1.弹出选择目录对话框 获取根路径
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.ShowDialog();
            root = path.SelectedPath;
            if (string.Equals(root, string.Empty)) return;
            this.outpath.Text = "正在导出...";

            //2. 遍历文件夹下的所有文件
            DirectoryInfo rootFolder = new DirectoryInfo(root);
            Scan(rootFolder);

            //3.配置信息
            if (configinfo == null)
            {
                configinfo = new Configinfo();
                configinfo.vid = 1.0f;
                configinfo.vMD5 = string.Empty;
                if(configinfo.infos == null)
                {
                    configinfo.infos = new Info[pathQueue.Count];
                    for (int i = 0; i < configinfo.infos.Length; i++)
                    {
                        string p = pathQueue.Dequeue();
                        Info info = new Info();
                        info.id = "000" + (i+1).ToString();
                        info.path = p;
                        info.version = "1.0";
                        string pp = Path.Combine(rootFolder.FullName, p);
                        info.md5 = Md5File(pp);
                        configinfo.infos[i] = info;
                    }
                }
            }
            //4.序列化
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Configinfo));
            MemoryStream msObj = new MemoryStream();
            //将序列化之后的Json格式数据写入流中
            js.WriteObject(msObj, configinfo);
            msObj.Position = 0;
            //从0这个位置开始读取流中的数据
            StreamReader sr = new StreamReader(msObj, Encoding.UTF8);
            string json = sr.ReadToEnd();
            sr.Close();
            msObj.Close();
            string end = json.Replace(@"\", "");
            Console.WriteLine(end);
            //5.保存到文件
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string fileName = sfd.FileName;
                File.WriteAllText(fileName, end);
                this.outpath.Text = "导出路径:" + fileName; string folder = Path.GetDirectoryName(fileName);
                string versionFileMD5 = Md5File(fileName);//计算版本文件的md5
                configinfo.vMD5 = versionFileMD5;
                //6.再次序列化写入文件
                js = new DataContractJsonSerializer(typeof(Configinfo));
                msObj = new MemoryStream();
                js.WriteObject(msObj, configinfo);
                msObj.Position = 0;
                sr = new StreamReader(msObj, Encoding.UTF8);
                json = sr.ReadToEnd();
                sr.Close();
                msObj.Close();
                end = json.Replace(@"\", "");
                //Console.WriteLine(end);
                File.WriteAllText(fileName, end);
                if (MessageBox.Show("导出成功", "Tool", MessageBoxButtons.OK) == DialogResult.OK)
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                    Close();
                }
               
            }

        }



        private void Scan(FileSystemInfo info)
        {
            if (!info.Exists) return;
            DirectoryInfo dir = info as DirectoryInfo;
            //不是目录  
            if (dir == null) return;
            FileSystemInfo[] files = dir.GetFileSystemInfos();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i] as FileInfo;
                //是文件  
                if (file != null)
                {
                    int start = root.Length;
                    int total = file.FullName.Length;
                    string relapath = file.FullName.Substring(start + 1, total - (start+1)).Replace(@"\", @"/");
                   // Console.WriteLine(relapath);
                    if(pathQueue == null)
                    {
                        pathQueue = new Queue<string>();
                    }
                    pathQueue.Enqueue(relapath);
                }
                else
                {
                    Scan(files[i]);
                }
            }
        }


        private string Md5File(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                MD5 md5 = MD5.Create();
                byte[] data = md5.ComputeHash(stream);
                var sb = new StringBuilder();

                foreach (var b in data)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString().ToLower();
            }
        }

        private void Serialize()
        {
           
        }

    }


   

}
