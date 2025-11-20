namespace JT808.Gateway;

public class JT808GatewayWebApiOptions
{
    /// <summary>
    /// 路由前缀
    /// </summary>
    public string RoutePrefix { get; set; } = "jt808api";

    /// <summary>
    /// 是否验证鉴权
    /// </summary>
    public bool VerifyAuthorization { get; set; } = true;
}