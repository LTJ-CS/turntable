using System;
using Sdk.Runtime.Utility;
using UnityEngine;

namespace GameScript.Runtime.GameLogic
{
    public class TimeUpdateControl
    {
        /// <summary>
        /// 跟服务器的时间差
        /// </summary>
        private long timeDelta;

        /// <summary>
        /// 服务器的当前时间
        /// </summary>
        private long serverTime;

        /// <summary>
        /// 上次更新的时间
        /// </summary>
        private long lastUpdateTime;

        public void Init()
        {
            SyncServerTime();
        }

        public long CurrentServerTime()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long nowTime = (long)timeSpan.TotalSeconds;
            // 如果上次更新时间超过了5分钟，更新一次
            if (nowTime - lastUpdateTime > 300)
            {
                SyncServerTime();
            }

            return nowTime - timeDelta;
        }

        public long CurrentServerMillTime()
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long nowTime = (long)timeSpan.TotalMilliseconds;
            // 如果上次更新时间超过了5分钟，更新一次
            if (nowTime - lastUpdateTime * 1000 > 300 * 1000)
            {
                SyncServerTime();
            }

            return nowTime - timeDelta;
        }

        /// <summary>
        /// 同步服务端时间
        /// </summary>
        private void SyncServerTime()
        {
            // 先更新lastUpdateTime，避免多次请求
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            lastUpdateTime = (long)timeSpan.TotalMilliseconds / 1000;
            _ = NetManager.SyncServerTimeUtc(SetServerTime);
        }

        private void SetServerTime(long severTimeStamp)
        {
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var delta = (long)timeSpan.TotalMilliseconds / 1000 - severTimeStamp;
            timeDelta = delta;
            serverTime = severTimeStamp;
        }

        public DateTime NowTime()
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime now = start.AddMilliseconds(CurrentServerTime() * 1000);
            return now;
        }

        void Tst()
        {
            // 获取当前Unix时间戳（秒）
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Console.WriteLine($"当前Unix时间戳（秒）: {unixTimestamp}");

            // 获取当前Unix时间戳（毫秒）
            long unixTimestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            Console.WriteLine($"当前Unix时间戳（毫秒）: {unixTimestampMilliseconds}");

            // 获取当前日期和时间
            DateTime now = DateTime.Now;

            // 格式化日期为“x月x日”格式
            string formattedDate = now.ToString("M月d日");
            Console.WriteLine($"当前日期（x月x日格式）: {formattedDate}");
        }
    }
}