// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text;

namespace Microsoft.PowerShell.CrossCompatibility
{
    /// <summary>
    /// Enumeration of Windows OS SKUs corresponding
    /// to GetProfileInfo() from WinNT.h.
    /// </summary>
    public enum WindowsSku : uint
    {
        Undefined = 0,

        Ultimate = 1,

        HomeBasic = 2,

        HomePremium = 3,

        Enterprise = 4,

        HomeBasicN = 5,

        Business = 6,

        StandardServer = 7,

        DatacenterServer = 8,

        SmallbusinessServer = 9,

        EnterpriseServer = 10,

        Starter = 11,

        DatacenterServerCore = 12,

        StandardServerCore = 13,

        EnterpriseServerCore = 14,

        EnterpriseServerIa64 = 15,

        BusinessN = 16,

        WebServer = 17,

        ClusterServer = 18,

        HomeServer = 19,

        StorageExpressServer = 20,

        StorageStandardServer = 21,

        StorageWorkgroupServer = 22,

        StorageEnterpriseServer = 23,

        ServerForSmallbusiness = 24,

        SmallbusinessServerPremium = 25,

        HomePremiumN = 26,

        EnterpriseN = 27,

        UltimateN = 28,

        WebServerCore = 29,

        MediumbusinessServerManagement = 30,

        MediumbusinessServerSecurity = 31,

        MediumbusinessServerMessaging = 32,

        ServerFoundation = 33,

        HomePremiumServer = 34,

        ServerForSmallbusinessV = 35,

        StandardServerV = 36,

        DatacenterServerV = 37,

        EnterpriseServerC = 38,

        DatacenterServerCoreV = 39,

        StandardServerCoreV = 40,

        EnterpriseServerCoreV = 41,

        Hyperv = 42,

        StorageExpressServerCore = 43,

        StorageStandardServerCore = 44,

        StorageWorkgroupServerCore = 45,

        StorageEnterpriseServerCore = 46,

        StarterN = 47,

        Professional = 48,

        ProfessionalN = 49,

        SbSolutionServer = 50,

        ServerForSbSolutions = 51,

        StandardServerSolutions = 52,

        StandardServerSolutionsCore = 53,

        SbSolutionServerEm = 54,

        ServerForSbSolutionsEm = 55,

        SolutionEmbeddedserver = 56,

        EssentialbusinessServerMgmt = 59,

        EssentialbusinessServerAddl = 60,

        EssentialbusinessServerMgmtsvc = 61,

        EssentialbusinessServerAddlsvc = 62,

        SmallbusinessServerPremiumCore = 63,

        ClusterServerV = 64,

        StarterE = 66,

        HomeBasicE = 67,

        HomePremiumE = 68,

        ProfessionalE = 69,

        EnterpriseE = 70,

        UltimateE = 71,

        EnterpriseEvaluation = 72,

        MultipointStandardServer = 76,

        MultipointPremiumServer = 77,

        StandardEvaluationServer = 79,

        AtacenterEvaluationServer = 80,

        EnterpriseNEvaluation = 84,

        StorageWorkgroupEvaluationServer = 95,

        StorageStandardEvaluationServer = 96,

        CoreArm = 97,

        CoreN = 98,

        CoreCountryspecific = 99,

        CoreSinglelanguage = 100,

        Core = 101,

        ProfessionalWmc = 103,

        MobileCore = 104,

        Education = 121,

        EducationN = 122,

        Iotuap = 123,

        EnterpriseS = 125,

        EnterpriseSN = 126,

        EnterpriseSEvaluation = 129,

        EnterpriseSNEvaluation = 130,

        Iotuapcommercial = 131,

        MobileEnterprise = 133,

        DatacenterNanoServer = 143,

        StandardNanoServer = 144,

        DatacenterAServerCore = 145,

        StandardAServerCore = 146,

        DatacenterWsServerCore = 147,

        StandardWsServerCore = 148,

        ProWorkstation = 161,

        ProWorkstationN = 162,
    }

    /// <summary>
    /// Helper methods for Windows SKU enumeration handling.
    /// </summary>
    public static class WindowsSkuMethods
    {
        /// <summary>
        /// Converts a SKU enumeration to the corresponding GetProductInfo() enum name.
        /// </summary>
        /// <param name="sku">The SKU enumeration value.</param>
        /// <returns>The full name of the enumeration value from the return of the GetProductInfo() call.</returns>
        public static string GetProductInfoName(this WindowsSku sku)
        {
            const string prefix = "PRODUCT";
            string skuEnumName = sku.ToString();

            int minimumLength = prefix.Length + skuEnumName.Length;
            var sb = new StringBuilder(minimumLength).Append(prefix);
            foreach (char c in skuEnumName)
            {
                if (char.IsUpper(c))
                {
                    sb.Append('_');
                }

                sb.Append(char.ToUpper(c));
            }

            return sb.ToString();
        }
    }
}
