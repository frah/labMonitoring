using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;

namespace LabMonitoring
{
    /// <summary>
    /// 簡易HTTPサーバ
    /// </summary>
    public sealed class HttpServer : Logger
    {
        /// <summary>
        /// 現在のログを得るためのデリゲートプロパティ
        /// </summary>
        public currentLog CurLog { get; set; }
        private HttpListener listener;
        private readonly object lockObj = new object();

        /// <summary>
        /// サーバをスタートする
        /// </summary>
        public void Start()
        {
            Log("HttpServer starting...: http://localhost:9999/");
            lock (this.lockObj)
            {
                try
                {
                    this.listener = new HttpListener();
                    this.listener.Prefixes.Add("http://*:9999/");
                    this.listener.Start();
                    this.listener.BeginGetContext(this.OnRequested, this.listener);
                }
                catch (HttpListenerException ex)
                {
                    Log("Http server start failed: " + ex.ErrorCode);
                    DebugLog(ex.StackTrace);
                    return;
                }
            }
            Log("HttpServer OK");
        }

        /// <summary>
        /// サーバを停止する
        /// </summary>
        public void Stop()
        {
            lock (this.lockObj)
            {
                this.listener.Close();
            }
            Log("HttpServer stopped");
        }

        /// <summary>
        /// リクエストを受け取った時の処理
        /// </summary>
        /// <param name="result">非同期処理</param>
        public void OnRequested(IAsyncResult result)
        {
            lock (this.lockObj)
            {
                HttpListener listener = (HttpListener)result.AsyncState;
                if (!listener.IsListening)
                {
                    Log("listening finished.");
                    return;
                }

                HttpListenerContext ctx = listener.EndGetContext(result);
                HttpListenerRequest req = null;
                HttpListenerResponse res = null;
                StreamWriter writer = null;

                try
                {
                    req = ctx.Request;
                    res = ctx.Response;

                    if (req.RawUrl.EndsWith("favicon.ico"))
                    {
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                    else
                    {
                        Log(string.Format("Http request is received: [{0}] {1} - {2}", req.UserHostAddress, req.HttpMethod, req.RawUrl));

                        writer = new StreamWriter(res.OutputStream);

                        res.StatusCode = (int)HttpStatusCode.OK;
                        res.ContentEncoding = Encoding.UTF8;
                        string resStr = "";

                        if (CurLog != null)
                        {
                            res.ContentType = "text/plain";
                            resStr = CurLog();
                        }
                        else
                        {
                            res.ContentType = "text/html";
                            resStr = "<h1>No any logs</h1>";
                        }
                        res.ContentLength64 = Encoding.UTF8.GetByteCount(resStr);
                        writer.Write(resStr);
                        writer.Flush();

                        Log(string.Format("Http response is sended: {0} - {1} - {2}bytes", res.StatusCode, res.ContentType, res.ContentLength64));
                    }
                }
                catch (Exception ex)
                {
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    Log(ex.ToString());
                }
                finally
                {
                    try
                    {
                        if (null != writer) writer.Close();
                        if (null != res) res.Close();
                    }
                    catch (Exception ex)
                    {
                        Log(ex.ToString());
                    }
                }

                listener.BeginGetContext(this.OnRequested, listener);
            }
        }
    }
}
