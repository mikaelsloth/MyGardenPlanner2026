namespace MyGardenPlanner2026.Tests.Unit.KeyRing;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyGardenPlanner2026.Core.Entities.Admin;
using MyGardenPlanner2026.Core.Entities.Common;
using MyGardenPlanner2026.Infrastructure.Data;
using MyGardenPlanner2026.Infrastructure.KeyRing;
using System.Xml;
using System.Xml.Linq;
using Xunit;

public class AdminDbXmlRepositoryTests : TestDbContext
{
    private AdminDbXmlRepository CreateSut() => new(new FakeAdminDbContextFactory(CreateDbContext));

    [Fact]
    public void GetAllElements_WhenEmpty_ReturnsEmpty() =>
        CreateSut().GetAllElements().Should().BeEmpty();

    [Fact]
    public void StoreElement_ThenGetAllElements_ReturnsSameXml()
    {
        var sut = CreateSut();
        var element = XElement.Parse("<key id=\"k1\"><value>  abc  </value></key>", LoadOptions.PreserveWhitespace);

        sut.StoreElement(element, "key-k1");

        var stored = sut.GetAllElements().Should().ContainSingle().Subject;
        XNode.DeepEquals(stored, element).Should().BeTrue();
    }

    [Fact]
    public void StoreElement_SavesFriendlyName()
    {
        CreateSut().StoreElement(new XElement("key"), "key-abc");

        using var context = CreateDbContext();
        context.DataProtectionKeys.Single().FriendlyName.Should().Be("key-abc");
    }

    [Fact]
    public void GetAllElements_ReturnsElementsInInsertOrder()
    {
        var sut = CreateSut();
        sut.StoreElement(new XElement("key", new XAttribute("n", 1)), "a");
        sut.StoreElement(new XElement("key", new XAttribute("n", 2)), "b");
        sut.StoreElement(new XElement("key", new XAttribute("n", 3)), "c");

        sut.GetAllElements().Select(e => (string?)e.Attribute("n")).Should().Equal("1", "2", "3");
    }

    [Fact]
    public void StoreElement_NullElement_Throws()
    {
        var act = () => CreateSut().StoreElement(null!, "key");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetAllElements_WithInvalidXmlRow_Throws()
    {
        using (var context = CreateDbContext())
        {
            context.DataProtectionKeys.Add(new DataProtectionKeyRecord { FriendlyName = "kaput", Xml = "<ikke-lukket" });
            context.SaveChanges();
        }

        var act = () => CreateSut().GetAllElements();

        act.Should().Throw<XmlException>();
    }

    [Fact]
    public void KeyRecord_IsStoredInAdminSchema()
    {
        using var context = CreateDbContext();

        context.Model.FindEntityType(typeof(DataProtectionKeyRecord))!
            .GetSchema().Should().Be(PlannerDbContext.AdminSchema);
    }

    [Fact]
    public void KeyRecord_IsNotSoftDelete_SoKeyMaterialIsNeverAudited() =>
        typeof(ISoftDelete).IsAssignableFrom(typeof(DataProtectionKeyRecord)).Should().BeFalse();
}