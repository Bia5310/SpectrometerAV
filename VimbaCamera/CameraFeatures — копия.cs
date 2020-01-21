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
        public enum FeatureType : int { None = 0, Exposure = 1, Gain = 2, Gamma = 3 };

        public abstract class CameraFeature
        {
            protected FeatureType featureType = FeatureType.None;

            /// <summary>Тип фичи. Exposure, Gain, Gamma...</summary>
            public FeatureType FeatureType
            {
                get
                {
                    return featureType;
                }
            }


            protected string f_Name;

            protected bool isAvailable = false;
            public bool IsAvailable
            {
                get { return isAvailable; }
            }

            public override string ToString()
            {
                return f_Name;
            }

            public delegate void OnFeatureChangedHandler(FeatureType featureType);
            /// <summary>Вызывается, если какое-то свойство фичи изменилось</summary>
            public abstract event OnFeatureChangedHandler onFeatureChanged;

            /// <summary> Метож подписан на изменение фичи Vimba.Feature</summary>
            /// <param name="feature"></param>
            protected abstract void FeatureChanged(Feature feature);

            public CameraFeature(Camera camera)
            {
                if (camera == null)
                    throw new NullReferenceException("Camera in null");
            }
        }

        public class CameraFeatureExposure : CameraFeature
        {
            Feature f_ExposureTimeAbs = null;
            Feature f_ExposureAuto = null;
            Feature f_ExposureTimeInc = null;

            private dynamic exposureTimeAbs = 0;
            public dynamic ExposureTimeAbs
            {
                set
                {
                    try
                    {
                        if (f_ExposureTimeAbs == null)
                            return;

                        switch (f_ExposureTimeAbs.DataType)
                        {
                            case VmbFeatureDataType.VmbFeatureDataFloat:
                                f_ExposureTimeAbs.FloatValue = value;
                                exposureTimeAbs = f_ExposureTimeAbs.FloatValue;
                                break;
                            case VmbFeatureDataType.VmbFeatureDataInt:
                                f_ExposureTimeAbs.IntValue = value;
                                exposureTimeAbs = f_ExposureTimeAbs.IntValue;
                                break;
                        }
                    }
                    catch(Exception ex){}
                }
                get
                {
                    return exposureTimeAbs;
                }
            }

            private dynamic maxTimeAbs = 0;
            public dynamic MaxTimeAbs
            {
                get
                {
                    return maxTimeAbs;
                }
            }

            private dynamic minTimeAbs = 0;
            public dynamic MinTimeAbs
            {
                get
                {
                    return minTimeAbs;
                }
            }

            private dynamic exposureTimeAbsIncrement = 1;
            public dynamic ExposureTimeAbsIncrement
            {
                get
                {
                    return exposureTimeAbsIncrement;
                }
            }

            private bool hasTimeAbsIncrement = false;
            public bool HasTimeAbsIncrement
            {
                get { return hasTimeAbsIncrement; }
            }

            private bool autoExposure = false;

            public override event OnFeatureChangedHandler onFeatureChanged;

            public bool Auto
            {
                set
                {
                    if (f_ExposureAuto == null)
                        return;

                    try
                    {
                        if (value)
                        {
                            if (f_ExposureAuto.IsEnumValueAvailable("Continuous"))
                            {
                                f_ExposureAuto.EnumValue = "Continuous";
                                autoExposure = f_ExposureAuto.EnumValue == "Continuous";
                            }
                        }
                        else
                        {
                            if (f_ExposureAuto.IsEnumValueAvailable("Off"))
                            {
                                f_ExposureAuto.EnumValue = "Off";
                                autoExposure = f_ExposureAuto.EnumValue != "Continuous";
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
                featureType = FeatureType.Exposure;
                isAvailable = camera.Features.ContainsName("ExposureTimeAbs");

                if(camera.Features.ContainsName("ExposureTimeAbs"))
                {
                    f_ExposureTimeAbs = camera.Features["ExposureTimeAbs"];
                    InitExposureTimeAbs();
                    f_ExposureTimeAbs.OnFeatureChanged += FeatureChanged;
                }
                if(camera.Features.ContainsName("ExposureTimeIncrement"))
                {
                    f_ExposureTimeInc = camera.Features["ExposureTimeIncrement"];
                    f_ExposureTimeInc.OnFeatureChanged += FeatureChanged;
                    InitExposureInc();
                }
                if(camera.Features.ContainsName("ExposureAuto"))
                {
                    f_ExposureAuto = camera.Features["ExposureAuto"];
                    InitExposureAuto();
                    f_ExposureAuto.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitExposureTimeAbs()
            {
                if (f_ExposureTimeAbs == null)
                    return;

                try
                {
                    switch (f_ExposureTimeAbs.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            exposureTimeAbs = f_ExposureTimeAbs.IntValue;
                            maxTimeAbs = f_ExposureTimeAbs.IntRangeMax;
                            minTimeAbs = f_ExposureTimeAbs.IntRangeMin;
                            exposureTimeAbsIncrement = f_ExposureTimeAbs.IntIncrement;
                            hasTimeAbsIncrement = true;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            exposureTimeAbs = f_ExposureTimeAbs.FloatValue;
                            maxTimeAbs = f_ExposureTimeAbs.FloatRangeMax;
                            minTimeAbs = f_ExposureTimeAbs.FloatRangeMin;
                            hasTimeAbsIncrement = f_ExposureTimeAbs.FloatHasIncrement;
                            if (hasTimeAbsIncrement)
                                exposureTimeAbsIncrement = f_ExposureTimeAbs.IntIncrement;
                            else
                                exposureTimeAbsIncrement = 1;
                            break;
                    }
                }
                catch(Exception) { }
            }

            private void InitExposureAuto()
            {
                if (f_ExposureAuto == null)
                    return;

                try
                {
                    if (f_ExposureAuto.EnumValue == "Off" || f_ExposureAuto.EnumValue == "Continuous")
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
                    switch (feature.Name)
                    {
                        case "ExposureTimeAbs":
                            InitExposureTimeAbs();
                            break;
                        case "ExposureTimeIncrement":
                            InitExposureInc();
                            break;
                        case "ExposureAuto":
                            InitExposureAuto();
                            break;
                    }
                }
                catch (Exception) { }

                onFeatureChanged?.Invoke(featureType);
            }
        }

        public class CameraFeatureGain : CameraFeature
        {
            Feature f_Gain = null;
            Feature f_GainAuto = null;

            private dynamic maxGain = 0;
            public dynamic MaxGain
            {
                get
                {
                    return maxGain;
                }
            }

            private dynamic minGain = 0;
            public dynamic MinGain
            {
                get
                {
                    return minGain;
                }
            }

            private dynamic gainIncrement = 1;
            public dynamic GainIncrement
            {
                get
                {
                    return gainIncrement;
                }
            }

            private bool hasGainIncrement = false;
            public bool HasGainIncrement
            {
                get { return hasGainIncrement; }
            }

            private bool autoGain = false;

            public override event OnFeatureChangedHandler onFeatureChanged;

            public bool Auto
            {
                set
                {
                    if (f_GainAuto == null)
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

            private dynamic gain = 0;
            public dynamic GainValue
            {
                get { return gain; }
                set
                {
                    if (f_Gain == null)
                        return;
                    
                    try
                    {
                        switch (f_Gain.DataType)
                        {
                            case VmbFeatureDataType.VmbFeatureDataInt:
                                f_Gain.IntValue = value;
                                gain = f_Gain.IntValue;
                                break;
                            case VmbFeatureDataType.VmbFeatureDataFloat:
                                f_Gain.FloatValue = value;
                                gain = f_Gain.FloatValue;
                                break;
                        }
                    }
                    catch(Exception){}
                }
            }

            public CameraFeatureGain(Camera camera) : base(camera)
            {
                featureType = FeatureType.Gain;
                isAvailable = camera.Features.ContainsName("Gain");

                if (camera.Features.ContainsName("Gain"))
                {
                    f_Gain = camera.Features["Gain"];
                    InitGain();
                    f_Gain.OnFeatureChanged += FeatureChanged;
                }
                if(camera.Features.ContainsName("GainAuto"))
                {
                    f_GainAuto = camera.Features["GainAuto"];
                    InitGainAuto();
                    f_GainAuto.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitGain()
            {
                if (f_Gain == null)
                    return;
                
                try
                {
                    switch (f_Gain.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            gain = f_Gain.IntValue;
                            maxGain = f_Gain.IntRangeMax;
                            minGain = f_Gain.IntRangeMin;
                            gainIncrement = f_Gain.IntIncrement;
                            hasGainIncrement = true;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            gain = f_Gain.FloatValue;
                            maxGain = f_Gain.FloatRangeMax;
                            minGain = f_Gain.FloatRangeMin;
                            hasGainIncrement = f_Gain.FloatHasIncrement;
                            if (f_Gain.FloatHasIncrement)
                                gainIncrement = f_Gain.FloatIncrement;
                            else
                                gainIncrement = 1d;
                            break;
                    }
                }
                catch(Exception) { }
            }

            private void InitGainAuto()
            {
                if (f_GainAuto == null)
                    return;
                try
                {
                    if (f_GainAuto.EnumValue == "Off" || f_GainAuto.EnumValue == "Continuous")
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
                InitGain();
                InitGainAuto();
                onFeatureChanged?.Invoke(featureType);
            }
        }

        public class CameraFeatureGamma : CameraFeature
        {
            Feature f_Gamma = null;

            private dynamic maxGamma = 0;
            public dynamic MaxGamma
            {
                get
                {
                    return maxGamma;
                }
            }

            private dynamic minGamma = 0;
            public dynamic MinGamma
            {
                get
                {
                    return minGamma;
                }
            }

            private dynamic gammaIncrement = 1;
            public dynamic GammaIncrement
            {
                get
                {
                    return gammaIncrement;
                }
            }

            public override event OnFeatureChangedHandler onFeatureChanged;

            private dynamic gamma = 0;
            public dynamic GammaValue
            {
                get { return gamma; }
                set
                {
                    if (f_Gamma == null)
                        return;

                    try
                    {
                        switch (f_Gamma.DataType)
                        {
                            case VmbFeatureDataType.VmbFeatureDataInt:
                                f_Gamma.IntValue = value;
                                gamma = f_Gamma.IntValue;
                                break;
                            case VmbFeatureDataType.VmbFeatureDataFloat:
                                f_Gamma.FloatValue = value;
                                gamma = f_Gamma.FloatValue;
                                break;
                        }
                    }
                    catch (Exception) { }
                }
            }

            public CameraFeatureGamma(Camera camera) : base(camera)
            {
                featureType = FeatureType.Gamma;
                isAvailable = camera.Features.ContainsName("Gamma");

                if (camera.Features.ContainsName("Gamma"))
                {
                    f_Gamma = camera.Features["Gamma"];
                    InitGamma();
                    f_Gamma.OnFeatureChanged += FeatureChanged;
                }
            }

            private void InitGamma()
            {
                if (f_Gamma == null)
                    return;
                try
                {
                    switch (f_Gamma.DataType)
                    {
                        case VmbFeatureDataType.VmbFeatureDataInt:
                            gamma = f_Gamma.IntValue;
                            maxGamma = f_Gamma.IntRangeMax;
                            minGamma = f_Gamma.IntRangeMin;
                            gammaIncrement = f_Gamma.IntIncrement;
                            break;
                        case VmbFeatureDataType.VmbFeatureDataFloat:
                            gamma = f_Gamma.FloatValue;
                            maxGamma = f_Gamma.FloatRangeMax;
                            minGamma = f_Gamma.FloatRangeMin;
                            if (f_Gamma.FloatHasIncrement)
                                gammaIncrement = f_Gamma.FloatIncrement;
                            else
                                gammaIncrement = 1d;
                            break;
                    }
                }
                catch(Exception) { }
            }

            protected override void FeatureChanged(Feature feature)
            {
                InitGamma();
                onFeatureChanged?.Invoke(featureType);
            }
        }
    }
}
