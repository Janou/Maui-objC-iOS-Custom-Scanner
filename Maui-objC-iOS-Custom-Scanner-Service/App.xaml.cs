namespace Maui_objC_iOS_Custom_Scanner_Service
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "Maui_objC_iOS_Custom_Scanner_Service" };
        }
    }
}
