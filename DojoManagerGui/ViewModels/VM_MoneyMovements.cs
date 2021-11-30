using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DojoManagerApi;
using DojoManagerApi.Entities;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace DojoManagerGui.ViewModels
{
    public class VM_MoneyMovement
    {
        public MoneyMovement Movement { get; set; }
        public DateTime Date { get; set; }
        public VM_MoneyMovement(MoneyMovement movement)
        {
            Movement = (MoneyMovement)EntityWrapper.Wrap(movement);
        }
    }
    public class VM_MoneyMovements : VM_FunctionPage, INotifyPropertyChanged
    {
        private DateTime? _StartDateFilter;
        private DateTime? _EndDateFilter;

        public Subject SubjectAssociated { get; }
        public ObservableCollection<MoneyMovement> Movements { get; set; }
        public override string Name => "Entrate/Uscite";
        public override string IconPath => "M600 100C323.85 100 100 323.85 100 600S323.85 1100 600 1100S1100 876.15 1100 600S876.15 100 600 100zM600 200A400 400 0 1 1 600 1000A400 400 0 0 1 600 200zM502.5000000000001 650H750V550H502.5000000000001A125 125 0 0 1 705.7 479.5L790.7 422.85A225 225 0 0 0 401.4000000000001 550H350V650H401.35A225 225 0 0 0 790.75 777.15L705.7 720.5A125 125 0 0 1 502.5000000000001 650z";

        public RelayCommand<MoneyMovement> RemoveMovementCommand { get; }
        public RelayCommand AddNewMovementCommand { get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand PrintMovementsCommand { get; }
        public DateTime? StartDateFilter { get => _StartDateFilter; set { _StartDateFilter = value; Refresh(); } }
        public DateTime? EndDateFilter { get => _EndDateFilter; set { _EndDateFilter = value; Refresh(); } }
        public string SubjectNameFilter { get; set; }
        public bool IsFiltersBoxVisible { get; set; }
        public bool IsAddAddButtonVisible { get; set; }

        public int MovementsCount => Movements.Count;
        public decimal Incomes
        {
            get
            {
                return Movements.Where(m => m.Direction == MoneyMovementDirection.In).Sum(m => m.Amount);
            }
        }
        public decimal Expenses
        {
            get { return Movements.Where(m => m.Direction == MoneyMovementDirection.Out).Sum(m => m.Amount); }
        }
        public decimal MovementsTotal
        {
            get { return Movements.Sum(m => m.Direction == MoneyMovementDirection.In ? m.Amount : -m.Amount); }
        }
        public VM_MoneyMovements(Subject subjectAssociated)
        {
            StartDateFilter = new DateTime(DateTime.Now.Year, 1, 1);
            EndDateFilter = new DateTime(DateTime.Now.Year + 1, 1, 1);
            SubjectAssociated = subjectAssociated;
            Movements = new ObservableCollection<MoneyMovement>();
            Refresh();
            RemoveMovementCommand = new RelayCommand<MoneyMovement>(RemoveMovement);
            AddNewMovementCommand = new RelayCommand(AddNewMovement);
            SearchCommand = new RelayCommand(Refresh);
            WeakReferenceMessenger.Default.Register<EntityListChangedMessage<MoneyMovement>>(this,
                (r, a) =>
                {
                    var rec = (VM_MoneyMovements)r;
                    if (rec != a.Sender)
                        this.Refresh();
                });
            PrintMovementsCommand = new RelayCommand(PrintMovements);
            IsFiltersBoxVisible = subjectAssociated == null;
            IsAddAddButtonVisible = subjectAssociated != null;
        }



        public void Refresh()
        {
            var startDate = StartDateFilter ?? DateTime.MinValue;
            var endData = EndDateFilter ?? DateTime.MaxValue;

            IEnumerable<MoneyMovement> movements = Enumerable.Empty<MoneyMovement>();
            if (SubjectAssociated != null)
            {
                movements = App.Db.ListMovements(startDate, endData, SubjectAssociated.Id);
            }
            else
            {
                movements = App.Db.ListMovements(startDate, endData);
                if (!string.IsNullOrWhiteSpace(SubjectNameFilter))
                    movements = movements.Where(m =>
                        m.Counterpart.Name.Contains(SubjectNameFilter, StringComparison.InvariantCultureIgnoreCase));
            }
            Movements = new ObservableCollection<MoneyMovement>(
                    movements.Select(m => (MoneyMovement)EntityWrapper.Wrap(m)));
        }

        public void AddNewMovement()
        {
            if (SubjectAssociated != null)
            {
                var mov = App.Db.AddNewMovement(SubjectAssociated);
                this.Movements.Add(mov);
                App.Db.Save();
                WeakReferenceMessenger.Default.Send(
                   new EntityListChangedMessage<MoneyMovement>(this, new MoneyMovement[] { mov }, Array.Empty<MoneyMovement>()));
            }
        }
        private async void RemoveMovement(MoneyMovement? obj)
        {
            if (obj != null)
            {
                if (obj is DebitPayment p)
                {
                    await App.ShowMessage("Attenzione!",
                        "Non è possibile rimuovere questo movimento da questa schermata poichè ha un debito associato." + Environment.NewLine +
                        $"Prova a rimuoverlo dalla lista pagamenti della persona ({p.Counterpart.Name}).");
                }
                else
                {
                    await App.AskAndExecuteAsync(() =>
                    {
                        this.Movements.Remove(obj);
                        obj.Counterpart.Movements.Remove(obj);
                        App.Db.Delete(obj);
                        App.Db.Save();

                        WeakReferenceMessenger.Default.Send(
                            new EntityListChangedMessage<MoneyMovement>(this, Array.Empty<MoneyMovement>(), new MoneyMovement[] { obj }));
                    });

                }
            }
        }

        private void PrintMovements()
        {
            StringBuilder html = new StringBuilder();
            HtmlTags.HtmlDocument doc = new HtmlTags.HtmlDocument();
            doc.AddStyle(@"

table {
    border-collapse: collapse;
    margin: 25px 0;
    font-size: 0.9em;
    font-family: sans-serif;
    min-width: 800px;
    max-width: 900px;
    box-shadow: 0 0 20px rgba(0, 0, 0, 0.15);
}

table thead tr {
  max-width: 500px;
  margin-left: 5px;
  margin-right: auto;
  padding-left: 10px;
  padding-right: 10px;
}
table thead tr {
    background-color: #009879;
    color: #ffffff;
    text-align: left;
}
table th,
table td {
    padding: 12px 15px;
    word-wrap: break-word;
}

table tbody tr {
    border-bottom: 1px solid #dddddd;
}

table tbody tr:nth-of-type(even) {
    background-color: #f3f3f3;
}

table tbody tr:last-of-type {
    border-bottom: 2px solid #009879;
} 
");
            doc.Head.Title($"Resoconto movimenti {Config.Instance.NomeAssociazione}");

            doc.Body.Add($"Resoconto movimenti {Config.Instance.NomeAssociazione}");
            //{ StartDateFilter: yyyy / MM / dd}
            //{ EndDateFilter: yyyy / MM / dd}

            var tabTotali = doc.Body.Add("table");
            var hederRow = tabTotali.Add("thead").Add("tr");
            hederRow.Add("th").Text("Tabella riassuntiva");
             

            if (StartDateFilter != null)
            {
                var tr = tabTotali.Add("tr");
                tr.Add("td").Text("Inizio periodo riferimento");
                tr.Add("td").Text($"{StartDateFilter:yyyy /MM/dd}");
            }
            if (StartDateFilter != null)
            {
                var tr = tabTotali.Add("tr");
                tr.Add("td").Text("Fine periodo riferimento");
                tr.Add("td").Text($"{EndDateFilter:yyyy/MM/dd}");
            }

            var r1 = tabTotali.Add("tr");
            r1.Add("td").Text("Numero movimenti");
            r1.Add("td").Text(MovementsCount.ToString());

            var r2 = tabTotali.Add("tr");
            r2.Add("td").Text("Totale entrate");
            r2.Add("td").Text($"{Incomes:F2}€");

            var r3 = tabTotali.Add("tr");
            r3.Add("td").Text("Totale spese");
            r3.Add("td").Text($"{Expenses:F2}€");

            var r4 = tabTotali.Add("tr");
            r4.Add("td").Text("Totale");
            r4.Add("td").Text($"{MovementsTotal:F2}€");

            var tab = doc.Body.Add("table");
            hederRow = tab.Add("thead").Add("tr");
            hederRow.Add("th").Text("Soggetto");
            hederRow.Add("th").Text("Data");
            hederRow.Add("th").Text("Somma");
            hederRow.Add("th").Text("Note");
            foreach (var mov in Movements)
            {

                var row = tab.Add("tr");
                row.Add("td").Text(mov.Counterpart.FullName);
                row.Add("td").Text(mov.Date.ToString("yyyy/MM/dd"));
                row.Add("td").Text($"{mov.AmountSigned:F2}€");
                row.Add("td").Text(mov.Notes);
            }
            var docDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            doc.WriteToFile(Path.Combine(docDir, "movements.html"));
        }
    }
}
