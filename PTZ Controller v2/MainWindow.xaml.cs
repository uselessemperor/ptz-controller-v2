using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PTZ_Controller_v2.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PTZ_Controller_v2
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly ICameraRepository _cameraRepository;
        private readonly AppServices _services;
        public ObservableCollection<CameraCardViewModel> Cameras { get; } = new();



        public MainWindow(ICameraRepository cameraRepository, AppServices services)
        {
            InitializeComponent();

            _cameraRepository = cameraRepository;
            _services = services;
            this.Activated += MainWindow_Activated;
        }
        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            await LoadCamerasAsync();
        }
        private async Task LoadCamerasAsync()
        {
            Cameras.Clear();

            var cameras = await _cameraRepository.GetCamerasAsync();

            foreach (var camera in cameras)
            {
                Cameras.Add(new CameraCardViewModel(camera));
            }
        }


        private void AddCamera_Click(object sender, RoutedEventArgs e)
        {

            var addCameraWindow = new AddCameraWindow(_cameraRepository);
            addCameraWindow.Activate();

        }
        private void ControlCamera_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is CameraCardViewModel cameraCard)
            {
                var controlWindow = new CameraControlWindow(
                    cameraCard.Camera,
                    _services);

                controlWindow.Activate();
            }
        }

        private async void DeleteCamera_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is CameraCardViewModel cameraCard)
            {
                await _cameraRepository.DeleteCameraAsync(cameraCard.Camera);
                Cameras.Remove(cameraCard);
            }
        }
        private void OpenControlHub_Click(object sender, RoutedEventArgs e)
        {
            var hubWindow = new CameraControlHubWindow(_services);
            hubWindow.Activate();
        }
    }
}
