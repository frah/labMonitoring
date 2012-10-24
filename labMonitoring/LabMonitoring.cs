using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Twitterizer;
using Twitterizer.Streaming;
using OpenCvSharp;

namespace LabMonitoring
{
    delegate void logOutput(string str);

    class Camera
    {
        private const double WIDTH = 640;
        private const double HEIGHT = 480;

        private CvCapture cap;
        private CvHaarClassifierCascade cvHCC;
        private CvMemStorage stor;
        private logOutput l;

        public Camera(logOutput output = null)
        {
            // カメラの用意
            cap = Cv.CreateCameraCapture(0);
            log(cap.CaptureType + ", " + cap.FrameWidth + "x" + cap.FrameHeight + ", " + cap.Mode);

            Cv.SetCaptureProperty(cap, CaptureProperty.FrameWidth, WIDTH);
            Cv.SetCaptureProperty(cap, CaptureProperty.FrameHeight, HEIGHT);

            // 検出器の用意
            cvHCC = Cv.Load<CvHaarClassifierCascade>("haarcascade_profileface.xml");
            stor = Cv.CreateMemStorage(0);

            l = output;
        }

        public IplImage Capture()
        {
            using (IplImage src = Cv.QueryFrame(cap))
            {
                IplImage dst = Cv.CloneImage(src);
                Cv.Zero(dst);

                /* 回転 */
                CvMat rotMat = Cv.CreateMat(2, 3, MatrixType.F32C1);
                CvPoint2D32f center = Cv.Point2D32f(src.Width / 2, src.Height / 2);
                rotMat = Cv._2DRotationMatrix(center, 180, 1.0);
                Cv.WarpAffine(src, dst, rotMat);
                Cv.ReleaseMat(rotMat);

                /* 顔検出 */
                using (IplImage tmp = Cv.CreateImage(dst.Size, BitDepth.U8, 1))
                {
                    Cv.CvtColor(dst, tmp, ColorConversion.BgrToGray);
                    Cv.EqualizeHist(tmp, tmp);
                    CvSeq<CvAvgComp> face = Cv.HaarDetectObjects(tmp, cvHCC, stor);

                    foreach (CvRect rect in face)
                    {
                        log(rect.ToString());
                        Cv.Rectangle(dst, rect, CvColor.Red, 2, LineType.AntiAlias);
                    }
                }

                return dst;
            }
        }

        ~Camera()
        {
            Cv.ReleaseMemStorage(stor);
            Cv.ReleaseHaarClassifierCascade(cvHCC);
            Cv.ReleaseCapture(cap);
        }

        private void log(string str)
        {
            if (l != null)
            {
                l(str);
            }
            else
            {
#if DEBUG
                Console.WriteLine(str);
#endif
            }
        }
    }

    class Twitter
    {
        private OAuthTokens token;
        private TwitterStream stream;
        private logOutput l;

        public Twitter(logOutput output = null)
        {
            token = new OAuthTokens
            {
                ConsumerKey = Properties.Settings.Default.consumerKey,
                ConsumerSecret = Properties.Settings.Default.consumerSecret,
                AccessToken = Properties.Settings.Default.accessToken,
                AccessTokenSecret = Properties.Settings.Default.accessTokenSecret
            };
            stream = new TwitterStream(token, "labMonitor", null);
            l = output;
        }

        public void start()
        {
            try
            {
                stream.StartUserStream(null, 
                    (x) => {l("UserStream stopped: "+x);},
                    new StatusCreatedCallback(onStatus),
                    null, null, null, null);
                
            }
            catch (TwitterizerException ex)
            {
                throw ex;
            }
            //System.Threading.Thread.Sleep(-1);
        }

        public void end()
        {
            stream.EndStream();
        }

        private void onStatus(TwitterStatus target)
        {
            if (!target.Text.StartsWith("@frahabot")) return;
            log("@" + target.User.ScreenName + ": " + target.Text);
            if (!target.Text.Contains("カメラ")) return;

            log("Recieve command tweet");

            Camera cam = new Camera(l);
            StatusUpdateOptions opt = new StatusUpdateOptions();
            opt.InReplyToStatusId = target.Id;
            opt.UseSSL = true;
            /*opt.Latitude = 34.731557;
            opt.Longitude = 135.734187;
            opt.PlacePin = true;*/

            using (IplImage img = cam.Capture())
            using (var b = img.ToBitmap())
            using (MemoryStream ms = new MemoryStream())
            {
#if DEBUG
                using (CvWindow debWin = new CvWindow(img))
                {
                    CvWindow.WaitKey();
                }
#endif
                b.Save(ms, ImageFormat.Png);

                var res = TwitterStatus.UpdateWithMedia(token, "@" + target.User.ScreenName + " 今はこんな状況です", ms.ToArray(), opt);
                if (res.Result.Equals(RequestResult.Success))
                {
                    log("tweet complete");
                }
                else
                {
                    log(res.Result.ToString());
                    log(res.ErrorMessage);
                    log(res.Content);
                }
            }
        }

        private void log(string str)
        {
            if (l != null)
            {
                l(str);
            }
            else
            {
#if DEBUG
                Console.WriteLine(str);
#endif
            }
        }
    }
}

