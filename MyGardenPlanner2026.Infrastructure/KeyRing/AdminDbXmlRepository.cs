namespace MyGardenPlanner2026.Infrastructure.KeyRing;

using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Infrastructure.Data;
using System.Xml;
using System.Xml.Linq;

/// <summary>
/// Gemmer Data Protection-nøgler i admin.DataProtectionKeys. Bruger IAdminDbContextFactory, fordi
/// app-brugeren kun har SELECT på admin-schema. Ugyldig XML i en række kaster (højt og tydeligt), så en
/// beskadiget nøglering aldrig stille erstattes af en ny nøgle.
/// </summary>
public sealed class AdminDbXmlRepository(IAdminDbContextFactory contextFactory) : IXmlRepository
{
    private static readonly XmlReaderSettings ReaderSettings = new() { DtdProcessing = DtdProcessing.Prohibit };

    public IReadOnlyCollection<XElement> GetAllElements()
    {
        using var context = contextFactory.CreateDbContext();

        return [.. context.DataProtectionKeys
            .AsNoTracking()
            .OrderBy(k => k.Id)
            .Select(k => k.Xml)
            .AsEnumerable()
            .Select(Parse)];
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        ArgumentNullException.ThrowIfNull(element);

        using var context = contextFactory.CreateDbContext();
        context.DataProtectionKeys.Add(new DataProtectionKeyRecord
        {
            FriendlyName = friendlyName,
            Xml = element.ToString(SaveOptions.DisableFormatting)
        });
        context.SaveChanges();
    }

    private static XElement Parse(string xml)
    {
        using var reader = XmlReader.Create(new StringReader(xml), ReaderSettings);
        return XDocument.Load(reader, LoadOptions.PreserveWhitespace).Root!;
    }
}