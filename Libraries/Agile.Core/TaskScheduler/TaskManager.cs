using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core.TaskScheduler
{
    public class TaskManager
    {
        private IScheduler _scheduler;

        /// <summary>
        /// 实例
        /// </summary>
        public static TaskManager Instance { get; } = new TaskManager();

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {

        }

        /// <summary>
        /// 返回任务计划（调度器）
        /// </summary>
        public void AddScheduleJobAsync()
        {

        }

        /// <summary>
        /// 暂停/删除 指定的计划
        /// </summary>
        public void StopOrDelScheduleJobAsync()
        {

        }

        /// <summary>
        /// 恢复运行暂停的任务
        /// </summary>
        public void ResumeJobAsync()
        {

        }

        /// <summary>
        /// 查询任务
        /// </summary>
        public void QueryJobAsync()
        {

        }

        /// <summary>
        /// 立即执行
        /// </summary>
        public void TriggerJobAsync()
        {

        }

        /// <summary>
        /// 获取job日志
        /// </summary>
        public void GetJobLogsAsync()
        {

        }

        /// <summary>
        /// 获取所有Job
        /// </summary>
        public void GetAllJobAsync()
        {

        }

        /// <summary>
        /// 开启调度器
        /// </summary>
        public void StartScheduleAsync()
        {

        }

        /// <summary>
        /// 停止任务调度
        /// </summary>
        public void StopScheduleAsync()
        {

        }
    }
}
