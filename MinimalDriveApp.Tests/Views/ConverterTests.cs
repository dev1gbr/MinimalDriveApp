using MinimalDriveApp.Views;
using System.Globalization;

namespace MinimalDriveApp.Tests.Views;

public class ConverterTests
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    // --- BytesToGbConverter ---

    [Theory]
    [InlineData(1_073_741_824L, "1.0 GB")]
    [InlineData(2_147_483_648L, "2.0 GB")]
    [InlineData(536_870_912L,   "0.5 GB")]
    public void BytesToGb_FormatsCorrectly(long bytes, string expected)
    {
        var result = new BytesToGbConverter().Convert(bytes, typeof(string), null!, Inv);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void BytesToGb_ReturnsDash_ForNonLongInput()
    {
        var result = new BytesToGbConverter().Convert("bad", typeof(string), null!, Inv);
        Assert.Equal("—", result);
    }

    // --- NullableDateConverter ---

    [Fact]
    public void NullableDate_ReturnsDash_ForNullInput()
    {
        var result = new NullableDateConverter().Convert(null!, typeof(string), null!, Inv);
        Assert.Equal("—", result);
    }

    [Fact]
    public void NullableDate_FormatsDateTime_WhenValueProvided()
    {
        var dt = new DateTime(2026, 6, 11, 10, 30, 0, DateTimeKind.Utc);
        var result = new NullableDateConverter().Convert(dt, typeof(string), null!, Inv) as string;
        Assert.NotNull(result);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}", result);
    }
}
