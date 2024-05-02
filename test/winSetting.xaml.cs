using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPET.Evian.AutoWork
{
    /// <summary>
    /// winSetting.xaml 的交互逻辑
    /// </summary>
    public partial class winSetting : Window
    {
        AutoWork vts;


        public winSetting(AutoWork vts)
        {
            InitializeComponent();
            this.vts = vts;
            SwitchOn.IsChecked = vts.Set.Enable;
            WorkSet.Text = vts.Set.WorkSet.ToString();
            Work.IsChecked = vts.Set.Work;
            StudySet.Text = vts.Set.StudySet.ToString();
            Study.IsChecked = vts.Set.Study;
            MoneySet.Text=vts.Set.MoneyMin.ToString();
            Violence.IsChecked = vts.Set.Violence;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            vts.winSetting = null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
            if (vts.Set.Enable != SwitchOn.IsChecked.Value)
            {
                vts.Set.Enable = SwitchOn.IsChecked.Value;
            }
            if (vts.Set.Work != Work.IsChecked.Value)
            {
                vts.Set.Work = Work.IsChecked.Value;
            }
            if (vts.Set.Study != Study.IsChecked.Value)
            {
                vts.Set.Study = Study.IsChecked.Value;
            }
            if (vts.Set.Violence != Violence.IsChecked.Value)
            {
                vts.Set.Violence = Violence.IsChecked.Value;
            }
            vts.Set.WorkSet = Convert.ToDouble(WorkSet.Text);
            vts.Set.StudySet = Convert.ToDouble(StudySet.Text);
            vts.Set.MoneyMin = Convert.ToDouble(MoneySet.Text);
            vts.MW.Set["AutoWork"] = LPSConvert.SerializeObject(vts.Set, "AutoWork");
            if(vts.Set.WorkSet == 0)
            {
                vts.Set.WorkSet = Math.Round(vts.Set.WorkMax - 0.01, 2);
                WorkSet.Text = (vts.Set.WorkMax - 0.01).ToString("0.00");
            }
            if (vts.Set.StudySet == 0)
            {
                vts.Set.StudySet = Math.Round(vts.Set.StudyMax-0.01, 2);
                StudySet.Text = (vts.Set.StudyMax - 0.01).ToString("0.00");
            }
            if (Study.IsChecked.Value && vts.MW.GameSavesData.GameSave.Money <= vts.Set.MoneyMin) 
            {
                Study.IsChecked = false;
                vts.Set.Enable = false;
                SwitchOn.IsChecked = false;
                MessageBoxX.Show("金钱过少，请工作赚钱".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            if (Work.IsChecked.Value==Study.IsChecked.Value)
            {
                if(Study.IsChecked.Value == true)
                {
                    vts.Set.Work = false;
                    vts.Set.Study = false;
                    Work.IsChecked = false;
                    Study.IsChecked = false;
                    vts.Set.Enable = false;
                    SwitchOn.IsChecked = false;
                    MessageBoxX.Show("请不要同时开启两个模式，以免卡死".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return;
                }
                else
                {
                    if(vts.Set.Enable==true||SwitchOn.IsChecked==true)
                    {
                        vts.Set.Enable = false;
                        SwitchOn.IsChecked = false;
                        MessageBoxX.Show("如需开启mod，请选择一个模式".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                        return;
                    }  
                }
            }
            if ((vts.Set.WorkSet <= vts.Set.WorkMax || vts.Set.Violence == true) && vts.Set.Work && vts.Set.Enable == true) ///满足工作条件
            {
                vts.autowork_origin();
            }
            else if((vts.Set.StudySet <= vts.Set.StudyMax || vts.Set.Violence == true) && vts.Set.Study && vts.Set.Enable == true) ///满足学习条件
            {
                vts.autowork_origin();
            }
            else if (vts.Set.Enable == false)
            {
                Close();
            }
            else if (vts.Set.WorkSet > vts.Set.WorkMax && vts.Set.Work)
            {
                vts.Set.Enable = false;
                SwitchOn.IsChecked = false;
                vts.Set.WorkSet = Math.Round(vts.Set.WorkMax, 2);
                WorkSet.Text=vts.Set.WorkSet.ToString("0.00");
                MessageBoxX.Show("无可选择的工作,请重新设置".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            else if (vts.Set.StudySet > vts.Set.StudyMax && vts.Set.Study)
            {
                vts.Set.Enable = false;
                SwitchOn.IsChecked = false;
                vts.Set.StudySet = Math.Round(vts.Set.StudyMax, 2);
                StudySet.Text = vts.Set.StudySet.ToString("0.00");
                MessageBoxX.Show("无可选择的学习,请重新设置".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            else
            {
                MessageBoxX.Show("未知错误".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
            }
            Close();
        }

        private void Open_Saves(object sender, RoutedEventArgs e)
        { 
            if (Directory.Exists(GraphCore.CachePath + @"\Saves"))
            {
                var path = GraphCore.CachePath + $"\\Saves";
                Process.Start("explorer.exe",path);
            }
            else
            {
                MessageBoxX.Show("存储文件夹不存在，请重启桌宠以创建存储文件夹".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
            }
        }
    }
}
