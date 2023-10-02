﻿namespace MBKC.API.Constants
{
    public static class MessageConstant
    {
        public static class AuthenticationMessage
        {
            public const string ResetPasswordSuccessfully = "Reset Password Successfully.";
        }

        public static class VerificationMessage
        {
            public const string SentEmailConfirmationSuccessfully = "Sent Email Confirmation Successfully.";
            public const string ConfirmedOTPCodeSuccessfully = "Confirmed OTP Code Successfully.";
        }
        public static class KitchenCenterMessage
        {
            public const string CreatedNewKitchenCenterSuccessfully = "Created New Kitchen Center Successfully.";
            public const string UpdatedKitchenCenterSuccessfully = "Updated Kitchen Center Information Successfully.";
            public const string UpdatedKitchenCenterStatusSuccessfully = "Updated Kitchen Center Status Successfully.";
            public const string DeletedKitchenCenterSuccessfully = "Deleted Kitchen Center Successfully.";
        }

        public static class BrandMessage
        {
            public const string CreatedNewBrandSuccessfully = "Created New Brand Successfully.";
            public const string UpdatedBrandSuccessfully = "Updated Brand Successfully.";
            public const string UpdatedBrandStatusSuccessfully = "Updated Brand Status Successfully.";
            public const string UpdatedBrandProfileSuccessfully = "Updated Brand Profile Successfully.";
            public const string DeletedBrandSuccessfully = "Deleted brand successfully.";
        }

        public static class StoreMessage
        {
            public const string UpdatedStoreStatusSuccessfully = "Updated Store Status Successfully.";
            public const string UpdatedStoreInformationSuccessfully = "Updated Store Information Successfully.";
            public const string DeletedStoreSuccessfully = "Deleted Store Successfully.";
            public const string RegisteredNewStoreSuccessfully = "Registered New Store Successfully.";
            public const string ConfirmedStoreRegistrationSuccessfully = "Confirmed Store Registration Successfully.";
        }

        public static class CategoryMessage
        {
            public const string CreatedNewCategorySuccessfully = "Created New Category Successfylly.";
            public const string UpdatedCategorySuccessfully = "Updated Category Successfully.";
            public const string DeletedCategorySuccessfully = "Deleted Category Successfully.";
            public const string CreatedExtraCategoriesToNormalCategorySuccessfully = "Create Extra-Categories To Normal Category Successfully.";
        }

        public static class BankingAccountMessage
        {
            public const string CreatedNewBankingAccountSuccessfully = "Created New Banking Account Successfully.";
            public const string DeletedBankingAccountSuccessfully = "Deleted Banking Account Successfully.";
            public const string UpdatedStatusBankingAccountSuccessfully = "Updated Banking Account Status Successfully.";
            public const string UpdatedBankingAccountSuccessfully = "Updated Banking Account Information Successfully.";
        }
    }
}