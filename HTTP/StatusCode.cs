using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP
{
    public enum StatusCode
    {
        CONTINUE = 100,
        SWITCHING_PROTOCOLS = 101,
        PROCESSING = 102,
        EARLY_HINTS = 103,

        OK = 200,
        CREATED = 201,
        ACCEPTED = 202,
        NON_AUTHORITATIVE_INFORMATION = 203,
        NO_CONTENT = 204,
        RESET_CONTENT = 205,
        PARTIAL_CONTENT = 206,
        MULTI_STATUS = 207,
        ALREADY_REPORTED = 208,
        IM_USED = 226,

        MULTIPLE_CHOICES = 300,
        MOVED_PERMANENTLY = 301,
        FOUND = 302,
        SEE_OTHER = 303,
        NOT_MODIFIED = 304,
        USE_PROXY = 305,
        SWITCH_PROXY = 306,
        TEMPORARY_REDIRECT = 307,
        PERMANENT_REDIRECT = 308,

        BAD_REQUEST = 400,
        UNAUTHORIZED = 401,
        PAYMENT_REQUIRED = 402,
        FORBIDDEN = 403,
        NOT_FOUND = 404,
        METHOD_NOT_ALLOWED = 405,
        NOT_ACCEPTABLE = 406,
        PROXY_AUTHENTICATION_REQUIRED = 407,
        REQUEST_TIMEOUT = 408,
        CONFLICT = 409,
        GONE = 410,
        LENGTH_REQUIRED = 411,
        PRECONDITION_FAILED = 412,
        PAYLOAD_TOO_LARGE = 413,
        URI_TOO_LONG = 414,
        UNSUPPORTED_MEDIA_TYPE = 415,
        RANGE_NOT_SATISFIABLE = 416,
        EXPECTATION_FAILED = 417,
        IM_A_TEAPOT = 418,
        MISDIRECTED_REQUEST = 421,
        UNPROCESSABLE_ENTITY = 422,
        LOCKED = 423,
        FAILED_DEPENDENCY = 424,
        UPGRADE_REQUIRED = 426,
        PRECONDITION_REQUIRED = 428,
        TOO_MANY_REQUESTS = 429,
        REQUEST_HEADER_FIELDS_TOO_LARGE = 431,
        UNAVAILABLE_FOR_LEGAL_REASONS = 451,

        INTERNAL_SERVER_ERROR = 500,
        NOT_IMPLEMENTED = 501,
        BAD_GATEWAY = 502,
        SERVICE_UNAVAILABLE = 503,
        GATEWAY_TIMEOUT = 504,
        HTTP_VERSION_NOT_SUPPORTED = 505,
        VARIANT_ALSO_NEGOTIATES = 506,
        INSUFFICIENT_STORAGE = 507,
        LOOP_DETECTED = 508,
        NOT_EXTENDED = 510,
        NETWORK_AUTHENTICATION_REQUIRED = 511,
    }

    public static class StatusCodeToString
    {
        public static string ToFriendlyString(this StatusCode Status)
        {
            switch (Status)
            {
                case StatusCode.CONTINUE:
                    return "Continue";
                case StatusCode.SWITCHING_PROTOCOLS:
                    return "Switching Protocols";
                case StatusCode.PROCESSING:
                    return "Processing";
                case StatusCode.EARLY_HINTS:
                    return "Early Hints";
                case StatusCode.OK:
                    return "OK";
                case StatusCode.CREATED:
                    return "Created";
                case StatusCode.ACCEPTED:
                    return "Accepted";
                case StatusCode.NON_AUTHORITATIVE_INFORMATION:
                    return "Non-Authoritative Information";
                case StatusCode.NO_CONTENT:
                    return "No Content";
                case StatusCode.RESET_CONTENT:
                    return "Reset Content";
                case StatusCode.PARTIAL_CONTENT:
                    return "Partial Content";
                case StatusCode.MULTI_STATUS:
                    return "Multi-Status";
                case StatusCode.ALREADY_REPORTED:
                    return "Already Reported";
                case StatusCode.IM_USED:
                    return "IM Used";

                case StatusCode.MULTIPLE_CHOICES:
                    return "Multiple Choices";
                case StatusCode.MOVED_PERMANENTLY:
                    return "Moved permanently";
                case StatusCode.FOUND:
                    return "Found";
                case StatusCode.SEE_OTHER:
                    return "See Other";
                case StatusCode.NOT_MODIFIED:
                    return "Not modified";
                case StatusCode.USE_PROXY:
                    return "Use Proxy";
                case StatusCode.SWITCH_PROXY:
                    return "Switch Proxy";
                case StatusCode.TEMPORARY_REDIRECT:
                    return "Temporary Redirect";
                case StatusCode.PERMANENT_REDIRECT:
                    return "Permanent Redirect";

                case StatusCode.BAD_REQUEST:
                    return "Bad Request";
                case StatusCode.UNAUTHORIZED:
                    return "Unauthorized";
                case StatusCode.PAYMENT_REQUIRED:
                    return "Payment Required";
                case StatusCode.FORBIDDEN:
                    return "Forbidden";
                case StatusCode.NOT_FOUND:
                    return "Not Found";
                case StatusCode.METHOD_NOT_ALLOWED:
                    return "Method Not Allowed";
                case StatusCode.NOT_ACCEPTABLE:
                    return "Not Acceptable";
                case StatusCode.PROXY_AUTHENTICATION_REQUIRED:
                    return "Proxy Authentication Required";
                case StatusCode.REQUEST_TIMEOUT:
                    return "Request Timeout";
                case StatusCode.CONFLICT:
                    return "Conflict";
                case StatusCode.GONE:
                    return "Gone";
                case StatusCode.LENGTH_REQUIRED:
                    return "Length Required";
                case StatusCode.PRECONDITION_FAILED:
                    return "Precondition Failed";
                case StatusCode.PAYLOAD_TOO_LARGE:
                    return "Payload Too Large";
                case StatusCode.URI_TOO_LONG:
                    return "URI Too Long";
                case StatusCode.UNSUPPORTED_MEDIA_TYPE:
                    return "Unsupported Media Type";
                case StatusCode.RANGE_NOT_SATISFIABLE:
                    return "Range Not Satisfiable";
                case StatusCode.EXPECTATION_FAILED:
                    return "Expectation Failed";
                case StatusCode.IM_A_TEAPOT:
                    return "I'm A Teapot";
                case StatusCode.MISDIRECTED_REQUEST:
                    return "Misdirected Request";
                case StatusCode.UNPROCESSABLE_ENTITY:
                    return "Unprocessable Entity";
                case StatusCode.LOCKED:
                    return "Locked";
                case StatusCode.FAILED_DEPENDENCY:
                    return "Failed Dependency";
                case StatusCode.UPGRADE_REQUIRED:
                    return "Upgrade Required";
                case StatusCode.PRECONDITION_REQUIRED:
                    return "Precondition Required";
                case StatusCode.TOO_MANY_REQUESTS:
                    return "Too Many Requests";
                case StatusCode.REQUEST_HEADER_FIELDS_TOO_LARGE:
                    return "Request Header Fields Too Large";
                case StatusCode.UNAVAILABLE_FOR_LEGAL_REASONS:
                    return "Unavailable For Legal Reasons";

                case StatusCode.INTERNAL_SERVER_ERROR:
                    return "Internal Server Error";
                case StatusCode.NOT_IMPLEMENTED:
                    return "Not Implemented";
                case StatusCode.BAD_GATEWAY:
                    return "Bad Gateway";
                case StatusCode.SERVICE_UNAVAILABLE:
                    return "Service Unavailable";
                case StatusCode.GATEWAY_TIMEOUT:
                    return "Gateway Timeout";
                case StatusCode.HTTP_VERSION_NOT_SUPPORTED:
                    return "HTTP Version Not Supported";
                case StatusCode.VARIANT_ALSO_NEGOTIATES:
                    return "Variant Also Negotiates";
                case StatusCode.INSUFFICIENT_STORAGE:
                    return "Insufficient Storage";
                case StatusCode.LOOP_DETECTED:
                    return "Loop Detected";
                case StatusCode.NOT_EXTENDED:
                    return "Not Extended";
                case StatusCode.NETWORK_AUTHENTICATION_REQUIRED:
                    return "Network Authentication Required";

                default:
                    throw new ArgumentException();
            }
        }
    }
}
