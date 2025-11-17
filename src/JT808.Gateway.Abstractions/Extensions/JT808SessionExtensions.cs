using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Threading.Tasks;
using JT808.Gateway.Abstractions.Enums;

namespace JT808.Gateway.Abstractions
{
    /// <summary>
    /// JT808会话扩展
    /// </summary>
    public static class JT808SessionExtensions
    {
        /// <summary>
        /// 下发消息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data"></param>
        public static async Task<Result> SendAsync(this IJT808Session session, byte[] data)
        {
            var result = new Result();
            try
            {
                if (data == null)
                {
                    result.Reason = FailReason.EmptyData;
                }
                else if (session.TransportProtocolType == JT808TransportProtocolType.tcp)
                {
                    if (session.Client.Connected)
                    {
                        await session.Client.SendAsync(data, SocketFlags.None);
                        result.Success = true;
                    }
                    else
                    {
                        result.Reason = FailReason.SessionNotConnected;
                    }
                }
                else
                {
                    await session.Client.SendToAsync(data, SocketFlags.None, session.RemoteEndPoint);
                    result.Success = true;
                }
            }
            catch (AggregateException ex)
            {
                result.Reason = FailReason.SessionInvalidAggregate;
                result.Exception = ex;
            }
            catch (Exception ex)
            {
                result.Reason = FailReason.Exception;
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// 下发消息
        /// </summary>
        /// <param name="session"></param>
        /// <param name="data"></param>
        public static Result Send(this IJT808Session session, byte[] data)
        {
            var result = new Result();
            try
            {
                if (data == null)
                {
                    result.Reason = FailReason.EmptyData;
                }
                else if (session.TransportProtocolType == JT808TransportProtocolType.tcp)
                {
                    if (session.Client.Connected)
                    {
                        session.Client.Send(data, SocketFlags.None);
                        result.Success = true;
                    }
                    else
                    {
                        result.Reason = FailReason.SessionNotConnected;
                    }
                }
                else
                {
                    session.Client.SendTo(data, SocketFlags.None, session.RemoteEndPoint);
                    result.Success = true;
                }
            }
            catch (AggregateException ex)
            {
                result.Reason = FailReason.SessionInvalidAggregate;
                result.Exception = ex;
            }
            catch (Exception ex)
            {
                result.Reason = FailReason.Exception;
                result.Exception = ex;
            }
            return result;
        }

        public class UnknownErrorException : Exception
        {
            public UnknownErrorException() : base("An unknown error occurred.")
            {

            }
        }
        /// <summary>
        /// 结果
        /// </summary>
        public class Result
        {
            /// <summary>
            /// 是否成功
            /// </summary>
            public bool Success { get; set; }
            /// <summary>
            /// 失败原因
            /// </summary>
            public FailReason Reason { get; set; } = FailReason.None;
            /// <summary>
            /// 异常
            /// </summary>
            [MemberNotNullWhen(false, nameof(Success))]
            public Exception Exception { get; set; }
        }

        /// <summary>
        /// 失败原因
        /// </summary>
        public enum FailReason
        {
            /// <summary>
            /// 未知
            /// </summary>
            None,
            /// <summary>
            /// 空数据
            /// </summary>
            EmptyData,
            /// <summary>
            /// 非已连接状态
            /// </summary>
            SessionNotConnected,
            /// <summary>
            /// 无效的发送参数
            /// </summary>
            SessionInvalidAggregate,
            /// <summary>
            /// 未知异常
            /// </summary>
            Exception
        }
    }
}
