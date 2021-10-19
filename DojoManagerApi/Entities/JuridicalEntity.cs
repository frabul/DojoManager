using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading.Tasks;

namespace DojoManagerApi.Entities
{
    [Owned]
    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public int Number { get; set; }
        public string PostCode { get; set; }
    }
    public class JuridicalEntity : IJuridicalEntity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string EMail { get; set; }
        public virtual Address Address { get; set; }
        public virtual string PhoneNumber { get; set; }
    }

    public class Certificate
    {
        public virtual int Id { get; set; }
        public virtual DateTime Expiry { get; set; }
        public virtual bool IsCompetitive { get; set; }
        public virtual string ImagePath { get; set; }
    }

    public enum SubscriptionType
    {
        Generic = 1,
        Kensei_Dojo_Annual_Association = 1,
        CIK_Annual_Association,
    }

    public enum CashFlowDirection { In, Out }

    public class CashFlow
    {
        public virtual int Id { get; set; }
        public virtual JuridicalEntity Counterpart { get; set; }
        public virtual decimal Amount { get; set; }
        public virtual string Notes { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual CashFlowDirection Direction { get; set; }
    }

    public class DebitPayment : CashFlow
    {
        public virtual Debit Debit { get; set; } //payment is also related to a debit  
        public DebitPayment()
        {
            Direction = CashFlowDirection.In;
        }
    }

    
}
