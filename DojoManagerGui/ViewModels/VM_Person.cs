using DojoManagerApi.Entities;
using System.Linq;
using System;
using System.ComponentModel;
using PropertyChanged;
using DojoManagerApi;
using Microsoft.Toolkit.Mvvm.Input;
using System.Timers;
using System.Windows.Threading;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace DojoManagerGui.ViewModels
{
    public class EnumKendDegreeUtils
    {
        public static List<object> GetDegreeList()
        {
            List<object> list = new List<object>()
            {
                new { Value= KendoDegree.Kyu6, Description = "6° Kyu" },
                new { Value= KendoDegree.Kyu5, Description = "5° Kyu" },
                new { Value= KendoDegree.Kyu4, Description = "4° Kyu" },
                new { Value= KendoDegree.Kyu3, Description = "3° Kyu" },
                new { Value= KendoDegree.Kyu2, Description = "2° Kyu" },
                new { Value= KendoDegree.Kyu1, Description = "1° Kyu" },

                new { Value= KendoDegree.Dan1, Description = "1° Dan" },
                new { Value= KendoDegree.Dan2, Description = "2° Dan" },
                new { Value= KendoDegree.Dan3, Description = "3° Dan" },
                new { Value= KendoDegree.Dan4, Description = "4° Dan" },
                new { Value= KendoDegree.Dan5, Description = "5° Dan" },
                new { Value= KendoDegree.Dan6, Description = "6° Dan" },
                new { Value= KendoDegree.Dan7, Description = "7° Dan" },
                new { Value= KendoDegree.Dan8, Description = "8° Dan" },
            };
            return list;
        }
    }
    public class VM_Person : INotifyPropertyChanged
    {
        public VM_Person(Person person)
        {
            if (person.Address == null)
                person.Address = new Address();

            Person = (Person)EntityWrapper.Wrap(person);
            var hash = Person.GetHashCode();
            AddNewCard = new RelayCommand(
                () => Person.AddCard(
                           new MembershipCard()
                           {
                               ValidityStartDate = DateTime.Now,
                               ExpirationDate = DateTime.Now
                           }
                       ));
            RemoveCard = new RelayCommand<MembershipCard>(async c =>
            {
                if (c != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        Person.RemoveCard(c);
                    });
            });
            AddNewCertificate = new RelayCommand(() => Person.AddCertificate(new Certificate() { Expiry = DateTime.Now }));
            RemoveCertificate = new RelayCommand<Certificate>(async c =>
            {
                if (c != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        Person.RemoveCertificate(c);
                    });

            });
            ShowCertificateImageCommand = new RelayCommand<Certificate>(ShowCertificateImage);
            SetCertificateImageCommand = new RelayCommand<Certificate>(SetCertificateImage);
            AddSubscriptionCommand = new RelayCommand(
                    () => Person.AddSubscription(new Subscription() { StartDate = DateTime.Now, EndDate = DateTime.Now }, 0),
                    () => Person != null);

            RemoveSubscriptionCommand = new RelayCommand<Subscription>(
                    async s =>
                    {
                        await App.AskAndExecuteAsync(() =>
                        {
                            Person.RemoveSubscription(s);
                            this.OnPropertyChanged(nameof(Payments));
                        });
                    }
                );
            AddPaymentCommand = new RelayCommand<Subscription>(AddPayment);
            RemovePaymentCommand = new RelayCommand<DebitPayment>(async dp =>
            {
                if (dp != null)
                    await App.AskAndExecuteAsync(() =>
                    {
                        dp.Debit.RemovePayment(dp);
                        App.Db.Delete(dp);
                        App.Db.Save();
                        this.OnPropertyChanged(nameof(Payments));
                        WeakReferenceMessenger.Default.Send(
                            new EntityListChangedMessage<MoneyMovement>(
                                this,
                                Array.Empty<MoneyMovement>(),
                                new MoneyMovement[] { dp }));
                    });

            });

            AddExaminationCommand = new RelayCommand(() => Person.AddNewExamination());
            RemoveExaminationCommand = new RelayCommand<KendoExamination>(async e =>
            {
                await App.AskAndExecuteAsync(() =>
                {
                    if (e != null)
                        Person.RemoveExamination(e);
                });
            });

            SetPersonPictureCommand = new RelayCommand(() => SetPersonPicture());
            AssignNewCodeCommand = new RelayCommand<MembershipCard>(AssignNewCode);
            PrintReceiptCommand = new RelayCommand<DebitPayment>(PrintReceipt);
            PrintMemberCardCommand = new RelayCommand(PrintMemberCard, () => IsMember);


            IsMember = Person.Cards.Where(c =>
                c.Association == Config.Instance.NomeAssociazione
                && !c.Invalidated
                && DateTime.Now < c.ExpirationDate && DateTime.Now >= c.ValidityStartDate).Any();

            DispatcherTimer dispatcherTimer = new DispatcherTimer();

            dispatcherTimer.Interval = TimeSpan.FromSeconds(0.3);
            dispatcherTimer.Tick += new EventHandler((s, e) =>
            {
                this.OnPropertyChanged(nameof(Debit));
                //this.OnPropertyChanged(nameof(Payments));
            });
            dispatcherTimer.Start();
        }

        private void AddPayment(Subscription? s)
        {
            if (s == null)
                return;

            //add payment
            var mov = s.Debit.AddPayment(0, DateTime.Now, Person.Origin);
            mov.PayerName = Person.FullName;
            mov.PayerCode = Person.TaxIdentificationNumber;

            //set receipt for payment
            mov.Receipt = new Receipt(mov);
            App.Db.SetReceiptNumber(mov.Receipt);

            //save 
            App.Db.Save();
            this.OnPropertyChanged(nameof(Payments));
            WeakReferenceMessenger.Default.Send(
                new EntityListChangedMessage<MoneyMovement>(
                    this,
                    new MoneyMovement[] { mov },
                    Array.Empty<MoneyMovement>()));


        }

        public bool IsMember { get; private set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public Person Person { get; }
        public decimal Debit => Person.Subscriptions.Select(s => s.Debit).Sum(d => d.Amount - d.Payments.Sum(pay => pay.Amount));
        public DateTime? CertiFicateExpiration => Person.Certificates.OrderByDescending(c => c.Expiry).FirstOrDefault()?.Expiry;
        public DateTime? DojoSubscription => Person.Cards
            .Where(s => s.Association.Equals(App.ClubName, StringComparison.InvariantCultureIgnoreCase))
            .OrderByDescending(c => c.ValidityStartDate)
            .FirstOrDefault()
            ?.ValidityStartDate;

        public IEnumerable<DebitPayment> Payments => Person.Subscriptions.SelectMany(s => s.Debit.Payments).ToList();
        public IList<Subscription> Subscriptions => Person.Subscriptions;
        public string[] DefaultSubscriptions { get; } = new string[] {
            "Iscrizione annuale Kensei Dojo",
            "Iscrizione annuale CIK" };

        public RelayCommand AddSubscriptionCommand { get; }
        public RelayCommand<Subscription> RemoveSubscriptionCommand { get; }
        public RelayCommand<Subscription> AddPaymentCommand { get; }
        public RelayCommand AddNewCard { get; }
        public RelayCommand<MembershipCard> RemoveCard { get; }
        public RelayCommand AddNewCertificate { get; }
        public RelayCommand<Certificate> RemoveCertificate { get; }
        public RelayCommand<Certificate> ShowCertificateImageCommand { get; }
        public RelayCommand<Certificate> SetCertificateImageCommand { get; }
        public RelayCommand<DebitPayment> RemovePaymentCommand { get; }
        public RelayCommand AddExaminationCommand { get; }
        public RelayCommand<KendoExamination> RemoveExaminationCommand { get; }
        public RelayCommand SetPersonPictureCommand { get; }
        public RelayCommand<MembershipCard> AssignNewCodeCommand { get; }
        public RelayCommand<DebitPayment> PrintReceiptCommand { get; }
        public List<object> KendoDegrees => EnumKendDegreeUtils.GetDegreeList();
        public string[] SubscriptionsSuggested => Config.Instance.SuggerimentiSottoscrizioni;
        public string[] CardsSuggested => Config.Instance.SuggerimentiAssociazioni;
        public RelayCommand PrintMemberCardCommand { get; }
        public string[] MemberTypesSuggested => Config.Instance.SuggerimentiTipiSocio;
        public ImageSource PersonPicture
        {
            get
            {
                var bytes = App.Db.GetImageBytes(Person);
                BitmapImage image = null;
                if (bytes.Length > 0)
                {
                    image = new BitmapImage();
                    using (MemoryStream mem = new MemoryStream(bytes))
                    {
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = mem;
                        image.EndInit();
                        image.Freeze();
                    }
                }
                return image;
            }
        }


        public void SetPersonPicture()
        {
            var selectedPicture = App.SelectImage();
            if (selectedPicture != null)
            {
                App.Db.SetImage(Person, selectedPicture);
            }
            OnPropertyChanged(nameof(PersonPicture));
        }

        public void ShowCertificateImage(Certificate cert)
        {
            if (cert.ImageFileName == null)
                return;
            var filePath = App.Db.GetImagePath(cert);
            if (!File.Exists(filePath))
            {
                // todo log problem
                return;
            }

            var temp = Path.Combine(
                Path.GetTempPath(),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                cert.ImageFileName 
                );
            try
            {
                File.Copy(filePath, temp, true);
                new Process
                {
                    StartInfo = new ProcessStartInfo(temp)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            catch (Exception ex)
            {

            }


        }

        public void SetCertificateImage(Certificate cert)
        {
            var selectedFile = App.SelectFile();
            if (selectedFile != null)
            {
                App.Db.SetImage(cert, selectedFile);
                //cert.ImageFileName = App.Db.GetImagePath(cert); 
            }

        }

        private void AssignNewCode(MembershipCard? card)
        {
            if (card == null || string.IsNullOrEmpty(card.Association))
                return;
            var otherCards = card.Person.Cards.Where((c) => c.Association == card.Association && c != card);
            if (otherCards.Any())
                card.CardId = otherCards.First().CardId;
            else
            {
                card.CardId = App.Db.GetNewMembershipCardCode(card.Association);
            }

        }
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrintReceipt(DebitPayment? payment)
        {
            if (payment == null)
                return;
            Dictionary<string, object> variables = new();
            var numRicevuta = $"{payment.Receipt.NumberInYear}/{payment.Receipt.Date.Year}";
            var nomeCompleto = $"{Person.Name} {Person.SecondName}";
            variables.Add("num_ric", numRicevuta);
            variables.Add("data_ric", payment.Date.ToString("yyyy/MM/dd"));
            variables.Add("nome_intestatario", payment.PayerName);
            variables.Add("nome_socio", nomeCompleto);
            variables.Add("cod_fisc_intestatario", payment.PayerCode);
            variables.Add("cod_fisc_socio", payment.Debit.Subscription.Person.TaxIdentificationNumber);
            variables.Add("luogo_nascita", payment.Debit.Subscription.Person.BirthLocation);
            variables.Add("nasc_dat", payment.Debit.Subscription.Person.BirthDate.ToString("yyyy/MM/dd"));
            variables.Add("dettaglio_ric", payment.Debit.Subscription.Description);
            variables.Add("importo_ric", payment.Amount.ToString());
            variables.Add("totale_ric", payment.Amount.ToString());
            variables.Add("causale_ric", payment.Notes);

            try
            {
                var targetDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var targetFileName = $"Ricevuta_KSD_{numRicevuta}.docx".Replace("/", "-");
                DocTemplateCompiler.Compile(
                    "RicevutaKSD.docx",
                    Path.Combine(targetDir, targetFileName),
                    variables);
            }
            catch (Exception ex)
            {
                App.ShowMessage("Errore", $"Errore nel salvataggio della ricevuta: {ex.Message}");
            }

        }
        private void PrintMemberCard()
        {
            var dojoCard = Person.Cards
                .Where(c => c.Association == Config.Instance.NomeAssociazione)
                .OrderByDescending(c => c.ExpirationDate)
                .FirstOrDefault();
            if (dojoCard == null)
            {
                App.ShowMessage("Errore", $"Tessera di socio non trovata");
                return;
            }

            Dictionary<string, object> variables = new();
            var imgPath = App.Db.GetImagePath(Person);
            BitmapImage img = new BitmapImage(new Uri(imgPath));
            variables.Add("foto_soc", img);
            variables.Add("n_i", dojoCard.CardId.ToString());
            variables.Add("data_i", DateTime.Now.ToString("yyyy/MM/dd"));
            variables.Add("tipo_soc", dojoCard.MemberType);
            variables.Add("cognome_soc", Person.SecondName);
            variables.Add("nome_soc", Person.Name);
            variables.Add("luogo_nascita", Person.BirthLocation);
            variables.Add("data_nascita", Person.BirthDate.ToString("yyyy/MM/dd"));
            variables.Add("cod_fisc", Person.TaxIdentificationNumber);
            variables.Add("luogo_residenza", Person.Address.City);
            variables.Add("cap_res", Person.Address.PostCode);
            variables.Add("indirizzo_soc", $"{Person.Address.Street}, {Person.Address.Number}");
            variables.Add("tel_soc", Person.PhoneNumber);
            variables.Add("email_soc", Person.EMail);
            variables.Add("altri_recapiti", "");
            var examinations = Person.Examinations.OrderBy(x => x.DegreeAcquired).ToArray();

            for (int i = 0; i < 13; i++)
            {
                string luogo = i >= examinations.Length ? "" : examinations[i].Location;
                string data = i >= examinations.Length ? "" : examinations[i].Date.ToString("yyyy/MM/dd");

                if (luogo == null)
                    luogo = "";

                if (i < 5)
                {
                    variables.Add($"{5 - i}kyu_luogo", luogo);
                    variables.Add($"{5 - i}kyu_data", data);
                }
                else
                {
                    variables.Add($"{i - 4}dan_luogo", luogo);
                    variables.Add($"{i - 4}dan_data", data);
                }

            }

            variables.Add("data_rec", "");
            try
            {
                var targetDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var targetFileName = $"Scheda_Socio_KSD_{Person.Name}_{Person.SecondName}.docx";
                DocTemplateCompiler.Compile("SchedaLibroSociKSD.docx", Path.Combine(targetDir, targetFileName), variables);
            }
            catch (Exception ex)
            {
                App.ShowMessage("Errore", $"Errore nel salvataggio della scheda: {ex.Message}");
            }
        }


    }

}
