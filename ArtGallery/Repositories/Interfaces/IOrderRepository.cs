using ArtGallery.Models;

namespace ArtGallery.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Tranh> GetArtworkById(int id);
        Task<(bool success, string message)> CreateOrder(GiaoDich giaoDich);
        Task<List<GiaoDich>> GetOrdersByUserId(string userId);
    }
}
