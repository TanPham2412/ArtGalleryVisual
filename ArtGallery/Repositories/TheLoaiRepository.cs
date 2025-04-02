using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtGallery.Repositories
{
    public class TheLoaiRepository : ITheLoaiRepositories
    {
        private readonly ArtGalleryContext _context;

        public TheLoaiRepository(ArtGalleryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TheLoai>> GetAllAsync()
        {
            return await _context.TheLoais.OrderBy(t => t.TenTheLoai).ToListAsync();
        }

        public async Task<TheLoai> GetByIdAsync(int id)
        {
            return await _context.TheLoais
                .Include(t => t.MaTranhs)
                .FirstOrDefaultAsync(t => t.MaTheLoai == id);
        }

        public async Task<TheLoai> GetByNameAsync(string name)
        {
            return await _context.TheLoais
                .Include(t => t.MaTranhs)
                .FirstOrDefaultAsync(t => t.TenTheLoai.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Tranh>> GetArtworksByTheLoaiIdAsync(int id)
        {
            var theLoai = await _context.TheLoais
                .Include(t => t.MaTranhs)
                .ThenInclude(t => t.MaNguoiDungNavigation)
                .Include(t => t.MaTranhs)
                .ThenInclude(t => t.LuotThiches)
                .FirstOrDefaultAsync(t => t.MaTheLoai == id);

            return theLoai?.MaTranhs ?? new List<Tranh>();
        }
    }
}
