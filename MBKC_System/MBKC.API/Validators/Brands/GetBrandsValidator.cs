﻿using FluentValidation;
using MBKC.Service.DTOs.Brands;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MBKC.API.Validators.Brands
{
    public class GetBrandsValidator : AbstractValidator<GetBrandsRequest>
    {
        public GetBrandsValidator()
        {
            RuleFor(x => x.CurrentPage)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Custom((currentPage, context) =>
                {
                    if (currentPage is not null && currentPage <= 0)
                    {
                        context.AddFailure("CurrentPage", "Current page number is required more than 0.");
                    }
                });

            RuleFor(x => x.ItemsPerPage)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Custom((itemsPerPage, context) =>
                {
                    if (itemsPerPage is not null && itemsPerPage <= 0)
                    {
                        context.AddFailure("ItemsPerPage", "Items per page number is required more than 0.");
                    }
                });

            RuleFor(x => x.SortBy)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .Custom((sortBy, context) =>
                {
                    PropertyInfo[] properties = typeof(GetBrandResponse).GetProperties();
                    string strRegex = @"(^[a-zA-Z]*_(ASC|asc)$)|(^[a-zA-Z]*_(DESC|desc))";
                    Regex regex = new Regex(strRegex);
                    if (sortBy is not null)
                    {
                        if (regex.IsMatch(sortBy.Trim()) == false)
                        {
                            context.AddFailure("SortBy", "Sort by is required following format: propertyName_ASC | propertyName_DESC.");
                        }
                        string[] sortByParts = sortBy.Split("_");
                        if (properties.Any(x => x.Name.ToLower().Equals(sortByParts[0].Trim().ToLower())) == false)
                        {
                            context.AddFailure("SortBy", "Property name in format does not exist in the system.");
                        }
                    }
                });
        }
    }
}
