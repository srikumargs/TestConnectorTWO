
namespace Sage.Connector.MockPlugin.Configuration
{
    /// <summary>
    /// Keys used for management and connection calls.
    /// These are for use in both description an current values.
    /// Any given back office may have their own set of these. These are simply the ones used by the mock.
    /// As management and connection credentials share the same form but may or may not overlap 
    /// </summary>
    static public class CommonPropertyKeys
    {
        /// <summary>
        /// Example to specify the user
        /// </summary>
        static public readonly string UserId = "UserId";
        
        /// <summary>
        /// example to specify the password for the user
        /// </summary>
        static public readonly string Password = "Password";

        /// <summary>
        /// Example to specify the company.
        /// </summary>
        static public readonly string CompanyId = "CompanyId";

        /// <summary>
        /// Example to specify the company path for back offices that use it.
        /// </summary>
        static public readonly string CompanyPath = "CompanyPath";

        /// <summary>
        /// Example "Special" key to show round tripping
        /// </summary>
        static public readonly string BackOfficeModeAdminKey = "DummyBackOfficeSpecialValue";
    }
}
