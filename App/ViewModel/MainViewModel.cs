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
    public int year;
    [ObservableProperty]
    public int month;
    [ObservableProperty]
    public double payment;

    public PaymentInfo(int year, int month, double payment)
    {
        Year = year;
        Month = month;
        Payment = payment;
    }
}

public partial class MainViewModel : ObservableObject
{
    public string WorkerStringXml = "";
    [ObservableProperty]
    ObservableCollection<Worker> workers;

    [ObservableProperty]
    Worker viewWorker;

    [ObservableProperty]
    bool isActive_ScrollView;
    [RelayCommand]
    public void Find(string q)
    {
        ViewWorker = Workers.Where(worker => worker.Name == q).FirstOrDefault(ViewWorker);
        IsActive_ScrollView = true;
    }
    async Task ProcessWorkersXml(ObservableCollection<Worker> Workers)
    {
        
        using var stream = await FileSystem.OpenAppPackageFileAsync("Workers.xml");
        using var reader = new StreamReader(stream);
        WorkerStringXml = reader.ReadToEnd();

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
    public MainViewModel()
    {
        ViewWorker = new Worker("qwe", "rty", [], []);
        Workers = [];
        IsActive_ScrollView = false;
        ProcessWorkersXml(Workers);
    }
    
}
