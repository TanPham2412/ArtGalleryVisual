using ArtGallery.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtGallery.Repositories.Interfaces
{
    public interface ITheLoaiRepositories
    {
        Task<IEnumerable<TheLoai>> GetAllAsync();
        Task<TheLoai> GetByIdAsync(int id);
        Task<IEnumerable<Tranh>> GetArtworksByTheLoaiIdAsync(int id);
        Task<TheLoai> GetByNameAsync(string name);
    }
}
