using FluentValidation;
using QLSanPham.Application.Features.Common;
using QLSanPham.Application.Features.Loai.Commands;
using QLSanPham.Application.Features.Loai.Queries;

namespace QLSanPham.Application.Features.Loai.Validation;

public sealed class GetLoaiListQueryValidator : AbstractValidator<GetLoaiListQuery>
{
    public GetLoaiListQueryValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("page phải lớn hơn hoặc bằng 1.");

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("pageSize phải nằm trong khoảng từ 1 đến 100.");

        RuleFor(request => request.SortBy)
            .Must(value => CatalogValidationRules.IsAllowedValue(value, "id", "ten"))
            .When(request => request.SortBy is not null)
            .WithMessage("sortBy chỉ hỗ trợ id hoặc ten.");

        RuleFor(request => request.SortOrder)
            .Must(value => CatalogValidationRules.IsAllowedValue(value, "asc", "desc"))
            .When(request => request.SortOrder is not null)
            .WithMessage("sortOrder chỉ hỗ trợ asc hoặc desc.");
    }
}

public sealed class GetLoaiByIdQueryValidator : AbstractValidator<GetLoaiByIdQuery>
{
    public GetLoaiByIdQueryValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");
    }
}

public sealed class CreateLoaiCommandValidator : AbstractValidator<CreateLoaiCommand>
{
    public CreateLoaiCommandValidator()
    {
        RuleFor(request => request.Data)
            .NotNull()
            .WithMessage("Thông tin loại sản phẩm không được để trống.");

        When(request => request.Data is not null, () =>
        {
            CatalogValidationRules.RequiredTrimmedName(
                RuleFor(request => request.Data.Ten), "Tên loại", 100);

            RuleFor(request => request.Data.Mota)
                .MaximumLength(255)
                .WithMessage("Mô tả loại không được vượt quá 255 ký tự.");
        });
    }
}

public sealed class UpdateLoaiCommandValidator : AbstractValidator<UpdateLoaiCommand>
{
    public UpdateLoaiCommandValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");

        RuleFor(request => request.Data)
            .NotNull()
            .WithMessage("Thông tin loại sản phẩm không được để trống.");

        When(request => request.Data is not null, () =>
        {
            CatalogValidationRules.RequiredTrimmedName(
                RuleFor(request => request.Data.Ten), "Tên loại", 100);

            RuleFor(request => request.Data.Mota)
                .MaximumLength(255)
                .WithMessage("Mô tả loại không được vượt quá 255 ký tự.");
        });
    }
}

public sealed class DeleteLoaiCommandValidator : AbstractValidator<DeleteLoaiCommand>
{
    public DeleteLoaiCommandValidator()
    {
        RuleFor(request => request.Id)
            .GreaterThan(0)
            .WithMessage("id phải lớn hơn hoặc bằng 1.");
    }
}
