using ACCPAC.Advantage;
using Sage300ERP.Plugin.Native.Interfaces;
/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using System.Collections.Generic;

namespace Sage300Erp.Plugin.Native
{
    /// <summary>
    /// The abstract class for a Sage 300 ERP view.  This class manages operations such as opening views (and disposing them when the class is disposed) and reading view errors/messages.
    /// </summary>
    public abstract class BaseView : IBaseView
    {
        private Session _ErpSession;
        private DBLink _DbLink;
        private Dictionary<string, View> _Views = new Dictionary<string, View>();
        private bool disposed = false;

        /// <summary>
        /// Returns the main view for this class.
        /// </summary>
        /// <returns>The <see cref="View"/> the main view for this class.</returns>
        protected abstract View View { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="erpSession">The Sage 300 session. <see cref="Session"/></param>
        /// <param name="dbLink">The link to the Sage 300 database. <see cref="DBLink"/></param>
        public BaseView(Session erpSession, DBLink dbLink)
        {
            _ErpSession = erpSession;
            _DbLink = dbLink;
        }

        ~BaseView()
        {
            Dispose(false);
        }

        /// <summary>
        /// Call Dispose in order for this class to free up its resources (e.g. any Sage 300 views that is has opened).
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    foreach (KeyValuePair<string, View> pair in _Views)
                    {
                        pair.Value.Dispose();
                    }
                    disposed = true;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Returns the number of records in the main view for this class.
        /// </summary>
        public int Count
        {
            get
            {
                return View.FilterCount("", 0);
            }
        }

        public int Order
        {
            get { return View.Order; }
            set { View.Order = value; }
        }

        public void Browse(string filter, bool ascending)
        {
            View.Browse(filter, ascending);
        }

        public void FilterSelect(string filter, bool ascending, int order, ViewFilterOrigin origin)
        {
            View.FilterSelect(filter, ascending, order, origin);
        }

        public bool FilterFetch(bool lockRecord)
        {
            return View.FilterFetch(lockRecord);
        }

        public bool GoTop()
        {
            return View.GoTop();
        }

        public bool GoNext()
        {
            return View.GoNext();
        }

        public void Insert()
        {
            View.Insert();
        }

        public void Delete()
        {
            View.Delete();
        }

        public void Update()
        {
            View.Update();
        }

        public bool Read(bool lockRecord)
        {
            return View.Read(lockRecord);
        }

        public void RecordClear()
        {
            View.RecordClear();
        }
        /// <summary>
        /// Lookup the name or description in a secondary view for a code field in a view (e.g. looking up the description in the A/R Terms view for a terms code stored in the A/R Invoice).
        /// </summary>
        /// <param name="codeView">The source view that contains the value for the code (e.g. the A/R Invoice view)</param>
        /// <param name="sourceFieldName">The name of the code field in the source view (e.g TERMCODE)</param>
        /// <param name="descriptionView">The target view that contains the description for the code (e.g. the A/R Terms view)</param>
        /// <param name="targetFieldName">The name of the code field in the target view (e.g. CODETERM)</param>
        /// <param name="descriptionFieldName">The name of the description field (e.g. TEXTDESC)</param>
        /// <returns></returns>
        protected string LookupDescription(View codeView, string sourceFieldName, View descriptionView, string targetFieldName, string descriptionFieldName)
        {
            string code = GetValue(codeView, sourceFieldName);
            if (!string.IsNullOrWhiteSpace(code))
            {
                descriptionView.Fields.FieldByName(targetFieldName).SetValue(code, true);
                if (descriptionView.Read(false))
                    return GetValue(descriptionView, descriptionFieldName);
            }
            return code;
        }

        /// <summary>
        /// This method will return a list of errors or warnings that are currently on the Sage 300 error stack.
        /// </summary>
        public List<ViewError> ViewErrors
        {
            get
            {
                List<ViewError> errors = new List<ViewError>();

                if (_ErpSession.Errors != null)
                {
                    long lCount = _ErpSession.Errors.Count;

                    if (lCount != 0)
                    {
                        for (int iIndex = 0; iIndex < lCount; iIndex++)
                        {
                            errors.Add(new ViewError(_ErpSession.Errors[iIndex]));
                        }
                    }

                    _ErpSession.Errors.Clear();
                }

                return errors;
            }
        }

        /// <summary>
        /// Retrieves the value from the field as a string - but returns null if the string is empty or null.
        /// </summary>
        /// <param name="view">The <see cref="View"/> whose field will be read.</param>
        /// <param name="fieldName">The name of the field that will be read.</param>
        /// <returns></returns>
        protected String GetValue(View view, string fieldName)
        {
            string value = view.Fields.FieldByName(fieldName).Value.ToString();
            return String.IsNullOrEmpty(value) ? null : value;
        }

        /// <summary>
        /// Opens a Sage 300 view.  The view will be automatically closed when the <see cref="Dispose"/> method for this class is called.
        /// </summary>
        /// <param name="viewID">The roto ID of the view (e.g. "AR0024")</param>
        /// <returns>The <see cref="View"/> that has been opened.</returns>
        protected View OpenView(string viewID)
        {
            View view;

            if (!_Views.TryGetValue(viewID, out view))
            {
                view = _DbLink.OpenView(viewID);
                _Views.Add(viewID, view);
            }

            return view;
        }

        /// <summary>
        /// Opens a Sage 300 view with the option of read-only mode.  The view will be automatically closed when the <see cref="Dispose"/> method for this class is called.
        /// </summary>
        /// <param name="viewID">The roto ID of the view (e.g. "AR0024")</param>
        /// <returns>The <see cref="View"/> that has been opened.</returns>
        protected View OpenView(string viewID, bool readOnly)
        {
            View view;

            if (!readOnly)
                return OpenView(viewID);

            // Parameters for typical read-only mode.
            const ViewOpenModes openModes = ViewOpenModes.Readonly;
            const int prefetch = 0;  // # of records to buffer when in read-only (i.e. using dbFetch). 
                                     // Setting to 0 means use the value defined by DO_FETCHCOUNT in the view (default == 8).
            const ViewOpenDirectives openDirectives = ViewOpenDirectives.InstanceOpen;
            object openExtra = null;
            ProcessServerSetup processServerSetup = null;

            if (!_Views.TryGetValue(viewID, out view))
            {
                view = _DbLink.OpenView(viewID, openModes, prefetch, openDirectives, openExtra, processServerSetup);
                _Views.Add(viewID, view);
            }

            return view;
        }

        /// <summary>
        /// Opens a Sage 300 view with full parameters.  The view will be automatically closed when the <see cref="Dispose"/> method for this class is called.
        /// </summary>
        /// <param name="viewID">The roto ID of the view (e.g. "AR0024")</param>
        /// <returns>The <see cref="View"/> that has been opened.</returns>
        protected View OpenView(string viewID, ViewOpenModes openModes, int prefetch, ViewOpenDirectives openDirectives, object openExtra, ProcessServerSetup processServerSetup)
        {
            View view;

            if (!_Views.TryGetValue(viewID, out view))
            {
                view = _DbLink.OpenView(viewID, openModes, prefetch, openDirectives, openExtra, processServerSetup);
                _Views.Add(viewID, view);
            }

            return view;
        }
    }


    /// <summary>
    /// Class for storing Sage 300 ERP error and warning details.
    /// </summary>
    public class ViewError : IErrorComInterop
    {
        private string _Code;
        private string _HelpFile;
        private int _HelpID;
        private string _Message;
        private ErrorPriority _Priority;
        private string _Source;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="error">The <see cref="Error"/> that contains details about the view error/warning.</param>
        public ViewError(Error error)
        {
            _Code = error.Code;
            _HelpFile = error.HelpFile;
            _HelpID = error.HelpID;
            _Message = error.Message;
            _Priority = error.Priority;
            _Source = error.Source;
        }

        /// <summary>
        /// The error code.
        /// </summary>
        public string Code { get { return _Code; } }

        /// <summary>
        /// The Sage 300 ERP help file .
        /// </summary>
        public string HelpFile { get { return _HelpFile; } }

        /// <summary>
        /// The help id for this error in the Sage 300 ERP help file.
        /// </summary>
        public int HelpID { get { return _HelpID; } }

        /// <summary>
        /// The error/warning message.
        /// </summary>
        public string Message { get { return _Message; } }

        /// <summary>
        /// The <see cref="ErrorPriority"/>
        /// </summary>
        public ErrorPriority Priority { get { return _Priority; } }

        /// <summary>
        /// The source of the error.
        /// </summary>
        public string Source { get { return _Source; } }
    }
}
