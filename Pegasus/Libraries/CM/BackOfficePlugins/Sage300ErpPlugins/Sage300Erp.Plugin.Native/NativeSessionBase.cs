using ACCPAC.Advantage;
using Sage.Connector.DomainContracts.Core;
using Sage.Connector.DomainContracts.Core.BackOffice;
using System;

namespace Sage300Erp.Plugin.Native
{
    /// <summary>
    /// Abstract Native base handles the back office session
    /// </summary>
    public class NativeSessionBase: IBackOfficeSessionHandler, IDisposable
    {

        protected Session ErpSession = null;
        protected DBLink DbLink = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backOfficeConfiguration"></param>
        /// <returns></returns>
        public virtual Response BeginSession(IBackOfficeCompanyConfiguration backOfficeConfiguration)
        {
            var response = new Response();
            try
            {
                ErpSession = Common.GetErpSession(backOfficeConfiguration);
                DbLink = ErpSession.OpenDBLink(DBLinkType.Company, DBLinkFlags.ReadOnly);
            }
            catch (Exception ex)
            {
                var diagnosis = new Diagnosis
                {
                    Severity = Severity.Error,
                    UserFacingMessage =
                        String.Format("Unable to log into the back office company: {0}",
                            backOfficeConfiguration.CompanyId),
                    RawMessage =
                        String.Format(
                            "Error {0}, occured for User: {1}, Password: {2}, Company:{3}, Connection: {4} at {5}",
                            ex.Message, backOfficeConfiguration.UserId, backOfficeConfiguration.Password,
                            backOfficeConfiguration.CompanyId, backOfficeConfiguration.ConnectionInformation,
                            ex.StackTrace)


                };

                response.Diagnoses.Add(diagnosis);

                return response;
            }

            response.Status = Status.Success;
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndSession()
        {
            if (DbLink != null)
            {
                DbLink.Dispose();
                DbLink = null;
            }

            if (ErpSession != null)
            {
                ErpSession.Dispose();
                ErpSession = null;
            }
        }

        public void Dispose()
        {
           EndSession();
        }
    }
}
