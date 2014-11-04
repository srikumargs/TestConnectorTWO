﻿using System;
using Sage.Connector.ProcessExecution.AddinView.Events;
using Sage.Connector.ProcessExecution.Interfaces;

namespace Sage.Connector.ProcessExecution.AddinSideAdapter
{
    /// <summary>
    /// App Notification Event Args Contract to View AddIn Adapter
    /// </summary>
    public class AppNotificationEventArgsContractToViewAddInAdapter : AppNotificationEventArgs, IDisposable
    {

        private readonly IAppNotificationEventArgs _contract;

        private readonly System.AddIn.Pipeline.ContractHandle _handle;

        // Track whether Dispose has been called. 
        private bool _disposed;

        /// <summary>
        /// App Notification Event Args Contract to View AddIn Adapter
        /// </summary>
        /// <param name="contract"><see cref="IAppNotificationEventArgs"/></param>
        public AppNotificationEventArgsContractToViewAddInAdapter(IAppNotificationEventArgs contract)
        {
            _contract = contract;
            _handle = new System.AddIn.Pipeline.ContractHandle(contract);
        }

        /// <summary>
        /// App Notification data to be interrogated. 
        /// </summary>
        public override string Data
        {
            get
            {
                return _contract.Data;
            }
        }

        /// <summary>
        /// Get the adapter source contract for the app notification.
        /// </summary>
        /// <returns><see cref="IAppNotificationEventArgs"/></returns>
        internal IAppNotificationEventArgs GetSourceContract()
        {
            return _contract;
        }

        /// <summary>
        /// Dispose properly of this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method. 
            // Therefore, you should call GC.SupressFinalize to 
            // take this object off the finalization queue 
            // and prevent finalization code for this object 
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios. 
        /// If disposing equals true, the method has been called directly 
        /// or indirectly by a user's code. Managed and unmanaged resources 
        /// can be disposed. 
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed. 
        /// </summary>
        /// <param name="disposing">When false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed. 
        ///  </param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (!_disposed)
            {
                // If disposing equals true, dispose all managed 
                // and unmanaged resources. 
                if (disposing)
                {
                    // Dispose managed resources.
                    _handle.Dispose();
                }

                // Note disposing has been done.
                _disposed = true;

            }
        }
    }
}
