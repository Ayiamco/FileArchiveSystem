using System;
using System.Linq;
using System.Web;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;

namespace archivesystemDomain.Services
{
    public class UpsertFile : IUpsertFile
    {
        public File Save(File model, HttpPostedFileBase file = null)
        {
            //if (model!= null)
            //{
            //    model.Name = $"{Guid.NewGuid():N}.{System.IO.Path.GetFileName(file.FileName)?.Split('.').Last()}";

            //    model.ContentType = file.ContentType;
            //    using (var reader = new System.IO.BinaryReader(file.InputStream))
            //    {
            //        model.Content = reader.ReadBytes(file.ContentLength);
            //    }
            //    model.UpdatedAt = DateTime.Now;
            //    return model;
            //}
            //else
            //{
            //    model = new File()
            //    {
            //        Name = $"{Guid.NewGuid():N}.{System.IO.Path.GetFileName(file.FileName)?.Split('.').Last()}",
            //        ContentType = file.ContentType
            //    };

            //    using (var reader = new System.IO.BinaryReader(file.InputStream))
            //    {
            //        model.Content = reader.ReadBytes(file.ContentLength);
            //    }
            //    model.CreatedAt = DateTime.Now;
            //    model.UpdatedAt = DateTime.Now;
            //    return model;

            //}

            return model;
        }

      
    }
}