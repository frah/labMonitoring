using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;
using Twitterizer;

namespace LabMonitoring
{
    /// <summary>
    /// カメラ制御用クラス
    /// </summary>
    class Camera : ITweetHandler
    {
        /// <summary>
        /// 画像幅サイズ
        /// </summary>
        private const double WIDTH = 640;
        /// <summary>
        /// 画像縦サイズ
        /// </summary>
        private const double HEIGHT = 480;

        private CvCapture cap;
        private CvHaarClassifierCascade cvHCC;
        private CvMemStorage stor;
        private logOutput l;

        /// <summary>
        /// 1つ目のカメラでカメラ初期化
        /// </summary>
        /// <param name="output"></param>
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

        /// <summary>
        /// 画面をキャプチャしpng形式のバイト配列で出力
        /// </summary>
        /// <returns>ping形式バイト配列</returns>
        public byte[] Capture()
        {
            using (IplImage src = Cv.QueryFrame(cap))
            using (MemoryStream ms = new MemoryStream())
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

                using (var b = dst.ToBitmap())
                {
                    b.Save(ms, ImageFormat.Png);
                }

                return ms.ToArray();
            }
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~Camera()
        {
            Cv.ReleaseMemStorage(stor);
            Cv.ReleaseHaarClassifierCascade(cvHCC);
            Cv.ReleaseCapture(cap);
        }

        /// <summary>
        /// ログ出力用関数
        /// </summary>
        /// <param name="str">出力ログ</param>
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

        public void HandleStatus(TwitterStatus target, logOutput log)
        {
            if (!target.Text.Contains("カメラ")) return;

            log("Recieve command tweet");

            StatusUpdateOptions opt = new StatusUpdateOptions();
            opt.InReplyToStatusId = target.Id;
            opt.UseSSL = true;
            /*opt.Latitude = 34.731557;
            opt.Longitude = 135.734187;
            opt.PlacePin = true;*/

            var res = Twitter.StatusUpdateWithMedia("@" + target.User.ScreenName + " 今はこんな状況です", this.Capture(), opt);
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
}
