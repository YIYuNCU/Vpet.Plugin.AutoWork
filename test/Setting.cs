using LinePutScript;
using LinePutScript.Converter;
using System;

namespace VPET.Evian.AutoWork
{
    public class Setting : Line
    {
        public Setting(ILine line) : base(line)
        {
        }
        public Setting()
        {
        }
        /// <summary>
        /// 最大收支比-工作
        /// </summary>
        public double WorkMax
        {
            get => workmax; set
            {
                workmax = value;
                WorkMaxStr = $"{value:f2}";
            }
        }
        private double workmax = 1.0;
        public string WorkMaxStr { get; private set; } = "1.0";
        /// <summary>
        /// 最大收支比-学习
        /// </summary>
        public double StudyMax
        {
            get => studymax; set
            {
                studymax = value;
                StudyMaxStr = $"{value:f2}";
            }
        }
        private double studymax = 1.0;
        public string StudyMaxStr { get; private set; } = "1.0";
        /// <summary>
        /// 收益
        /// </summary>
        public double Income
        {
            get => income; set
            {
                income = value;
                IncomeStr = $"{value:f2}";
            }
        }
        private double income = 1.0;
        public string IncomeStr { get; private set; } = "1.0";
        /// <summary>
        /// 倍率
        /// </summary>
        public int DOUBLE
        {
            get => doublee; set
            {
                doublee = value;
                DOUBLEStr = $"{value:f2}";
            }
        }
        private int doublee = 1;
        public string DOUBLEStr { get; private set; } = "1.0";
        /// <summary>
        /// 最小收支比-工作
        /// </summary>
        [Line]
        public double WorkSet
        {
            get => workset; set
            {
                workset = value;
                WorkSetStr = $"{value:f2}";
            }
        }
        private double workset = 1;
        public string WorkSetStr { get; private set; } = "1.0";
        /// <summary>
        /// 最小收支比-学习
        /// </summary>
        [Line]
        public double StudySet
        {
            get => studyset; set
            {
                studyset = value;
                StudySetStr = $"{value:f2}";
            }
        }
        private double studyset = 1;
        public string StudySetStr { get; private set; } = "1.0";
        /// <summary>
        /// 开启学习最小金钱
        /// </summary>
        [Line]
        public double MoneyMin
        {
            get => moneymin; set
            {
                moneymin = value;
                MoneyMinStr = $"{value:f2}";
            }
        }
        private double moneymin = 100;
        public string MoneyMinStr { get; private set; } = "100";
        /// <summary>
        /// 最小收益
        /// </summary>
        [Line]
        public double MinDeposit
        {
            get => mindeposit; set
            {
                mindeposit = value;
                MinDepositStr = $"{value:f2}";
            }
        }
        private double mindeposit = 100;
        public string MinDepositStr { get; private set; } = "100";
        /// <summary>
        /// 存档数
        /// </summary>
        [Line]
        public int SaveNum
        {
            get => savenum; set
            {
                savenum = value;
                SaveNumStr = $"{value}";
            }
        }
        private int savenum = 0;
        public string SaveNumStr { get; private set; } = "0";
        /// <summary>
        /// 启用AutoWork
        /// </summary>
        [Line]
        public bool Enable { get; set; } = true;
        /// <summary>
        /// 模式-工作
        /// </summary>
        [Line]
        public bool Work { get; set; } = true;
        /// <summary>
        /// 模式-学习
        /// </summary>
        [Line]
        public bool Study { get; set; } = true;
    }
}
