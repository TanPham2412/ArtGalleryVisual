using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ArtGallery.Controllers
{
    public class LikeArtworkController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public LikeArtworkController(ArtGalleryContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: LuotThiches
        public async Task<IActionResult> Index()
        {
            var artGalleryContext = _context.LuotThiches.Include(l => l.MaNguoiDungNavigation).Include(l => l.MaTranhNavigation);
            return View(await artGalleryContext.ToListAsync());
        }

        // GET: LuotThiches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var luotThich = await _context.LuotThiches
                .Include(l => l.MaNguoiDungNavigation)
                .Include(l => l.MaTranhNavigation)
                .FirstOrDefaultAsync(m => m.MaLuotThich == id);
            if (luotThich == null)
            {
                return NotFound();
            }

            return View(luotThich);
        }

        // GET: LuotThiches/Create
        public IActionResult Create()
        {
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "Id", "Id");
            ViewData["MaTranh"] = new SelectList(_context.Tranhs, "MaTranh", "MaTranh");
            return View();
        }

        // POST: LuotThiches/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaLuotThich,MaTranh,MaNguoiDung,NgayThich")] LuotThich luotThich)
        {
            if (ModelState.IsValid)
            {
                _context.Add(luotThich);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "Id", "Id", luotThich.MaNguoiDung);
            ViewData["MaTranh"] = new SelectList(_context.Tranhs, "MaTranh", "MaTranh", luotThich.MaTranh);
            return View(luotThich);
        }

        // GET: LuotThiches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var luotThich = await _context.LuotThiches.FindAsync(id);
            if (luotThich == null)
            {
                return NotFound();
            }
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "Id", "Id", luotThich.MaNguoiDung);
            ViewData["MaTranh"] = new SelectList(_context.Tranhs, "MaTranh", "MaTranh", luotThich.MaTranh);
            return View(luotThich);
        }

        // POST: LuotThiches/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaLuotThich,MaTranh,MaNguoiDung,NgayThich")] LuotThich luotThich)
        {
            if (id != luotThich.MaLuotThich)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(luotThich);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LuotThichExists(luotThich.MaLuotThich))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MaNguoiDung"] = new SelectList(_context.NguoiDungs, "Id", "Id", luotThich.MaNguoiDung);
            ViewData["MaTranh"] = new SelectList(_context.Tranhs, "MaTranh", "MaTranh", luotThich.MaTranh);
            return View(luotThich);
        }

        // GET: LuotThiches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var luotThich = await _context.LuotThiches
                .Include(l => l.MaNguoiDungNavigation)
                .Include(l => l.MaTranhNavigation)
                .FirstOrDefaultAsync(m => m.MaLuotThich == id);
            if (luotThich == null)
            {
                return NotFound();
            }

            return View(luotThich);
        }

        // POST: LuotThiches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var luotThich = await _context.LuotThiches.FindAsync(id);
            if (luotThich != null)
            {
                _context.LuotThiches.Remove(luotThich);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LuotThichExists(int id)
        {
            return _context.LuotThiches.Any(e => e.MaLuotThich == id);
        }

        [Authorize]
        public async Task<IActionResult> Display()
        {
            var currentUserId = _userManager.GetUserId(User);
            
            var likedArtworks = await _context.LuotThiches
                .Where(lt => lt.MaNguoiDung == currentUserId)
                .Include(lt => lt.MaTranhNavigation)
                    .ThenInclude(t => t.MaNguoiDungNavigation)
                .OrderByDescending(lt => lt.NgayThich)
                .Select(lt => lt.MaTranhNavigation)
                .ToListAsync();

            return View(likedArtworks);
        }
    }
}
