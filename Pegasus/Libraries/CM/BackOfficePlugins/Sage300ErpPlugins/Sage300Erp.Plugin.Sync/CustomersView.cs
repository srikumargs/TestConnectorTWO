using ACCPAC.Advantage;
using Sage300Erp.Plugin.Model;
using Sage300Erp.Plugin.Native;
using Sage300ERP.Plugin.Native.Interfaces;
using Sage300ERP.Plugin.Native.Types;
/*
 * Copyright (c) 2013 Sage Software, Inc.  All rights reserved.
*/
using System;
using System.Diagnostics.CodeAnalysis;

namespace Sage300Erp.Plugin.Sync
{
    /// <summary>
    /// Wrapper class around the A/R Customers view.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "False positive.")]
    public class CustomersView : BaseView, ICustomersView
    {
        private View _View;
        private readonly View _termsView;
        private readonly View _customerBalance;
        private readonly string _currency;
        private readonly View _salespersonsView;
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="erpSession">The Sage 300 ERP <see cref="Session"/></param>
        /// <param name="dbLink">The <see cref="DBLink"/></param>
        public CustomersView(Session erpSession, DBLink dbLink, string currency)
            : base(erpSession, dbLink)
        {
            _View = OpenView(Customers.ViewID);
            _termsView = OpenView(ARTerms.ViewID);
            _customerBalance = OpenView(ARCustomerBalance.ViewID);
            _currency = currency;
            _salespersonsView = OpenView(ARSalespersons.ViewID);
        }

        protected override View View { get { return _View; } }

        public bool CustomerExists(string customer)
        {
            _View.Fields.FieldByName(Customers.ID).SetValue(customer, true);
            return _View.Exists;
        }

        public string ExternalId
        {
            get 
            { 
                return GetValue(_View, Customers.ID); 
            }

            set
            {
                _View.Fields.FieldByName(Customers.ID).SetValue(value, true);
            }
        }

        public bool CheckCustomerCurrency(string customer, string currency)
        {
            ExternalId = customer;
            if (_View.Read(false))
                return (CurrencyCode == currency);
            else
                return false;  
        }
        public string Name { get { return GetValue(_View, Customers.Name); } }
        public string Street1 { get { return GetValue(_View, Customers.Address1); } }
        public string Street2 { get { return GetValue(_View, Customers.Address2); } }
        public string Street3 { get { return GetValue(_View, Customers.Address3); } }
        public string Street4 { get { return GetValue(_View, Customers.Address4); } }
        public string City { get { return GetValue(_View, Customers.City); } }
        public string StateProvince { get { return GetValue(_View, Customers.State); } }
        public string PostalCode { get { return GetValue(_View, Customers.PostalCode); } }
        public string Country { get { return GetValue(_View, Customers.Country); } }
        public string Phone { get { return GetValue(_View, Customers.Phone); } }
        public string Email { get { return GetValue(_View, Customers.Email); } }
        public string URL { get { return GetValue(_View, Customers.URL); } }
        public string Contact { get { return GetValue(_View, Customers.Contact); } }
        public string PhoneWork { get { return GetValue(_View, Customers.ContactPhone); } }
        public string EmailWork { get { return GetValue(_View, Customers.ContactEmail); } }


        public string SalespersonName
        {
            get { return LookupDescription(_View, Customers.SalesPersonCode, _salespersonsView, ARSalespersons.SalespersonCode, ARSalespersons.Name); }
        }

        public DateTime LastStatementDate 
        {
            get 
            {
                object date = _View.Fields.FieldByName(Customers.LastStatementDate).Value;
               return (date == null) ? DateTime.MinValue : (DateTime)date;
            }
        }

        public decimal BalanceForward
        {
            get { return (decimal)_View.Fields.FieldByName(Customers.BalanceForward).Value; }
        }

        public AccountType AccountType { get { return (AccountType)Enum.Parse( typeof(AccountType),  _View.Fields.FieldByName(Customers.AccountType).Value.ToString());} }


        public bool CheckCreditLimit 
        {
            get 
            {
                if (!CurrencyCode.Equals(_currency))
                    return false;

                if (Int32.Parse(_View.Fields.FieldByName(Customers.CheckCreditLimit).Value.ToString()) == 1)
                    return true;
                else
                    return false;
             } 
        }

        public decimal CreditLimit 
        {
            get 
            {
                if (!CurrencyCode.Equals(_currency))
                    return decimal.Zero;

                return (decimal)_View.Fields.FieldByName(Customers.CreditLimit).Value;
            
            } 
        }
      
        public bool CreditOnHold
        {
            get 
            {
                if (Int32.Parse(_View.Fields.FieldByName(Customers.CreditOnHold).Value.ToString()) == 1)
                    return true;
                else
                    return false;
             } 
        }

        public bool PrintStatement
        {
            get
            {
                if (Int32.Parse(_View.Fields.FieldByName(Customers.PrintStatement).Value.ToString()) == 1)
                    return true;
                else
                    return false;
            }
        }

        public string PaymentTerms 
        { 
             get { return LookupDescription(_View, Customers.TermCode, _termsView, ARTerms.TermsCode, ARTerms.Description); }
        }

        public decimal BalanceDue
        { 
            get 
            {
                _customerBalance.RecordClear();
                _customerBalance.Fields.FieldByName(ARCustomerBalance.IDCUST).SetValue(ExternalId, true);
                _customerBalance.Fields.FieldByName(ARCustomerBalance.SWNOLIMIT).SetValue(1, true);
                _customerBalance.Process();
                return (decimal)_customerBalance.Fields.FieldByName(ARCustomerBalance.AMTTOTCUST).Value;   
            }
        }

        public decimal CreditAvailable 
        {
            get
            {
                if (!CurrencyCode.Equals(_currency))
                    return decimal.Zero;

               return (CreditLimit > 0) ? (CreditLimit - BalanceDue) : 0;
            }
        }

        public StatusType Status 
        {
            get 
            {
                if (Int32.Parse(GetValue(_View, Customers.Status)) == 0)
                    return StatusType.InActive;
                else
                    return StatusType.Active;
            }
        }

        public string CurrencyCode { get { return GetValue(_View, Customers.Currency); } }
        public string TaxGroup { get { return GetValue(_View, Customers.TaxGroup); } }

        public int[] TaxClasses
        {
            get
            {
                int[] classes = new int[] {
                    Convert.ToInt32(_View.Fields.FieldByName(Customers.TaxClass1).Value.ToString()),
                    Convert.ToInt32(_View.Fields.FieldByName(Customers.TaxClass2).Value.ToString()),
                    Convert.ToInt32(_View.Fields.FieldByName(Customers.TaxClass3).Value.ToString()),
                    Convert.ToInt32(_View.Fields.FieldByName(Customers.TaxClass4).Value.ToString()),
                    Convert.ToInt32(_View.Fields.FieldByName(Customers.TaxClass5).Value.ToString()),
                };
                return classes;
            }
        }
    }
}
