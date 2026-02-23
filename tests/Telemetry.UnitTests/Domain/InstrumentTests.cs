using FluentAssertions;
using Telemetry.Domain.Entities;
using Xunit;

namespace Telemetry.UnitTests.Domain;

public class InstrumentTests
{
    [Fact]
    public void Create_SetsDefaults()
    {
        var instrument = Instrument.Create("LC-001", "Chromatograph", "SN-12345");

        instrument.Name.Should().Be("LC-001");
        instrument.Type.Should().Be("Chromatograph");
        instrument.SerialNumber.Should().Be("SN-12345");
        instrument.Status.Should().Be("Unknown");
        instrument.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ThrowsWhenNameEmpty()
    {
        var act = () => Instrument.Create("", "Type");
        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void UpdateHealth_UpdatesStatusAndLastHealthCheck()
    {
        var instrument = Instrument.Create("I1", "Type");
        instrument.UpdateHealth("Healthy");

        instrument.Status.Should().Be("Healthy");
        instrument.LastHealthCheck.Should().NotBeNull();
    }
}
