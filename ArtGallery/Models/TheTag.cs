using System;
using System.Collections.Generic;

namespace ArtGallery.Models;

public partial class TheTag
{
    public int MaTag { get; set; }

    public string TenTag { get; set; } = null!;

    public virtual ICollection<Tranh> MaTranhs { get; set; } = new List<Tranh>();
}
