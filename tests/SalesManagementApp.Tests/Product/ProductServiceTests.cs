using System.Collections.Generic;
using NUnit.Framework;
using SalesManagementApp.Core.Application.Exceptions;
using SalesManagementApp.Core.Application.Services;
using SalesManagementApp.Tests.TestHelpers;

namespace SalesManagementApp.Tests.Product;

/// <summary>
/// ProductService の仕様を検証するNUnitテストクラスです。
/// </summary>
public class ProductServiceTests
{
    private ProductService FService = null!;
    private List<SalesManagementApp.Core.Domain.Entities.Product> FProducts = null!;

    [SetUp]
    /// <summary>
    /// 各テストの実行前にテストデータと依存オブジェクトを初期化します。
    /// </summary>
    public void SetUp()
    {
        FService = new ProductService();
        FProducts = new List<SalesManagementApp.Core.Domain.Entities.Product>();
    }

    [Test]
    /// <summary>
    /// 正常な商品情報を登録した場合に商品一覧へ追加されることを検証します。
    /// </summary>
    public void Register_WhenInputIsValid_AddsProduct()
    {
        var product = new ProductBuilder().WithId("P100").Build();

        FService.Register(FProducts, product);

        Assert.That(FProducts.Count, Is.EqualTo(1));
        Assert.That(FProducts[0].ProductId, Is.EqualTo("P100"));
    }

    [Test]
    /// <summary>
    /// 既存と同じ商品IDを登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenProductIdIsDuplicate_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 末尾空白を含む重複商品IDを登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenProductIdHasTrailingSpaceAndDuplicateExists_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());
        var duplicate = new ProductBuilder().WithId("P001 ").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 前後空白を除去すると重複する商品名を登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenProductNameIsDuplicateAfterTrim_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("  Coffee  ").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 大文字小文字だけが異なる商品名を登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenProductNameDiffersOnlyByCase_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("coffee").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 全角半角だけが異なる商品名を登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenProductNameDiffersOnlyByFullHalfWidth_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("ｺｰﾋｰ").Build());
        var duplicate = new ProductBuilder().WithId("P002").WithName("コーヒー").Build();

        Assert.That(() => FService.Register(FProducts, duplicate), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 単価が負数の商品を登録した場合に検証例外が発生することを確認します。
    /// </summary>
    public void Register_WhenPriceIsNegative_ThrowsValidationException()
    {
        var invalid = new ProductBuilder().WithPrice(-1).Build();

        Assert.That(() => FService.Register(FProducts, invalid), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 既存商品の更新を行った場合に名称・単価・カテゴリが反映されることを検証します。
    /// </summary>
    public void Update_WhenProductExists_UpdatesFields()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        FService.Update(FProducts, update);

        Assert.That(FProducts[0].ProductName, Is.EqualTo("New"));
        Assert.That(FProducts[0].UnitPrice, Is.EqualTo(999));
        Assert.That(FProducts[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    /// <summary>
    /// 商品IDに末尾空白があっても対象商品を特定して更新できることを検証します。
    /// </summary>
    public void Update_WhenProductIdHasTrailingSpace_FindsAndUpdatesTarget()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Old").Build());
        var update = new ProductBuilder().WithId("P001 ").WithName("New").WithPrice(999).WithCategory("Snack").Build();

        FService.Update(FProducts, update);

        Assert.That(FProducts[0].ProductId, Is.EqualTo("P001"));
        Assert.That(FProducts[0].ProductName, Is.EqualTo("New"));
        Assert.That(FProducts[0].UnitPrice, Is.EqualTo(999));
        Assert.That(FProducts[0].Category, Is.EqualTo("Snack"));
    }

    [Test]
    /// <summary>
    /// 更新後の商品名が他商品と重複する場合に検証例外が発生することを確認します。
    /// </summary>
    public void Update_WhenProductNameDuplicatesAnotherProduct_ThrowsValidationException()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Coffee").Build());
        FProducts.Add(new ProductBuilder().WithId("P002").WithName("Tea").Build());
        var update = new ProductBuilder().WithId("P002").WithName(" coffee ").Build();

        Assert.That(() => FService.Update(FProducts, update), Throws.TypeOf<DomainValidationException>());
    }

    [Test]
    /// <summary>
    /// 既存商品を削除した場合に商品一覧から対象が除外されることを検証します。
    /// </summary>
    public void Delete_WhenProductExists_RemovesProduct()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").Build());

        FService.Delete(FProducts, "P001");

        Assert.That(FProducts, Is.Empty);
    }

    [Test]
    /// <summary>
    /// ID・名称・カテゴリ条件に一致する商品だけを絞り込めることを検証します。
    /// </summary>
    public void GetFiltered_WhenConditionsMatch_ReturnsFilteredProducts()
    {
        FProducts.Add(new ProductBuilder().WithId("P001").WithName("Cola").WithCategory("Drink").Build());
        FProducts.Add(new ProductBuilder().WithId("P002").WithName("Tea").WithCategory("Drink").Build());
        FProducts.Add(new ProductBuilder().WithId("P003").WithName("Bread").WithCategory("Food").Build());

        var filtered = FService.GetFiltered(FProducts, "P00", "ea", "Drink");

        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered[0].ProductId, Is.EqualTo("P002"));
    }
}
