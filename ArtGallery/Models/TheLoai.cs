using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class TheLoai
{
    public int MaTheLoai { get; set; }

    public string TenTheLoai { get; set; } = null!;

    public virtual ICollection<Tranh> MaTranhs { get; set; } = new List<Tranh>();
}
