using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;

namespace archivesystemWebUI.Services
{
    public class FileService : IFileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFolderRepo _folderRepo;
        private readonly IFileRepo _fileRepo;

        public FileService(
            IUnitOfWork unitOfWork,
            IFolderRepo folderRepo,
            IFileRepo fileRepo)
        {
            _unitOfWork = unitOfWork;
            _folderRepo = folderRepo;
            _fileRepo = fileRepo;
        }

        public (bool save, FileMetaVm model) Create(FileMetaVm model, HttpPostedFileBase fileBase)
        {
           var  file = new archivesystemDomain.Entities.File();
            file.IsArchived = model.Archive;
            file.AccessLevelId = model.AccessLevelId;
            file.UploadedById = model.UploadedById;
            file.Name = $"{model.Title}.{fileBase.FileName?.Split('.').Last()}";
            file.ContentType = fileBase.ContentType;
            file.CreatedAt = DateTime.Now;
            file.UpdatedAt = DateTime.Now;
            file.FileContent = new FileContent
            {
                Title = $"{Guid.NewGuid():N}.{fileBase.FileName?.Split('.').Last()}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };

            if (model.Archive)
                file.FileContent.Content = ZipFile(fileBase, model.Title + "." + model.FileBase.FileName.Split('.').Last());
            else
                file.FileContent.Content = ReadBytes(fileBase);


            _folderRepo.FindWithNavProps(f => f.Id == model.FolderId, _ => _.Files)
                       .SingleOrDefault()?.Files.Add(file);
            _unitOfWork.Save();
            return (true, model);

        }
        private byte[] ReadBytes(HttpPostedFileBase file)
        {
            using (var reader = new BinaryReader(file.InputStream))
            {
                var bytes= reader.ReadBytes(file.ContentLength);
                return bytes;
            }
    
        }
        public ICollection<archivesystemDomain.Entities.File> GetFiles(int folderId)
        {
            var files = _fileRepo.FindWithNavProps(_ => _.FolderId == folderId,
                        _ => _.FileContent, f => f.AccessLevel).ToList();

            return files;

        }

        public archivesystemDomain.Entities.File Details(int id)
        {
            return _fileRepo.FindWithNavProps(f => f.Id == id, x=> x.UploadedBy).SingleOrDefault();
        }

        public RequestResponse<string> DeleteFile(int id)
        {
            var file=_fileRepo.Get(id);
            if (file == null) return new RequestResponse<string> { Status = HttpStatusCode.NotFound };
            if(file.IsArchived) return new RequestResponse<string> { Status = HttpStatusCode.Forbidden };
            
            _fileRepo.Remove(file);
            _unitOfWork.Save();
            return new RequestResponse<string> { Status = HttpStatusCode.OK };
        }
        public archivesystemDomain.Entities.File GetFile(int id, string fileName)
        {
            var file = _fileRepo.Get(id);
            return file;
        }

        public RequestResponse<string> ArchiveFile (int fileId)
        {
            try 
            {
                var file = _fileRepo.Get(fileId);
                if (file == null) return new RequestResponse<string>
                { Status = HttpStatusCode.BadRequest, Message = "Error:file was not found" };

                file.IsArchived = true;
                _unitOfWork.Save();
                return new RequestResponse<string> { Status = HttpStatusCode.OK, Message = "file was archived successfully" };
            }
            catch
            {
                return new RequestResponse<string>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "An Error Occurred"
                };
            }
        }

        private byte[]  ZipFile(HttpPostedFileBase file,string fileName)
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    byte[] bytes = ReadBytes(file);
                    var zipEntry = archive.CreateEntry(fileName, CompressionLevel.Optimal); 
                    using (var zipStream = zipEntry.Open())
                    {
                        zipStream.Write(bytes, 0, bytes.Length);
                    }

                }
                return ms.ToArray();
            }
        }
    }
}