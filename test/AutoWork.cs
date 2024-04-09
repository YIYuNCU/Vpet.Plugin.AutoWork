﻿using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System.Windows;
using System.Windows.Controls;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using static VPet_Simulator.Core.GraphHelper;
using System.Timers;
using LinePutScript.Dictionary;
using System.IO;
using System.Text;
using System.Windows.Shapes;
using System;
using System.Runtime.InteropServices;

namespace VPET.Evian.AutoWork
{
    public class AutoWork : MainPlugin
    {
        public Setting Set;

        GameSave_v2 GameSave;

        public override string PluginName => "AutoWork";
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public AutoWork(IMainWindow mainwin) : base(mainwin)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
        }
        List<Work> ws= new List<Work>();
        List<Work> ss= new List<Work>();
        List<Work> ps;
        public override void LoadPlugin()
        {
            ///从Setting.lps中读取存储的设置
            Set = new Setting(MW.Set["AutoWork"]);
            Set.Enable = false;
            Set.WorkSet = MW.Set["AutoWork"].GetDouble("WorkSet");
            Set.Work = MW.Set["AutoWork"].GetBool("Work");
            Set.StudySet = MW.Set["AutoWork"].GetDouble("StudySet");
            Set.Study = MW.Set["AutoWork"].GetBool("Study");
            Set.MoneyMin = MW.Set["AutoWork"].GetDouble("MoneyMin");
            Set.MoneyMin = MW.Set["AutoWork"].GetDouble("MoneyMin");
            Set.SaveNum = MW.Set["AutoWork"].GetInt("SaveNum");
            ///Set.MinDeposit = MW.Set["AutoWork"].GetDouble("MinDeposit");
            ///添加列表项
            MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
            modset.Visibility = Visibility.Visible;
            var menuItem = new MenuItem()
            {
                Header = "AutoWork",
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuItem.Click += (s, e) => { Setting(); };
            modset.Items.Add(menuItem);
            ///添加存储区
            if (!Directory.Exists(GraphCore.CachePath + @"\Saves"))
                Directory.CreateDirectory(GraphCore.CachePath + @"\Saves");
            ///获取工作
            MW.Main.WorkList(out ws, out ss, out ps);
            ///确定是否存在工作
            if (ws.Count == 0)
            {
                MessageBoxX.Show("无可选择的工作".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            ///确定是否存在学习
            if (ss.Count == 0)
            {
                MessageBoxX.Show("无可选择的学习".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            ///确定工作收支比上限
            var value = 1.25;
            var num = 0;
            List<Work> work;
            while (value > 1 && num == 0) ///收支比小于1的亏本，不考虑
            {
                value -= 0.01;
                work = ws.FindAll(x => (x.Get() / x.Spend()) >= value && //正收益
                !x.IsOverLoad()); //不超模
                num = work.Count;
            }
            if (value == 1) 
            {
                MessageBoxX.Show("无可选择的工作".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            Set.WorkMax = value;
            ///确定学习收支比上限
            value = 1.25;
            num = 0;
            List<Work> study;
            while (value > 1 && num == 0) ///收支比小于1的亏本，不考虑
            {
                value -= 0.01;
                study = ss.FindAll(x => (x.Get() / x.Spend()) >= value && //正收益
                !x.IsOverLoad()); //不超模
                num = study.Count;
            }
            if (value == 1)  
            {
                MessageBoxX.Show("无可选择的学习".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            Set.StudyMax = value;
            ///将自动购买功能挂在FinishWorkHandle上
            MW.Main.WorkTimer.E_FinishWork += autowork;
            ///保存设置
            MW.Set["AutoWork"] = LPSConvert.SerializeObject(Set, "AutoWork");
        }
        ///添加自定
        public override void LoadDIY()
        {
            MW.Main.ToolBar.AddMenuButton(VPet_Simulator.Core.ToolBar.MenuType.DIY, "AutoWork", SWITCH);
        }
            ///base.LoadPlugin();
        
        public winSetting winSetting;
        private void SWITCH()
        {
            if (Set.Enable == true) 
            {
                Set.Enable = false;
                MW.Main.WorkTimer.Stop();
            }
            else
            {
                if(Set.Work==true)
                {
                    List<Work> work = ws.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                    work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.WorkSet);
                    if (work.Count==0)
                    {
                        Set.Work = false;
                        MessageBoxX.Show("无可选择的工作,请重新设置".Translate(), "错误".Translate(), 
                            MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                        return;
                    }
                    Set.Enable=true;
                    autowork_origin();
                }
                else if (Set.Study == true)
                {
                    List<Work> study = ss.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                    study = study.FindAll(x => (x.Get() / x.Spend()) >= Set.StudySet);
                    if (study.Count==0)
                    {
                        Set.Study = false;
                        MessageBoxX.Show("无可选择的学习,请重新设置".Translate(), "错误".Translate(),
                            MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                        return;
                    }
                    Set.Enable = true;
                    autowork_origin();
                }
                else
                {
                    MessageBoxX.Show("如需开启mod，请选择一个模式".Translate(), "错误".Translate(),
                        MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return;
                }
            }
        }
        public override void Setting()
        {
            if (winSetting == null)
            {
                winSetting = new winSetting(this);
                winSetting.Show();
            }
            else
            {
                winSetting.Topmost = true;
            }
        }
        private async void autowork(WorkTimer.FinishWorkInfo obj)
        {
            var path = GraphCore.CachePath + $"\\Saves\\Save.txt";
             if (Set.Enable) 
            {
                if (!Directory.Exists(GraphCore.CachePath + @"\Saves"))
                {
                MessageBoxX.Show("存储文件夹不存在，请重启桌宠以创建存储文件夹".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
                }

                if (MW.GameSavesData.GameSave.Mode == IGameSave.ModeType.PoorCondition)
                {
                    MessageBoxX.Show("健康值过低，请补充健康值后再开启".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    Set.Enable = false;
                    return;
                }
                if (Set.Study == true && MW.GameSavesData.GameSave.Money <= Set.MoneyMin)
                {
                    Set.Study = false;
                    Set.Enable = false;
                    MessageBoxX.Show("金钱过少，请工作赚钱".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                }
                await Task.Delay(5000);
                if (Set.Work == true)
                {
                    List<Work> work = ws.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                    work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.WorkSet);
                    var item = work[Function.Rnd.Next(work.Count)];
                    var Double = Math.Min(4000, MW.GameSavesData.GameSave.Level) / (item.LevelLimit + 10); 
                    item.Double(Convert.ToInt32(Double));
                    MessageBoxX.Show(Convert.ToInt32(Double).ToString(), "倍率".Translate(), MessageBoxButton.OK, MessageBoxIcon.Info, DefaultButton.YesOK, 5);
                    if (!File.Exists(path))
                    {
                        StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode);
                        sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                        sw.Close();
                        sw = null;
                    }
                    else
                    {
                        StreamWriter sw = new StreamWriter(path, true, Encoding.Unicode);
                        sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                        sw.Close();
                        sw = null;
                    }
                    MW.Main.StartWork(item);
                }
                else if (Set.Study == true) 
                {
                    List<Work> study = ss.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                    study = study.FindAll(x => (x.Get() / x.Spend()) >= Set.StudySet);
                    var item = study[Function.Rnd.Next(study.Count)];
                    var Double = Math.Min(4000, MW.GameSavesData.GameSave.Level) / (item.LevelLimit + 10);
                    item.Double(Convert.ToInt32(Double));
                    MessageBoxX.Show(Convert.ToInt32(Double).ToString(), "倍率".Translate(), MessageBoxButton.OK, MessageBoxIcon.Info, DefaultButton.YesOK, 5);
                    if (!File.Exists(path))
                    {
                        StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode);
                        sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                        sw.Close();
                        sw = null;
                    }
                    else
                    {
                        StreamWriter sw = new StreamWriter(path, true, Encoding.Unicode);
                        sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                        sw.Close();
                        sw = null;
                    }
                    MW.Main.StartWork(item);
                }
                else return; 
            }
        }
        public void autowork_origin()
        {
            var path = GraphCore.CachePath + $"\\Saves\\Save.txt";
            if (!Directory.Exists(GraphCore.CachePath + @"\Saves"))
            {
                MessageBoxX.Show("存储文件夹不存在，请重启桌宠以创建存储文件夹".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            if (MW.GameSavesData.GameSave.Mode == IGameSave.ModeType.PoorCondition)
            {
                MessageBoxX.Show("健康值过低，请补充健康值后再开启".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                Set.Enable = false;
                return;
            }
            if (Set.Work == true)
            {
                List<Work> work = ws.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                !x.IsOverLoad()); //不超模
                work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.WorkSet);
                var item = work[Function.Rnd.Next(work.Count)];
                var Double = Math.Min(4000, MW.GameSavesData.GameSave.Level) / (item.LevelLimit + 10);
                item.Double(Convert.ToInt32(Double));
                MessageBoxX.Show(Convert.ToInt32(Double).ToString(), "倍率".Translate(), MessageBoxButton.OK, MessageBoxIcon.Info, DefaultButton.YesOK, 5);
                if (!File.Exists(path))
                {
                    StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode);
                    sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                    sw.Close();
                    sw = null;
                }
                else
                {
                    StreamWriter sw = new StreamWriter(path, true, Encoding.Unicode);
                    sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                    sw.Close();
                    sw = null;
                }
                MW.Main.StartWork(item);
            }
            else if (Set.Study == true)
            {
                List<Work> study = ss.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                !x.IsOverLoad()); //不超模
                study = study.FindAll(x => (x.Get() / x.Spend()) >= Set.StudySet);
                var item = study[Function.Rnd.Next(study.Count)];
                var Double = Math.Min(4000, MW.GameSavesData.GameSave.Level) / (item.LevelLimit + 10);
                item.Double(Convert.ToInt32(Double));
                MessageBoxX.Show(Convert.ToInt32(Double).ToString(), "倍率".Translate(), MessageBoxButton.OK, MessageBoxIcon.Info, DefaultButton.YesOK, 5);
                if (!File.Exists(path))
                {
                    StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode);
                    sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                    sw.Close();
                    sw = null;
                }
                else
                {
                    StreamWriter sw = new StreamWriter(path, true, Encoding.Unicode);
                    sw.WriteLine(item.Name.Translate().ToString() + " " + "倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString() + " " + DateTime.Now.ToString());
                    sw.Close();
                    sw = null; 
                }
                MW.Main.StartWork(item);
            }
            else return;
        }
        public override void EndGame()
        {
            var path = GraphCore.CachePath + @"\Saves";
            if (!Directory.Exists(GraphCore.CachePath + @"\Saves"))
            {
                base.EndGame();
                return;
            }
            if (!File.Exists(path + $"\\Save.txt"))
            {
                base.EndGame();
                return;
            }
            if (Set.SaveNum >= 20) 
            {
                File.Delete(path + $"\\Save" + Convert.ToString(Set.SaveNum - 20) + $".txt");
            }
            else if(Set.SaveNum >= 60000)
            {
                Set.SaveNum = 0;
            }
            else
            {
                Set.SaveNum++;
            }
            MW.Set["AutoWork"] = LPSConvert.SerializeObject(Set, "AutoWork");
            if(File.Exists(path + $"\\Save" + Set.SaveNum.ToString() + $".txt"))
            {
                File.Delete(path + $"\\Save" + Set.SaveNum.ToString() + $".txt");
            }
            File.Copy(path + $"\\Save.txt",path+$"\\Save"+Set.SaveNum.ToString()+$".txt");
            File.Delete(path + $"\\Save.txt");
            base.Save();
            base.EndGame();
        }
    }
}
