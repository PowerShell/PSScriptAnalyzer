using System.Text;

namespace CrossCompatibility.Utility
{
    /// <summary>
    /// Enumeration of Windows OS SKUs corresponding
    /// to GetProfileInfo() from WinNT.h.
    /// </summary>
    public enum WindowsSku
    {
        Undefined = 0,

        Ultimate = 1,

        HomeBasic = 2,

        HomePremium = 3,

        Enterprise = 4,

        Business = 6,

        StandardServer = 7,

        DatacenterServer = 8,

        SmallbusinessServer = 9,

        EnterpriseServer = 10,

        Starter = 11,

        DatacenterServerCore = 12,

        StandardServerCore = 13,

        EnterpriseServerCore = 14,

        WebServer = 17,

        HomeServer = 19,

        StorageExpressServer = 20,

        StorageStandardServer = 21,

        StorageWorkgroupServer = 22,

        StorageEnterpriseServer = 23,

        ServerForSmallbusiness = 24,

        SmallbusinessServerPremium = 25,

        EnterpriseN = 27,

        UltimateN = 28,

        WebServerCore = 29,

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

        SbSolutionServer = 50,

        SmallbusinessServerPremiumCore = 63,

        ClusterServerV = 64,

        CoreArm = 97,

        Core = 101,

        ProfessionalWmc = 103,

        MobileCore = 104,

        Iotuap = 123,

        DatacenterNanoServer = 143,

        StandardNanoServer = 144,

        DatacenterWsServerCore = 147,

        StandardWsServerCore = 148,
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