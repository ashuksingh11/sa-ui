using System.Threading.Tasks;

namespace SamsungAccountUI.Controllers.Base
{
    public interface IController
    {
        Task LoadAsync();
        Task HandleInputAsync(object input);
        Task OnDeviceSpecificAction(string action, object data);
    }
}