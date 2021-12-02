﻿using Datahub.CKAN.Service;
using Datahub.Metadata.DTO;
using System.Threading.Tasks;

namespace Datahub.Portal.Services
{
    public interface IOpenDataService
    {
        Task<CKANApiResult> PublishFileAsUrl(FieldValueContainer fileMetadata, string url);
        Task PublishFile(FieldValueContainer fileMetadata, long sharedRecordId, string fileId, string fileName, string fileUrl);
        bool IsStaging();
    }
}