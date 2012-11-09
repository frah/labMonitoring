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
        private int retryCount = 0;

        const string nick = "LabBot";
        const string helpMes = "log: show current log\r\nconf: show current settings\r\nhelp,?: show this message";

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
            conn.Listener.OnPublic += new PublicMessageEventHandler(Listener_OnPublic);
            conn.Listener.OnPrivate += new PrivateMessageEventHandler(OnPrivate);
            conn.Listener.OnError += new ErrorMessageEventHandler(OnError);
            conn.Listener.OnDisconnected += new DisconnectedEventHandler(OnDisconnected);
            conn.Listener.OnNames += new NamesEventHandler(Listener_OnNames);
        }

        void Listener_OnNames(string channel, string[] nicks, bool last)
        {
            if (last)
            {
                Log("Complete to join channel '" + channel + "'.");
                conn.Sender.ChangeChannelMode(channel, ModeAction.Add, ChannelMode.Password, password);
                conn.Sender.ChangeChannelMode(channel, ModeAction.Add, ChannelMode.ChannelOperator, nick);
            }
        }

        private void CreateConnection()
        {
            Identd.Start(nick);
            conn = new Connection(new ConnectionArgs(nick, server), false, true);
            conn.TextEncoding = Encoding.UTF8;
        }

        private void SendMultilineText(string text)
        {
            string[] texts = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in texts)
            {
                conn.Sender.PublicMessage(channel, t);
            }
        }

        private void SendMultilinePrivateText(string nick, string text)
        {
            string[] texts = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in texts)
            {
                conn.Sender.PrivateMessage(nick, t);
            }
        }

        void Listener_OnPublic(UserInfo user, string channel, string message)
        {
            if (channel.Equals(this.channel))
            {
                try
                {
                    Log("Public IRC command received: " + message);
                    switch (message)
                    {
                        case "log":
                            SendMultilineText(CurLog());
                            break;
                        case "conf":
                            SendMultilineText(Properties.Settings.Default.Kamatte.ToString());
                            break;
                        default:
                            SendMultilineText(helpMes);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    DebugLog("Any errors occurred: " + ex.ToString());
                }
            }
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
                Log("IRC connection complete. Try to join channel " + channel + ".");
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
                    Log("PrivateIRC command received: " + message);
                    switch (message)
                    {
                        case "log":
                            SendMultilinePrivateText(user.Nick, CurLog());
                            break;
                        case "conf":
                            SendMultilinePrivateText(user.Nick, Properties.Settings.Default.Kamatte.ToString());
                            break;
                        default:
                            SendMultilinePrivateText(user.Nick, helpMes);
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
            Log("Connection to the server has been closed.");
            if (++retryCount < 6)
            {
                Log("Retrying... " + retryCount + "/5");
                CreateConnection();
                Start();
            }
            else
            {
                Log("Retrying failed.");
            }
        }
    }
}
