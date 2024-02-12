using Datahub.Metadata.DTO;
using System.IO;
using System.Threading.Tasks;

namespace Datahub.CKAN.Service;

public interface ICKANService
{
    Task<CKANApiResult> CreateOrFetchPackage(FieldValueContainer fieldValues, bool allFields);
    Task<CKANApiResult> AddResourcePackage(string packageId, string filename, string filePurpose, FieldValueContainer metadata, Stream fileContentStream, long? contentLength = null);

    // TODO cleanup old methods
    Task<CKANApiResult> CreatePackage(FieldValueContainer fieldValues, bool allFields, string url = null);
    Task<CKANApiResult> AddResourcePackageOld(string packageId, string fileName, Stream fileContent, long? contentLength = null);
    Task<CKANApiResult> DeletePackage(string packageId);
}


#nullable enable
public record CKANApiResult(bool Succeeded, string ErrorMessage, object? CkanObject = null);