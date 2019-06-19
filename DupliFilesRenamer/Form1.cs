using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DulpliFilesRenamer
{
    public partial class Form1 : Form
    {
        public static Hashtable config = new Hashtable();
        public static Hashtable def = new Hashtable();
        public static bool autoplay = false;
        public Form1()
        {
            InitializeComponent();
            Form1.CheckForIllegalCrossThreadCalls = false;

            if (DLL.main.FileSearch("./database") == false)
            {
                DLL.main.ForderCreate("./database");
            }

            nao0x0.flag.debug = true;

            def["replace_after_filename"] = textBox4_replace_after_filename.Text;
            def["date_format"] = textBox1_date_format.Text;

            DLL.main.appcheck(DLL.main.myappname());
            DLL.main.FormStart("DupliFilesRenamer", DLL.main.myappname());
            //DLL.form.Icon("Fuji", Properties.Resources.Icon, this);


            string json = nao0x0.JSON.Load();
            if (json != string.Empty)
            {
                config = nao0x0.JSON.ToHashtable(json);

                if (config["folder_path"] != null)
                textBox3_folderpath.Text = config["folder_path"].ToString();

                if(config["replace_files"] != null)
                textBox2_replacefiles.Text = config["replace_files"].ToString();

                if (config["replace_after_filename"] != null)
                textBox4_replace_after_filename.Text = config["replace_after_filename"].ToString();

                if (config["ext"] != null)
                textBox1_ext.Text = config["ext"].ToString();

                if (config["date_format"] != null)
                    textBox1_date_format.Text = config["date_format"].ToString();

            }


            string[] cmds = System.Environment.GetCommandLineArgs();
            try
            {
                if (cmds[1] == "/autoplay")
                {
                    autoplay = true;
                }

            }
            catch (Exception e) { }
        }

        public void Save()
        {
            string folder_path = textBox3_folderpath.Text;
            string replace_files = textBox2_replacefiles.Text;
            string replace_after_filename = textBox4_replace_after_filename.Text;
            string ext = textBox1_ext.Text;
            string date_format = textBox1_date_format.Text;

            string[] replaces_files = replace_files.Split('|');


            Hashtable setconfig = new Hashtable();

            setconfig["folder_path"] = folder_path;

            setconfig["replace_files"] = replace_files;

            if (replace_after_filename != (string)def["replace_after_filename"])
                setconfig["replace_after_filename"] = replace_after_filename;

            setconfig["ext"] = ext;

            if (date_format != (string)def["date_format"])
                setconfig["date_format"] = date_format;

            nao0x0.JSON.Save(nao0x0.JSON.ToJSON(setconfig));
        }

        public string set_replace_after_filename(string replace_after_filename,string date,string ext)
        {
            string result = replace_after_filename;
            result = DLL.main.StringReplace(result, "{$date}", date);
            result = DLL.main.StringReplace(result, "{$ext}", ext);
            nao0x0.PC.DebugLog("set_replace_after_filename:"+result);
            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string folder_path = textBox3_folderpath.Text;
            string replace_files = textBox2_replacefiles.Text;
            string replace_after_filename = textBox4_replace_after_filename.Text;
            string ext = textBox1_ext.Text;
            string date_format = textBox1_date_format.Text;
            int total_replace_files = 0;

            string[] replaces_files = replace_files.Split('|');

            foreach(string get_replace_files in replaces_files)
            {
                string[] files = nao0x0.IO.GetFolderSubFiles(folder_path, get_replace_files + ext);
                foreach (string getfile in files)
                {
                    FileInfo fi = new FileInfo(getfile);
                    DateTime file_date = fi.LastWriteTime;
                    string date = file_date.ToString(date_format);
                    string ToFileName = DLL.main.StringRegexSearch(fi.Name, get_replace_files) + set_replace_after_filename(replace_after_filename, date, ext);
                    nao0x0.PC.DebugLog("ToFileName:" + ToFileName);
                    string ToReplacePath = folder_path + @"\" + ToFileName;
                    

                    if (DLL.main.FileSearch(ToReplacePath) == true)
                    {
                        string SourceFilePath = ToReplacePath;
                        nao0x0.PC.DebugLog("同名のファイルが存在するため、連番を付与します");
                        for (int i = 1; i <= 100; i++)
                        {
                            ToReplacePath = folder_path + @"\" + DLL.main.StringRegexSearch(fi.Name, get_replace_files) + set_replace_after_filename(replace_after_filename, date,"_" + i + ext);
                            if (DLL.main.FileSearch(ToReplacePath) == false)
                            {
                                nao0x0.PC.DebugLog("moving:" + getfile + " => " + ToReplacePath);
                                System.IO.File.Move(getfile, ToReplacePath);
                                break;
                            }
                            else if (i == 99)
                            {
                                nao0x0.PC.DebugLog("連番が上限の99個を超えています。このファイルは置換をスキップします。FileName:" + SourceFilePath);
                                break;
                            }
                        }
                    }
                    else
                    {
                        nao0x0.PC.DebugLog("moving:" + getfile + " => " + ToReplacePath);
                        System.IO.File.Move(getfile, ToReplacePath);
                    }
                }
                total_replace_files += files.Length;

            }

            Save();

            if (autoplay == false)
            {
                MessageBox.Show(total_replace_files.ToString() + "個のファイルを置き換えました。");
            }
            else
            {
                this.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(textBox3_folderpath.Text);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(autoplay == true)
            {
                button1.PerformClick();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save();
        }
    }
}
