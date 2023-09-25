using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.VisualBasic;

namespace GorbatyukAA_Interview
{
    public partial class Form1 : Form
    {
        private List<Process> processes = null; //список процессов

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            processes = new List<Process>();
            GetProcesses();
            RefresrProcessesList();
        }
        private void GetProcesses() // заполняет список и обновляет его 
        {
            processes.Clear(); // очистим список
            processes = Process.GetProcesses().ToList<Process>(); // заполняем список, получаем все системные процессы и приводим к листу
        }

        private void RefresrProcessesList() //заполняет листвью контентом
        {
            listView1.Items.Clear(); // очищаем лист
            double memSize = 0; // память
            foreach (Process p in processes) // перебираем все процессы
            {
                memSize = 0;

                PerformanceCounter pc = new PerformanceCounter();
                pc.CategoryName = "Process";
                pc.CounterName = "Working Set - Private";
                pc.InstanceName = p.ProcessName;

                memSize = (double)pc.NextValue() / (1000 * 1000); // память в мегабайтах
                
                string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), p.StartTime.ToString()};

                listView1.Items.Add(new ListViewItem(row));

                pc.Close();
                pc.Dispose();
            }

            Text = "Запущено процессов:" + processes.Count.ToString();

        }

        private void RefresrProcessesList(List<Process> processes, string keyword) // для фильтрации
        {
            try
            {
                listView1.Items.Clear(); // очищаем лист
                double memSize = 0; // память
                foreach (Process p in processes) // перебираем все процессы
                {

                    if (p != null)
                    {
                        memSize = 0;

                        PerformanceCounter pc = new PerformanceCounter();
                        pc.CategoryName = "Process";
                        pc.CounterName = "Working Set - Private";
                        pc.InstanceName = p.ProcessName;

                        memSize = (double)pc.NextValue() / (1000 * 1000);
                        DateTime time = new DateTime();
                        time=p.StartTime;
                        string[] row = new string[] { p.ProcessName.ToString(), Math.Round(memSize, 1).ToString(), time.ToString() };

                        listView1.Items.Add(new ListViewItem(row));
                        pc.Close();
                        pc.Dispose();

                    }
                
                }

                Text = $"Запущено процессов '{keyword}'" + processes.Count.ToString();
            }catch (Exception) { }
        }

        // Завершить один процесс
        private void KillProcess (Process process)
        {
            process.Kill();

            process.WaitForExit();//ждать до завершения процесса 
        }

        //Завершение дерева процессов
        private void KillProcessAndChildren(int pid)
        {
            if (pid == 0)
            {
                return;
            }

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID="+ pid);
            ManagementObjectCollection objectCollection = searcher.Get();

            foreach(ManagementObject obj in objectCollection)
            {
                KillProcessAndChildren(Convert.ToInt32(obj["ProcessID"]));
            }
            try
            {
                Process p = Process.GetProcessById(pid);
                p.Kill();
                p.WaitForExit();
            }catch(ArgumentException)
            {

            }
        }

        // получаем ID родительского процесса
        private int GetParentProcessId (Process p)
        {
            int parentID = 0;

            try
            {
                ManagementObject managementObject = new ManagementObject("win32_process.handle='" + p.Id + "'");
                managementObject.Get();
                parentID = Convert.ToInt32(managementObject["ParantProcessId"]);
            }
            catch (Exception) { }
            return parentID;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GetProcesses();
            RefresrProcessesList();

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0]; // сравниваем имя каждого процесса в списке с ячейками в правой колонке. Выбираем первый перзультат
                    KillProcess(processToKill);
                    GetProcesses();
                    RefresrProcessesList();
                }
            }
            catch(Exception) { }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0]; // сравниваем имя каждого процесса в списке с ячейками в правой колонке. Выбираем первый перзультат
                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses(); 
                    RefresrProcessesList(); 
                }
            }
            catch (Exception) { }
        }

        private void завершитьДеревоПроцессовToolStripMenuItem_Click(object sender, EventArgs e)
        {

            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0]; // сравниваем имя каждого процесса в списке с ячейками в правой колонке. Выбираем первый перзультат
                    KillProcessAndChildren(GetParentProcessId(processToKill));
                    GetProcesses();
                    RefresrProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void завершитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (listView1.SelectedItems[0] != null)
                {
                    Process processToKill = processes.Where((x) => x.ProcessName == listView1.SelectedItems[0].SubItems[0].Text).ToList()[0]; // сравниваем имя каждого процесса в списке с ячейками в правой колонке. Выбираем первый перзультат
                    KillProcess(processToKill);
                    GetProcesses();
                    RefresrProcessesList();
                }
            }
            catch (Exception) { }
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e) // фильтр
        {
            GetProcesses();
            
            List<Process> filteredprocesses = processes.Where((x)=>x.ProcessName.ToLower().Contains(toolStripTextBox1.Text.ToLower())).ToList<Process>();

            RefresrProcessesList(filteredprocesses, toolStripTextBox1.Text);
        }
    }
}
