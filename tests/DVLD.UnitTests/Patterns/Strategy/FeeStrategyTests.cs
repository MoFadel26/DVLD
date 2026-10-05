using DVLD.Application.Patterns.Strategy;
using DVLD.Domain.Enums;
using Xunit;

namespace DVLD.UnitTests.Patterns.Strategy;

public class FeeStrategyTests
{
    [Fact]
    public async Task NewLicenseFeeStrategy_CalculatesBaseFeePlusClassFee()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(NewLicenseFeeStrategy_CalculatesBaseFeePlusClassFee));
        var strategy = new NewLicenseFeeStrategy(uow);

        // Class 3 fee is $20; Base App fee is $5 -> Total = $25
        var request = new FeeCalculationRequest(EnApplicationType.NewDrivingLicense, LicenseClassId: 3);
        decimal total = await strategy.CalculateTotalFeeAsync(request);

        Assert.Equal(25.00m, total);
    }

    [Fact]
    public async Task RetakeTestFeeStrategy_CalculatesBaseFeePlusTestFee()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(RetakeTestFeeStrategy_CalculatesBaseFeePlusTestFee));
        var strategy = new RetakeTestFeeStrategy(uow);

        // Vision test fee is $10; Base Retake fee is $5 -> Total = $15
        var request = new FeeCalculationRequest(EnApplicationType.RetakeTest, TestType: EnTestType.VisionTest);
        decimal total = await strategy.CalculateTotalFeeAsync(request);

        Assert.Equal(15.00m, total);
    }

    [Fact]
    public async Task ReplaceLostFeeStrategy_CalculatesTotalCorrectly()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(ReplaceLostFeeStrategy_CalculatesTotalCorrectly));
        var strategy = new ReplaceLostFeeStrategy(uow);

        // Base $5 + Lost $10 = $15
        var request = new FeeCalculationRequest(EnApplicationType.ReplaceLostDrivingLicense);
        decimal total = await strategy.CalculateTotalFeeAsync(request);

        Assert.Equal(15.00m, total);
    }

    [Fact]
    public async Task ReplaceDamagedFeeStrategy_CalculatesTotalCorrectly()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(ReplaceDamagedFeeStrategy_CalculatesTotalCorrectly));
        var strategy = new ReplaceDamagedFeeStrategy(uow);

        // Base $5 + Damaged $5 = $10
        var request = new FeeCalculationRequest(EnApplicationType.ReplaceDamagedDrivingLicense);
        decimal total = await strategy.CalculateTotalFeeAsync(request);

        Assert.Equal(10.00m, total);
    }

    [Fact]
    public async Task ReleaseDetainedFeeStrategy_CalculatesBasePlusFine()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(ReleaseDetainedFeeStrategy_CalculatesBasePlusFine));
        var strategy = new ReleaseDetainedFeeStrategy(uow);

        // Base $5 + Fine $120 = $125
        var request = new FeeCalculationRequest(EnApplicationType.ReleaseDetainedDrivingLicense, AdditionalFee: 120.00m);
        decimal total = await strategy.CalculateTotalFeeAsync(request);

        Assert.Equal(125.00m, total);
    }

    [Fact]
    public async Task FeeCalculationContext_DispatchesToCorrectStrategy()
    {
        var (_, uow) = TestDbContextFactory.Create(nameof(FeeCalculationContext_DispatchesToCorrectStrategy));
        var strategies = new IFeeCalculationStrategy[]
        {
            new NewLicenseFeeStrategy(uow),
            new RetakeTestFeeStrategy(uow),
            new RenewLicenseFeeStrategy(uow),
            new ReplaceLostFeeStrategy(uow),
            new ReplaceDamagedFeeStrategy(uow),
            new ReleaseDetainedFeeStrategy(uow),
            new InternationalLicenseFeeStrategy(uow)
        };

        var context = new FeeCalculationContext(strategies);

        var request = new FeeCalculationRequest(EnApplicationType.IssueInternationalLicense);
        decimal total = await context.CalculateFeeAsync(request);

        // Base $5 + International $50 = $55
        Assert.Equal(55.00m, total);
    }
}
