#region
using System;
using System.Collections.Generic;
using System.Net;
#endregion
namespace ProcessDashboard
{
    public class DataSetLocationResolver
    {
        private const string LowerPrefixes = "=.-/!";

        public void ResolveFromToken(string token, out string serverUrl, out string datasetId)
        {
            // the token is designed to be easy for users to type, and to be resilient to typos
            // (such as improper capitalization, extraneous space characters, or confusion between
            // 0 and O). Unpack that token to produce a URL to the Google URL shortening service.
            var shortGoogleUrl = ToUrl(token);

            // contact Google and retrieve the long URL that this short URL maps to.
            var longUrl = LookupUrl(shortGoogleUrl);

            // extract relevant info from the long URL.
            ResolveFromUri(longUrl, out serverUrl, out datasetId);
        }

        public void ResolveFromUri(Uri uri, out string serverUrl, out string datasetId)
        {
            // parse the long URL, and extract relevant info from it.
            //var p = HttpUtility.ParseQueryString(uri.Query);

            var p = ParseQueryString(uri.Query);
            serverUrl = p["serverUrl"];
            datasetId = p["datasetId"];
            if (serverUrl == null || datasetId == null)
                throw new ArgumentException("Unrecognized token");
        }

        public Dictionary<string, string> ParseQueryString(string query)
        {
            if (query.Length == 0)
                return null;

            string decoded = WebUtility.HtmlDecode(query);
            int decodedLength = decoded.Length;
            int namePos = 0;
            bool first = true;
            Dictionary<string, string> result = new Dictionary<string, string>();
            while (namePos <= decodedLength)
            {
                int valuePos = -1, valueEnd = -1;
                for (int q = namePos; q < decodedLength; q++)
                {
                    if (valuePos == -1 && decoded[q] == '=')
                    {
                        valuePos = q + 1;
                    }
                    else if (decoded[q] == '&')
                    {
                        valueEnd = q;
                        break;
                    }
                }

                if (first)
                {
                    first = false;
                    if (decoded[namePos] == '?')
                        namePos++;
                }

                string name, value;
                if (valuePos == -1)
                {
                    name = null;
                    valuePos = namePos;
                }
                else
                {
                    name = WebUtility.UrlDecode(decoded.Substring(namePos, valuePos - namePos - 1));
                }
                if (valueEnd < 0)
                {
                    namePos = -1;
                    valueEnd = decoded.Length;
                }
                else
                {
                    namePos = valueEnd + 1;
                }
                value = WebUtility.UrlDecode(decoded.Substring(valuePos, valueEnd - valuePos));

                result.Add(name, value);
                if (namePos == -1)
                    break;
            }
            return result;
        }

        private string ToUrl(string token)
        {
            token = token.Trim().ToUpper().Replace('0', 'O').Replace('@', '0').Replace(',', '.');
            if (token.Length < 2 || token[0] != 'G')
                throw new ArgumentException("Unrecognized token scheme");

            var result = "http://goo.gl/";
            var numLower = 0;
            var literal = false;
            foreach (var c in token.Substring(1))
            {
                if (literal)
                {
                    result += c;
                    literal = false;
                }
                else if ('A' <= c && c <= 'Z')
                {
                    if (numLower > 0)
                    {
                        result += char.ToLower(c);
                        numLower--;
                    }
                    else
                    {
                        result += c;
                    }
                }
                else if ('0' <= c && c <= '9')
                {
                    result += c;
                }
                else if (c == '=')
                {
                    literal = true;
                }
                else if (numLower <= 0)
                {
                    numLower = LowerPrefixes.IndexOf(c);
                }
            }
            return result;
        }

        private Uri LookupUrl(string shortUrl)
        {
			var req = WebRequest.CreateHttp(shortUrl);
			req.Method = "HEAD";
			req.AllowAutoRedirect = false;
			var resp = (HttpWebResponse)req.GetResponse();
			return new Uri(resp.Headers["Location"]);
        }
    }
}