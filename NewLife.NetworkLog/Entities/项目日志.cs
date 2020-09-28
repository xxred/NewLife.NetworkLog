using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using XCode;
using XCode.Configuration;
using XCode.DataAccessLayer;

namespace NewLife.NetworkLog.Entities
{
    /// <summary>项目日志</summary>
    [Serializable]
    [DataObject]
    [Description("项目日志")]
    [BindIndex("IX_ProjectLog_ProjectID", false, "ProjectID")]
    [BindIndex("IX_ProjectLog_Time", false, "Time")]
    [BindIndex("IX_ProjectLog_ThreadName", false, "ThreadName")]
    [BindTable("ProjectLog", Description = "项目日志", ConnName = "NewLife.NetworkLog", DbType = DatabaseType.None)]
    public partial class ProjectLog<TEntity> : IProjectLog
    {
        #region 属性
        private Int32 _ID;
        /// <summary>编号</summary>
        [DisplayName("编号")]
        [Description("编号")]
        [DataObjectField(true, true, false, 0)]
        [BindColumn("ID", "编号", "")]
        public Int32 ID { get => _ID; set { if (OnPropertyChanging("ID", value)) { _ID = value; OnPropertyChanged("ID"); } } }

        private Int32 _ProjectID;
        /// <summary>项目ID</summary>
        [DisplayName("项目ID")]
        [Description("项目ID")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ProjectID", "项目ID", "", Master = true)]
        public Int32 ProjectID { get => _ProjectID; set { if (OnPropertyChanging("ProjectID", value)) { _ProjectID = value; OnPropertyChanged("ProjectID"); } } }

        private Int64 _Time;
        /// <summary>时间戳(日志信息不包含时间，则插入当前时间)</summary>
        [DisplayName("时间戳")]
        [Description("时间戳(日志信息不包含时间，则插入当前时间)")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("Time", "时间戳(日志信息不包含时间，则插入当前时间)", "")]
        public Int64 Time { get => _Time; set { if (OnPropertyChanging("Time", value)) { _Time = value; OnPropertyChanged("Time"); } } }

        private Int32 _ThreadID;
        /// <summary>线程ID</summary>
        [DisplayName("线程ID")]
        [Description("线程ID")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ThreadID", "线程ID", "")]
        public Int32 ThreadID { get => _ThreadID; set { if (OnPropertyChanging("ThreadID", value)) { _ThreadID = value; OnPropertyChanged("ThreadID"); } } }

        private ThreadType _ThreadType;
        /// <summary>线程类型</summary>
        [DisplayName("线程类型")]
        [Description("线程类型")]
        [DataObjectField(false, false, false, 0)]
        [BindColumn("ThreadType", "线程类型", "")]
        public ThreadType ThreadType { get => _ThreadType; set { if (OnPropertyChanging("ThreadType", value)) { _ThreadType = value; OnPropertyChanged("ThreadType"); } } }

        private String _ThreadName;
        /// <summary>线程名</summary>
        [DisplayName("线程名")]
        [Description("线程名")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("ThreadName", "线程名", "")]
        public String ThreadName { get => _ThreadName; set { if (OnPropertyChanging("ThreadName", value)) { _ThreadName = value; OnPropertyChanged("ThreadName"); } } }

        private String _Message;
        /// <summary>内容</summary>
        [DisplayName("内容")]
        [Description("内容")]
        [DataObjectField(false, false, true, 50)]
        [BindColumn("Message", "内容", "")]
        public String Message { get => _Message; set { if (OnPropertyChanging("Message", value)) { _Message = value; OnPropertyChanged("Message"); } } }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        public override Object this[String name]
        {
            get
            {
                switch (name)
                {
                    case "ID": return _ID;
                    case "ProjectID": return _ProjectID;
                    case "Time": return _Time;
                    case "ThreadID": return _ThreadID;
                    case "ThreadType": return _ThreadType;
                    case "ThreadName": return _ThreadName;
                    case "Message": return _Message;
                    default: return base[name];
                }
            }
            set
            {
                switch (name)
                {
                    case "ID": _ID = value.ToInt(); break;
                    case "ProjectID": _ProjectID = value.ToInt(); break;
                    case "Time": _Time = value.ToLong(); break;
                    case "ThreadID": _ThreadID = value.ToInt(); break;
                    case "ThreadType": _ThreadType = (ThreadType)value; break;
                    case "ThreadName": _ThreadName = Convert.ToString(value); break;
                    case "Message": _Message = Convert.ToString(value); break;
                    default: base[name] = value; break;
                }
            }
        }
        #endregion

        #region 字段名
        /// <summary>取得项目日志字段信息的快捷方式</summary>
        public partial class _
        {
            /// <summary>编号</summary>
            public static readonly Field ID = FindByName("ID");

            /// <summary>项目ID</summary>
            public static readonly Field ProjectID = FindByName("ProjectID");

            /// <summary>时间戳(日志信息不包含时间，则插入当前时间)</summary>
            public static readonly Field Time = FindByName("Time");

            /// <summary>线程ID</summary>
            public static readonly Field ThreadID = FindByName("ThreadID");

            /// <summary>线程类型</summary>
            public static readonly Field ThreadType = FindByName("ThreadType");

            /// <summary>线程名</summary>
            public static readonly Field ThreadName = FindByName("ThreadName");

            /// <summary>内容</summary>
            public static readonly Field Message = FindByName("Message");

            static Field FindByName(String name) => Meta.Table.FindByName(name);
        }

        /// <summary>取得项目日志字段名称的快捷方式</summary>
        public partial class __
        {
            /// <summary>编号</summary>
            public const String ID = "ID";

            /// <summary>项目ID</summary>
            public const String ProjectID = "ProjectID";

            /// <summary>时间戳(日志信息不包含时间，则插入当前时间)</summary>
            public const String Time = "Time";

            /// <summary>线程ID</summary>
            public const String ThreadID = "ThreadID";

            /// <summary>线程类型</summary>
            public const String ThreadType = "ThreadType";

            /// <summary>线程名</summary>
            public const String ThreadName = "ThreadName";

            /// <summary>内容</summary>
            public const String Message = "Message";
        }
        #endregion
    }

    /// <summary>项目日志接口</summary>
    public partial interface IProjectLog
    {
        #region 属性
        /// <summary>编号</summary>
        Int32 ID { get; set; }

        /// <summary>项目ID</summary>
        Int32 ProjectID { get; set; }

        /// <summary>时间戳(日志信息不包含时间，则插入当前时间)</summary>
        Int64 Time { get; set; }

        /// <summary>线程ID</summary>
        Int32 ThreadID { get; set; }

        /// <summary>线程类型</summary>
        ThreadType ThreadType { get; set; }

        /// <summary>线程名</summary>
        String ThreadName { get; set; }

        /// <summary>内容</summary>
        String Message { get; set; }
        #endregion

        #region 获取/设置 字段值
        /// <summary>获取/设置 字段值</summary>
        /// <param name="name">字段名</param>
        /// <returns></returns>
        Object this[String name] { get; set; }
        #endregion
    }
}