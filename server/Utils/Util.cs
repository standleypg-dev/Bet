using server.Models;

namespace server.Utils;

public class Util
{
    public static IConfiguration _config { get; }
    static Util()
    {
        _config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
    }
    public static string GetRole(Role role)
    {
        string r = string.Empty;
        switch (role)
        {
            case Role.USER:
                r = "user";
                break;
            case Role.ADMIN:
                r = "admin";
                break;
        }
        return r;
    }

    public static string GetiSmsApiURL<T>(RequestType requestType, T source)
    {
        string username = Util._config.GetValue<string>("iSms:username");
        string password = Util._config.GetValue<string>("iSms:password");
        string url = @$"https://www.isms.com.my/2FA/request.php?";

        switch (requestType)
        {
            case RequestType.SEND:
                {
                    Send send = (Send)Convert.ChangeType(source!, typeof(Send));
                    url += @$"mobile={send.Mobile}&country_code={send.CountryCode}&un={username}&pass={password}&sendid={send.SendId}&type ={send.Type}&message ={send.Message}";
                }
                break;
            case RequestType.VERIFY:
                {
                    Verify verify = (Verify)Convert.ChangeType(source!, typeof(Verify));
                    url += @$"mobile={verify.Mobile}&country_code={verify.CountryCode}&un={username}&pass={password}&sendid={verify.SendId}&interval={verify.Interval}&method={verify.Method}&code={verify.SmsCode}&sms_id={verify.SmsId}&uuid={verify.Uuid}";
                }
                break;
        }
        return url;
    }
}
