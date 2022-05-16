using System.Reflection;
using System.Text.RegularExpressions;

namespace Noctus.Application.Modules.AccountGen.Outlook
{
    public static class OutlookConstants
    {
        public static class Keys
        {
            public static string PhoneNumber = "pn";
            public static string PhoneOperationCode = "pnOperationCode";
            public static string PhoneNumberLastFourDigits = "pnLastFourDigit";
            public static string PhoneNumberCountryCode = "pCC";

            public static string ChallengeType = "challengeType";
            public static string HipCaptchaSolution = "captchaSolvedToken";
            public static string HipSmsSolution = "captchaSmsCode";
            public static string HipId = "hipHid";
            public static string HipSId = "sid";
            public static string HipUrl = "hipUrl";
            public static string FId = "fid";

            public static string CipherPass = "cipherPass";
            public static string EncryptKey = "encryptKey";
            public static string RandomNum = "randomNum";

            public static string IsElevatedLogged = "isLoggedPva";
            public static string UaId = "uaid";
        }

        public static class RegexPatterns
        {
            public static Regex RsaKey = new("(?:Key=\"(.*?)\";)(?=.*randomNum=\"(.*?)\";)");
            public static Regex SmsHip = new("(?:\"hipToken\":\\s?\"(.+?)\")(?=.*\"hipChallengeUrl\":\"(.+?)\")");
            public static Regex PPFT = new("PPFT\".*value=\"(.*?)\"");
            public static Regex EncryptedNetId = new("[\"]*encryptedNetId[\"]*:\\s*[\"'](.*?)[\"']");
            public static Regex ManagePageCanary = new("[\"]*apiCanary[\"]*:\\s*[\"'](.*?)[\"']");
            public static Regex UrlPost = new("urlPost:\\s*[\"'](.*?)[\"']");
            public static Regex MobileNumE = new("\"data\":\\s*\"(.*?)\"");
            public static Regex FlowToken = new("sFT:\\s*[\"'](.*?)[\"']");
            public static Regex FourDigitCode = new("\\d{4}");
            public static Regex SevenDigitCode = new("\\d{7}");
            public static Regex TwoLetterUsernameRecoveryMail = new("([a-z])([a-z])\\*{5}");
        }

        public static class Website
        {
            public const string SignUpUrl = "https://signup.live.com/signup?lic=1";
            public const string HipUrl = "https://client.hip.live.com/";
            public const string CheckAvailabilityUrl = "https://signup.live.com/API/CheckAvailableSigninNames?lic=1";
            public const string CreateAccountUrl = "https://signup.live.com/API/CreateAccount?lic=1";
            public const string SetAliasUrl = "https://account.live.com/AddAssocId";
            public const string OneTimeCodeUrl = "https://login.live.com/pp1600/GetOneTimeCode.srf?id=38936";
            public const string GenerateRecoveryUrl = "https://account.live.com/API/Proofs/GenerateRecoveryCode";
            public const string ManageUrl = "https://account.live.com/proofs/Manage/additional";
            public const string ManageUrl2 = "https://account.live.com/proofs/manage/additional?refd=account.microsoft.com&refp=security&client_flight=m365.suiteheader";
            public const string AddProofUrl =
                "https://account.live.com/proofs/Add?mpcxt=CATB&ru=https://login.live.com/login.srf";
            public const string MailboxUrl = "https://outlook.live.com/mail/0/inbox";
            public const string ForwardingUrl =
                "https://outlook.live.com/owa/0/service.svc?action=NewInboxRule&app=Mail";
            public const string SafeSenderUrl = "https://outlook.live.com/owa/0/service.svc?action=SetMailboxJunkEmailConfiguration&app=Mail";

            public const string LoginManageAccountQueryString =
                "wa=wsignin1.0&wp=SA_20MIN&wreply=https://account.live.com/proofs/manage/additional?refd=account.microsoft.com&refp=security&client_flight=m365.suiteheader";

            public const string MailboxLoginQueryString = "wa=wsignin1.0&wreply=https://outlook.live.com/owa/?refd=account.microsoft.com&wp=MBI_SSL";

            public const string CaptchaEnforcement = "1041";

            public const string HipPhoneEnforcement = "1042";

            public const string PublicKey = "4B8F32B06B3633468A617C4D5781E6B301099447";

            public const string CaptchaKey = "B7D8911C-5CC8-A9A3-35B0-554ACEE604DA";

            public const string MicrosoftSecurityEmail = "account-security-noreply@accountprotection.microsoft.com";

            /**
             * Encrypt data on MS account
             * @param {*} r ???
             * @param {*} a proof code
             * @param {*} t type
             * @param {*} e new password
             * @param {*} n key
             * @param {*} o randomNum
             */
            public static string MsEncryptionFunction = "function Encrypt(r,a,t,e,n,o){var u=[];switch(t.toLowerCase()){case\"chgsqsa\":if(null==r||null==a)return null;u=PackageSAData(r,a);break;case\"chgpwd\":if(null==r||null==e)return null;u=PackageNewAndOldPwd(r,e);break;case\"pwd\":if(null==r)return null;u=PackagePwdOnly(r);break;case\"pin\":if(null==r)return null;u=PackagePinOnly(r);break;case\"proof\":if(null==r&&null==a)return null;u=PackageLoginIntData(null!=r?r:a);break;case\"saproof\":if(null==a)return null;u=PackageSADataForProof(a);break;case\"newpwd\":if(null==e)return null;u=PackageNewPwdOnly(e)}if(null==u||void 0===u)return u;if(void 0!==n&&void 0!==parseRSAKeyFromString)var i=parseRSAKeyFromString(n);return RSAEncrypt(u,i,o)}function PackageSAData(r,a){var t=[],e=0;t[e++]=1,t[e++]=1,t[e++]=0;var n,o=a.length;for(t[e++]=2*o,n=0;o>n;n++)t[e++]=255&a.charCodeAt(n),t[e++]=(65280&a.charCodeAt(n))>>8;var u=r.length;for(t[e++]=u,n=0;u>n;n++)t[e++]=127&r.charCodeAt(n);return t}function PackagePwdOnly(r){var a=[],t=0;a[t++]=1,a[t++]=1,a[t++]=0,a[t++]=0;var e,n=r.length;for(a[t++]=n,e=0;n>e;e++)a[t++]=127&r.charCodeAt(e);return a}function PackagePinOnly(r){var a=[],t=0;a[t++]=1,a[t++]=2,a[t++]=0,a[t++]=0,a[t++]=0;var e,n=r.length;for(a[t++]=n,e=0;n>e;e++)a[t++]=127&r.charCodeAt(e);return a}function PackageLoginIntData(r){var a,t=[],e=0;for(a=0;a<r.length;a++)t[e++]=255&r.charCodeAt(a),t[e++]=(65280&r.charCodeAt(a))>>8;return t}function PackageSADataForProof(r){var a,t=[],e=0;for(a=0;a<r.length;a++)t[e++]=127&r.charCodeAt(a),t[e++]=(65280&r.charCodeAt(a))>>8;return t}function PackageNewPwdOnly(r){var a=[],t=0;a[t++]=1,a[t++]=1;var e,n=r.length;for(a[t++]=n,e=0;n>e;e++)a[t++]=127&r.charCodeAt(e);return a[t++]=0,a[t++]=0,a}function PackageNewAndOldPwd(r,a){var t=[],e=0;t[e++]=1,t[e++]=1;var n,o=a.length;for(t[e++]=o,n=0;o>n;n++)t[e++]=127&a.charCodeAt(n);for(t[e++]=0,o=r.length,t[e++]=o,n=0;o>n;n++)t[e++]=127&r.charCodeAt(n);return t}function mapByteToBase64(r){return r>=0&&26>r?String.fromCharCode(65+r):r>=26&&52>r?String.fromCharCode(97+r-26):r>=52&&62>r?String.fromCharCode(48+r-52):62==r?'+':'/'}function base64Encode(r,a){var t,e='';for(t=a;4>t;t++)r>>=6;for(t=0;a>t;t++)e=mapByteToBase64(63&r)+e,r>>=6;return e}function byteArrayToBase64(r){var a,t,e=r.length,n='';for(a=e-3;a>=0;a-=3)n+=base64Encode(t=r[a]|r[a+1]<<8|r[a+2]<<16,4);var o=e%3;for(t=0,a+=2;a>=0;a--)t=t<<8|r[a];return 1==o?n=n+base64Encode(t<<16,2)+'==':2==o&&(n=n+base64Encode(t<<8,3)+'='),n}function parseRSAKeyFromString(r){var a=r.indexOf(';');if(0>a)return null;var t=r.substr(0,a),e=r.substr(a+1),n=t.indexOf('=');if(0>n)return null;var o=t.substr(n+1);if(0>(n=e.indexOf('=')))return null;var u=e.substr(n+1),i=new Object;return i.n=hexStringToMP(u),i.e=parseInt(o,16),i}function RSAEncrypt(r,a,t){for(var e=[],n=2*a.n.size-42,o=0;o<r.length;o+=n){var u;if(o+n>=r.length)(u=RSAEncryptBlock(r.slice(o),a,t))&&(e=u.concat(e));else(u=RSAEncryptBlock(r.slice(o,o+n),a,t))&&(e=u.concat(e))}return byteArrayToBase64(e)}function RSAEncryptBlock(r,a,t){var e=a.n,n=a.e,o=r.length,u=2*e.size;if(o+42>u)return null;applyPKCSv2Padding(r,u,t);var i=modularExp(byteArrayToMP(r=r.reverse()),n,e);i.size=e.size;var l=mpToByteArray(i);return l.reverse()}function JSMPnumber(){this.size=1,this.data=[],this.data[0]=0}function duplicateMP(r){var a=new JSMPnumber;return a.size=r.size,a.data=r.data.slice(0),a}function byteArrayToMP(r){var a=new JSMPnumber,t=0,e=r.length,n=e>>1;for(t=0;n>t;t++)a.data[t]=r[2*t]+(r[1+2*t]<<8);return e%2&&(a.data[t++]=r[e-1]),a.size=t,a}function mpToByteArray(r){var a=[],t=0,e=r.size;for(t=0;e>t;t++){a[2*t]=255&r.data[t];var n=r.data[t]>>>8;a[2*t+1]=n}return a}function modularExp(r,a,t){for(var e=[],n=0;a>0;)e[n]=1&a,a>>>=1,n++;for(var o=duplicateMP(r),u=n-2;u>=0;u--)o=modularMultiply(o,o,t),1==e[u]&&(o=modularMultiply(o,r,t));return o}function modularMultiply(r,a,t){return divideMP(multiplyMP(r,a),t).r}function multiplyMP(r,a){var t,e,n=new JSMPnumber;for(n.size=r.size+a.size,t=0;t<n.size;t++)n.data[t]=0;var o=r.data,u=a.data,i=n.data;if(r==a){for(t=0;t<r.size;t++)i[2*t]+=o[t]*o[t];for(t=1;t<r.size;t++)for(e=0;t>e;e++)i[t+e]+=2*o[t]*o[e]}else for(t=0;t<r.size;t++)for(e=0;e<a.size;e++)i[t+e]+=o[t]*u[e];return normalizeJSMP(n),n}function normalizeJSMP(r){var a,t,e,n;for(e=r.size,t=0,a=0;e>a;a++)n=r.data[a],n+=t,n-=65536*(t=Math.floor(n/65536)),r.data[a]=n}function removeLeadingZeroes(r){for(var a=r.size-1;a>0&&0==r.data[a--];)r.size--}function divideMP(r,a){var t=r.size,e=a.size,n=a.data[e-1],o=a.data[e-1]+a.data[e-2]/65536,u=new JSMPnumber;u.size=t-e+1,r.data[t]=0;for(var i=t-1;i>=e-1;i--){var l=i-e+1,f=Math.floor((65536*r.data[i+1]+r.data[i])/o);if(f>0){var c=multiplyAndSubtract(r,f,a,l);for(0>c&&multiplyAndSubtract(r,--f,a,l);c>0&&r.data[i]>=n;)(c=multiplyAndSubtract(r,1,a,l))>0&&f++}u.data[l]=f}return removeLeadingZeroes(r),{q:u,r:r}}function multiplyAndSubtract(r,a,t,e){var n,o=r.data.slice(0),u=0,i=r.data;for(n=0;n<t.size;n++){var l=u+t.data[n]*a;(l-=65536*(u=l>>>16))>i[n+e]?(i[n+e]+=65536-l,u++):i[n+e]-=l}return u>0&&(i[n+e]-=u),i[n+e]<0?(r.data=o.slice(0),-1):1}function applyPKCSv2Padding(r,a,t){var e,n=a-r.length-40-2,o=[];for(e=0;n>e;e++)o[e]=0;o[n]=1;var u=[218,57,163,238,94,107,75,13,50,85,191,239,149,96,24,144,175,216,7,9].concat(o,r),i=[];for(e=0;20>e;e++)i[e]=Math.floor(256*Math.random());var l=XORarrays(u,MGF(i=SHA1(i.concat(t)),a-21)),f=XORarrays(i,MGF(l,20)),c=[];for(c[0]=0,c=c.concat(f,l),e=0;e<c.length;e++)r[e]=c[e]}function MGF(r,a){if(a>4096)return null;var t=r.slice(0),e=t.length;t[e++]=0,t[e++]=0,t[e++]=0,t[e]=0;for(var n=0,o=[];o.length<a;)t[e]=n++,o=o.concat(SHA1(t));return o.slice(0,a)}function XORarrays(r,a){if(r.length!=a.length)return null;for(var t=[],e=r.length,n=0;e>n;n++)t[n]=r[n]^a[n];return t}function SHA1(r){var a,t=r.slice(0);PadSHA1Input(t);var e={A:1732584193,B:4023233417,C:2562383102,D:271733878,E:3285377520};for(a=0;a<t.length;a+=64)SHA1RoundFunction(e,t,a);var n=[];return wordToBytes(e.A,n,0),wordToBytes(e.B,n,4),wordToBytes(e.C,n,8),wordToBytes(e.D,n,12),wordToBytes(e.E,n,16),n}function wordToBytes(r,a,t){var e;for(e=3;e>=0;e--)a[t+e]=255&r,r>>>=8}function PadSHA1Input(r){var a,t=r.length,e=t,n=t%64,o=55>n?56:120;for(r[e++]=128,a=n+1;o>a;a++)r[e++]=0;var u=8*t;for(a=1;8>a;a++)r[e+8-a]=255&u,u>>>=8}function SHA1RoundFunction(r,a,t){var e,n,o,u,i=[],l=r.A,f=r.B,c=r.C,d=r.D,s=r.E;for(n=0,o=t;16>n;n++,o+=4)i[n]=a[o]<<24|a[o+1]<<16|a[o+2]<<8|a[o+3]<<0;for(n=16;80>n;n++)i[n]=rotateLeft(i[n-3]^i[n-8]^i[n-14]^i[n-16],1);for(e=0;20>e;e++)u=rotateLeft(l,5)+(f&c|~f&d)+s+i[e]+1518500249&4294967295,s=d,d=c,c=rotateLeft(f,30),f=l,l=u;for(e=20;40>e;e++)u=rotateLeft(l,5)+(f^c^d)+s+i[e]+1859775393&4294967295,s=d,d=c,c=rotateLeft(f,30),f=l,l=u;for(e=40;60>e;e++)u=rotateLeft(l,5)+(f&c|f&d|c&d)+s+i[e]+2400959708&4294967295,s=d,d=c,c=rotateLeft(f,30),f=l,l=u;for(e=60;80>e;e++)u=rotateLeft(l,5)+(f^c^d)+s+i[e]+3395469782&4294967295,s=d,d=c,c=rotateLeft(f,30),f=l,l=u;r.A=r.A+l&4294967295,r.B=r.B+f&4294967295,r.C=r.C+c&4294967295,r.D=r.D+d&4294967295,r.E=r.E+s&4294967295}function rotateLeft(r,a){return(r&(1<<32-a)-1)<<a|r>>>32-a}function hexStringToMP(r){var a,t,e=Math.ceil(r.length/4),n=new JSMPnumber;for(n.size=e,a=0;e>a;a++)t=r.substr(4*a,4),n.data[e-1-a]=parseInt(t,16);return n}";
        }
    }
}
