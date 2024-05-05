using LinePutScript;
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
using System.Reflection;
using System.Net.Http.Headers;
using Microsoft.VisualBasic;
using System.Diagnostics;

namespace VPET.Evian.AutoWork
{
    public class AutoWork : MainPlugin
    {
        public Setting Set;

        DateTime StartTime;
        string Mpath = "";
        string Spath = "";
        public override string PluginName => "AutoWork";
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public AutoWork(IMainWindow mainwin) : base(mainwin)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        {
        }
        List<Work> ws= new List<Work>();
        List<Work> ss= new List<Work>();
        List<Work> ps;
        Work nowwork = new Work();
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
            Set.Violence = MW.Set["AutoWork"].GetBool("Violence");
            if (MW.GameSavesData["AutoWork"].GetString("LastDel") != null)
                Set.LastDel = MW.GameSavesData["AutoWork"].GetString("LastDel");
            else
                MW.GameSavesData["AutoWork"][(gstr)"LastDel"] = Set.LastDel;
            if (MW.GameSavesData["AutoWork"].GetString("VioEarn") != null)
            {
                Set.VioEarn = MW.GameSavesData["AutoWork"].GetDouble("VioEarn");
            }
            else
            {
                MW.GameSavesData["AutoWork"][(gstr)"LastDel"] = Set.LastDel;
                MW.GameSavesData["AutoWork"][(gstr)"VioEarn"] = Set.VioEarn.ToString("0.00");
            }
            if (MW.GameSavesData["AutoWork"].GetString("EarnM") != null)
            {
                Set.EarnM = MW.GameSavesData["AutoWork"].GetDouble("EarnM");
            }
            else
            {
                MW.GameSavesData["AutoWork"][(gstr)"EarnM"] = Set.EarnM.ToString("0.00");
            }
            if (MW.GameSavesData["AutoWork"].GetString("EarnE") != null)
            {
                Set.EarnE = MW.GameSavesData["AutoWork"].GetDouble("EarnE");
            }
            else
            {
                MW.GameSavesData["AutoWork"][(gstr)"EarnE"] = Set.EarnE.ToString("0.00");
            }
            Set.Income = MW.GameSavesData.GameSave.Money;
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
            if(LoaddllPath("AutoWork")!= "")
            {
                Mpath = LoaddllPath("AutoWork");
                if (!Directory.Exists(Mpath + @"\Saves"))
                    Directory.CreateDirectory(Mpath + @"\Saves");
                Spath = Mpath + @"\Saves";
            }
            else
            {
                Mpath = GraphCore.CachePath;
                if (!Directory.Exists(GraphCore.CachePath + @"\Saves"))
                    Directory.CreateDirectory(GraphCore.CachePath + @"\Saves");
                Spath = Mpath + @"\Saves";
            }
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
            var value = 1.4;
            var num = 0;
            List<Work> work;
            while (value > 1 && num == 0) ///收支比小于1的亏本，不考虑
            {
                value -= 0.01;
                work = ws.FindAll(x => (x.Get() / x.Spend()) >= value && //正收益
                !x.IsOverLoad() &&//不超模
                MW.GameSavesData.GameSave.Level>= x.LevelLimit
                ); 
                num = work.Count;
            }
            if (value == 1) 
            {
                MessageBoxX.Show("无可选择的工作".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            Set.WorkMax = value;
            if(Set.WorkSet == 0)
            {
                Set.WorkSet = Math.Round(Set.WorkMax - 0.01, 2);
            }
            ///确定学习收支比上限
            value = 1.4;
            num = 0;
            List<Work> study;
            while (value > 1 && num == 0) ///收支比小于1的亏本，不考虑
            {
                value -= 0.01;
                study = ss.FindAll(x => (x.Get() / x.Spend()) >= value && //正收益
                !x.IsOverLoad() &&//不超模
                MW.GameSavesData.GameSave.Level >= x.LevelLimit
                );
                num = study.Count;
            }
            if (value == 1)  
            {
                MessageBoxX.Show("无可选择的学习".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return;
            }
            Set.StudyMax = value;
            if(Set.StudySet == 0)
            {
                Set.StudySet = Math.Round(Set.StudyMax - 0.01, 2);
            }
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
        private async void SWITCH()
        {
            if (Set.Enable == true) 
            {
                if (MW.Main.State == Main.WorkingState.Work)
                    MW.Main.WorkTimer.Stop();
                await Task.Delay(1000);
                Set.Enable = false;
            }
            else
            {
                if(Set.Work==true)
                {
                    Set.Enable=true;
                    autowork_origin();
                }
                else if (Set.Study == true)
                {
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
        private Work FIXOverLoad(Work item)
        {
            if(!item.IsOverLoad()) 
            {
                return item;
            }
            var levellimit = 1.1 * item.LevelLimit + 10;
            if (item.Type == Work.WorkType.Work)
            {
                if (item.MoneyBase > levellimit)
                {
                    item.MoneyBase = levellimit;
                }
            }
            if (item.Type != Work.WorkType.Work)
            {
                if (item.MoneyBase > levellimit * 10) 
                {
                    item.MoneyBase = levellimit * 10;
                }
            }
            while (item.IsOverLoad())
            {
                item.StrengthDrink += 0.1 * item.StrengthDrink;
                item.StrengthFood += 0.1 * item.StrengthFood;
                item.Feeling += 0.1 * item.Feeling;
            }
            while (!item.IsOverLoad())
            {
                item.StrengthFood -= 1;
                item.StrengthDrink -= 1;
                item.Feeling -= 1;
            }
            item.StrengthFood += 1;
            item.StrengthDrink += 1;
            item.Feeling += 1;
            return item;
        }
        private void storage(WorkTimer.FinishWorkInfo obj, int Double)
        {
            var path = Spath + @"\Save.txt";
            var gains = 0.00;
            string WorkType = "";
            var pay = 0.0;
            TimeSpan ts = DateTime.Now - StartTime;
            if (ts.TotalMinutes < 0.2) 
            {
                return;
            }
            if (obj.work.Type == Work.WorkType.Work)
            {
                gains = Set.Income - MW.GameSavesData.GameSave.Money;
                gains = 0 - gains;
                if(Set.Violence == true)
                {
                    Set.VioEarn += Math.Abs(gains);
                    MW.GameSavesData["AutoWork"][(gstr)"VioEarn"] = Set.VioEarn.ToString();
                }
                Set.EarnM += Math.Abs(gains);
                MW.GameSavesData["AutoWork"][(gstr)"EarnM"] = Set.EarnM.ToString();
                MW.GameSavesData.GameSave.Money -= 0.2 * gains;
                gains -= 0.2 * gains;
                WorkType = "工作";
            }
            else if (obj.work.Type == Work.WorkType.Study)
            {
                pay = Set.Income - MW.GameSavesData.GameSave.Money;
                MW.GameSavesData.GameSave.Money -= 0.2 * pay;
                if(Set.Violence == true)
                {
                    Set.VioEarn += Math.Abs(obj.count * 0.05);
                    MW.GameSavesData["AutoWork"][(gstr)"VioEarn"] = Set.VioEarn.ToString();
                }
                Set.EarnE += Math.Abs(obj.count);
                MW.GameSavesData["AutoWork"][(gstr)"EarnE"] = Set.EarnE.ToString();
                pay += 0.2 * pay;
                WorkType = "学习";
            }
            StreamWriter sw;
            if (!File.Exists(path))
                sw = new StreamWriter(path, false, Encoding.Unicode);
            else
                sw = new StreamWriter(path, true, Encoding.Unicode);
            if (obj.work.Type == Work.WorkType.Study)
            {
                sw.WriteLine("");
                sw.WriteLine(WorkType.Translate().ToString() + ":" + "\t" + obj.work.Name.Translate().ToString());
                sw.WriteLine("倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString());
                sw.WriteLine("收益".Translate().ToString() + ": " + Convert.ToInt64(obj.count).ToString() + "Exp");
                sw.WriteLine("花销".Translate().ToString() + ": " + Convert.ToInt64(pay).ToString());
                sw.WriteLine("完成时间".Translate().ToString() + ": " + DateTime.Now.ToString());
                sw.WriteLine("时间花费".Translate().ToString() + ": " + ts.TotalMinutes.ToString("0.00") + "Min");
            }
            else
            {
                sw.WriteLine("");
                sw.WriteLine(WorkType.Translate().ToString() + ":" + "\t" + obj.work.Name.Translate().ToString());
                sw.WriteLine("倍率".Translate().ToString() + ": " + Convert.ToInt32(Double).ToString());
                sw.WriteLine("收益".Translate().ToString() + ": " + Convert.ToInt64(gains).ToString());
                sw.WriteLine("完成时间".Translate().ToString() + ": " + DateTime.Now.ToString());
                sw.WriteLine("时间花费".Translate().ToString() + ": " + ts.TotalMinutes.ToString("0.00") + "Min");
            }
            sw.Close();
            sw = null;
            return;
        }
        private Work violence(bool type)
        {
            if (type)
            {
                var work = ws.FindAll(x => x.Name.ToString() == "autowork".ToString());
                if(work.Count == 0)
                {
                    MessageBoxX.Show("缺失文件，请检查文件完整性".Translate(), "错误".Translate(),
                        MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return null;
                }
                var item = work[0];
                item.MoneyBase = 1.1 * MW.GameSavesData.GameSave.Level + 10;
                item.FinishBonus = 2;
                item.StrengthDrink = MW.GameSavesData.GameSave.Level / 20;
                item.StrengthFood = MW.GameSavesData.GameSave.Level / 20;
                item.Feeling = MW.GameSavesData.GameSave.Level / 20;
                item.LevelLimit = MW.GameSavesData.GameSave.Level;
                item = FIXOverLoad(item);
                return item;
            }
            else
            {
                var work = ss.FindAll(x => x.Name.ToString() == "autostudy".ToString());
                if (work.Count == 0)
                {
                    MessageBoxX.Show("缺失文件，请检查文件完整性".Translate(), "错误".Translate(),
                        MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return null;
                }
                var item = work[0];
                item.MoneyBase = 1.1 * MW.GameSavesData.GameSave.Level+ 10;
                item.MoneyBase *= 10;
                item.FinishBonus = 2;
                item.StrengthDrink = MW.GameSavesData.GameSave.Level / 20;
                item.StrengthFood = item.StrengthDrink;
                item.Feeling = item.StrengthFood;
                item.LevelLimit = MW.GameSavesData.GameSave.Level;
                item = FIXOverLoad(item);
                return item;
            }
        }
        private void get_work(bool type)///type==0找学习，type==1找工作
        {
            StartTime = DateTime.Now;
            Set.Income = MW.GameSavesData.GameSave.Money;
            if (Set.Violence == true)
            {
                if (violence(true) != null)
                {
                    if(Set.Work == true)
                    {
                        var viol = violence(true);
                        nowwork = viol;
                        MW.Main.StartWork(viol);
                        return;
                    }
                    else if (Set.Study == true)
                    {
                        var viol = violence(false);
                        nowwork = viol;
                        MW.Main.StartWork(viol);
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            List<Work> work;
            if (type)
            {
                work = ws.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.WorkSet);
            }
            else
            {
                work = ss.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                    !x.IsOverLoad()); //不超模
                work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.StudySet);
            }
            var item = work[Function.Rnd.Next(work.Count)];
            item=(Work)item.Clone();
            var Double = Math.Min(4000, MW.GameSavesData.GameSave.Level) / (item.LevelLimit + 10)*1.00;
            Double = Double * 0.8;
            if (Double < 1)
            {
                Double = 1;
            }
            item = item.Double(Convert.ToInt32(Double));
            Set.DOUBLE = Convert.ToInt32(Double);
            item = FIXOverLoad(item);
            nowwork = item;
            ///MessageBoxX.Show(Convert.ToInt32(Double).ToString(), "倍率".Translate(), MessageBoxButton.OK, MessageBoxIcon.Info, DefaultButton.YesOK, 5);
            MW.Main.StartWork(item);
        }
        private bool open_condition()
        {
            var o_path = GraphCore.CachePath + @"\Saves";
            var n_path = LoaddllPath("AutoWork") + @"\Saves";
            if (Directory.Exists(o_path) && LoaddllPath("AutoWork") != "")
            {
                if(Directory.Exists(n_path))
                {
                    Directory.Delete(n_path);
                }
                Directory.Move(o_path, n_path);
            }
            if (!Directory.Exists(Spath))
            {
                MessageBoxX.Show("存储文件夹不存在，请重启桌宠以创建存储文件夹".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return false;
            }
            if (MW.GameSavesData.GameSave.Mode == IGameSave.ModeType.PoorCondition)
            {
                MessageBoxX.Show("健康值过低，请补充健康值后再开启".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                Set.Enable = false;
                return false;
            }
            if (Set.Study == true && MW.GameSavesData.GameSave.Money - Set.VioEarn * 0.2-100*(MW.GameSavesData.GameSave.Level-9) <= Set.MoneyMin) 
            {
                Set.Study = false;
                Set.Enable = false;
                MessageBoxX.Show("金钱过少，请工作赚钱".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return false;
            }
            if (MW.GameSavesData.GameSave.Level < 10) 
            {
                MessageBoxX.Show("等级低于10级，不满足开启条件".Translate(), "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                return false;
            }
            if (Set.Work == true)
            {
                List<Work> work = ws.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                !x.IsOverLoad()); //不超模
                work = work.FindAll(x => (x.Get() / x.Spend()) >= Set.WorkSet);
                if (work.Count == 0)
                {
                    Set.Work = false;
                    Set.Enable = false;
                    MessageBoxX.Show("无可选择的工作,请重新设置".Translate(), "错误".Translate(),
                        MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return false;
                }               
            }
            else if (Set.Study == true)
            {
                List<Work> study = ss.FindAll(x => (x.Get() / x.Spend()) >= 1.0 && //正收益
                !x.IsOverLoad()); //不超模
                study = study.FindAll(x => (x.Get() / x.Spend()) >= Set.StudySet);
                if (study.Count == 0)
                {
                    Set.Study = false;
                    Set.Enable = false;
                    MessageBoxX.Show("无可选择的学习,请重新设置".Translate(), "错误".Translate(),
                        MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return false;
                }                
            }
            DateTime ld;
            DateTime ldnew;
            ld = Convert.ToDateTime(Set.LastDel).AddDays(7);
            ldnew = DateTime.Now;
            if (ld >= ldnew.AddDays(14) || ldnew >= ld) 
            {
                if (MW.GameSavesData.GameSave.Money < Set.VioEarn * 0.2 && Set.Violence == true)
                {
                    Set.Enable = false;
                    Set.Violence = false;
                    MessageBoxX.Show("剩余金钱不够购买暴力模式功能月卡".Translate() + "\r\n"
                        + "预计需要".Translate() + (Set.VioEarn * 0.2).ToString().Translate()
                        , "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return false;
                }
                else
                {
                    MW.GameSavesData.GameSave.Money -= Set.VioEarn * 0.2;
                    Set.VioEarn = 0;
                    MW.GameSavesData["AutoWork"][(gstr)"VioEarn"] = Set.VioEarn.ToString();
                }
                if (MW.GameSavesData.GameSave.Money < (MW.GameSavesData.GameSave.Level - 10) * 100)
                {
                    Set.Enable = false;
                    Set.Violence = false;
                    MessageBoxX.Show("剩余金钱不够购买本mod功能月卡".Translate() + "\r\n"
                        + "预计需要".Translate() + ((MW.GameSavesData.GameSave.Level - 10) * 100).ToString().Translate()
                        , "错误".Translate(), MessageBoxButton.OK, MessageBoxIcon.Error, DefaultButton.YesOK, 5);
                    return false;
                }
                else
                {
                    MW.GameSavesData.GameSave.Money -= (MW.GameSavesData.GameSave.Level - 10) * 100;
                    Set.LastDel = DateTime.Now.ToShortDateString();
                    MW.GameSavesData["AutoWork"][(gstr)"LastDel"] = Set.LastDel;
                }
            }
            return true;
        }
        private async void autowork(WorkTimer.FinishWorkInfo obj)
        {
            if (Set.Enable)
            {
                if (!open_condition())
                    return;
                storage(obj, Set.DOUBLE);
            }
                await Task.Delay(5000);
            if (Set.Enable) 
            {
                if (Set.Work == true)
                {
                    get_work(true);
                }
                else if (Set.Study == true) 
                {
                    get_work(false);
                }
                else return; 
            }
        }
        public async void autowork_origin()
        {
            if (MW.Main.State == Main.WorkingState.Work) 
            {
                if(Set.Enable == true)
                    return;
                if (MW.Main.State == Main.WorkingState.Work)
                    MW.Main.WorkTimer.Stop();
                await Task.Delay(5000);
            }
            if(!open_condition()) 
                return;
            if (Set.Work == true)
            {
                get_work(true);
            }
            else if (Set.Study == true)
            {
                get_work(false);
            }
            else return;
        }
        public override void EndGame()
        {
            var path = Spath;
            if (!Directory.Exists(path))
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
                if (File.Exists(path + $"\\Save" + Convert.ToString(Set.SaveNum - 20) + $".txt"))
                    File.Delete(path + $"\\Save" + Convert.ToString(Set.SaveNum - 20) + $".txt");
                Set.SaveNum++;
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
        public string LoaddllPath(string dll)
        {
            Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in loadedAssemblies)
            {
                string assemblyName = assembly.GetName().Name;

                if (assemblyName == dll)
                {
                    string assemblyPath = assembly.Location;

                    string assemblyDirectory = System.IO.Path.GetDirectoryName(assemblyPath);

                    string parentDirectory = Directory.GetParent(assemblyDirectory).FullName;



                    return parentDirectory;
                }
            }
            return "";
        }
    }
}

