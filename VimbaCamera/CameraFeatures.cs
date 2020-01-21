using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVT.VmbAPINET;

namespace VimbaCameraNET
{
    public partial class VimbaCamera
    {
        public enum FeatureType : int { None = 0, Exposure, Gain, Gamma, Width, Height, OffsetX, OffsetY, BinningX, BinningY, FrameRate, BlackLevel, AcquisitionFrameRateLimit };

        public abstract class CameraFeature
        {
            /// <summary>Тип фичи. Exposure, Gain, Gamma...</summary>
            public abstract FeatureType FeatureType{ get; }
            /// <summary>Имя фичи</summary>
            public abstract string Name{ get; }
            //Устанавливаем в конструкторе
            protected Feature f_Feature; //Отвечает за главное. Например: Gain, Width, Exposure, Gamma...

            private bool isReadonly = false;
            ///<summary>Возможно только чтение фичи</summary>
            public bool IsReadonly
            {
                get { return isReadonly; }
            }
            /// <summary>Доступность фичи на данной камере</summary>
            protected bool isAvailable = false;
            /// <summary> Есть ли данная фича у этой камеры или нет </summary>
            public bool IsAvailable{ get { return isAvailable; } }

            public delegate void OnFeatureChangedHandler(FeatureType featureType);
            /// <summary>Вызывается, если какое-то свойство фичи изменилось</summary>
            public virtual event OnFeatureChangedHandler OnFeatureChanged;

            private dynamic featureValue = 0;
            public dynamic Value
            {
                set
                {
                    try
                    {
                        if (f_Feature == null)
                            return;

                        //fit to increment
                        dynamic val = value;
                        if(hasValueIncrement)
                            val = ((int)(value / featureValueIncrement))*featureValueIncrement;

                        switch (f_Feature.DataType)
                        {
                            case VmbFeatureDataType.VmbFeatureDataFloat:
                                f_Feature.FloatValue = val;
                                featureValue = f_Feature.FloatValue;
                                break;
                            case VmbFeatureDataType.VmbFeatureDataInt:
                                f_Feature.IntValue = (int) val;
                                featureValue = f_Feature.IntValue;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                get
                {
                    return featureValue;
                }
            }

            private dynamic maxValue = 0;
            public dynamic MaxValue
            {
                get
                {
                    return maxValue;
                }
            }

            private dynamic minValue = 0;
            public dynamic MinValue
            {
                get
                {
                    return minValue;
                }
            }

            private dynamic featureValueIncrement = 1;
            public dynamic ValueIncrement
            {
                get
                {
                    return featureValueIncrement;
                }
            }

            private bool hasValueIncrement = false;
            public bool HasValueIncrement
            {
                get { return hasValueIncrement; }
            }

            public CameraFeature(Camera camera)
            {
                if (camera == null)
                    throw new NullReferenceException("Camera in null");

                InitNameIndex(camera);

                if (isAvailable)
                {
                    f_Feature = camera.Features[Name];
                    InitFeature();
                    f_Feature.OnFeatureChanged += FeatureChanged;
                }
            }

            protected virtual void InitNameIndex(Camera camera)
            {
                isAvailable = camera.Features.ContainsName(Name);
            }

            private void InitFeature()
            {
                if (f_Feature == null)
                    return;

                try
                {
                    switch (f_Feature.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            featureValue = f_Feature.IntValue;
                            maxValue = f_Feature.IntRangeMax;
                            minValue = f_Feature.IntRangeMin;
                            featureValueIncrement = f_Feature.IntIncrement;
                            hasValueIncrement = true;
                            
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            featureValue = f_Feature.FloatValue;
                            maxValue = f_Feature.FloatRangeMax;
                            minValue = f_Feature.FloatRangeMin;
                            hasValueIncrement = f_Feature.FloatHasIncrement;
                            if (hasValueIncrement)
                                featureValueIncrement = Math.Round(f_Feature.FloatIncrement, 5);
                            else
                                featureValueIncrement = 1;
                            break;
                    }
                    isReadonly = !f_Feature.IsWritable();
                }
                catch (Exception) { }
            }

            /// <summary> Метод подписан на изменение фичи Vimba.Feature</summary>
            protected virtual void FeatureChanged(Feature feature)
            {
                try
                {
                    if (feature.Name == Name)
                    {
                        InitFeature();
                    }
                }
                catch (Exception) { }
                
                OnFeatureChanged?.Invoke(FeatureType);
            }
        }

        public class CameraFeatureExposure : CameraFeature
        {
            private Feature f_ExposureAuto = null; //Например: GainAuto, ExposureAuto
            private string featureAutoName = "ExposureAuto";
            private Feature f_ExposureTimeInc = null;
            private string featureIncName = "ExposureTimeIncrement";

            public override FeatureType FeatureType => FeatureType.Exposure;

            private bool autoExposure = false;

            private dynamic exposureTimeAbsIncrement = 1;
            public dynamic ExposureTimeAbsIncrement
            {
                get
                {
                    return exposureTimeAbsIncrement;
                }
            }

            private string[] names = { "ExposureTimeAbs", "ExposureTime" };
            private int index = 0;

            protected override void InitNameIndex(Camera camera)
            {
                if (camera.Features.ContainsName(names[0]))
                {
                    index = 0;
                    isAvailable = true;
                }
                else
                {
                    if (camera.Features.ContainsName(names[1]))
                    {
                        index = 1;
                        isAvailable = true;
                    }
                    else
                    {
                        isAvailable = false;
                    }
                }
            }

            public override string Name => names[index];
            
            public bool Auto
            {
                set
                {
                    if (f_ExposureAuto == null)
                        return;
                    if (autoExposure == value)
                        return;

                    try
                    {
                        if (value)
                        {
                            if (f_ExposureAuto.IsEnumValueAvailable("Continuous"))
                            {
                                f_ExposureAuto.EnumValue = "Continuous";
                                autoExposure = true;// f_ExposureAuto.EnumValue == "Continuous";
                            }
                        }
                        else
                        {
                            if (f_ExposureAuto.IsEnumValueAvailable("Off"))
                            {
                                f_ExposureAuto.EnumValue = "Off";
                                autoExposure = false;// !(f_ExposureAuto.EnumValue == "Off");
                            }
                        }
                    }
                    catch(Exception) { }
                }
                get
                {
                    return autoExposure;
                }
            }

            public CameraFeatureExposure(Camera camera) : base(camera)
            {
                if(camera.Features.ContainsName(featureIncName))
                {
                    f_ExposureTimeInc = camera.Features[featureIncName];
                    InitExposureInc();
                    f_ExposureTimeInc.OnFeatureChanged += FeatureChanged;
                }
                if(camera.Features.ContainsName(featureAutoName))
                {
                    f_ExposureAuto = camera.Features[featureAutoName];
                    InitExposureAuto();
                    f_ExposureAuto.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitExposureAuto()
            {
                if (f_ExposureAuto == null)
                    return;

                try
                {
                    if (f_ExposureAuto.EnumValue == "Off" || f_ExposureAuto.EnumValue == "Once")
                    {
                        autoExposure = false;
                    }
                    else
                    {
                        autoExposure = true;
                    }
                }
                catch (Exception) { }
            }

            private void InitExposureInc()
            {
                if (f_ExposureTimeInc == null)
                    return;

                try
                {
                    switch (f_ExposureTimeInc.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            exposureTimeAbsIncrement = f_ExposureTimeInc.IntValue;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            exposureTimeAbsIncrement = f_ExposureTimeInc.FloatValue;
                            break;
                    }
                }
                catch (Exception) { }
            }

            protected override void FeatureChanged(Feature feature)
            {
                try
                {
                    if (feature.Name == Name)
                        base.FeatureChanged(feature);
                    if (feature.Name == featureIncName)
                        InitExposureInc();
                    if (feature.Name == featureAutoName)
                        InitExposureAuto();
                }
                catch (Exception) { }
            }
        }

        public class CameraFeatureGain : CameraFeature
        {
            private Feature f_GainAuto = null;

            private string[] names = { "Gain", "GainRaw" };
            private int index = 0;

            protected override void InitNameIndex(Camera camera)
            {
                if (camera.Features.ContainsName(names[0]))
                {
                    index = 0;
                    isAvailable = true;
                }
                else
                    if (camera.Features.ContainsName(names[1]))
                    {
                        index = 1;
                        isAvailable = true;
                    }
                    else
                    {
                        isAvailable = false;
                    }
            }

            private string featureAutoName = "GainAuto";

            public override string Name
            {
                get
                {
                    return names[index];
                }
            }

            public override FeatureType FeatureType => FeatureType.Gain;

            private bool autoGain = false;

            public bool Auto
            {
                set
                {
                    if (f_GainAuto == null)
                        return;
                    if (autoGain == value)
                        return;

                    try
                    {
                        if (value) //if true
                        {
                            if (f_GainAuto.IsEnumValueAvailable("Continuous"))
                            {
                                f_GainAuto.EnumValue = "Continuous";
                                autoGain = true;
                            }
                        }
                        else
                        {
                            if (f_GainAuto.IsEnumValueAvailable("Off"))
                            {
                                f_GainAuto.EnumValue = "Off";
                                autoGain = false;
                            }
                        }
                    }
                    catch (Exception) { }
                }
                get
                {
                    return autoGain;
                }
            }

            public CameraFeatureGain(Camera camera) : base(camera)
            {
                if(camera.Features.ContainsName(featureAutoName))
                {
                    f_GainAuto = camera.Features[featureAutoName];
                    InitGainAuto();
                    f_GainAuto.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitGainAuto()
            {
                if (f_GainAuto == null)
                    return;
                try
                {
                    if (f_GainAuto.EnumValue == "Off" || f_GainAuto.EnumValue == "Once")
                    {
                        autoGain = false;
                    }
                    else
                    {
                        autoGain = true;
                    }
                }
                catch(Exception) { }
            }

            protected override void FeatureChanged(Feature feature)
            {
                try
                {
                    if (feature.Name == featureAutoName)
                        InitGainAuto();
                    else
                        base.FeatureChanged(feature);
                }
                catch (Exception) { }
            }
        }

        public class CameraFeatureBlackLevel : CameraFeature
        {
            public override string Name => "BlackLevel";

            public override FeatureType FeatureType => FeatureType.BlackLevel;

            public CameraFeatureBlackLevel(Camera camera) : base(camera) { }
        }

        public class CameraFeatureGamma : CameraFeature
        {
            public override string Name => "Gamma";

            public override FeatureType FeatureType => FeatureType.Gamma;

            public CameraFeatureGamma(Camera camera) : base(camera) { }
        }

        public class CameraFeatureWidth : CameraFeature
        {
            private Feature f_WidthMax = null;
            private string featureWidthMaxName = "WidthMax";

            public override string Name => "Width";

            public override FeatureType FeatureType => FeatureType.Width;

            private dynamic maxWidth = 0;
            public dynamic MaximumWidth
            {
                get
                {
                    return maxWidth;
                }
            }

            public CameraFeatureWidth(Camera camera) : base(camera)
            {
                if (camera.Features.ContainsName(featureWidthMaxName))
                {
                    f_WidthMax = camera.Features[featureWidthMaxName];
                    InitWidthMax();
                    f_WidthMax.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitWidthMax()
            {
                if (f_WidthMax == null)
                    return;

                try
                {
                    switch (f_WidthMax.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            maxWidth = f_WidthMax.IntValue;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            maxWidth = f_WidthMax.FloatValue;
                            break;
                    }
                }
                catch (Exception) { }
            }

            protected override void FeatureChanged(Feature feature)
            {
                try
                {
                    if (feature.Name == Name)
                        base.FeatureChanged(feature);
                    if (feature.Name == featureWidthMaxName)
                        InitWidthMax();
                }
                catch (Exception) { }
            }
        }

        public class CameraFeatureHeight : CameraFeature
        {
            private Feature f_HeightMax = null;
            private string featureHeightMaxName = "HeightMax";

            public override string Name => "Height";

            public override FeatureType FeatureType => FeatureType.Height;

            private dynamic maxHeight = 0;
            public dynamic MaximumHeight
            {
                get
                {
                    return maxHeight;
                }
            }

            public CameraFeatureHeight(Camera camera) : base(camera)
            {
                if (camera.Features.ContainsName(featureHeightMaxName))
                {
                    f_HeightMax = camera.Features[featureHeightMaxName];
                    InitHeightMax();
                    f_HeightMax.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitHeightMax()
            {
                if (f_HeightMax == null)
                    return;

                try
                {
                    switch (f_HeightMax.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            maxHeight = f_HeightMax.IntValue;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            maxHeight = f_HeightMax.FloatValue;
                            break;
                    }
                }
                catch (Exception) { }
            }

            protected override void FeatureChanged(Feature feature)
            {
                try
                {
                    if (feature.Name == Name)
                        base.FeatureChanged(feature);
                    if (feature.Name == featureHeightMaxName)
                        InitHeightMax();
                }
                catch (Exception) { }
            }
        }

        public class CameraFeatureOffsetX : CameraFeature
        {
            public override string Name => "OffsetX";

            public override FeatureType FeatureType => FeatureType.OffsetX;

            public CameraFeatureOffsetX(Camera camera) : base(camera) { }
        }

        public class CameraFeatureOffsetY : CameraFeature
        {
            public override string Name => "OffsetY";

            public override FeatureType FeatureType => FeatureType.OffsetY;

            public CameraFeatureOffsetY(Camera camera) : base(camera) { }
        }

        public class CameraFeatureBinningX : CameraFeature
        {
            public override string Name => "BinningHorizontal";

            public override FeatureType FeatureType => FeatureType.BinningX;

            public CameraFeatureBinningX(Camera camera) : base(camera) { }
        }

        public class CameraFeatureBinningY : CameraFeature
        {
            public override string Name => "BinningVertical";

            public override FeatureType FeatureType => FeatureType.BinningY;

            public CameraFeatureBinningY(Camera camera) : base(camera) { }
        }

        public class CameraFeatureAcquisitionLimit : CameraFeature
        {
            public override string Name => "AcquisitionFrameRateLimit";

            public override FeatureType FeatureType => FeatureType.AcquisitionFrameRateLimit;

            public CameraFeatureAcquisitionLimit(Camera camera) : base(camera) { }
        }

        public class CameraFeatureFrameRate : CameraFeature
        {
            private string[] names = { "StatFrameRate", "AcquisitionFrameRate" };
            private int index = 0;

            protected override void InitNameIndex(Camera camera)
            {
                if (camera.Features.ContainsName(names[0]))
                {
                    index = 0;
                    isAvailable = true;
                }
                else
                    if (camera.Features.ContainsName(names[1]))
                    {
                        index = 1;
                        isAvailable = true;
                    }
                    else
                    {
                        isAvailable = false;
                    }
            }

            public override string Name
            {
                get
                {
                    return names[index];
                }
            }

            public override FeatureType FeatureType => FeatureType.FrameRate;

            public CameraFeatureFrameRate(Camera camera) : base(camera) { }
        }
    }
}
