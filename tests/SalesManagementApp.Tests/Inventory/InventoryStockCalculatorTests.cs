using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;

namespace SalesManagementApp.Tests.Inventory;

/// <summary>
/// InventoryStockCalculator の仕様を検証するNUnitテストクラスです。
/// </summary>
public class InventoryStockCalculatorTests
{
    private InventoryStockCalculator FCalculator = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FCalculator = new InventoryStockCalculator();
    }

    [Test]
    /// <summary>
    /// 現在在庫と入庫数量が妥当な場合に入庫後在庫を正しく計算できることを検証します。
    /// </summary>
    public void CalculateAfterInbound_WhenInputsAreValid_ReturnsIncreasedStock()
    {
        var wResult = FCalculator.CalculateAfterInbound(10, 5);

        Assert.That(wResult, Is.EqualTo(15));
    }

    [Test]
    /// <summary>
    /// 入庫後在庫がオーバーフローする場合に検証例外が発生することを確認します。
    /// </summary>
    public void CalculateAfterInbound_WhenResultOverflows_ThrowsValidationException()
    {
        Assert.That(
            () => FCalculator.CalculateAfterInbound(int.MaxValue, 1),
            Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 現在在庫と出庫数量が妥当な場合に出庫後在庫を正しく計算できることを検証します。
    /// </summary>
    public void CalculateAfterOutbound_WhenInputsAreValid_ReturnsDecreasedStock()
    {
        var wResult = FCalculator.CalculateAfterOutbound(10, 4);

        Assert.That(wResult, Is.EqualTo(6));
    }

    [Test]
    /// <summary>
    /// 出庫数量が在庫数を上回る場合に検証例外が発生することを確認します。
    /// </summary>
    public void CalculateAfterOutbound_WhenStockIsInsufficient_ThrowsValidationException()
    {
        Assert.That(
            () => FCalculator.CalculateAfterOutbound(3, 4),
            Throws.TypeOf<DomainValidationException>());
    }
}
