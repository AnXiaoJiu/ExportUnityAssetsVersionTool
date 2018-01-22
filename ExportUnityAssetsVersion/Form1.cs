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
            this.outpath.Text = "正在导出...";

            //2. 遍历文件夹下的所有文件
            DirectoryInfo rootFolder = new DirectoryInfo(root);
            Scan(rootFolder);

            //3.配置信息
            if (configinfo == null)
            {
                configinfo = new Configinfo();
                configinfo.vid = 1.0f;
                if(configinfo.infos == null)
                {
                    configinfo.infos = new Info[pathQueue.Count];
                    for (int i = 0; i < configinfo.infos.Length; i++)
                    {
                        configinfo.infos[i] = new Info();
                        configinfo.infos[i].id = "000" + (i+1).ToString();
                        configinfo.infos[i].path = pathQueue.Dequeue();
                        configinfo.infos[i].version = "1.0";
                    }
                }
            }
            //4.反序列化
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

                this.outpath.Text = "导出路径:" + fileName;
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

    }


}
