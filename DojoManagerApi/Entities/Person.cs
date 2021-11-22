using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DojoManagerApi.Entities
{


    [WrapMe]
    public class Person : Subject, IEntityWrapper<Person>
    {
        public virtual string TaxIdentificationNumber { get; set; }
        public virtual DateTime BirthDate { get; set; }
        public virtual string BirthLocation { get; set; }
        public virtual string PictureFileName { get; set; }
        public virtual IList<Certificate> Certificates { get; set; } = new List<Certificate>();
        public virtual IList<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual IList<MembershipCard> Cards { get; set; } = new List<MembershipCard>();
        public virtual IList<KendoExamination> Examinations { get; set; } = new List<KendoExamination>();

        [AutomapIgnore]
        public virtual Person Origin => this;

        public virtual void RemoveCard(MembershipCard c)
        {
            c.Person = null;
            Cards.Remove(c);
        }

        //public virtual IList<Debit> Debits { get; set; } = new List<Debit>();
        public virtual void AddCard(MembershipCard card)
        {
            card.Person = Origin;
            Cards.Add(card);
        }

        public virtual void AddCertificate(Certificate certificate)
        {
            certificate.Person = Origin;
            Certificates.Add(certificate);
        }
        public virtual void AddSubscription(Subscription subscription, int cost)
        {
            var deb = new Debit() { Amount = cost };
            subscription.Debit = deb;
            subscription.Person = Origin; 
            deb.Subscription = subscription.Origin;
            Subscriptions.Add(subscription.Origin);
            //this.Debits.Add(deb);
            //return deb;
        }
        public virtual void RemoveCertificate(Certificate certificate)
        {
            certificate.Person = null;
            Certificates.Remove(certificate);
        }

        public virtual void RemoveSubscription(Subscription s)
        {

            foreach (var p in s.Debit.Payments.ToArray())
            {
                s.Debit.RemovePayment(p);
            } 
            s.Debit.Subscription = null;
            s.Debit = null;
            s.Person = null;
            Subscriptions.Remove(s);
        }

        public virtual decimal TotalDue()
        {
            return Subscriptions.Select(s => s.Debit.Amount - s.Debit.Payments.Select(p => p.Amount).Sum()).Sum();
        }


        public virtual string PrintData()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Person #{Id} - Name: {Name}, Certificats: {Certificates.Count},  Subscriptions: {Subscriptions.Count}, Cards: {Cards.Count}\n");

            builder.AppendLine("  Certificates");
            foreach (var c in Certificates)
                builder.AppendLine(c.PrintData().PadRight(4));

            builder.AppendLine("  Subscriptions");
            foreach (var s in Subscriptions)
                builder.AppendLine(s.PrintData().PadRight(4));

            builder.AppendLine("  Cards");
            foreach (var c in Cards)
                builder.AppendLine(c.ToString().PadRight(4));
            return builder.ToString();
        }

        public override string ToString()
        {
            return $"{{ Id: {Id} - Name: {Name} - CertCnt: {Certificates.Count} }}";
        }

        public virtual void AddNewExamination()
        {
            KendoDegree currentDegree = KendoDegree.Kyu6;
            var passedExams = Examinations.Where(e => e.Passed);
            if (passedExams.Any())
                currentDegree = (KendoDegree)passedExams.Max(e => e.DegreeAcquired);
            var newEx = new KendoExamination()
            {
                DegreeAcquired = currentDegree + 1,
                Person = this.Origin, 
                Date = DateTime.Now
            };
            this.Examinations.Add(newEx);
        }
        public virtual void RemoveExamination(KendoExamination e)
        {
            e.Person = null;
            this.Examinations.Remove(e);
        }
    }


}
