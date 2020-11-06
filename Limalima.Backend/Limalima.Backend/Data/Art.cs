using System;
using System.Collections.Generic;

namespace Limalima.Backend.Data
{
    public class Art
    {
        public Guid OwnerId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Description { get; set; }
        public ArtStatus Status { get; set; }


        //public int CategoryId { get; set; }
        //public Category Category { get; set; }

        //public List<ArtMaterial> ArtMaterials { get; set; }
        public List<ArtPhoto> ArtPhotos { get; set; }

        public Guid DeliveryTemplateId { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberShowPublic { get; set; }

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
        TransactionCopy = 10000
    }
    public class ArtPhoto
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public Guid ArtId { get; set; }
        public Art Art { get; set; }
    }
}
