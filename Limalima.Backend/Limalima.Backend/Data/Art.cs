using System;
using System.Collections.Generic;

namespace Limalima.Backend.Data
{
    public class Art
    {

        public Guid ArtId { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }
        public ArtStatus Status { get; set; }

        public string CategoriesImported { get; set; }
        public string TagsImported { get; set; }

        public string MaterialsImported { get; set; }
        public List<ArtPhoto> ArtPhotos { get; set; }

        public string MainPhotoUrl { get; set; }
    }
    public enum ArtStatus
    {
        Unknown = 0,
        Draft = 10,
        DraftPhoto = 20,
        DraftDelivery = 30,
        DraftSummary = 99,
        InVerification = 100,
        Denied = 200,
        Published = 500,
        Sold = 1000,
        Expired = 2000,
        //Archived = 8000,
        Deleted = 9999,
        TransactionCopy = 10000,
        Imported=50000
    }
    public class ArtPhoto
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public Guid ArtId { get; set; }
        public Art Art { get; set; }
    }
}
