using AVT.VmbAPINET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VimbaCameraNET
{
    public partial class VimbaCamera
    {
        #region Поля, переменные, объекты

        private static Vimba m_Vimba = null;
        /// <summary>Vimba (одна на все экземпляры класса VimbaCamera)</summary>
        public static Vimba Vimba
        {
            get
            {
                if (m_Vimba == null)
                    throw new NullReferenceException("Vimba is not inited");

                return m_Vimba;
            }
        }

        /// <summary>Сколько экземпляров объекта VimbaCamera было создано</summary>
        private static int instances_created = 0;

        private Camera m_Camera = null;
        /// <summary>VimbaAPI.NET Camera</summary>
        public Camera Camera
        {
            get
            {
                return m_Camera;
            }
        }

        public bool IsOpened
        {
            get { return m_Camera != null; }
        }

        private static bool isVimbaInited = false;
        /// <summary>Инициализирована ли Vimba.API</summary>
        private static bool IsVimbaInited { get { return isVimbaInited; } }

        private long payloadSize;

        private bool m_Accusition = false;
        /// <summary>Флаг показывает, идет ли в данный момент захват кадров или нет</summary>
        public bool Accusition
        {
            get { return m_Accusition; }
            private set
            {
                m_Accusition = value;
                OnAccusitionChanged?.Invoke(m_Accusition);
            }
        }

        public CameraFeatureExposure Exposure = null;
        public CameraFeatureGain Gain = null;
        public CameraFeatureGamma Gamma = null;
        public CameraFeatureWidth Width = null;
        public CameraFeatureOffsetX OffsetX = null;
        public CameraFeatureHeight Height = null;
        public CameraFeatureOffsetY OffsetY = null;
        public CameraFeatureBinningX BinningX = null;
        public CameraFeatureBinningY BinningY = null;
        public CameraFeatureFrameRate StatFrameRate = null;
        public CameraFeatureBlackLevel BlackLevel = null;
        public CameraFeatureAcquisitionLimit AcqisitionLimit = null;

        public string PixelFormat
        {
            get
            {
                try
                {
                    return Camera.Features["PixelFormat"].EnumValue;
                }
                catch(Exception exc)
                {
                    LogWrite(exc.Message);
                    return "";
                }
            }
            set
            {
                try
                {
                    Camera.Features["PixelFormat"].EnumValue = value;
                }
                catch(Exception exc)
                {
                    LogWrite(exc.Message);
                }
            }
        }

        #endregion

        #region Делегаты и события
        public delegate void OnFrameReceivedHandler(VimbaCamera camera, Frame frame);
        /// <summary>Вызывается при получении кадра</summary>
        public OnFrameReceivedHandler FrameReceivedHandler = null;

        /// <summary>Вызывается при подключении/отключении камеры</summary>
        public static event Vimba.OnCameraListChangeHandler OnCameraListChanged = null;

        /// <summary>Вызывается при подключении/отключении камеры</summary>
        public static event Vimba.OnInterfaceListChangeHandler OnInterfaceListChangedHandler = null;

        public delegate void OnLogMessageHandler(string message);
        /// <summary>Подпишитесь на это событие, чтобы получать сообщения от VimbaCamera</summary>
        public static event OnLogMessageHandler OnLogMessage = null;

        public delegate void OnAccusitionChangedHandler(bool accusition);
        /// <summary>Вызывается при изменении Accusition</summary>
        public event OnAccusitionChangedHandler OnAccusitionChanged = null;

        #endregion

        #region Конструкторы, деструктор

        static VimbaCamera()
        {
            StartupVimba();
        }
        public VimbaCamera()
        {
            instances_created++;
        }

        public VimbaCamera(string cameraID)
        {
            instances_created++;
            OpenCamera(cameraID);
        }

        public VimbaCamera(CameraInfo cameraInfo)
        {
            instances_created++;
            OpenCamera(cameraInfo.ID);
        }

        ~VimbaCamera() //Деструктор
        {
            instances_created--;
            try
            {
                CloseCamera();

                if (instances_created == 0)
                    if (m_Vimba != null)
                    {
                        m_Vimba.Shutdown();
                        m_Vimba = null;
                        isVimbaInited = false;
                    }
            }
            catch (Exception) { }
        }

        #endregion

        #region Функции

        /// <summary>Инициализирует Vimba. Достаточно быть вызван один раз перед началом работы с камерами</summary>
        /// <returns>Is Vimba inited?</returns>
        public static bool StartupVimba()
        {
            try
            {
                if (m_Vimba == null)
                {
                    m_Vimba = new Vimba();
                    m_Vimba.Startup();
                    isVimbaInited = true;
                }
            }
            catch (Exception)
            {
                m_Vimba = null;
                isVimbaInited = false;
            }

            return isVimbaInited;
        }

        public static bool ShutdownVimba()
        {
            try
            {
                if(m_Vimba != null)
                {
                    m_Vimba.Shutdown();
                    m_Vimba = null;
                    isVimbaInited = false;
                    return !isVimbaInited;
                }
            }
            catch(Exception) { }

            return !isVimbaInited;
        }

        /// <summary>Коллекция всех подключенных камер</summary>
        public static CameraCollection Cameras
        {
            get
            {
                if (m_Vimba == null)
                    throw new NullReferenceException("Vimba not inited");
                lock(m_Vimba.Cameras.SyncRoot)
                    return m_Vimba.Cameras;
            }
        }

        public static InterfaceCollection Interfaces
        {
            get
            {
                if (m_Vimba == null)
                    throw new NullReferenceException("Vimba not inited");
                lock(m_Vimba.Interfaces.SyncRoot)
                    return m_Vimba.Interfaces;
            }
        }

        public static int PixelFormatToBits(string pixelFormat)
        {
            switch(pixelFormat)
            {
                case "Mono16": return 16;
                case "Mono14": return 14;
                case "Mono12": return 12;
                case "Mono10": return 10;
                case "Mono8": return 8;
                default: throw new Exception("Format " + pixelFormat + " not available");
            }
        }

        /// <summary>Открывает камеру</summary>
        /// <param name="ID">ID камеры</param>
        public void OpenCamera(string ID)
        {
            if (m_Camera == null)
            {
                try
                {
                    m_Camera = m_Vimba.GetCameraByID(ID);
                    m_Camera.Open(VmbAccessModeType.VmbAccessModeFull);

                    cameraInfo = new CameraInfo(m_Camera);
                    LogWrite("Камера открыта: " + cameraInfo.ToString());                                                                                                     

                    FeatureCollection features = m_Camera.Features;
                    Feature feature = features["PixelFormat"];

                    try
                    {
                        if(feature.EnumValues.Contains("Mono16") && feature.IsEnumValueAvailable("Mono16"))
                        {
                            feature.EnumValue = "Mono16";
                        }
                        else
                        {
                            if(feature.EnumValues.Contains("Mono14") && feature.IsEnumValueAvailable("Mono14"))
                            {
                                feature.EnumValue = "Mono14";
                            }
                            else
                            {
                                if (feature.EnumValues.Contains("Mono12") && feature.IsEnumValueAvailable("Mono12"))
                                {
                                    feature.EnumValue = "Mono12";
                                }
                                else
                                {
                                    if (feature.EnumValues.Contains("Mono10") && feature.IsEnumValueAvailable("Mono10"))
                                    {
                                        feature.EnumValue = "Mono10";
                                    }
                                    else
                                        throw new Exception();
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        LogWrite("PixelFormats error. " + exc.ToString());
                    }

                    //Инициализируем фичи CameraFeature
                    InitFeatures();

                    // Set the GeV packet size to the highest possible value
                    // (In this example we do not test whether this cam actually is a GigE cam)
                    try
                    {
                        if (m_Camera.InterfaceType == VmbInterfaceType.VmbInterfaceEthernet)
                        {
                            this.m_Camera.Features["GVSPAdjustPacketSize"].RunCommand();
                            while (false == this.m_Camera.Features["GVSPAdjustPacketSize"].IsCommandDone())
                            {
                                // Do Nothing
                            }
                        }
                    }
                    catch
                    {
                        // Do Nothing
                    }

                    try
                    {
                        if(m_Camera.Features.ContainsName("AcquisitionFrameRateEnable"))
                        {
                            m_Camera.Features["AcquisitionFrameRateEnable"].BoolValue = false; //делаем fps зависимым от экспозиции и других параметров
                        }
                    }
                    catch(Exception exc)
                    {

                    }
                }
                catch(VimbaException vexc)
                {
                    m_Camera = null;
                    LogWrite(vexc.Message);
                }
                catch (Exception exc)
                {
                    LogWrite(exc.Message);
                }
            }
        }

        /// <summary>Закрывает камеру</summary>
        public void CloseCamera()
        {
            if (m_Camera != null)
            {
                try
                {
                    if (m_Accusition)
                    {
                        StopContiniousAsyncAccusition();
                    }
                }
                catch (Exception exc)
                {
                    LogWrite(exc.Message);
                }

                try
                {
                    m_Camera.Close();
                    cameraInfo = new CameraInfo(null);
                    LogWrite("Камера закрыта");
                }
                catch(Exception exc)
                {
                    LogWrite(exc.Message);
                }

                m_Camera = null;
            }
        }

        private void InitFeatures()
        {
            if(m_Camera != null)
            {
                Exposure = new CameraFeatureExposure(m_Camera);
                Gain = new CameraFeatureGain(m_Camera);
                Gamma = new CameraFeatureGamma(m_Camera);
                Width = new CameraFeatureWidth(m_Camera);
                Height = new CameraFeatureHeight(m_Camera);
                OffsetX = new CameraFeatureOffsetX(m_Camera);
                OffsetY = new CameraFeatureOffsetY(m_Camera);
                BinningX = new CameraFeatureBinningX(m_Camera);
                BinningY = new CameraFeatureBinningY(m_Camera);
                StatFrameRate = new CameraFeatureFrameRate(m_Camera);
                BlackLevel = new CameraFeatureBlackLevel(m_Camera);
                AcqisitionLimit = new CameraFeatureAcquisitionLimit(m_Camera);
            }
        }

        private CameraInfo cameraInfo = new CameraInfo(null);
        public CameraInfo CameraInfo
        {
            get
            {
                return cameraInfo;
            }
        }

        public static Camera GetCameraByID(string cameraID)
        {
            if (m_Vimba == null)
                throw new NullReferenceException();

            return m_Vimba.GetCameraByID(cameraID);
        }

        public static Camera GetCameraByID(CameraInfo cameraInfo)
        {
            if (cameraInfo == null)
                throw new NullReferenceException();

            return GetCameraByID(cameraInfo.ID);
        }

        public static Interface GetInterfaceByID(string interfaceID)
        {
            if (m_Vimba == null)
                throw new NullReferenceException("Vimba not inited");

            return m_Vimba.GetInterfaceByID(interfaceID);
        }

        public static VmbVersionInfo_t GetVimbaVersion()
        {
            if (m_Vimba == null)
                throw new NullReferenceException("Vimba not inited");

            return m_Vimba.Version;
        }

        public override string ToString()
        {
            if (m_Camera == null)
                return "Камера не открыта";

            return cameraInfo.ToString();
        }

        private void cameraListChanged(VmbUpdateTriggerType triggerType)
        {
            OnCameraListChanged?.Invoke(triggerType);
        }
        private void onInterfaceListChanged(VmbUpdateTriggerType triggerType)
        {
            OnInterfaceListChangedHandler?.Invoke(triggerType);
        }

        /// <summary>Осуществляет захват нескольких кадров в синхронном режиме</summary>
        /// <param name="frames">Массив, в который будут помещены захваченные кадры. Размер массива - количество захватываемых кадров</param>
        /// <param name="timeout">Таймаут, по истечению которого будет сгенерировано исключение. Относится к одиночному кадру, а не ко всей серии</param>
        public void AcquireMultipleImages(ref Frame[] frames, int timeout)
        {
            m_Camera.AcquireMultipleImages(ref frames, timeout);
        }

        /// <summary>Осуществляет захват нескольких кадров в синхронном режиме</summary>
        /// <param name="frames">Массив, в который будут помещены захваченные кадры. Размер массива - количество захватываемых кадров</param>
        /// <param name="timeout">Таймаут, по истечению которого будет сгенерировано исключение. Относится к одиночному кадру, а не ко всей серии</param>
        /// <param name="numFramesCompleted">Количество кадров с действительными данными</param>
        public void AcquireMultipleImages(ref Frame[] frames, int timeout, ref int numFramesCompleted)
        {
            m_Camera.AcquireMultipleImages(ref frames, timeout, ref numFramesCompleted);
        }

        /// <summary>Захват одиночного кадра</summary>
        /// <param name="frame">Frame, в который будут записаны данные кадра</param>
        /// <param name="timeout">Таймаут, по истечению которого будет сгенерировано исключение</param>
        public void AcquireSingleImage(ref Frame frame, int timeout)
        {
            m_Camera.AcquireSingleImage(ref frame, timeout);
        }

        /// <summary>Начать постоянный асинхронный прием кадров</summary>
        public void StartContiniousAsyncAccusition(bool enableLog = true)
        {
            if (m_Accusition)
            {
                StopContiniousAsyncAccusition();
            }

            if (m_Camera == null)
                throw new NullReferenceException("Camera is NULL");

            try
            {
                FeatureCollection features = m_Camera.Features;
                Feature feature = features["PayloadSize"];
                payloadSize = feature.IntValue;

                Frame[] frameArray = new Frame[3];

                for (int i = 0; i < frameArray.Length; ++i)
                {
                    frameArray[i] = new Frame(payloadSize);
                    m_Camera.AnnounceFrame(frameArray[i]);
                }

                m_Camera.StartCapture();

                for (int i = 0; i < frameArray.Length; ++i)
                {
                    m_Camera.QueueFrame(frameArray[i]);
                }

                if (FrameReceivedHandler != null)
                {
                    m_Camera.OnFrameReceived += frameReceived;
                }

                feature = features["AcquisitionMode"];
                if(feature.EnumValues.Contains("Contineous"))
                    feature.EnumValue = "Continuous";

                feature = features["TriggerSource"];
                if (feature.EnumValues.Contains("Freerun"))
                    feature.EnumValue = "Freerun";

                feature = features["AcquisitionStart"];
                feature.RunCommand();
                do { } while (!feature.IsCommandDone());

                Accusition = true;

                if (enableLog)
                    LogWrite("Асинхронный захват начат");
            }
            catch (Exception exc)
            {
                LogWrite(exc.Message);
                return;
            }
        }



        /// <summary>Остановить постоянный асинхронный прием кадров</summary>
        public void StopContiniousAsyncAccusition(bool enableLog = true)
        {
            if (!m_Accusition)
                return;

            if (m_Camera == null)
                throw new NullReferenceException("Camera is null");

            try
            {
                FeatureCollection features = m_Camera.Features;
                Feature feature = features["AcquisitionStop"];
                feature.RunCommand();
                do { } while (!feature.IsCommandDone());

                if (FrameReceivedHandler != null)
                    m_Camera.OnFrameReceived -= frameReceived;

                m_Camera.EndCapture();
                m_Camera.FlushQueue();
                m_Camera.RevokeAllFrames();
                Accusition = false;

                if(enableLog)
                    LogWrite("Асинхронный захват остановлен");
            }
            catch (Exception exc)
            {
                LogWrite(exc.Message);
            }
        }

        public void RestoreFullROI(bool restoreBinning = true, bool restartAcqusition = true)
        {
            try
            {
                if(restartAcqusition)
                    StopContiniousAsyncAccusition();

                if(restoreBinning)
                {
                    BinningX.Value = BinningX.MinValue;
                    BinningY.Value = BinningY.MinValue;
                }

                OffsetX.Value = OffsetX.MinValue;
                OffsetY.Value = OffsetY.MinValue;

                Width.Value = Width.MaxValue;
                Height.Value = Height.MaxValue;

                if(restartAcqusition)
                    StartContiniousAsyncAccusition();
            }
            catch(Exception exc)
            {
                LogWrite(exc.Message);
            }
        }

        public void SetUpROI(int offsetX, int offsetY, int width, int height)
        {
            if(Camera != null)
            {
                try
                {
                    if(offsetX + width > Width.MaximumWidth || offsetY + height > Height.MaximumHeight || offsetX < 0 || offsetY < 0)
                    {
                        throw new Exception("Неподходящие знчения");
                    }
                    else
                    {
                        RestoreFullROI(false, false);
                        StopContiniousAsyncAccusition();
                        
                        Width.Value = width;
                        Height.Value = height;
                        OffsetX.Value = offsetX;
                        OffsetY.Value = offsetY;

                        StartContiniousAsyncAccusition();
                    }
                }
                catch(Exception exc)
                {
                    LogWrite(exc.Message);
                }
            }
        }

        public void RunSoftwareTrigger()
        {
            try
            {
                Feature f = Camera.Features["TriggerSoftware"];
                f.RunCommand();
                while(!f.IsCommandDone())
                { }
            }
            catch (Exception) { }
        }

        /// <summary>Пишет лог</summary>
        /// <param name="message">Сообщение</param>
        private void LogWrite(string message)
        {
            OnLogMessage?.Invoke(message);
        }

        private void frameReceived(Frame frame)
        {
            FrameReceivedHandler?.Invoke(this, frame);
        }

        #endregion
    }
}
