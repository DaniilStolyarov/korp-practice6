using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Xml;

namespace App.ViewModel;

public partial class Worker : ObservableObject
{
    [ObservableProperty]
    public string name;
    [ObservableProperty]
    public string birthString;
    [ObservableProperty]
    public ObservableCollection<WorkInfo> workExperience;
    [ObservableProperty]
    public ObservableCollection<PaymentInfo> paymentExperience;
    public Worker(string name, string birthString, ObservableCollection<WorkInfo> workExperience, ObservableCollection<PaymentInfo> paymentExperience)
    {
        Name = name;
        BirthString = birthString;
        WorkExperience = workExperience;
        PaymentExperience = paymentExperience;
    }
}
public partial class WorkInfo : ObservableObject
{
    [ObservableProperty]
    public string name;
    [ObservableProperty]
    public string start;
    [ObservableProperty]
    public string finish;
    [ObservableProperty]
    public string department;
    public WorkInfo(string name, string start, string finish, string department)
    {
        Name = name;
        Start = start;
        Finish = finish;
        Department = department;
    }
}

public partial class PaymentInfo : ObservableObject
{
    [ObservableProperty]
    public int? year;
    [ObservableProperty]
    public int? month;
    [ObservableProperty]
    public double? payment;

    public PaymentInfo(int? year, int? month, double? payment)
    {
        Year = year;
        Month = month;
        Payment = payment;
    }
}

public partial class MainViewModel : ObservableObject
{
    public static string WorkersFileName = "Workers.xml";
    public string WorkerStringXml = "";
    [ObservableProperty]
    ObservableCollection<Worker> workers;

    [ObservableProperty]
    Worker viewWorker;

    [ObservableProperty]
    bool isActive_ScrollView;

    public MainViewModel()
    {
        ViewWorker = new Worker("qwe", "rty", [], []);
        Workers = [];
        IsActive_ScrollView = false;
        ProcessWorkersXml();
    }
    [RelayCommand]
    public void Find(string q)
    {
        ViewWorker = Workers.Where(worker => worker.Name == q).FirstOrDefault(ViewWorker);
        IsActive_ScrollView = true;
    }
    [RelayCommand]
    public void AddEmptyWorkExperience()
    {
        ViewWorker.WorkExperience.Add(new WorkInfo("", "", "", ""));
    }
    [RelayCommand]
    public void AddEmptyPaymentExperience()
    {
        ViewWorker.PaymentExperience.Add(new PaymentInfo(null, null, null));
    }
    [RelayCommand]
    public void SaveToXML()
    {
        XmlDocument resXml = new();
        XmlElement xWorkers = resXml.CreateElement("Сотрудники");
        foreach (var worker in Workers)
        {
            XmlElement xWorker = resXml.CreateElement("Сотрудник");
            XmlElement xName = resXml.CreateElement("ФИО");
                xName.AppendChild(resXml.CreateTextNode(worker.Name));
            XmlElement xYear = resXml.CreateElement("Год_рождения");
                xYear.AppendChild(resXml.CreateTextNode(worker.BirthString));
            XmlElement xWorkList = resXml.CreateElement("Список_Работ");
            foreach(var workInfo in worker.WorkExperience)
            {
                XmlElement xWorkInfo = resXml.CreateElement("Работа");
                XmlElement xWorkInfo_Name = resXml.CreateElement("Название_должности");
                    xWorkInfo_Name.AppendChild(resXml.CreateTextNode(workInfo.Name));
                XmlElement xWorkInfo_Start = resXml.CreateElement("Дата_начала");
                    xWorkInfo_Start.AppendChild(resXml.CreateTextNode(workInfo.Start));
                XmlElement xWorkInfo_Finish = resXml.CreateElement("Дата_окончания");
                    xWorkInfo_Finish.AppendChild(resXml.CreateTextNode(workInfo.Finish));
                XmlElement xWorkInfo_Department = resXml.CreateElement("Отдел");
                    xWorkInfo_Department.AppendChild(resXml.CreateTextNode(workInfo.Department));

                xWorkInfo.AppendChild(xWorkInfo_Name);
                xWorkInfo.AppendChild(xWorkInfo_Start);
                xWorkInfo.AppendChild(xWorkInfo_Finish);
                xWorkInfo.AppendChild(xWorkInfo_Department);
                xWorkList.AppendChild(xWorkInfo);
            }
            XmlElement xPaymentList = resXml.CreateElement("Список_Зарплат");
            foreach (var paymentInfo in worker.PaymentExperience)
            {
                XmlElement xPayment = resXml.CreateElement("Зарплата");
                XmlElement xPayment_Year = resXml.CreateElement("Год");
                xPayment_Year.AppendChild(resXml.CreateTextNode(paymentInfo.Year == null ? "" : paymentInfo.Year.ToString()));
                XmlElement xPayment_Month = resXml.CreateElement("Месяц");
                xPayment_Month.AppendChild(resXml.CreateTextNode(paymentInfo.Month == null ? "" : paymentInfo.Month.ToString()));
                XmlElement xPayment_Payment = resXml.CreateElement("Итого");
                xPayment_Payment.AppendChild(resXml.CreateTextNode(paymentInfo.Payment == null ? "" : paymentInfo.Payment.ToString()));
                xPayment.AppendChild(xPayment_Year);
                xPayment.AppendChild(xPayment_Month);
                xPayment.AppendChild(xPayment_Payment);
                xPaymentList.AppendChild(xPayment);
            }
            xWorker.AppendChild(xName);
            xWorker.AppendChild(xYear);
            xWorker.AppendChild(xWorkList);
            xWorker.AppendChild(xPaymentList);
            xWorkers.AppendChild(xWorker);
        }
        resXml.AppendChild(xWorkers);
        SaveToFile("Workers.xml", 
            resXml
            ).Wait();
    }
    public async Task SaveToFile(string filename, XmlDocument doc)
    {
        // Create an output filename
        string targetFile = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
        doc.Save(targetFile);
        ViewWorker.Name = targetFile;
    }
    async Task readDefaultWorkers()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("Workers.xml");
        using var reader = new StreamReader(stream);
        WorkerStringXml = reader.ReadToEnd();
    }
    async Task ProcessWorkersXml()
    {

        if (File.Exists(Path.Combine(FileSystem.Current.AppDataDirectory, WorkersFileName)))
        {
            WorkerStringXml = File.ReadAllText(Path.Combine(FileSystem.Current.AppDataDirectory, WorkersFileName));
        }
        else
        {
            await readDefaultWorkers();
        }

        XmlDocument xDocument = new();
        xDocument.LoadXml(WorkerStringXml);

        XmlElement xWorkers = xDocument.DocumentElement;
        foreach (XmlNode xWorker in xWorkers)
        {
            string name = xWorker.SelectSingleNode("ФИО").InnerText;
            string birthDateString = xWorker.SelectSingleNode("Год_рождения").InnerText;
            ObservableCollection<WorkInfo> workList = [];
            foreach (XmlNode Work in xWorker.SelectSingleNode("Список_Работ"))
            {
                workList.Add(new WorkInfo(
                    Work.SelectSingleNode("Название_должности").InnerText,
                    Work.SelectSingleNode("Дата_начала").InnerText,
                    Work.SelectSingleNode("Дата_окончания").InnerText,
                    Work.SelectSingleNode("Отдел").InnerText
                    ));
            }
            ObservableCollection<PaymentInfo> paymentList = [];
            foreach (XmlNode Payment in xWorker.SelectSingleNode("Список_Зарплат"))
            {
                paymentList.Add(
                    new PaymentInfo(
                    int.Parse(Payment.SelectSingleNode("Год").InnerText),
                    int.Parse(Payment.SelectSingleNode("Месяц").InnerText),
                    double.Parse(Payment.SelectSingleNode("Итого").InnerText)
                    ));
            }
            Workers.Add(new Worker(name, birthDateString, workList, paymentList));
        }
    }
}
