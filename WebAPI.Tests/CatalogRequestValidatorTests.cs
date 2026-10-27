using QLSanPham.Application.Features.Loai.Commands;
using QLSanPham.Application.Features.Loai.Queries;
using QLSanPham.Application.Features.Loai.Validation;
using QLSanPham.Application.Features.SanPham.Commands;
using QLSanPham.Application.Features.SanPham.Queries;
using QLSanPham.Application.Features.SanPham.Validation;
using QLSanPham.Application.DTOs.Loai;
using QLSanPham.Application.DTOs.SanPham;

namespace WebAPI.Tests;

public class CatalogRequestValidatorTests
{
    [Fact]
    public void GetLoaiListQueryValidator_RejectsInvalidPagingAndSorting()
    {
        var validator = new GetLoaiListQueryValidator();
        var query = new GetLoaiListQuery(0, 101, "createdAt", "sideways");

        var result = validator.Validate(query);

        Assert.Contains(result.Errors, error => error.PropertyName == "Page");
        Assert.Contains(result.Errors, error => error.PropertyName == "PageSize");
        Assert.Contains(result.Errors, error => error.PropertyName == "SortBy");
        Assert.Contains(result.Errors, error => error.PropertyName == "SortOrder");
    }

    [Fact]
    public void GetSanPhamListQueryValidator_AcceptsNormalizedSortValuesAndNullFilter()
    {
        var validator = new GetSanPhamListQueryValidator();
        var query = new GetSanPhamListQuery(1, 100, null, "  TenSP ", " DESC ", null);

        var result = validator.Validate(query);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateLoaiCommandValidator_RejectsBlankOrOversizedNameAndDescription()
    {
        var validator = new CreateLoaiCommandValidator();
        var command = new CreateLoaiCommand(new CreateLoaiDto
        {
            Ten = "   ",
            Mota = new string('x', 256)
        });

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Data.Ten");
        Assert.Contains(result.Errors, error => error.PropertyName == "Data.Mota");
    }

    [Fact]
    public void CreateSanPhamCommandValidator_RejectsInvalidNameDescriptionPriceQuantityAndCategory()
    {
        var validator = new CreateSanPhamCommandValidator();
        var command = new CreateSanPhamCommand(new CreateSanPhamDto
        {
            TenSP = "   ",
            MoTaSP = new string('x', 501),
            Gia = 0,
            SoLuong = -1,
            LoaiId = 0
        });

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, error => error.PropertyName == "Data.TenSP");
        Assert.Contains(result.Errors, error => error.PropertyName == "Data.MoTaSP");
        Assert.Contains(result.Errors, error => error.PropertyName == "Data.Gia");
        Assert.Contains(result.Errors, error => error.PropertyName == "Data.SoLuong");
        Assert.Contains(result.Errors, error => error.PropertyName == "Data.LoaiId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DeleteSanPhamCommandValidator_RejectsNonPositiveId(int id)
    {
        var validator = new DeleteSanPhamCommandValidator();

        var result = validator.Validate(new DeleteSanPhamCommand(id));

        Assert.Contains(result.Errors, error => error.PropertyName == "Id");
    }

    [Fact]
    public void GetLoaiByIdQueryValidator_AllowsPositiveId()
    {
        var validator = new GetLoaiByIdQueryValidator();

        var result = validator.Validate(new GetLoaiByIdQuery(1));

        Assert.True(result.IsValid);
    }
}
