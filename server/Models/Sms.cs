using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace server.Models;

public class Sms
{
    public string Mobile_;
    public string Mobile
    {
        get { return Mobile_; }
        set { Mobile_ = value.Substring(2, value.Length - 2); }
    }
    public string CountryCode_;
    public string CountryCode
    {
        get { return CountryCode_; }
        set { CountryCode_ = value.Substring(0, 2); }
    }
    public Int64 SendId { get; private set; } = 62033;
}

public class Send : Sms
{
    public int Type { get; private set; } = 1;
    public string Message { get; private set; } = "Your BK8 Verification OTP Code is :%OTP% ";
}

public class Verify : Sms
{
    public int Interval { get; private set; } = 5;//3 mins
    public string Method { get; private set; } = "verify";
    public string SmsCode { get; set; }
    public string SmsId { get; set; }
    public string Uuid { get; set; }
}
