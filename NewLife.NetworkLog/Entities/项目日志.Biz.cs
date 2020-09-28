using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using NewLife;
using NewLife.Data;
using NewLife.Log;
using NewLife.Model;
using NewLife.Reflection;
using NewLife.Threading;
using NewLife.Web;
using XCode;
using XCode.Cache;
using XCode.Configuration;
using XCode.DataAccessLayer;
using XCode.Membership;

namespace NewLife.NetworkLog.Entities
{

    public enum ThreadType
    {

        /// <summary>
        /// 线程池
        /// </summary>
        [Description("线程池")]
        Y,
        /// <summary>
        /// 网页
        /// </summary>
        [Description("网页")]
        W,
        /// <summary>
        /// 普通
        /// </summary>
        [Description("普通")]
        N
    }

    /// <summary>项目日志</summary>
    [Serializable]
    [ModelCheckMode(ModelCheckModes.CheckTableWhenFirstUse)]
    public class ProjectLog : ProjectLog<ProjectLog> { }

    /// <summary>项目日志</summary>
    public partial class ProjectLog<TEntity> : Entity<TEntity> where TEntity : ProjectLog<TEntity>, new()
    {
        #region 对象操作
        static ProjectLog()
        {
            // 用于引发基类的静态构造函数，所有层次的泛型实体类都应该有一个
            var entity = new TEntity();

            // 累加字段，生成 Update xx Set Count=Count+1234 Where xxx
            //var df = Meta.Factory.AdditionalFields;
            //df.Add(__.ProjectID);

            // 过滤器 UserModule、TimeModule、IPModule
        }

        /// <summary>验证数据，通过抛出异常的方式提示验证失败。</summary>
        /// <param name="isNew">是否插入</param>
        public override void Valid(Boolean isNew)
        {
            // 如果没有脏数据，则不需要进行任何处理
            if (!HasDirty) return;

            // 在新插入数据或者修改了指定字段时进行修正
        }

        ///// <summary>首次连接数据库时初始化数据，仅用于实体类重载，用户不应该调用该方法</summary>
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //protected override void InitData()
        //{
        //    // InitData一般用于当数据表没有数据时添加一些默认数据，该实体类的任何第一次数据库操作都会触发该方法，默认异步调用
        //    if (Meta.Session.Count > 0) return;

        //    if (XTrace.Debug) XTrace.WriteLine("开始初始化TEntity[项目日志]数据……");

        //    var entity = new TEntity();
        //    entity.ID = 0;
        //    entity.ProjectID = 0;
        //    entity.Time = DateTime.Now;
        //    entity.ThreadID = 0;
        //    entity.ThreadName = "abc";
        //    entity.Message = "abc";
        //    entity.Insert();

        //    if (XTrace.Debug) XTrace.WriteLine("完成初始化TEntity[项目日志]数据！");
        //}

        ///// <summary>已重载。基类先调用Valid(true)验证数据，然后在事务保护内调用OnInsert</summary>
        ///// <returns></returns>
        //public override Int32 Insert()
        //{
        //    return base.Insert();
        //}

        ///// <summary>已重载。在事务保护范围内处理业务，位于Valid之后</summary>
        ///// <returns></returns>
        //protected override Int32 OnDelete()
        //{
        //    return base.OnDelete();
        //}
        #endregion

        #region 扩展属性

        [JsonIgnore] public ProjectInfo ProjectInfo => ProjectInfo.FindByID(ProjectID);
        [JsonIgnore] public DateTime DateTime => new DateTime(Time);
        #endregion

        #region 扩展查询
        /// <summary>根据编号查找</summary>
        /// <param name="id">编号</param>
        /// <returns>实体对象</returns>
        public static TEntity FindByID(Int32 id)
        {
            if (id <= 0) return null;

            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Find(e => e.ID == id);

            // 单对象缓存
            return Meta.SingleCache[id];

            //return Find(_.ID == id);
        }

        /// <summary>根据项目ID查找</summary>
        /// <param name="projectId">项目ID</param>
        /// <returns>实体列表</returns>
        public static IList<TEntity> FindAllByProjectID(Int32 projectId)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ProjectID == projectId);

            return FindAll(_.ProjectID == projectId);
        }

        /// <summary>根据线程名查找</summary>
        /// <param name="threadName">线程名</param>
        /// <returns>实体列表</returns>
        public static IList<TEntity> FindAllByThreadName(String threadName)
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.ThreadName == threadName);

            return FindAll(_.ThreadName == threadName);
        }

        /// <summary>获取一个小时前的数据</summary>
        /// <returns>实体列表</returns>
        public static IList<TEntity> FindAllByBeforeOneHour()
        {
            var beforeOneHour = DateTime.Now.AddHours(-1);
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.FindAll(e => e.Time > beforeOneHour.Ticks);

            return FindAll(_.Time > beforeOneHour);
        }

        /// <summary>获取最新100条</summary>
        /// <returns>实体列表</returns>
        public static IList<TEntity> FindAllOneHundred()
        {
            // 实体缓存
            if (Meta.Session.Count < 1000) return Meta.Cache.Entities.TakeLast(100).ToList();

            return FindAll(null, new PageParameter { PageSize = 100 });
        }

        /// <summary>获取最新100条</summary>
        /// <returns>实体列表</returns>
        public static IList<TEntity> Search(PageParameter page, string keyword, int? projectId, DateTime? start, DateTime? end)
        {
            page.RetrieveTotalCount = true;

            var exp = SearchWhereByKey(keyword);

            if (start != null)
            {
                exp &= _.Time > start.Value.Ticks;
            }

            if (end != null)
            {
                exp &= _.Time < end.Value.Ticks;
            }

            if (projectId > 0)
            {
                exp = _.ProjectID == projectId & exp;
            }

            return FindAll(exp, page);
        }
        #endregion

        #region 高级查询
        /// <summary>高级查询</summary>
        /// <param name="projectId">项目ID</param>
        /// <param name="threadName">线程名</param>
        /// <param name="start">时间开始</param>
        /// <param name="end">时间结束</param>
        /// <param name="key">关键字</param>
        /// <param name="page">分页参数信息。可携带统计和数据权限扩展查询等信息</param>
        /// <returns>实体列表</returns>
        public static IList<TEntity> Search(Int32 projectId, String threadName, DateTime start, DateTime end, String key, PageParameter page)
        {
            var exp = new WhereExpression();

            if (projectId >= 0) exp &= _.ProjectID == projectId;
            if (!threadName.IsNullOrEmpty()) exp &= _.ThreadName == threadName;
            exp &= _.Time.Between(start, end);
            if (!key.IsNullOrEmpty()) exp &= _.Message.Contains(key);

            return FindAll(exp, page);
        }

        // Select Count(ID) as ID,ThreadName From ProjectLog Where CreateTime>'2020-01-24 00:00:00' Group By ThreadName Order By ID Desc limit 20
        static readonly FieldCache<TEntity> _ThreadNameCache = new FieldCache<TEntity>(__.ThreadName)
        {
            //Where = _.CreateTime > DateTime.Today.AddDays(-30) & Expression.Empty
        };

        /// <summary>获取线程名列表，字段缓存10分钟，分组统计数据最多的前20种，用于魔方前台下拉选择</summary>
        /// <returns></returns>
        public static IDictionary<String, String> GetThreadNameList() => _ThreadNameCache.FindAllName();
        #endregion

        #region 业务操作
        #endregion
    }
}