using FluentValidation;
using QLSanPham.Application.Features.Common;
using QLSanPham.Application.Features.SanPham.Commands;
using QLSanPham.Application.Features.SanPham.Queries;

namespace QLSanPham.Application.Features.SanPham.Validation;

public sealed class GetSanPhamListQueryValidator : AbstractValidator<GetSanPhamListQuery>
{
    public GetSanPhamListQueryValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("page phải lớn hơn hoặc bằng 1.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("pageSize phải nằm trong khoảng từ 1 đến 100.");

        RuleFor(request => request.LoaiId)
            .Must(value => value is null or > 0)
            .WithMessage("loaiId phải lớn hơn hoặc bằng 1.");

        RuleFor(request => request.SortBy)
            .Must(value => CatalogValidationRules.IsAllowedValue(
                value, "id", "tensp", "gia", "soluong", "ngay"))
            .When(request => request.SortBy is not null)
            .WithMessage("sortBy chỉ hỗ trợ id, tenSP, gia, soLuong hoặc ngay.");

        RuleFor(request => request.SortOrder)
            .Must(value => CatalogValidationRules.IsAllowedValue(value, "asc", "desc"))
            .When(request => request.SortOrder is not null)
            .WithMessage("sortOrder chỉ hỗ trợ asc hoặc desc.");
    }
}

public sealed class GetSanPhamByIdQueryValidator : AbstractValidator<GetSanPhamByIdQuery>
{
    public GetSanPhamByIdQueryValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");
    }
}

public sealed class CreateSanPhamCommandValidator : AbstractValidator<CreateSanPhamCommand>
{
    public CreateSanPhamCommandValidator()
    {
        RuleFor(request => request.Data)
            .NotNull()
            .WithMessage("Thông tin sản phẩm không được để trống.");

        When(request => request.Data is not null, () =>
        {
            AddDataRules();
        });
    }

    private void AddDataRules()
    {
        CatalogValidationRules.RequiredTrimmedName(
            RuleFor(request => request.Data.TenSP), "Tên sản phẩm", 150);

        RuleFor(request => request.Data.MoTaSP)
            .MaximumLength(500)
            .WithMessage("Mô tả sản phẩm không được vượt quá 500 ký tự.");

        RuleFor(request => request.Data.Gia)
            .InclusiveBetween(0.01m, 9999999999999999m)
            .WithMessage("Giá phải lớn hơn 0 và không vượt giới hạn cho phép.");

        RuleFor(request => request.Data.SoLuong)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Số lượng không được âm.");

        RuleFor(request => request.Data.LoaiId)
            .GreaterThan(0)
            .WithMessage("Vui lòng chọn loại sản phẩm.");
    }
}

public sealed class UpdateSanPhamCommandValidator : AbstractValidator<UpdateSanPhamCommand>
{
    public UpdateSanPhamCommandValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");

        RuleFor(request => request.Data)
            .NotNull()
            .WithMessage("Thông tin sản phẩm không được để trống.");

        When(request => request.Data is not null, () =>
        {
            AddDataRules();
        });
    }

    private void AddDataRules()
    {
        CatalogValidationRules.RequiredTrimmedName(
            RuleFor(request => request.Data.TenSP), "Tên sản phẩm", 150);

        RuleFor(request => request.Data.MoTaSP)
            .MaximumLength(500)
            .WithMessage("Mô tả sản phẩm không được vượt quá 500 ký tự.");

        RuleFor(request => request.Data.Gia)
            .InclusiveBetween(0.01m, 9999999999999999m)
            .WithMessage("Giá phải lớn hơn 0 và không vượt giới hạn cho phép.");

        RuleFor(request => request.Data.SoLuong)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Số lượng không được âm.");

        RuleFor(request => request.Data.LoaiId)
            .GreaterThan(0)
            .WithMessage("Vui lòng chọn loại sản phẩm.");
    }
}

public sealed class DeleteSanPhamCommandValidator : AbstractValidator<DeleteSanPhamCommand>
{
    public DeleteSanPhamCommandValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");
    }
}
