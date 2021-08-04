﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
//File Manager's base functions are available in the below namespace
using Syncfusion.EJ2.FileManager.Base;
//File Manager's operations are available in the below namespace
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using Newtonsoft.Json;
using Syncfusion.EJ2.FileManager.Base.SQLFileProvider;
using Microsoft.Extensions.Configuration;

namespace WytSkyPWA.Server.Controllers
{
    [Route("api/[controller]")]
    public class FileManagerDBController : Controller
    {
        SQLFileProvider operation;
        public FileManagerDBController(IConfiguration configuration)
        {
            operation = new SQLFileProvider(configuration);
            operation.SetSQLConnection("FileManagerConnection", "Product", "0");
        }
        [Route("SQLFileOperations")]
        public object SQLFileOperations([FromBody] FileManagerDirectoryContent args)
        {
            if ((args.Action == "delete" || args.Action == "rename") && ((args.TargetPath == null) && (args.Path == "")))
            {
                FileManagerResponse response = new FileManagerResponse();
                response.Error = new ErrorDetails { Code = "403", Message = "Restricted to modify the root folder." };
                return operation.ToCamelCase(response);
            }

            switch (args.Action)
            {
                case "read":
                    // reads the file(s) or folder(s) from the given path.
                    return operation.ToCamelCase(operation.GetFiles(args.Path, false, args.Data));
                case "delete":
                    // deletes the selected file(s) or folder(s) from the given path.
                    return operation.ToCamelCase(operation.Delete(args.Path, args.Names, args.Data));
                case "details":
                    // gets the details of the selected file(s) or folder(s).
                    return operation.ToCamelCase(operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    // creates a new folder in a given path.
                    return operation.ToCamelCase(operation.Create(args.Path, args.Name, args.Data));
                case "search":
                    // gets the list of file(s) or folder(s) from a given path based on the searched key string.
                    return operation.ToCamelCase(operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive, args.Data));
                case "rename":
                    // renames a file or folder.
                    return operation.ToCamelCase(operation.Rename(args.Path, args.Name, args.NewName, false, args.Data));
                case "move":
                    // cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    return operation.ToCamelCase(operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
                case "copy":
                    // copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    return operation.ToCamelCase(operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
            }
            return null;
        }

        // uploads the file(s) into a specified path
        [Route("SQLUpload")]
        public IActionResult SQLUpload(string path, IList<IFormFile> uploadFiles, string action, string data)
        {
            FileManagerResponse uploadResponse;
            FileManagerDirectoryContent[] dataObject = new FileManagerDirectoryContent[1];
            dataObject[0] = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(data);
            uploadResponse = operation.Upload(path, uploadFiles, action, dataObject);
            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
            return Content("");
        }

        // downloads the selected file(s) and folder(s)
        [Route("SQLDownload")]
        public IActionResult SQLDownload(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            args.Path = (args.Path);
            return operation.Download(args.Path, args.Names, args.Data);
        }

        // gets the image(s) from the given path
        [Route("SQLGetImage")]
        public IActionResult SQLGetImage(FileManagerDirectoryContent args)
        {
            return operation.GetImage(args.Path, args.Id, true, null, args.Data);
        }

        public IActionResult Index()
        {
            return View();
        }

    }
}