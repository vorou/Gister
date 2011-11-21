using System;
using System.IO;
using System.Net;
using EchelonTouchInc.Gister.FluentHttp;
using FluentHttp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EchelonTouchInc.Gister.Api
{
    public class HttpGitHubSender : GitHubSender
    {
        public string SendGist(string fileName, string content, string githubusername, string githubpassword)
        {
            var gistAsJson = new GistJson().CreateFrom(fileName, content);

            var response = new FluentHttpRequest()
                .BaseUrl("https://api.github.com")
                .AuthenticateUsing(new HttpBasicAuthenticator(githubusername, githubpassword))
                .ResourcePath("/gists")
                .Method("POST")
                .Headers(h => h.Add("User-Agent", "Gister"))
                .Headers(h => h.Add("Content-Type", "application/json"))
                .Body(x => x.Append(gistAsJson))
                .OnResponseHeadersReceived((o, e) => e.SaveResponseIn(new MemoryStream()))
                .Execute();

            if (response.Response.HttpWebResponse.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException(response.Response.HttpWebResponse.StatusDescription);

            return PeelOutGistHtmlUrl(response);
        }

        private static string PeelOutGistHtmlUrl(FluentHttpAsyncResult response)
        {
            response.Response.SaveStream.Seek(0, SeekOrigin.Begin);
            var gistJson = FluentHttpRequest.ToString(response.Response.SaveStream);

            dynamic gist = JObject.Parse(gistJson);

            return (string) gist.html_url;
        }
    }
}