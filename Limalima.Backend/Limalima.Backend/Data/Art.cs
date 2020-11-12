using System;
using System.Collections.Generic;

namespace Limalima.Backend.Data
{
    public class Art
    {

        public Guid ArtId { get; set; } = Guid.NewGuid();
        public Guid OwnerId { get; set; }//daj random narazie

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }
        public ArtStatus Status { get; set; } //Imported

        public string CategoriesImported { get; set; }
        public string TagsImported { get; set; }

        public string MaterialsImported { get; set; }//semicolon separated, e.g.: "Różowe złoto;Srebro;Złoto"
        public List<ArtPhoto> ArtPhotos { get; set; } //sciagnac zdjecia, dodac watermark i zapisac do azure. na koniec zapis do bazy

        public string MainPhotoUrl { get; set; } //sciagnac zjdecia, dodac watermark i zapisac do azure, a tutaj Url z Azure
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
        public string Url { get; set; } //url z azure

        public Guid ArtId { get; set; }
        public Art Art { get; set; }
    }
}
