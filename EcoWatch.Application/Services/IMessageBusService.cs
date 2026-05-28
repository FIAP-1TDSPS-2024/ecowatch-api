using System.Threading.Tasks;

namespace EcoWatch.Application.Services
{
    public interface IMessageBusService
    {
        Task PublicarAlertaIncendioAsync(object ocorrenciaPayload);
    }
}