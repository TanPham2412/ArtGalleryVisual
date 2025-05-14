using System;
using ArtGallery.Models;

namespace ArtGallery.ViewModels
{
    public class ArtworkStatisticsViewModel
    {
        public Tranh Artwork { get; set; }
        public decimal TotalSales { get; set; }
    }
} 