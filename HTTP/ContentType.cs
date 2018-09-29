using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTTP
{
    public struct ContentType
    {
        public readonly string Type;
        public readonly string SubType;

        public readonly Option<string> Boundary;
        public readonly Option<string> Charset;

        private ContentType(string Type, string SubType, string Boundary = null, string Charset = null)
        {
            this.Type = Type;
            this.SubType = SubType;

            this.Boundary = (Boundary ?? Option<string>.None());
            this.Charset = (Charset ?? Option<string>.None());
        }

        public static ContentType Parse(string Str)
        {
            if (Str == null) throw new ArgumentException("null passed to ContentType constructor");

            if (Str.Contains(';'))
            {
                var toks = Str.Split(';').Select(s => s.Trim()).ToArray();

                var ct = Parse(toks[0]);

                string boundary = null;
                string charset = null;

                for(int i = 1; i < toks.Length; i++)
                {
                    if (toks[i].StartsWith("boundary="))
                    {
                        if (boundary != null) throw new ArgumentException();
                        boundary = toks[i].Substring("boundary=".Length);
                    }
                    else if (toks[i].StartsWith("charset="))
                    {
                        if (charset != null) throw new ArgumentException();
                        charset = toks[i].Substring("charset=".Length);
                    }
                    else throw new ArgumentException();
                }

                return new ContentType(ct.Type, ct.SubType, boundary, charset);

            }
            else
            {
                var toks = Str.Split('/');

                if (toks.Length != 2)
                {
                    throw new FormatException($"invalid content-type: {Str}");
                }
                else return new ContentType(toks[0], toks[1]);
            }
        }

        public static bool TryParse(string Str, out ContentType ContentType)
        {
            try
            {
                ContentType = Parse(Str);
                return true;
            }
            catch (Exception e)
            {
                ContentType = default(ContentType);
                return false;
            }
        }

        public override string ToString()
        {
            return $"{Type}/{SubType}";
        }

        public bool Equals(ContentType rhs)
        {
            return Type == rhs.Type && SubType == rhs.SubType;
        }

        public static implicit operator ContentType(string str)
        {
            return Parse(str);
        }

        public static Option<ContentType> Lookup(string Extension)
        {
            var last_ext = Extension.Split('.').Last();

            switch (last_ext)
            {
                /* this list stolen from https://github.com/samuelneff/MimeTypeMap */
                case "323": return Option<ContentType>.Some("text/h323");
                case "3g2": return Option<ContentType>.Some("video/3gpp2");
                case "3gp": return Option<ContentType>.Some("video/3gpp");
                case "3gp2": return Option<ContentType>.Some("video/3gpp2");
                case "3gpp": return Option<ContentType>.Some("video/3gpp");
                case "7z": return Option<ContentType>.Some("application/x-7z-compressed");
                case "aa": return Option<ContentType>.Some("audio/audible");
                case "AAC": return Option<ContentType>.Some("audio/aac");
                case "aaf": return Option<ContentType>.Some("application/octet-stream");
                case "aax": return Option<ContentType>.Some("audio/vnd.audible.aax");
                case "ac3": return Option<ContentType>.Some("audio/ac3");
                case "aca": return Option<ContentType>.Some("application/octet-stream");
                case "accda": return Option<ContentType>.Some("application/msaccess.addin");
                case "accdb": return Option<ContentType>.Some("application/msaccess");
                case "accdc": return Option<ContentType>.Some("application/msaccess.cab");
                case "accde": return Option<ContentType>.Some("application/msaccess");
                case "accdr": return Option<ContentType>.Some("application/msaccess.runtime");
                case "accdt": return Option<ContentType>.Some("application/msaccess");
                case "accdw": return Option<ContentType>.Some("application/msaccess.webapplication");
                case "accft": return Option<ContentType>.Some("application/msaccess.ftemplate");
                case "acx": return Option<ContentType>.Some("application/internet-property-stream");
                case "AddIn": return Option<ContentType>.Some("text/xml");
                case "ade": return Option<ContentType>.Some("application/msaccess");
                case "adobebridge": return Option<ContentType>.Some("application/x-bridge-url");
                case "adp": return Option<ContentType>.Some("application/msaccess");
                case "ADT": return Option<ContentType>.Some("audio/vnd.dlna.adts");
                case "ADTS": return Option<ContentType>.Some("audio/aac");
                case "afm": return Option<ContentType>.Some("application/octet-stream");
                case "ai": return Option<ContentType>.Some("application/postscript");
                case "aif": return Option<ContentType>.Some("audio/aiff");
                case "aifc": return Option<ContentType>.Some("audio/aiff");
                case "aiff": return Option<ContentType>.Some("audio/aiff");
                case "air": return Option<ContentType>.Some("application/vnd.adobe.air-application-installer-package+zip");
                case "amc": return Option<ContentType>.Some("application/mpeg");
                case "anx": return Option<ContentType>.Some("application/annodex");
                case "apk": return Option<ContentType>.Some("application/vnd.android.package-archive");
                case "application": return Option<ContentType>.Some("application/x-ms-application");
                case "art": return Option<ContentType>.Some("image/x-jg");
                case "asa": return Option<ContentType>.Some("application/xml");
                case "asax": return Option<ContentType>.Some("application/xml");
                case "ascx": return Option<ContentType>.Some("application/xml");
                case "asd": return Option<ContentType>.Some("application/octet-stream");
                case "asf": return Option<ContentType>.Some("video/x-ms-asf");
                case "ashx": return Option<ContentType>.Some("application/xml");
                case "asi": return Option<ContentType>.Some("application/octet-stream");
                case "asm": return Option<ContentType>.Some("text/plain");
                case "asmx": return Option<ContentType>.Some("application/xml");
                case "aspx": return Option<ContentType>.Some("application/xml");
                case "asr": return Option<ContentType>.Some("video/x-ms-asf");
                case "asx": return Option<ContentType>.Some("video/x-ms-asf");
                case "atom": return Option<ContentType>.Some("application/atom+xml");
                case "au": return Option<ContentType>.Some("audio/basic");
                case "avi": return Option<ContentType>.Some("video/x-msvideo");
                case "axa": return Option<ContentType>.Some("audio/annodex");
                case "axs": return Option<ContentType>.Some("application/olescript");
                case "axv": return Option<ContentType>.Some("video/annodex");
                case "bas": return Option<ContentType>.Some("text/plain");
                case "bcpio": return Option<ContentType>.Some("application/x-bcpio");
                case "bin": return Option<ContentType>.Some("application/octet-stream");
                case "bmp": return Option<ContentType>.Some("image/bmp");
                case "c": return Option<ContentType>.Some("text/plain");
                case "cab": return Option<ContentType>.Some("application/octet-stream");
                case "caf": return Option<ContentType>.Some("audio/x-caf");
                case "calx": return Option<ContentType>.Some("application/vnd.ms-office.calx");
                case "cat": return Option<ContentType>.Some("application/vnd.ms-pki.seccat");
                case "cc": return Option<ContentType>.Some("text/plain");
                case "cd": return Option<ContentType>.Some("text/plain");
                case "cdda": return Option<ContentType>.Some("audio/aiff");
                case "cdf": return Option<ContentType>.Some("application/x-cdf");
                case "cer": return Option<ContentType>.Some("application/x-x509-ca-cert");
                case "cfg": return Option<ContentType>.Some("text/plain");
                case "chm": return Option<ContentType>.Some("application/octet-stream");
                case "class": return Option<ContentType>.Some("application/x-java-applet");
                case "clp": return Option<ContentType>.Some("application/x-msclip");
                case "cmd": return Option<ContentType>.Some("text/plain");
                case "cmx": return Option<ContentType>.Some("image/x-cmx");
                case "cnf": return Option<ContentType>.Some("text/plain");
                case "cod": return Option<ContentType>.Some("image/cis-cod");
                case "config": return Option<ContentType>.Some("application/xml");
                case "contact": return Option<ContentType>.Some("text/x-ms-contact");
                case "coverage": return Option<ContentType>.Some("application/xml");
                case "cpio": return Option<ContentType>.Some("application/x-cpio");
                case "cpp": return Option<ContentType>.Some("text/plain");
                case "crd": return Option<ContentType>.Some("application/x-mscardfile");
                case "crl": return Option<ContentType>.Some("application/pkix-crl");
                case "crt": return Option<ContentType>.Some("application/x-x509-ca-cert");
                case "cs": return Option<ContentType>.Some("text/plain");
                case "csdproj": return Option<ContentType>.Some("text/plain");
                case "csh": return Option<ContentType>.Some("application/x-csh");
                case "csproj": return Option<ContentType>.Some("text/plain");
                case "css": return Option<ContentType>.Some("text/css");
                case "csv": return Option<ContentType>.Some("text/csv");
                case "cur": return Option<ContentType>.Some("application/octet-stream");
                case "cxx": return Option<ContentType>.Some("text/plain");
                case "dat": return Option<ContentType>.Some("application/octet-stream");
                case "datasource": return Option<ContentType>.Some("application/xml");
                case "dbproj": return Option<ContentType>.Some("text/plain");
                case "dcr": return Option<ContentType>.Some("application/x-director");
                case "def": return Option<ContentType>.Some("text/plain");
                case "deploy": return Option<ContentType>.Some("application/octet-stream");
                case "der": return Option<ContentType>.Some("application/x-x509-ca-cert");
                case "dgml": return Option<ContentType>.Some("application/xml");
                case "dib": return Option<ContentType>.Some("image/bmp");
                case "dif": return Option<ContentType>.Some("video/x-dv");
                case "dir": return Option<ContentType>.Some("application/x-director");
                case "disco": return Option<ContentType>.Some("text/xml");
                case "divx": return Option<ContentType>.Some("video/divx");
                case "dll": return Option<ContentType>.Some("application/x-msdownload");
                case "dll.config": return Option<ContentType>.Some("text/xml");
                case "dlm": return Option<ContentType>.Some("text/dlm");
                case "doc": return Option<ContentType>.Some("application/msword");
                case "docm": return Option<ContentType>.Some("application/vnd.ms-word.document.macroEnabled.12");
                case "docx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.wordprocessingml.document");
                case "dot": return Option<ContentType>.Some("application/msword");
                case "dotm": return Option<ContentType>.Some("application/vnd.ms-word.template.macroEnabled.12");
                case "dotx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.wordprocessingml.template");
                case "dsp": return Option<ContentType>.Some("application/octet-stream");
                case "dsw": return Option<ContentType>.Some("text/plain");
                case "dtd": return Option<ContentType>.Some("text/xml");
                case "dtsConfig": return Option<ContentType>.Some("text/xml");
                case "dv": return Option<ContentType>.Some("video/x-dv");
                case "dvi": return Option<ContentType>.Some("application/x-dvi");
                case "dwf": return Option<ContentType>.Some("drawing/x-dwf");
                case "dwg": return Option<ContentType>.Some("application/acad");
                case "dwp": return Option<ContentType>.Some("application/octet-stream");
                case "dxf": return Option<ContentType>.Some("application/x-dxf");
                case "dxr": return Option<ContentType>.Some("application/x-director");
                case "eml": return Option<ContentType>.Some("message/rfc822");
                case "emz": return Option<ContentType>.Some("application/octet-stream");
                case "eot": return Option<ContentType>.Some("application/vnd.ms-fontobject");
                case "eps": return Option<ContentType>.Some("application/postscript");
                case "etl": return Option<ContentType>.Some("application/etl");
                case "etx": return Option<ContentType>.Some("text/x-setext");
                case "evy": return Option<ContentType>.Some("application/envoy");
                case "exe": return Option<ContentType>.Some("application/octet-stream");
                case "exe.config": return Option<ContentType>.Some("text/xml");
                case "fdf": return Option<ContentType>.Some("application/vnd.fdf");
                case "fif": return Option<ContentType>.Some("application/fractals");
                case "filters": return Option<ContentType>.Some("application/xml");
                case "fla": return Option<ContentType>.Some("application/octet-stream");
                case "flac": return Option<ContentType>.Some("audio/flac");
                case "flr": return Option<ContentType>.Some("x-world/x-vrml");
                case "flv": return Option<ContentType>.Some("video/x-flv");
                case "fsscript": return Option<ContentType>.Some("application/fsharp-script");
                case "fsx": return Option<ContentType>.Some("application/fsharp-script");
                case "generictest": return Option<ContentType>.Some("application/xml");
                case "gif": return Option<ContentType>.Some("image/gif");
                case "gpx": return Option<ContentType>.Some("application/gpx+xml");
                case "group": return Option<ContentType>.Some("text/x-ms-group");
                case "gsm": return Option<ContentType>.Some("audio/x-gsm");
                case "gtar": return Option<ContentType>.Some("application/x-gtar");
                case "gz": return Option<ContentType>.Some("application/x-gzip");
                case "h": return Option<ContentType>.Some("text/plain");
                case "hdf": return Option<ContentType>.Some("application/x-hdf");
                case "hdml": return Option<ContentType>.Some("text/x-hdml");
                case "hhc": return Option<ContentType>.Some("application/x-oleobject");
                case "hhk": return Option<ContentType>.Some("application/octet-stream");
                case "hhp": return Option<ContentType>.Some("application/octet-stream");
                case "hlp": return Option<ContentType>.Some("application/winhlp");
                case "hpp": return Option<ContentType>.Some("text/plain");
                case "hqx": return Option<ContentType>.Some("application/mac-binhex40");
                case "hta": return Option<ContentType>.Some("application/hta");
                case "htc": return Option<ContentType>.Some("text/x-component");
                case "htm": return Option<ContentType>.Some("text/html");
                case "html": return Option<ContentType>.Some("text/html");
                case "htt": return Option<ContentType>.Some("text/webviewhtml");
                case "hxa": return Option<ContentType>.Some("application/xml");
                case "hxc": return Option<ContentType>.Some("application/xml");
                case "hxd": return Option<ContentType>.Some("application/octet-stream");
                case "hxe": return Option<ContentType>.Some("application/xml");
                case "hxf": return Option<ContentType>.Some("application/xml");
                case "hxh": return Option<ContentType>.Some("application/octet-stream");
                case "hxi": return Option<ContentType>.Some("application/octet-stream");
                case "hxk": return Option<ContentType>.Some("application/xml");
                case "hxq": return Option<ContentType>.Some("application/octet-stream");
                case "hxr": return Option<ContentType>.Some("application/octet-stream");
                case "hxs": return Option<ContentType>.Some("application/octet-stream");
                case "hxt": return Option<ContentType>.Some("text/html");
                case "hxv": return Option<ContentType>.Some("application/xml");
                case "hxw": return Option<ContentType>.Some("application/octet-stream");
                case "hxx": return Option<ContentType>.Some("text/plain");
                case "i": return Option<ContentType>.Some("text/plain");
                case "ico": return Option<ContentType>.Some("image/x-icon");
                case "ics": return Option<ContentType>.Some("application/octet-stream");
                case "idl": return Option<ContentType>.Some("text/plain");
                case "ief": return Option<ContentType>.Some("image/ief");
                case "iii": return Option<ContentType>.Some("application/x-iphone");
                case "inc": return Option<ContentType>.Some("text/plain");
                case "inf": return Option<ContentType>.Some("application/octet-stream");
                case "ini": return Option<ContentType>.Some("text/plain");
                case "inl": return Option<ContentType>.Some("text/plain");
                case "ins": return Option<ContentType>.Some("application/x-internet-signup");
                case "ipa": return Option<ContentType>.Some("application/x-itunes-ipa");
                case "ipg": return Option<ContentType>.Some("application/x-itunes-ipg");
                case "ipproj": return Option<ContentType>.Some("text/plain");
                case "ipsw": return Option<ContentType>.Some("application/x-itunes-ipsw");
                case "iqy": return Option<ContentType>.Some("text/x-ms-iqy");
                case "isp": return Option<ContentType>.Some("application/x-internet-signup");
                case "ite": return Option<ContentType>.Some("application/x-itunes-ite");
                case "itlp": return Option<ContentType>.Some("application/x-itunes-itlp");
                case "itms": return Option<ContentType>.Some("application/x-itunes-itms");
                case "itpc": return Option<ContentType>.Some("application/x-itunes-itpc");
                case "IVF": return Option<ContentType>.Some("video/x-ivf");
                case "jar": return Option<ContentType>.Some("application/java-archive");
                case "java": return Option<ContentType>.Some("application/octet-stream");
                case "jck": return Option<ContentType>.Some("application/liquidmotion");
                case "jcz": return Option<ContentType>.Some("application/liquidmotion");
                case "jfif": return Option<ContentType>.Some("image/pjpeg");
                case "jnlp": return Option<ContentType>.Some("application/x-java-jnlp-file");
                case "jpb": return Option<ContentType>.Some("application/octet-stream");
                case "jpe": return Option<ContentType>.Some("image/jpeg");
                case "jpeg": return Option<ContentType>.Some("image/jpeg");
                case "jpg": return Option<ContentType>.Some("image/jpeg");
                case "js": return Option<ContentType>.Some("application/javascript");
                case "json": return Option<ContentType>.Some("application/json");
                case "jsx": return Option<ContentType>.Some("text/jscript");
                case "jsxbin": return Option<ContentType>.Some("text/plain");
                case "latex": return Option<ContentType>.Some("application/x-latex");
                case "library-ms": return Option<ContentType>.Some("application/windows-library+xml");
                case "lit": return Option<ContentType>.Some("application/x-ms-reader");
                case "loadtest": return Option<ContentType>.Some("application/xml");
                case "lpk": return Option<ContentType>.Some("application/octet-stream");
                case "lsf": return Option<ContentType>.Some("video/x-la-asf");
                case "lst": return Option<ContentType>.Some("text/plain");
                case "lsx": return Option<ContentType>.Some("video/x-la-asf");
                case "lzh": return Option<ContentType>.Some("application/octet-stream");
                case "m13": return Option<ContentType>.Some("application/x-msmediaview");
                case "m14": return Option<ContentType>.Some("application/x-msmediaview");
                case "m1v": return Option<ContentType>.Some("video/mpeg");
                case "m2t": return Option<ContentType>.Some("video/vnd.dlna.mpeg-tts");
                case "m2ts": return Option<ContentType>.Some("video/vnd.dlna.mpeg-tts");
                case "m2v": return Option<ContentType>.Some("video/mpeg");
                case "m3u": return Option<ContentType>.Some("audio/x-mpegurl");
                case "m3u8": return Option<ContentType>.Some("audio/x-mpegurl");
                case "m4a": return Option<ContentType>.Some("audio/m4a");
                case "m4b": return Option<ContentType>.Some("audio/m4b");
                case "m4p": return Option<ContentType>.Some("audio/m4p");
                case "m4r": return Option<ContentType>.Some("audio/x-m4r");
                case "m4v": return Option<ContentType>.Some("video/x-m4v");
                case "mac": return Option<ContentType>.Some("image/x-macpaint");
                case "mak": return Option<ContentType>.Some("text/plain");
                case "man": return Option<ContentType>.Some("application/x-troff-man");
                case "manifest": return Option<ContentType>.Some("application/x-ms-manifest");
                case "map": return Option<ContentType>.Some("text/plain");
                case "master": return Option<ContentType>.Some("application/xml");
                case "mbox": return Option<ContentType>.Some("application/mbox");
                case "mda": return Option<ContentType>.Some("application/msaccess");
                case "mdb": return Option<ContentType>.Some("application/x-msaccess");
                case "mde": return Option<ContentType>.Some("application/msaccess");
                case "mdp": return Option<ContentType>.Some("application/octet-stream");
                case "me": return Option<ContentType>.Some("application/x-troff-me");
                case "mfp": return Option<ContentType>.Some("application/x-shockwave-flash");
                case "mht": return Option<ContentType>.Some("message/rfc822");
                case "mhtml": return Option<ContentType>.Some("message/rfc822");
                case "mid": return Option<ContentType>.Some("audio/mid");
                case "midi": return Option<ContentType>.Some("audio/mid");
                case "mix": return Option<ContentType>.Some("application/octet-stream");
                case "mk": return Option<ContentType>.Some("text/plain");
                case "mk3d": return Option<ContentType>.Some("video/x-matroska-3d");
                case "mka": return Option<ContentType>.Some("audio/x-matroska");
                case "mkv": return Option<ContentType>.Some("video/x-matroska");
                case "mmf": return Option<ContentType>.Some("application/x-smaf");
                case "mno": return Option<ContentType>.Some("text/xml");
                case "mny": return Option<ContentType>.Some("application/x-msmoney");
                case "mod": return Option<ContentType>.Some("video/mpeg");
                case "mov": return Option<ContentType>.Some("video/quicktime");
                case "movie": return Option<ContentType>.Some("video/x-sgi-movie");
                case "mp2": return Option<ContentType>.Some("video/mpeg");
                case "mp2v": return Option<ContentType>.Some("video/mpeg");
                case "mp3": return Option<ContentType>.Some("audio/mpeg");
                case "mp4": return Option<ContentType>.Some("video/mp4");
                case "mp4v": return Option<ContentType>.Some("video/mp4");
                case "mpa": return Option<ContentType>.Some("video/mpeg");
                case "mpe": return Option<ContentType>.Some("video/mpeg");
                case "mpeg": return Option<ContentType>.Some("video/mpeg");
                case "mpf": return Option<ContentType>.Some("application/vnd.ms-mediapackage");
                case "mpg": return Option<ContentType>.Some("video/mpeg");
                case "mpp": return Option<ContentType>.Some("application/vnd.ms-project");
                case "mpv2": return Option<ContentType>.Some("video/mpeg");
                case "mqv": return Option<ContentType>.Some("video/quicktime");
                case "ms": return Option<ContentType>.Some("application/x-troff-ms");
                case "msg": return Option<ContentType>.Some("application/vnd.ms-outlook");
                case "msi": return Option<ContentType>.Some("application/octet-stream");
                case "mso": return Option<ContentType>.Some("application/octet-stream");
                case "mts": return Option<ContentType>.Some("video/vnd.dlna.mpeg-tts");
                case "mtx": return Option<ContentType>.Some("application/xml");
                case "mvb": return Option<ContentType>.Some("application/x-msmediaview");
                case "mvc": return Option<ContentType>.Some("application/x-miva-compiled");
                case "mxp": return Option<ContentType>.Some("application/x-mmxp");
                case "nc": return Option<ContentType>.Some("application/x-netcdf");
                case "nsc": return Option<ContentType>.Some("video/x-ms-asf");
                case "nws": return Option<ContentType>.Some("message/rfc822");
                case "ocx": return Option<ContentType>.Some("application/octet-stream");
                case "oda": return Option<ContentType>.Some("application/oda");
                case "odb": return Option<ContentType>.Some("application/vnd.oasis.opendocument.database");
                case "odc": return Option<ContentType>.Some("application/vnd.oasis.opendocument.chart");
                case "odf": return Option<ContentType>.Some("application/vnd.oasis.opendocument.formula");
                case "odg": return Option<ContentType>.Some("application/vnd.oasis.opendocument.graphics");
                case "odh": return Option<ContentType>.Some("text/plain");
                case "odi": return Option<ContentType>.Some("application/vnd.oasis.opendocument.image");
                case "odl": return Option<ContentType>.Some("text/plain");
                case "odm": return Option<ContentType>.Some("application/vnd.oasis.opendocument.text-master");
                case "odp": return Option<ContentType>.Some("application/vnd.oasis.opendocument.presentation");
                case "ods": return Option<ContentType>.Some("application/vnd.oasis.opendocument.spreadsheet");
                case "odt": return Option<ContentType>.Some("application/vnd.oasis.opendocument.text");
                case "oga": return Option<ContentType>.Some("audio/ogg");
                case "ogg": return Option<ContentType>.Some("audio/ogg");
                case "ogv": return Option<ContentType>.Some("video/ogg");
                case "ogx": return Option<ContentType>.Some("application/ogg");
                case "one": return Option<ContentType>.Some("application/onenote");
                case "onea": return Option<ContentType>.Some("application/onenote");
                case "onepkg": return Option<ContentType>.Some("application/onenote");
                case "onetmp": return Option<ContentType>.Some("application/onenote");
                case "onetoc": return Option<ContentType>.Some("application/onenote");
                case "onetoc2": return Option<ContentType>.Some("application/onenote");
                case "opus": return Option<ContentType>.Some("audio/ogg");
                case "orderedtest": return Option<ContentType>.Some("application/xml");
                case "osdx": return Option<ContentType>.Some("application/opensearchdescription+xml");
                case "otf": return Option<ContentType>.Some("application/font-sfnt");
                case "otg": return Option<ContentType>.Some("application/vnd.oasis.opendocument.graphics-template");
                case "oth": return Option<ContentType>.Some("application/vnd.oasis.opendocument.text-web");
                case "otp": return Option<ContentType>.Some("application/vnd.oasis.opendocument.presentation-template");
                case "ots": return Option<ContentType>.Some("application/vnd.oasis.opendocument.spreadsheet-template");
                case "ott": return Option<ContentType>.Some("application/vnd.oasis.opendocument.text-template");
                case "oxt": return Option<ContentType>.Some("application/vnd.openofficeorg.extension");
                case "p10": return Option<ContentType>.Some("application/pkcs10");
                case "p12": return Option<ContentType>.Some("application/x-pkcs12");
                case "p7b": return Option<ContentType>.Some("application/x-pkcs7-certificates");
                case "p7c": return Option<ContentType>.Some("application/pkcs7-mime");
                case "p7m": return Option<ContentType>.Some("application/pkcs7-mime");
                case "p7r": return Option<ContentType>.Some("application/x-pkcs7-certreqresp");
                case "p7s": return Option<ContentType>.Some("application/pkcs7-signature");
                case "pbm": return Option<ContentType>.Some("image/x-portable-bitmap");
                case "pcast": return Option<ContentType>.Some("application/x-podcast");
                case "pct": return Option<ContentType>.Some("image/pict");
                case "pcx": return Option<ContentType>.Some("application/octet-stream");
                case "pcz": return Option<ContentType>.Some("application/octet-stream");
                case "pdf": return Option<ContentType>.Some("application/pdf");
                case "pfb": return Option<ContentType>.Some("application/octet-stream");
                case "pfm": return Option<ContentType>.Some("application/octet-stream");
                case "pfx": return Option<ContentType>.Some("application/x-pkcs12");
                case "pgm": return Option<ContentType>.Some("image/x-portable-graymap");
                case "pic": return Option<ContentType>.Some("image/pict");
                case "pict": return Option<ContentType>.Some("image/pict");
                case "pkgdef": return Option<ContentType>.Some("text/plain");
                case "pkgundef": return Option<ContentType>.Some("text/plain");
                case "pko": return Option<ContentType>.Some("application/vnd.ms-pki.pko");
                case "pls": return Option<ContentType>.Some("audio/scpls");
                case "pma": return Option<ContentType>.Some("application/x-perfmon");
                case "pmc": return Option<ContentType>.Some("application/x-perfmon");
                case "pml": return Option<ContentType>.Some("application/x-perfmon");
                case "pmr": return Option<ContentType>.Some("application/x-perfmon");
                case "pmw": return Option<ContentType>.Some("application/x-perfmon");
                case "png": return Option<ContentType>.Some("image/png");
                case "pnm": return Option<ContentType>.Some("image/x-portable-anymap");
                case "pnt": return Option<ContentType>.Some("image/x-macpaint");
                case "pntg": return Option<ContentType>.Some("image/x-macpaint");
                case "pnz": return Option<ContentType>.Some("image/png");
                case "pot": return Option<ContentType>.Some("application/vnd.ms-powerpoint");
                case "potm": return Option<ContentType>.Some("application/vnd.ms-powerpoint.template.macroEnabled.12");
                case "potx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.presentationml.template");
                case "ppa": return Option<ContentType>.Some("application/vnd.ms-powerpoint");
                case "ppam": return Option<ContentType>.Some("application/vnd.ms-powerpoint.addin.macroEnabled.12");
                case "ppm": return Option<ContentType>.Some("image/x-portable-pixmap");
                case "pps": return Option<ContentType>.Some("application/vnd.ms-powerpoint");
                case "ppsm": return Option<ContentType>.Some("application/vnd.ms-powerpoint.slideshow.macroEnabled.12");
                case "ppsx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.presentationml.slideshow");
                case "ppt": return Option<ContentType>.Some("application/vnd.ms-powerpoint");
                case "pptm": return Option<ContentType>.Some("application/vnd.ms-powerpoint.presentation.macroEnabled.12");
                case "pptx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.presentationml.presentation");
                case "prf": return Option<ContentType>.Some("application/pics-rules");
                case "prm": return Option<ContentType>.Some("application/octet-stream");
                case "prx": return Option<ContentType>.Some("application/octet-stream");
                case "ps": return Option<ContentType>.Some("application/postscript");
                case "psc1": return Option<ContentType>.Some("application/PowerShell");
                case "psd": return Option<ContentType>.Some("application/octet-stream");
                case "psess": return Option<ContentType>.Some("application/xml");
                case "psm": return Option<ContentType>.Some("application/octet-stream");
                case "psp": return Option<ContentType>.Some("application/octet-stream");
                case "pst": return Option<ContentType>.Some("application/vnd.ms-outlook");
                case "pub": return Option<ContentType>.Some("application/x-mspublisher");
                case "pwz": return Option<ContentType>.Some("application/vnd.ms-powerpoint");
                case "qht": return Option<ContentType>.Some("text/x-html-insertion");
                case "qhtm": return Option<ContentType>.Some("text/x-html-insertion");
                case "qt": return Option<ContentType>.Some("video/quicktime");
                case "qti": return Option<ContentType>.Some("image/x-quicktime");
                case "qtif": return Option<ContentType>.Some("image/x-quicktime");
                case "qtl": return Option<ContentType>.Some("application/x-quicktimeplayer");
                case "qxd": return Option<ContentType>.Some("application/octet-stream");
                case "ra": return Option<ContentType>.Some("audio/x-pn-realaudio");
                case "ram": return Option<ContentType>.Some("audio/x-pn-realaudio");
                case "rar": return Option<ContentType>.Some("application/x-rar-compressed");
                case "ras": return Option<ContentType>.Some("image/x-cmu-raster");
                case "rat": return Option<ContentType>.Some("application/rat-file");
                case "rc": return Option<ContentType>.Some("text/plain");
                case "rc2": return Option<ContentType>.Some("text/plain");
                case "rct": return Option<ContentType>.Some("text/plain");
                case "rdlc": return Option<ContentType>.Some("application/xml");
                case "reg": return Option<ContentType>.Some("text/plain");
                case "resx": return Option<ContentType>.Some("application/xml");
                case "rf": return Option<ContentType>.Some("image/vnd.rn-realflash");
                case "rgb": return Option<ContentType>.Some("image/x-rgb");
                case "rgs": return Option<ContentType>.Some("text/plain");
                case "rm": return Option<ContentType>.Some("application/vnd.rn-realmedia");
                case "rmi": return Option<ContentType>.Some("audio/mid");
                case "rmp": return Option<ContentType>.Some("application/vnd.rn-rn_music_package");
                case "roff": return Option<ContentType>.Some("application/x-troff");
                case "rpm": return Option<ContentType>.Some("audio/x-pn-realaudio-plugin");
                case "rqy": return Option<ContentType>.Some("text/x-ms-rqy");
                case "rtf": return Option<ContentType>.Some("application/rtf");
                case "rtx": return Option<ContentType>.Some("text/richtext");
                case "rvt": return Option<ContentType>.Some("application/octet-stream");
                case "ruleset": return Option<ContentType>.Some("application/xml");
                case "s": return Option<ContentType>.Some("text/plain");
                case "safariextz": return Option<ContentType>.Some("application/x-safari-safariextz");
                case "scd": return Option<ContentType>.Some("application/x-msschedule");
                case "scr": return Option<ContentType>.Some("text/plain");
                case "sct": return Option<ContentType>.Some("text/scriptlet");
                case "sd2": return Option<ContentType>.Some("audio/x-sd2");
                case "sdp": return Option<ContentType>.Some("application/sdp");
                case "sea": return Option<ContentType>.Some("application/octet-stream");
                case "searchConnector-ms": return Option<ContentType>.Some("application/windows-search-connector+xml");
                case "setpay": return Option<ContentType>.Some("application/set-payment-initiation");
                case "setreg": return Option<ContentType>.Some("application/set-registration-initiation");
                case "settings": return Option<ContentType>.Some("application/xml");
                case "sgimb": return Option<ContentType>.Some("application/x-sgimb");
                case "sgml": return Option<ContentType>.Some("text/sgml");
                case "sh": return Option<ContentType>.Some("application/x-sh");
                case "shar": return Option<ContentType>.Some("application/x-shar");
                case "shtml": return Option<ContentType>.Some("text/html");
                case "sit": return Option<ContentType>.Some("application/x-stuffit");
                case "sitemap": return Option<ContentType>.Some("application/xml");
                case "skin": return Option<ContentType>.Some("application/xml");
                case "skp": return Option<ContentType>.Some("application/x-koan");
                case "sldm": return Option<ContentType>.Some("application/vnd.ms-powerpoint.slide.macroEnabled.12");
                case "sldx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.presentationml.slide");
                case "slk": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "sln": return Option<ContentType>.Some("text/plain");
                case "slupkg-ms": return Option<ContentType>.Some("application/x-ms-license");
                case "smd": return Option<ContentType>.Some("audio/x-smd");
                case "smi": return Option<ContentType>.Some("application/octet-stream");
                case "smx": return Option<ContentType>.Some("audio/x-smd");
                case "smz": return Option<ContentType>.Some("audio/x-smd");
                case "snd": return Option<ContentType>.Some("audio/basic");
                case "snippet": return Option<ContentType>.Some("application/xml");
                case "snp": return Option<ContentType>.Some("application/octet-stream");
                case "sol": return Option<ContentType>.Some("text/plain");
                case "sor": return Option<ContentType>.Some("text/plain");
                case "spc": return Option<ContentType>.Some("application/x-pkcs7-certificates");
                case "spl": return Option<ContentType>.Some("application/futuresplash");
                case "spx": return Option<ContentType>.Some("audio/ogg");
                case "src": return Option<ContentType>.Some("application/x-wais-source");
                case "srf": return Option<ContentType>.Some("text/plain");
                case "SSISDeploymentManifest": return Option<ContentType>.Some("text/xml");
                case "ssm": return Option<ContentType>.Some("application/streamingmedia");
                case "sst": return Option<ContentType>.Some("application/vnd.ms-pki.certstore");
                case "stl": return Option<ContentType>.Some("application/vnd.ms-pki.stl");
                case "sv4cpio": return Option<ContentType>.Some("application/x-sv4cpio");
                case "sv4crc": return Option<ContentType>.Some("application/x-sv4crc");
                case "svc": return Option<ContentType>.Some("application/xml");
                case "svg": return Option<ContentType>.Some("image/svg+xml");
                case "swf": return Option<ContentType>.Some("application/x-shockwave-flash");
                case "step": return Option<ContentType>.Some("application/step");
                case "stp": return Option<ContentType>.Some("application/step");
                case "t": return Option<ContentType>.Some("application/x-troff");
                case "tar": return Option<ContentType>.Some("application/x-tar");
                case "tcl": return Option<ContentType>.Some("application/x-tcl");
                case "testrunconfig": return Option<ContentType>.Some("application/xml");
                case "testsettings": return Option<ContentType>.Some("application/xml");
                case "tex": return Option<ContentType>.Some("application/x-tex");
                case "texi": return Option<ContentType>.Some("application/x-texinfo");
                case "texinfo": return Option<ContentType>.Some("application/x-texinfo");
                case "tgz": return Option<ContentType>.Some("application/x-compressed");
                case "thmx": return Option<ContentType>.Some("application/vnd.ms-officetheme");
                case "thn": return Option<ContentType>.Some("application/octet-stream");
                case "tif": return Option<ContentType>.Some("image/tiff");
                case "tiff": return Option<ContentType>.Some("image/tiff");
                case "tlh": return Option<ContentType>.Some("text/plain");
                case "tli": return Option<ContentType>.Some("text/plain");
                case "toc": return Option<ContentType>.Some("application/octet-stream");
                case "tr": return Option<ContentType>.Some("application/x-troff");
                case "trm": return Option<ContentType>.Some("application/x-msterminal");
                case "trx": return Option<ContentType>.Some("application/xml");
                case "ts": return Option<ContentType>.Some("video/vnd.dlna.mpeg-tts");
                case "tsv": return Option<ContentType>.Some("text/tab-separated-values");
                case "ttf": return Option<ContentType>.Some("application/font-sfnt");
                case "tts": return Option<ContentType>.Some("video/vnd.dlna.mpeg-tts");
                case "txt": return Option<ContentType>.Some("text/plain");
                case "u32": return Option<ContentType>.Some("application/octet-stream");
                case "uls": return Option<ContentType>.Some("text/iuls");
                case "user": return Option<ContentType>.Some("text/plain");
                case "ustar": return Option<ContentType>.Some("application/x-ustar");
                case "vb": return Option<ContentType>.Some("text/plain");
                case "vbdproj": return Option<ContentType>.Some("text/plain");
                case "vbk": return Option<ContentType>.Some("video/mpeg");
                case "vbproj": return Option<ContentType>.Some("text/plain");
                case "vbs": return Option<ContentType>.Some("text/vbscript");
                case "vcf": return Option<ContentType>.Some("text/x-vcard");
                case "vcproj": return Option<ContentType>.Some("application/xml");
                case "vcs": return Option<ContentType>.Some("text/plain");
                case "vcxproj": return Option<ContentType>.Some("application/xml");
                case "vddproj": return Option<ContentType>.Some("text/plain");
                case "vdp": return Option<ContentType>.Some("text/plain");
                case "vdproj": return Option<ContentType>.Some("text/plain");
                case "vdx": return Option<ContentType>.Some("application/vnd.ms-visio.viewer");
                case "vml": return Option<ContentType>.Some("text/xml");
                case "vscontent": return Option<ContentType>.Some("application/xml");
                case "vsct": return Option<ContentType>.Some("text/xml");
                case "vsd": return Option<ContentType>.Some("application/vnd.visio");
                case "vsi": return Option<ContentType>.Some("application/ms-vsi");
                case "vsix": return Option<ContentType>.Some("application/vsix");
                case "vsixlangpack": return Option<ContentType>.Some("text/xml");
                case "vsixmanifest": return Option<ContentType>.Some("text/xml");
                case "vsmdi": return Option<ContentType>.Some("application/xml");
                case "vspscc": return Option<ContentType>.Some("text/plain");
                case "vss": return Option<ContentType>.Some("application/vnd.visio");
                case "vsscc": return Option<ContentType>.Some("text/plain");
                case "vssettings": return Option<ContentType>.Some("text/xml");
                case "vssscc": return Option<ContentType>.Some("text/plain");
                case "vst": return Option<ContentType>.Some("application/vnd.visio");
                case "vstemplate": return Option<ContentType>.Some("text/xml");
                case "vsto": return Option<ContentType>.Some("application/x-ms-vsto");
                case "vsw": return Option<ContentType>.Some("application/vnd.visio");
                case "vsx": return Option<ContentType>.Some("application/vnd.visio");
                case "vtt": return Option<ContentType>.Some("text/vtt");
                case "vtx": return Option<ContentType>.Some("application/vnd.visio");
                case "wasm": return Option<ContentType>.Some("application/wasm");
                case "wav": return Option<ContentType>.Some("audio/wav");
                case "wave": return Option<ContentType>.Some("audio/wav");
                case "wax": return Option<ContentType>.Some("audio/x-ms-wax");
                case "wbk": return Option<ContentType>.Some("application/msword");
                case "wbmp": return Option<ContentType>.Some("image/vnd.wap.wbmp");
                case "wcm": return Option<ContentType>.Some("application/vnd.ms-works");
                case "wdb": return Option<ContentType>.Some("application/vnd.ms-works");
                case "wdp": return Option<ContentType>.Some("image/vnd.ms-photo");
                case "webarchive": return Option<ContentType>.Some("application/x-safari-webarchive");
                case "webm": return Option<ContentType>.Some("video/webm");
                case "webp": return Option<ContentType>.Some("image/webp"); /* https://en.wikipedia.org/wiki/WebP */
                case "webtest": return Option<ContentType>.Some("application/xml");
                case "wiq": return Option<ContentType>.Some("application/xml");
                case "wiz": return Option<ContentType>.Some("application/msword");
                case "wks": return Option<ContentType>.Some("application/vnd.ms-works");
                case "WLMP": return Option<ContentType>.Some("application/wlmoviemaker");
                case "wlpginstall": return Option<ContentType>.Some("application/x-wlpg-detect");
                case "wlpginstall3": return Option<ContentType>.Some("application/x-wlpg3-detect");
                case "wm": return Option<ContentType>.Some("video/x-ms-wm");
                case "wma": return Option<ContentType>.Some("audio/x-ms-wma");
                case "wmd": return Option<ContentType>.Some("application/x-ms-wmd");
                case "wmf": return Option<ContentType>.Some("application/x-msmetafile");
                case "wml": return Option<ContentType>.Some("text/vnd.wap.wml");
                case "wmlc": return Option<ContentType>.Some("application/vnd.wap.wmlc");
                case "wmls": return Option<ContentType>.Some("text/vnd.wap.wmlscript");
                case "wmlsc": return Option<ContentType>.Some("application/vnd.wap.wmlscriptc");
                case "wmp": return Option<ContentType>.Some("video/x-ms-wmp");
                case "wmv": return Option<ContentType>.Some("video/x-ms-wmv");
                case "wmx": return Option<ContentType>.Some("video/x-ms-wmx");
                case "wmz": return Option<ContentType>.Some("application/x-ms-wmz");
                case "woff": return Option<ContentType>.Some("application/font-woff");
                case "woff2": return Option<ContentType>.Some("application/font-woff2");
                case "wpl": return Option<ContentType>.Some("application/vnd.ms-wpl");
                case "wps": return Option<ContentType>.Some("application/vnd.ms-works");
                case "wri": return Option<ContentType>.Some("application/x-mswrite");
                case "wrl": return Option<ContentType>.Some("x-world/x-vrml");
                case "wrz": return Option<ContentType>.Some("x-world/x-vrml");
                case "wsc": return Option<ContentType>.Some("text/scriptlet");
                case "wsdl": return Option<ContentType>.Some("text/xml");
                case "wvx": return Option<ContentType>.Some("video/x-ms-wvx");
                case "x": return Option<ContentType>.Some("application/directx");
                case "xaf": return Option<ContentType>.Some("x-world/x-vrml");
                case "xaml": return Option<ContentType>.Some("application/xaml+xml");
                case "xap": return Option<ContentType>.Some("application/x-silverlight-app");
                case "xbap": return Option<ContentType>.Some("application/x-ms-xbap");
                case "xbm": return Option<ContentType>.Some("image/x-xbitmap");
                case "xdr": return Option<ContentType>.Some("text/plain");
                case "xht": return Option<ContentType>.Some("application/xhtml+xml");
                case "xhtml": return Option<ContentType>.Some("application/xhtml+xml");
                case "xla": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xlam": return Option<ContentType>.Some("application/vnd.ms-excel.addin.macroEnabled.12");
                case "xlc": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xld": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xlk": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xll": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xlm": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xls": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xlsb": return Option<ContentType>.Some("application/vnd.ms-excel.sheet.binary.macroEnabled.12");
                case "xlsm": return Option<ContentType>.Some("application/vnd.ms-excel.sheet.macroEnabled.12");
                case "xlsx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                case "xlt": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xltm": return Option<ContentType>.Some("application/vnd.ms-excel.template.macroEnabled.12");
                case "xltx": return Option<ContentType>.Some("application/vnd.openxmlformats-officedocument.spreadsheetml.template");
                case "xlw": return Option<ContentType>.Some("application/vnd.ms-excel");
                case "xml": return Option<ContentType>.Some("text/xml");
                case "xmp": return Option<ContentType>.Some("application/octet-stream");
                case "xmta": return Option<ContentType>.Some("application/xml");
                case "xof": return Option<ContentType>.Some("x-world/x-vrml");
                case "XOML": return Option<ContentType>.Some("text/plain");
                case "xpm": return Option<ContentType>.Some("image/x-xpixmap");
                case "xps": return Option<ContentType>.Some("application/vnd.ms-xpsdocument");
                case "xrm-ms": return Option<ContentType>.Some("text/xml");
                case "xsc": return Option<ContentType>.Some("application/xml");
                case "xsd": return Option<ContentType>.Some("text/xml");
                case "xsf": return Option<ContentType>.Some("text/xml");
                case "xsl": return Option<ContentType>.Some("text/xml");
                case "xslt": return Option<ContentType>.Some("text/xml");
                case "xsn": return Option<ContentType>.Some("application/octet-stream");
                case "xss": return Option<ContentType>.Some("application/xml");
                case "xspf": return Option<ContentType>.Some("application/xspf+xml");
                case "xtp": return Option<ContentType>.Some("application/octet-stream");
                case "xwd": return Option<ContentType>.Some("image/x-xwindowdump");
                case "z": return Option<ContentType>.Some("application/x-compress");
                case "zip": return Option<ContentType>.Some("application/zip");
            }

            return Option<ContentType>.None();
        }
    }
}
