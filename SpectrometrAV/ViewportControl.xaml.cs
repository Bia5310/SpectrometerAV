using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SpectrometrAV
{
    public class ViewportControl : UserControl
    {
        public Image imageMain = null;

        public ViewportControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            imageMain = this.FindControl<Image>("ImageMain");
        }

        
    }
}
