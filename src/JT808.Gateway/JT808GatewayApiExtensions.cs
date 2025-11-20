using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JT808.Gateway.Abstractions.Dtos;
using JT808.Gateway.Authorization;
using JT808.Gateway.Services;
using JT808.Gateway.Session;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

[assembly: InternalsVisibleTo("JT808.Gateway.TestHosting")]
[assembly: InternalsVisibleTo("JT808.Gateway.Test")]
namespace JT808.Gateway;

/// <summary>
/// JT808网关注册扩展
/// </summary>
public static partial class JT808GatewayExtensions
{
    /// <summary>
    /// 添加JT808 WebApi
    /// </summary>
    /// <param name="app"></param>
    /// <param name="optionsFactory"></param>
    /// <returns></returns>
    public static IEndpointRouteBuilder UseJT808GatewayWebApi(this IEndpointRouteBuilder app, Action<JT808GatewayWebApiOptions> optionsFactory = null)
    {
        var options = new JT808GatewayWebApiOptions();
        optionsFactory?.Invoke(options);
        var group = app.MapGroup(options.RoutePrefix);

        if (options.VerifyAuthorization) group.AddEndpointFilter<JT808TokenFilter>();

        group.MapGet("/index", () => new JT808ResultDto<string>
        {
            Data = "Hello,JT808 WebApi",
            Code = JT808ResultCode.Ok
        });

        group.MapPost("/UnificationSend", async (JT808UnificationSendRequestDto parameter, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<bool>();

            try
            {
                result.Data = await sessionManager.TrySendByTerminalPhoneNoAsync(parameter.TerminalPhoneNo, Convert.FromHexString(parameter.HexData));
                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapGet("/Tcp/Session/GetAll", (JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<List<JT808TcpSessionInfoDto>>();

            try
            {
                result.Data = sessionManager.GetTcpAll().Select(s => new JT808TcpSessionInfoDto
                {
                    LastActiveTime = s.ActiveTime,
                    StartTime = s.StartTime,
                    TerminalPhoneNo = s.TerminalPhoneNo,
                    RemoteAddressIP = s.RemoteEndPoint.ToString(),
                })
                .ToList();

                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = new();
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapGet("/Tcp/Session/SessionTcpByPage", (int pageIndex, int pageSize, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<JT808PageResult<List<JT808TcpSessionInfoDto>>>();

            try
            {
                if (pageIndex < 0) pageIndex = 0;
                if (pageSize >= 1000) pageSize = 1000;

                var sessions = sessionManager.GetTcpByPage();

                var page = new JT808PageResult<List<JT808TcpSessionInfoDto>>
                {
                    Data = sessions.Select(s => new JT808TcpSessionInfoDto
                    {
                        LastActiveTime = s.ActiveTime,
                        StartTime = s.StartTime,
                        TerminalPhoneNo = s.TerminalPhoneNo,
                        RemoteAddressIP = s.RemoteEndPoint.ToString(),
                    })
                    .OrderByDescending(o => o.LastActiveTime)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList(),
                    Total = sessions.Count(),
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                result.Data = page;
                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = new();
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Tcp/Session/QuerySessionByTerminalPhoneNo", (JT808TerminalPhoneNoDto parameter, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<JT808TcpSessionInfoDto>();

            try
            {
                result.Data = sessionManager.GetTcpAll(w => w.TerminalPhoneNo == parameter.TerminalPhoneNo).Select(s => new JT808TcpSessionInfoDto
                {
                    LastActiveTime = s.ActiveTime,
                    StartTime = s.StartTime,
                    TerminalPhoneNo = s.TerminalPhoneNo,
                    RemoteAddressIP = s.RemoteEndPoint.ToString(),
                })
                .FirstOrDefault();

                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = null!;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Tcp/Session/RemoveByTerminalPhoneNo", (JT808TerminalPhoneNoDto parameter, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<bool>();

            try
            {
                sessionManager.RemoveByTerminalPhoneNo(parameter.TerminalPhoneNo);
                result.Code = JT808ResultCode.Ok;
                result.Data = true;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapGet("/Udp/Session/GetAll", (JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<List<JT808UdpSessionInfoDto>>();

            try
            {
                result.Data = sessionManager.GetUdpAll().Select(s => new JT808UdpSessionInfoDto
                {
                    LastActiveTime = s.ActiveTime,
                    StartTime = s.StartTime,
                    TerminalPhoneNo = s.TerminalPhoneNo,
                    RemoteAddressIP = s.RemoteEndPoint.ToString(),
                })
                .ToList();

                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = new();
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapGet("/Udp/Session/SessionUdpByPage", (int pageIndex, int pageSize, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<JT808PageResult<List<JT808UdpSessionInfoDto>>>();

            try
            {
                if (pageIndex < 0) pageIndex = 0;
                if (pageSize >= 1000) pageSize = 1000;

                var sessions = sessionManager.GetUdpByPage();

                var page = new JT808PageResult<List<JT808UdpSessionInfoDto>>
                {
                    Data = sessions.Select(s => new JT808UdpSessionInfoDto
                    {
                        LastActiveTime = s.ActiveTime,
                        StartTime = s.StartTime,
                        TerminalPhoneNo = s.TerminalPhoneNo,
                        RemoteAddressIP = s.RemoteEndPoint.ToString(),
                    })
                    .OrderByDescending(o => o.LastActiveTime)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList(),
                    Total = sessions.Count(),
                    PageIndex = pageIndex,
                    PageSize = pageSize
                };

                result.Data = page;
                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = new();
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Udp/Session/QuerySessionByTerminalPhoneNo", (JT808TerminalPhoneNoDto parameter, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<JT808UdpSessionInfoDto>();

            try
            {
                result.Data = sessionManager.GetUdpAll(w => w.TerminalPhoneNo == parameter.TerminalPhoneNo).Select(s => new JT808UdpSessionInfoDto
                {
                    LastActiveTime = s.ActiveTime,
                    StartTime = s.StartTime,
                    TerminalPhoneNo = s.TerminalPhoneNo,
                    RemoteAddressIP = s.RemoteEndPoint.ToString(),
                })
                .FirstOrDefault();

                result.Code = JT808ResultCode.Ok;
            }
            catch (Exception ex)
            {
                result.Data = null!;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Udp/Session/RemoveByTerminalPhoneNo", (JT808TerminalPhoneNoDto parameter, JT808SessionManager sessionManager) =>
        {
            var result = new JT808ResultDto<bool>();

            try
            {
                sessionManager.RemoveByTerminalPhoneNo(parameter.TerminalPhoneNo);
                result.Code = JT808ResultCode.Ok;
                result.Data = true;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Blacklist/Add", (JT808TerminalPhoneNoDto parameter, JT808BlacklistManager blacklistManager) =>
        {
            var result = new JT808ResultDto<bool>();

            try
            {
                blacklistManager.Add(parameter.TerminalPhoneNo);
                result.Code = JT808ResultCode.Ok;
                result.Data = true;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapPost("/Blacklist/Remove", (JT808TerminalPhoneNoDto parameter, JT808BlacklistManager blacklistManager) =>
        {
            var result = new JT808ResultDto<bool>();

            try
            {
                blacklistManager.Remove(parameter.TerminalPhoneNo);
                result.Code = JT808ResultCode.Ok;
                result.Data = true;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        group.MapGet("/Blacklist/GetAll", (JT808BlacklistManager blacklistManager) =>
        {
            var result = new JT808ResultDto<List<string>>();

            try
            {
                result.Code = JT808ResultCode.Ok;
                result.Data = blacklistManager.GetAll();
            }
            catch (Exception ex)
            {
                result.Data = new();
                result.Code = JT808ResultCode.Error;
                result.Message = ex.StackTrace;
            }

            return result;
        });

        return app;
    }
}