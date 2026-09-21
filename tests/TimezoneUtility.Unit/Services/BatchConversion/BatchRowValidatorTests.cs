using FluentAssertions;
using TimezoneUtility.Models;
using TimezoneUtility.Services.BatchConversion;

namespace TimezoneUtility.Unit.Services.BatchConversion;

/// <summary>
/// T022: Unit tests for BatchRowValidator equivalence classes.
/// Verifies REQ-003, REQ-008, REQ-009, REQ-010, REQ-015, REQ-028.
/// Corresponds to STP-003-A, STP-003-B (STS-003-A1, STS-003-A2, STS-003-B1, STS-003-B2).
/// </summary>
public class BatchRowValidatorTests
{
    private static BatchConversionRow Row(string ts, string src, string tgt, int rowNumber = 1) => new()
    {
        RowNumber = rowNumber,
        RawTimestamp = ts,
        RawSourceTimezone = src,
        RawTargetTimezone = tgt
    };

    [Fact]
    public void STS_003_B1_FullyValidRow_Passes()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "America/New_York", "Asia/Tokyo"));

        result.IsValid.Should().BeTrue();
        result.SourceZone.Should().NotBeNull();
        result.TargetZone.Should().NotBeNull();
    }

    [Fact]
    public void STS_003_A2_UnrecognizedSourceTimezone_FailsWithFieldSpecificReason()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "Not/AZone", "UTC"));

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("timezone");
        result.Reason.Should().Contain("Not/AZone");
    }

    [Fact]
    public void REQ_010_UnrecognizedTargetTimezone_Fails()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "UTC", "Europe/Atlantis"));

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("timezone");
        result.Reason.Should().Contain("Europe/Atlantis");
    }

    [Fact]
    public void REQ_015_MissingSourceTimezone_FailsIdentifyingTheField()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "", "UTC"));

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("source_timezone");
    }

    [Fact]
    public void REQ_015_MissingTimestamp_FailsIdentifyingTheField()
    {
        var result = BatchRowValidator.Validate(Row("", "UTC", "UTC"));

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("timestamp");
    }

    [Fact]
    public void REQ_015_MissingTargetTimezone_FailsIdentifyingTheField()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "UTC", ""));

        result.IsValid.Should().BeFalse();
        result.Reason.Should().Contain("target_timezone");
    }

    [Fact]
    public void STS_003_B2_ValidIanaIdentifiers_AreRecognized()
    {
        var result = BatchRowValidator.Validate(Row("2026-01-01 00:00:00", "America/New_York", "Asia/Tokyo"));

        result.IsValid.Should().BeTrue();
        result.SourceZone!.Id.Should().Be("America/New_York");
        result.TargetZone!.Id.Should().Be("Asia/Tokyo");
    }
}
