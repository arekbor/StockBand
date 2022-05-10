﻿using Microsoft.AspNetCore.Mvc;
using StockBand.Data;
using System.ComponentModel.DataAnnotations;

namespace StockBand.Models
{
    public class Track
    {
        [Key]
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Lyrics { get; set; }
        public string Extension { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public long PlaysCount { get; set; } = 0;
        public TrackAccess TrackAccess { get; set; } = TrackAccess.Inner;
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public bool IsDownloadle { get; set; }

    }
}