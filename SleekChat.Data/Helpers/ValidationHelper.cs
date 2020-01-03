using System;
using System.Collections.Generic;

namespace SleekChat.Data.Helpers
{
    public class ValidationHelper
    {
        private KeyValuePair<bool, string> result;

        public KeyValuePair<bool, string> IsBlank(string fieldName, string fieldValue)
        {
            return string.IsNullOrEmpty(fieldValue)
                ? Result($"'{fieldName}' is required but was not provided.")
                : Result(string.Empty);
        }


        public KeyValuePair<bool, string> IsValidActiveStatus(string fieldName, string fieldValue)
        {
            return Boolean.TryParse(fieldValue, out _)
                ? Result(string.Empty)
                : Result($"The entry for '{fieldName}' can only be 'true' or 'false'");
        }


        public KeyValuePair<bool, string> IsValidGuid(string fieldName, string fieldValue)
        {
            return Guid.TryParse(fieldValue, out _)
                ? Result(string.Empty)
                : Result($"The '{fieldName}' provided is not valid.");
        }


        public KeyValuePair<bool, string> IsValidEmail(string value)
        {
            try
            {
                _ = new System.Net.Mail.MailAddress(value);
                return Result(string.Empty);
            }
            catch (FormatException)
            {
                return Result("Email address is not in the right format.");
            }
        }


        public KeyValuePair<bool, string> PasswordsMatch(string value1, string value2)
        {
            result = IsBlank("Password", value1);
            if (result.Key == false)
                return result;

            result = IsBlank("Password Re-type", value2);
            if (result.Key == false)
                return result;

            return StringComparer.InvariantCulture.Compare(value1, value2) == 0
                ? Result(string.Empty)
                : Result($"Both passwords do not match.");
        }


        public KeyValuePair<bool, string> Result(string errorMsg)
        {
            return string.IsNullOrEmpty(errorMsg)
                ? new KeyValuePair<bool, string>(true, string.Empty)
                : new KeyValuePair<bool, string>(false, errorMsg);
        }
    }
}
