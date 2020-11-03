using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Limalima.Backend.Models
{
    public class AnnouceViewModel
    {
        public Guid ImageTempId { get; set; }
    }


    //wysylane przez jquery ajax
    public class FileUploadViewModel
    {
        public Guid ImageTempId { get; set; }
        public IFormFile File { get; set; }
    }
}
