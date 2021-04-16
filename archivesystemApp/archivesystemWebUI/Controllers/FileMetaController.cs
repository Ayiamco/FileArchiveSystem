using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using archivesystemDomain.Entities;
using archivesystemDomain.Interfaces;
using archivesystemWebUI.Infrastructures;
using archivesystemWebUI.Interfaces;
using archivesystemWebUI.Models;
using archivesystemWebUI.Models.FolderModels;
using Microsoft.AspNet.Identity;

namespace archivesystemWebUI.Controllers
{
    public class FileMetaController : Controller
    {
      
        private readonly IFileService _fileService;
        private readonly IAccessLevelService _accessLevelService;


        public FileMetaController(IFileService fileService , IAccessLevelService accessLevelService)
        {
            _fileService = fileService;
            _accessLevelService = accessLevelService;
        }
        
        // GET: FileMeta
        public ActionResult New(int folderId)
        {
    

            return PartialView(new FileMetaVm
            {
                AccessLevel =_accessLevelService.GetAll(),
                FolderId = folderId,
                UploadedById = User.Identity.GetUserId()
                
            });
        }

        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public JsonResult New(FileMetaVm model)
        {
            if (!ModelState.IsValid)
               return Json(new RequestResponse<string> { Status = HttpStatusCode.BadRequest,
                    Message = "Some required feilds were missing" });

            var file = _fileService.Create(model, model.FileBase);
            if (file.save)  return Json( new RequestResponse<string> {Status=HttpStatusCode.Created,
                Message="file added successfully" });

           return Json(new RequestResponse<string> { Status = HttpStatusCode.InternalServerError,
               Message = "An Error Occurred" });
        }

        [HttpPost]
        [ValidateHeaderAntiForgeryToken]
        public JsonResult Archive(int fileId)
        {
            if (!ModelState.IsValid) return Json(new RequestResponse<string> {
                    Status = HttpStatusCode.BadRequest,
                    Message = "fileId is missing"
                });

           var result= _fileService.ArchiveFile(fileId);
            return Json(result);  
        }


        public ActionResult Details(int id)
        {
            var model = _fileService.Details(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        
        public JsonResult Delete(int id, string fileName)
        {
            var result=_fileService.DeleteFile(id);
            if (result.Status == HttpStatusCode.NotFound) result.Message = $"{fileName} was not found";
            if (result.Status == HttpStatusCode.Forbidden) result.Message = $"Not allowed:{fileName} is archived";
            if (result.Status == HttpStatusCode.OK) result.Message = $"{fileName} was successfully deleted";
            
            return Json(result);
        }
        public ActionResult GetAllFiles(int folderId)
        {

            var files = _fileService.GetFiles(folderId);
           
            return PartialView(files);
        }

        public FileContentResult GetFile(int id, string fileName)
        {
            var file = _fileService.GetFile(id, fileName);

            if (file==null)
            {
                return null;
            }
            
            //Response.AppendHeader("Content-Disposition", "inline; filename=" + fileName
            
            if(file.IsArchived) return File(file.FileContent.Content, "application/zip", fileName.Split('.')[0]+".zip");

            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            return File(file.FileContent.Content, file.ContentType);
        }

       
    }
}