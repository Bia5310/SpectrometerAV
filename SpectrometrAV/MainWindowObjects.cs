using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpectrometrAV
{
    public partial class MainWindow
    {
        NumericUpDown numeric_start = null;
        NumericUpDown numeric_end = null;
        NumericUpDown numeric_step = null;

        Button button_capture = null;
        Button button_path = null;
        Button button_connAOF = null;
        Button button_power = null;
        Button button_loadDev = null;
        Button button_fullRoi = null;

        TextBox textBox_path = null;

        TextBlock textBlock_lastFolder;

        ViewportControl viewportControl = null;

        CustomSlider slider_frequency = null;
        CustomSlider slider_wavelength = null;
        CustomSlider slider_wavenumber = null;
        CustomSlider slider_exposure = null;
        CustomSlider slider_gain = null;
        CustomSlider slider_gamma = null;
        CustomSlider slider_offsetX = null;
        CustomSlider slider_offsetY = null;
        CustomSlider slider_width = null;
        CustomSlider slider_height = null;
        CustomSlider slider_binningX = null;
        CustomSlider slider_binningY = null;

        private void InitialiizeComponents()
        {
            
            button_capture = this.FindControl<Button>("Button_capture");
            button_path = this.FindControl<Button>("Button_path");
            button_connAOF = this.FindControl<Button>("button_connAOF");
            button_loadDev = this.FindControl<Button>("button_loadDev");
            button_power = this.FindControl<Button>("button_power");
            button_fullRoi = this.FindControl<Button>("button_fullRoi");

            numeric_start = this.FindControl<NumericUpDown>("Numeric_start");
            numeric_end = this.FindControl<NumericUpDown>("Numeric_end");
            numeric_step = this.FindControl<NumericUpDown>("Numeric_step");

            textBox_path = this.FindControl<TextBox>("textBox_path");

            textBlock_lastFolder = this.FindControl<TextBlock>("textBlock_lastFolder");

            viewportControl = this.FindControl<ViewportControl>("ViewportControl");

            slider_frequency = this.FindControl<CustomSlider>("slider_frequency");
            slider_wavelength = this.FindControl<CustomSlider>("slider_wavelength");
            slider_wavenumber = this.FindControl<CustomSlider>("slider_wavenumber");
            slider_exposure = this.FindControl<CustomSlider>("slider_exposure");
            slider_gain = this.FindControl<CustomSlider>("slider_gain");
            slider_gamma = this.FindControl<CustomSlider>("slider_gamma");
            slider_offsetX = this.FindControl<CustomSlider>("slider_offsetX");
            slider_offsetY = this.FindControl<CustomSlider>("slider_offsetY");
            slider_width = this.FindControl<CustomSlider>("slider_width");
            slider_height = this.FindControl<CustomSlider>("slider_height");
            slider_binningX = this.FindControl<CustomSlider>("slider_binningX");
            slider_binningY = this.FindControl<CustomSlider>("slider_binningY");
        }
    }
}
