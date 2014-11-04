using Sage.Connector.DomainContracts.Core;
using Sage300ERP.Plugin.Native.Interfaces;
using System;

namespace Sage300Erp.Plugin.Native
{
    /// <summary>
    /// Abstract class to handle the sync behaviour with the Native Sage 300 implementation.
    /// </summary>
    public abstract class AbstractNativeSyncBase : NativeSessionBase
    {
        //tdo:
        //private static ILog _Log = LogManager.GetLogger(EventSources.Plugin);
        protected int CompletedCount;
        protected int TotalCount;
        protected bool MoreData = false;
        protected bool TrackDeletes;
        protected string BrowseFilter = "";



        /// <summary>
        /// The main view for the upload/download.  This is the view that contains the records that will be browse/fetched or inserted/updated.
        /// </summary>
        protected abstract IBrowseable BrowseView { get; }

        /// <summary>
        /// Initialize the view
        /// </summary>
        /// <returns></returns>
        protected virtual Response Initialize()
        {
            var response = new Response();
            
            try
            {
                CompletedCount = 0;
                TotalCount = 0;
                TrackDeletes = true;

                SetBrowseFilter();
                Browse();
                MoreData = BrowseView.GoTop();
            }
            catch (Exception ex)
            {
                response.Status = Status.Failure;
                response.Diagnoses.Add(new Diagnosis
                {
                    Severity = Severity.Error,
                    UserFacingMessage = "Error during view initialization.",
                    RawMessage = ex.Message + ex.StackTrace
                });

                return response;
            }

            response.Status = Status.Success;
            return response;
        }


        /// <summary>
        /// 
        /// </summary>
        protected void GetValidNextEntity()
        {
            while (MoreData && !IsTheRecordInTheRange())
            {
                MoreData = BrowseView.GoNext();
            }

        }


        /// <summary>
        /// Set the browse filter if needed
        /// </summary>
        protected virtual void SetBrowseFilter()
        {
            BrowseFilter = "";
        }

        /// <summary>
        /// Set the total count of records for upload
        /// </summary>
        protected virtual void SetTotalCount()
        {
            //todo kms: determine if this is necessary
            //ConnectorSession.PluginResultsLog.SetTotalCount(EntityName, BrowseView.Count);
        }

        /// <summary>
        /// Browse the view
        /// </summary>
        protected virtual void Browse()
        {
            BrowseView.Browse(BrowseFilter, true);
        }

        /// <summary>
        /// Does the record meet the criteria for upload
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsTheRecordInTheRange()
        {
            return true;
        }
    }
}

