namespace Datahub.Core.DataTransfers.UnitTests
{
	public class DataTransferTests
    {
        [Test]
        public void EncodeDecodeCredentials()
        {
            var exp = DateTimeOffset.UtcNow;
            var creds = new UploadCredentials() {  SASToken = "tk1", SASTokenExpiry = exp , WorkspaceCode = "test1", DataHubEnvironment = "https://localhost:5001" };
            var encoded = CredentialEncoder.EncodeCredentials(creds);
            var decoded = CredentialEncoder.DecodeCredentials(encoded);
            Assert.That(decoded.SASToken, Is.EqualTo(creds.SASToken));
            Assert.That(decoded.SASTokenExpiry, Is.EqualTo(creds.SASTokenExpiry));
            Assert.That(decoded.WorkspaceCode, Is.EqualTo(creds.WorkspaceCode));
            Assert.That(decoded.DataHubEnvironment, Is.EqualTo(creds.DataHubEnvironment));
        }

        [Test]
        public void ValidateCredentials()
        {
            var exp = DateTimeOffset.UtcNow;
            var creds = new UploadCredentials() { SASToken = "tk1", SASTokenExpiry = exp, WorkspaceCode = "test1", DataHubEnvironment = "https://localhost:5001" };
            var encoded = CredentialEncoder.EncodeCredentials(creds);
            var bad = encoded + "asdasdasd";
            Assert.That(CredentialEncoder.IsValid(bad), Is.False);
        }
    }
}
