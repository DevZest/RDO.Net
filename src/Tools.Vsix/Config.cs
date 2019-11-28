namespace DevZest.Data.Tools
{
    static class Config
    {
        public static string LicenseServerBaseUrl
        {
            get
            {
#if DEBUG
                return "https://localhost:44312/api/license/";
#else
                return "https://my.devzest.com/api/license/";
#endif
            }
        }

        public static string RsaPublicKey
        {
            get
            {
#if DEBUG
                return "rEeITI8fDU+sj94bxje9Rz2DDCIlq0Lz4NwpiaR8FNcbUXQKKHzsE4E1cdsWI88coU0q5JMxQgX+q5loZGwxRNefqYn+8MnhXUF5q78eMqBeFzyi+t2b5UIQE0ZS0xSAAwSV9CM6l9s2JTTG48oF0p5lKTzCrxg0ZqaK4Mcz6vk=.AQAB";
#else
                return "0GulvyMBmh8YWTeQOXEeFLKkS87eTmj8xLi7zMM678WErFrQLz10rdSYMBYvsGagxCl1CPCw01RufFpepglpPF0HYkS6E+eL16ZS8lrvnR3yKeVe/wQL7ZRf1+8M/63IBEKLcajZwMY6FnqdmTADXzBKyWnA7MNsl2aViNv3Se0=.AQAB";
#endif
            }
        }

        public static string AzureIotHubHostName
        {
            get
            {
                return "my-devzest-dev.azure-devices.net";
//#if DEBUG
//                return "my-devzest-dev.azure-devices.net";
//#else
//                return "my-devzest.azure-devices.net";
//#endif
            }
        }
    }
}
