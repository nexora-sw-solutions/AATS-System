using System.Threading.Tasks;
using AATS.Desktop.Models;
using AATS.Desktop.ViewModels.Shared;

namespace AATS.Desktop.ViewModels.SecretarialAdvisory
{
    public partial class Form15ViewModel : AuditTableViewModelBase
    {
        public override string PageTitle => "Form - 15";
        public override string SearchPlaceholder => "Search Client, Company, ID...";
        public override string StatusHeader => "Process Status";
        public override string ProcessHeader => "Process Status";
        public override string ClientHeader => "Client Name";
        public override string CompanyHeader => "Company Name";
        public override bool IsStatusVisible => false;
        public override bool IsCompanyVisible => true;
        public override string GuideLinkText => "Learn more about Form - 15";
        public override string GuideDescription => "Manage Form - 15 records and registration workflow.";
        public override string GuideFeature1Title => "Document Management";
        public override string GuideFeature1Text => "Upload and track Form - 15, Payment, and Certified Copy documents.";
        public override string GuideFeature2Title => "Workflow Tracking";
        public override string GuideFeature2Text => "Track clients through the registration process stages.";

        public Form15ViewModel()
        {
        }
    }
}
