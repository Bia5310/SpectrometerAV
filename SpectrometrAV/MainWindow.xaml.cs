using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AVT.VmbAPINET;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using VimbaCameraNET;

namespace SpectrometrAV
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        VimbaCamera vimbaCamera = null;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            vimbaCamera.FrameReceivedHandler = Camera_FrameReceived;
        }

        private void AutoconnectToFirstCamera()
        {
            try
            {
                if (VimbaCamera.Cameras.Count > 0)
                {
                    vimbaCamera.CloseCamera();
                    //UnsubscribeEvents();

                    vimbaCamera.OpenCamera(VimbaCamera.Cameras[0].Id);
                    //CameraControlsGroup(vimbaCamera.IsOpened);
                    //pixelvars = (int)Math.Pow(2, VimbaCamera.PixelFormatToBits(vimbaCamera.PixelFormat));

                    if (vimbaCamera.IsOpened)
                    {
                        try
                        {
                            //SetupFeaturesControls();
                            //SubscribeEvents();
                        }
                        catch (Exception exc)
                        {
                            //MessageBox.Show(exc.ToString());
                        }

                        vimbaCamera.RestoreFullROI(true);
                        vimbaCamera.StartContiniousAsyncAccusition();
                        //FillTriggerFeatures();

                        //logger.Info("Подключена камера " + vimbaCamera.ToString());
                    }
                }
            }
            catch (Exception exc)
            {
                //LogWrite(exc.Message);
                //logger.Error(exc, "Autoconnect to camera failed");
            }
        }

        /// <summary>
        /// Обработчик события приема кадра
        /// </summary>
        /// <param name="frame"></param>
        private void Camera_FrameReceived(VimbaCamera vCamera, Frame frame)
        {
            Bitmap bitmap = new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format32bppRgb);//GetBitmap8FromFrame(frame); //null;//new Bitmap((int)frame.Width, (int)frame.Height, PixelFormat.Format24bppRgb);
            frame.Fill(bitmap);
            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, (int)frame.Width, (int)frame.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);
            Avalonia.Media.Imaging.Bitmap bmp = new Avalonia.Media.Imaging.Bitmap(Avalonia.Platform.PixelFormat.Rgba8888, bd.Scan0, ,1, bd.Stride);
            viewportControl.imageMain.Source = null;
        }
    }
}
