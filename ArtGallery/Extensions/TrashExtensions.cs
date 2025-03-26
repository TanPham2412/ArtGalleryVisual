using ArtGallery.Models;

public static class TrashExtensions
{
    public static bool DaThich(this IEnumerable<Tranh> model, int artworkId, string username)
    {
        using (var context = new ArtGalleryContext())
        {
            var userId = context.Users.FirstOrDefault(u => u.UserName == username)?.Id;
            if (userId == null) return false;
            
            return context.LuotThiches.Any(lt => 
                lt.MaTranh == artworkId && lt.MaNguoiDung == userId);
        }
    }
} 