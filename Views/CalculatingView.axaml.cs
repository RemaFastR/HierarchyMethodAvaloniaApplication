using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace HierarchyMethodAvaloniaApplication.Views
{
    public partial class CalculatingView : UserControl
    {
        public CalculatingView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
