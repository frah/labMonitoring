using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharkbite.Irc;

namespace LabMonitoring
{
    /// <summary>
    /// IRCからコントロールを受け付けるためのクラス
    /// </summary>
    class IrcController : Logger
    {
        /// <summary>
        /// 現在のログを得るためのデリゲートプロパティ
        /// </summary>
        public currentLog CurLog { get; set; }

        private Connection conn;
        private string server;
        private string channel;
        private string password;
        private string adminNick;

        const string nick = "LabMonitoringBot";
        const string helpMes = "log: show current log\nconf: show current settings\nhelp,?: show this message";

        /// <summary>
        /// IRC接続を初期化する
        /// </summary>
        /// <exception cref="System.ArgumentNullException">IRCサーバの設定が見つからない場合</exception>
        public IrcController()
        {
            server = Properties.Settings.Default.IrcServer;
            channel = Properties.Settings.Default.IrcChannel;
            password = Properties.Settings.Default.IrcPassword;
            adminNick = Properties.Settings.Default.IrcAdminNick;

            if (
                string.IsNullOrEmpty(server) ||
                string.IsNullOrEmpty(channel) ||
                string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(adminNick)
                )
            {
                throw new ArgumentNullException("IRC server settings not found");
            }

            CreateConnection();

            conn.Listener.OnRegistered += new RegisteredEventHandler(OnRegistered);
            conn.Listener.OnPrivate += new PrivateMessageEventHandler(OnPrivate);
            conn.Listener.OnError += new ErrorMessageEventHandler(OnError);
            conn.Listener.OnDisconnected += new DisconnectedEventHandler(OnDisconnected);
        }

        private void CreateConnection()
        {
            Identd.Start(nick);
            conn = new Connection(new ConnectionArgs(nick, server), false, true);
        }

        /// <summary>
        /// 接続を開始する
        /// </summary>
        public void Start()
        {
            try
            {
                Log("Try to connect IRC server... - " + server);
                conn.Connect();
            }
            catch (Exception ex)
            {
                Log("IRC connection failed: " + ex.ToString());
                Identd.Stop();
            }
        }

        /// <summary>
        /// 接続を解除する
        /// </summary>
        public void Stop()
        {
            if (conn.Connected)
                conn.Disconnect("LabMonitoring bot will shutdown. Bye");
        }

        /// <summary>
        /// サーバとの接続が確立した時に呼ばれるコールバック関数
        /// </summary>
        public void OnRegistered()
        {
            try
            {
                Identd.Stop();
                conn.Sender.Join(channel, password);
            }
            catch (Exception ex)
            {
                Log("IRC joinning channel failed: " + ex.ToString());
            }
        }

        /// <summary>
        /// プライベートメッセージ受信時のコールバック関数
        /// 操作本体
        /// </summary>
        /// <param name="user">送信ユーザ</param>
        /// <param name="message">受信メッセージ</param>
        public void OnPrivate(UserInfo user, string message)
        {
            try
            {
                if (user.Nick.Equals(adminNick))
                {
                    switch (message)
                    {
                        case "log":
                            conn.Sender.PrivateMessage(user.Nick, CurLog());
                            break;
                        case "conf":
                            conn.Sender.PrivateMessage(user.Nick, Properties.Settings.Default.Kamatte.ToString());
                            break;
                        default:
                            conn.Sender.PrivateMessage(user.Nick, helpMes);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog("Any errors occurred: " + ex.ToString());
            }
        }

        /// <summary>
        /// エラー発生時のコールバック関数
        /// </summary>
        /// <param name="code">エラーコード</param>
        /// <param name="message">エラーメッセージ</param>
        public void OnError(ReplyCode code, string message)
        {
            DebugLog("An error of type " + code + " due to " + message + " has occurred.");
        }

        /// <summary>
        /// 切断時のコールバック関数
        /// </summary>
        public void OnDisconnected()
        {
            DebugLog("Connection to the server has been closed.");
        }
    }
}
