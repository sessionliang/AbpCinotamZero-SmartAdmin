﻿using Abp.Modules;
using Cinotam.FileManager.Contracts;
using System.Collections.Generic;

namespace Cinotam.FileManager
{
    public class FileManagerModule : AbpModule
    {
        public List<IFileManagerServiceProvider> FileManagerServiceProviders = new List<IFileManagerServiceProvider>();

        protected FileManagerModule()
        {

        }
    }
}
