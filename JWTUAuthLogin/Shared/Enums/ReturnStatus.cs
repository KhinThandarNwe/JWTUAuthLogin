using System.ComponentModel;

namespace JWTUAuthLogin.Shared.Enums
{
    public enum HttpStatusCode
    {
        success = 101,

        //---------------------------------
        error = 200,
        error_Unknown = 201,
        error_BusinessLogic = 202,
        error_InvalidLicense = 203,

        //---------------------------------
        error_securityTokenExpiry = 210,
        error_InvalidUserNameAndPassword = 211,

        //---------------------------------
        error_NoConnection = 220,
        error_API = 221,

        [Description("System cannot find the record.")]
        error_NoRecordFound = 222,

        //---------------------------------
        error_NoUpdatePermission = 231,
        error_NoCreatePermission = 232,
        error_NoViewPermission = 233,
        error_NoDeletePermission = 234,
        error_Duplicate = 235,

        //License-----------------------------
        error_license_email_register = 241,

        [Description("Duplicate Contact Person.")]
        error_contact_person_register = 242,
    }
    public enum ReturnStatus
    {
        // 1XX Informational
        Continue = 100,
        SwitchingProtocols = 101,
        Processing = 102,

        // 2XX Success
        success = 200,
        Created = 201,
        Accepted = 202,
        NonAuthoritativeInformation = 203,
        NoContent = 204,
        ResetContent = 205,
        PartialContent = 206,
        MultiStatus = 207,
        AlreadyReported = 208,
        IMUsed = 226,

        // 3XX Redirection
        MultipleChoices = 300,
        MovedPermanently = 301,
        Found = 302,
        SeeOther = 303,
        NotModified = 304,
        UseProxy = 305,
        TemporaryRedirect = 307,
        PermanentRedirect = 308,

        // 4XX Client Error
        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,

        Forbidden = 403,
        error_NoUpdatePermission = 4021,
        error_NoCreatePermission = 4022,
        error_NoViewPermission = 4023,
        error_NoDeletePermission = 4024,

        NotFound = 404,
        MethodNotAllowed = 405,

        // NotAcceptable = 406,
        ProxyAuthenticationRequired = 407,

        Conflict = 409,
        error_Useothermodule = 4091,
        error_CodeConflict = 4092,
        error_NameConflict = 4093,
        error_CodeAndNameConflict = 4094,
        error_SubCodeConflict = 4095,
        error_SubNameConflict = 4096,
        error_SubCodeAndNameConflict = 4097,

        // The record already exists with the same unique constraint.
        AlreadyExists = 4098,

        Gone = 410,
        LengthRequired = 411,
        PreconditionFailed = 412,
        PayloadTooLarge = 413,
        RequestUriTooLong = 414,
        UnsupportedMediaType = 415,
        RequestedRangeNotSatisfiable = 416,
        ExpectationFailed = 417,
        ImATeapot = 418,
        MisdirectedRequest = 421,

        UnprocessableEntity = 422,
        error_InvalidLicense = 4221,
        error_InvalidUserCodeOrEmail = 4222,
        error_InvalidPassword = 4223,
        error_InvalidUserActive = 4224,
        error_UserAlreadyExist = 4225,

        error_EmailAlreadyExist = 4226,
        error_SamePassword = 4227,
        error_PhoneNumberAlreadyExist = 4228,
        error_LoginAccess = 4229,
        error_DeleteDafaultUser = 4230,

        Locked = 423,
        FailedDependency = 424,
        UpgradeRequired = 426,
        PreconditionRequired = 428,
        TooManyRequests = 429,
        RequestHeaderFieldsTooLarge = 431,
        ConnectionClosedWithoutResponse = 444,
        UnavailableForLegalReasons = 451,
        ClientClosedRequest = 499,

        // 5XX Server Error
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        HttpVersionNotSupported = 505,
        VariantAlsoNegotiates = 506,
        InsufficientStorage = 507,
        LoopDetected = 508,
        NotExtended = 510,
        NetworkAuthenticationRequired = 511,
        NetworkConnectTimeoutError = 599,

        // mobile login
        error_UserAlreadyLogIn = 406,
        force_logout = 408,

        MfaRequired = 4233,
        error_AccountLocked = 4231,
        error_PasswordExpired = 4232,
        error_FailedAttempt3 = 4234,
        error_FailedAttempt10 = 4235,
        error_InvalidUserCodeOrEmailOrPassword = 4236,
        error_TimeZoneMismatch = 4238,
        Error_LocalTimeZone = 419,

        NotAllowedCrossLeave = 4005,
    }
}
