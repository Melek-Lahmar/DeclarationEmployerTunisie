using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeclarationEmployer.Contracts.Declarations.Empcca;
using DeclarationEmployer.Desktop.Services;

namespace DeclarationEmployer.Desktop.ViewModels;

public sealed class AnnexWorkbenchViewModel : ObservableObject
{
    private readonly CurrentDeclarationService _currentDeclarationService;
    private readonly EmpccaAnnexesApiClient _empccaAnnexesApiClient;

    private AnnexWorkbenchConfiguration? _configuration;
    private AnnexWorkbenchNavigateHandler? _navigateHandler;
    private Guid? _editingLineId;
    private AnnexLineItem? _selectedLine;
    private string _statusMessage = "Pret.";
    private bool _isBusy;
    private bool _hasDeclaration;
    private string _companyName = "-";
    private string _fiscalYear = "-";
    private string _declarationTitle = "-";
    private string _declarationStatus = "-";
    private string _validationStatus = "Non controlee";
    private int _lineCount;
    private decimal _grossTotal;
    private decimal _withheldTotal;
    private decimal _netTotal;
    private int _orderNumber = 1;
    private int _beneficiaryIdentifierType = 2;
    private string _beneficiaryIdentifier = string.Empty;
    private string _beneficiaryName = string.Empty;
    private string? _beneficiaryActivity;
    private string? _beneficiaryJobTitle;
    private string _beneficiaryAddress = string.Empty;
    private int _familySituation = 1;
    private int _dependentChildrenCount;
    private DateTime _workPeriodStart = new(2025, 1, 1);
    private DateTime _workPeriodEnd = new(2025, 12, 31);
    private int _workPeriodDays = 365;
    private decimal _taxableIncome;
    private decimal _benefitsInKind;
    private decimal _grossTaxableIncome;
    private decimal _reinvestedIncome;
    private decimal _commonRegimeWithheldAmount;
    private decimal _foreignEmployeeWithheldAmount;
    private decimal _socialSolidarityContribution;
    private decimal _a1NetPaidAmount;
    private int _a2AmountType = 1;
    private decimal _grossProfessionalAmount;
    private decimal _realRegimeFeesAmount;
    private decimal _boardAndSecuritiesAmount;
    private decimal _occasionalWorkAmount;
    private decimal _realEstateCapitalGainAmount;
    private decimal _hotelRentAmount;
    private decimal _artistRemunerationAmount;
    private decimal _publicSectorVatWithheldAmount;
    private decimal _a2WithheldAmount;
    private decimal _a2NetPaidAmount;
    private decimal _savingsAccountInterest;
    private decimal _otherMovableCapitalIncome;
    private decimal _nonEstablishedBankLoanInterest;
    private decimal _a3WithheldAmount;
    private decimal _a3NetPaidAmount;
    private int _a4AmountType = 1;
    private decimal _professionalAmountRate;
    private decimal _professionalAmount;
    private decimal _constructionWorkRate;
    private decimal _constructionWorkAmount;
    private decimal _a4RealEstateCapitalGainRate;
    private decimal _a4RealEstateCapitalGainAmount;
    private decimal _securitiesCapitalGainRate;
    private decimal _securitiesCapitalGainAmount;
    private decimal _securitiesIncomeRate;
    private decimal _securitiesIncomeAmount;
    private decimal _privilegedTaxRegimeAmount;
    private decimal _a4VatWithheldAmount;
    private decimal _a4WithheldAmount;
    private decimal _a4NetPaidAmount;
    private decimal _purchasesFromTenPercentCompanies;
    private decimal _purchasesFromFifteenPercentCompanies;
    private decimal _purchasesFromTwoThirdsDeductionBusinesses;
    private decimal _purchasesFromOtherBusinesses;
    private decimal _a5VatWithheldAmount;
    private decimal _deliveryPlatformThreePercentWithheldAmount;
    private decimal _a5WithheldAmount;
    private decimal _a5NetPaidAmount;
    private int _rebateType;
    private decimal _rebateAmount;
    private decimal _flatRegimeSalesAmount;
    private decimal _flatRegimeSalesAdvanceAmount;
    private decimal _gamblingIncomeAmount;
    private decimal _gamblingWithheldAmount;
    private decimal _distributionNetworkSalesAmount;
    private decimal _distributionNetworkWithheldAmount;
    private decimal _cashCollectionsAmount;
    private decimal _alcoholSalesAmount;
    private decimal _alcoholSalesAdvanceAmount;
    private int _paidAmountType = 1;
    private decimal _a7PaidAmount;
    private decimal _a7WithheldAmount;
    private decimal _a7NetPaidAmount;

    public AnnexWorkbenchViewModel(CurrentDeclarationService currentDeclarationService, EmpccaAnnexesApiClient empccaAnnexesApiClient)
    {
        _currentDeclarationService = currentDeclarationService;
        _empccaAnnexesApiClient = empccaAnnexesApiClient;

        NewCommand = new RelayCommand(ResetForm);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ValidateCommand = new AsyncRelayCommand(ValidateAsync);
        OpenDeclarationsCommand = new RelayCommand(() => _navigateHandler?.Invoke());
    }

    public ObservableCollection<AnnexLineItem> Lines { get; } = [];
    public ObservableCollection<AnnexValidationMessage> ValidationMessages { get; } = [];

    public IReadOnlyList<AnnexOption> A1IdentifierTypes { get; } = [new(2, "CIN"), new(3, "Carte Sejour")];
    public IReadOnlyList<AnnexOption> ResidentIdentifierTypes { get; } = [new(1, "Matricule Fiscale"), new(2, "CIN")];
    public IReadOnlyList<AnnexOption> GeneralIdentifierTypes { get; } = [new(1, "Matricule Fiscale"), new(2, "CIN"), new(3, "Carte Sejour"), new(4, "Identifiant non resident")];
    public IReadOnlyList<AnnexOption> NonResidentIdentifierTypes { get; } = [new(3, "Carte Sejour"), new(4, "Identifiant non resident")];
    public IReadOnlyList<AnnexOption> FamilySituationOptions { get; } = [new(1, "Celibataire"), new(2, "Marie"), new(3, "Divorce"), new(4, "Veuf")];
    public IReadOnlyList<AnnexOption> A2AmountTypes { get; } = [new(0, "Autre"), new(1, "Honoraires 3%"), new(2, "Remunerations et primes"), new(3, "Remunerations salaries 15%"), new(4, "Plus-value immobiliere 2.5%"), new(5, "Loyer hotels 5%"), new(6, "Artistes createurs 3%")];
    public IReadOnlyList<AnnexOption> A4AmountTypes { get; } = [new(0, "Autre"), new(1, "Honoraires"), new(2, "Travaux <= 6 mois"), new(3, "Plus-value immobiliere"), new(4, "Valeur mobiliere"), new(5, "Plus-value cession"), new(6, "Regime privilegie")];
    public IReadOnlyList<AnnexOption> A6RebateTypes { get; } = [new(0, "Aucune"), new(1, "Commerciale"), new(2, "Non commerciale")];
    public IReadOnlyList<AnnexOption> A7PaidAmountTypes { get; } = [new(1, "Type 1"), new(2, "Type 2"), new(3, "Type 3"), new(4, "Type 4"), new(5, "Type 5"), new(6, "Type 6"), new(7, "Type 7"), new(8, "Type 8"), new(15, "Type 15"), new(16, "Type 16"), new(17, "Type 17"), new(19, "Type 19"), new(20, "Type 20"), new(21, "Type 21"), new(22, "Type 22"), new(23, "Type 23"), new(24, "Type 24"), new(25, "Type 25"), new(26, "Type 26"), new(27, "Type 27"), new(28, "Type 28"), new(29, "Type 29")];

    public IRelayCommand NewCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ValidateCommand { get; }
    public IRelayCommand OpenDeclarationsCommand { get; }

    public string Title => _configuration?.Title ?? "Annexe";
    public string Description => _configuration?.Description ?? string.Empty;
    public string AnnexCode => _configuration?.AnnexCode ?? string.Empty;
    public bool IsA1 => AnnexCode == "A1";
    public bool IsA2 => AnnexCode == "A2";
    public bool IsA3 => AnnexCode == "A3";
    public bool IsA4 => AnnexCode == "A4";
    public bool IsA5 => AnnexCode == "A5";
    public bool IsA6 => AnnexCode == "A6";
    public bool IsA7 => AnnexCode == "A7";

    public IReadOnlyList<AnnexOption> CurrentIdentifierTypes => AnnexCode switch
    {
        "A1" => A1IdentifierTypes,
        "A2" => ResidentIdentifierTypes,
        "A4" => NonResidentIdentifierTypes,
        _ => GeneralIdentifierTypes
    };

    public AnnexLineItem? SelectedLine
    {
        get => _selectedLine;
        set
        {
            if (SetProperty(ref _selectedLine, value) && value is not null)
            {
                LoadLineIntoForm(value.Source);
            }
        }
    }

    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public bool HasDeclaration { get => _hasDeclaration; set => SetProperty(ref _hasDeclaration, value); }
    public string CompanyName { get => _companyName; set => SetProperty(ref _companyName, value); }
    public string FiscalYear { get => _fiscalYear; set => SetProperty(ref _fiscalYear, value); }
    public string DeclarationTitle { get => _declarationTitle; set => SetProperty(ref _declarationTitle, value); }
    public string DeclarationStatus { get => _declarationStatus; set => SetProperty(ref _declarationStatus, value); }
    public string ValidationStatus { get => _validationStatus; set => SetProperty(ref _validationStatus, value); }
    public int LineCount { get => _lineCount; set => SetProperty(ref _lineCount, value); }
    public decimal GrossTotal { get => _grossTotal; set => SetProperty(ref _grossTotal, value); }
    public decimal WithheldTotal { get => _withheldTotal; set => SetProperty(ref _withheldTotal, value); }
    public decimal NetTotal { get => _netTotal; set => SetProperty(ref _netTotal, value); }
    public int OrderNumber { get => _orderNumber; set => SetProperty(ref _orderNumber, value); }
    public int BeneficiaryIdentifierType { get => _beneficiaryIdentifierType; set => SetProperty(ref _beneficiaryIdentifierType, value); }
    public string BeneficiaryIdentifier { get => _beneficiaryIdentifier; set => SetProperty(ref _beneficiaryIdentifier, value); }
    public string BeneficiaryName { get => _beneficiaryName; set => SetProperty(ref _beneficiaryName, value); }
    public string? BeneficiaryActivity { get => _beneficiaryActivity; set => SetProperty(ref _beneficiaryActivity, value); }
    public string? BeneficiaryJobTitle { get => _beneficiaryJobTitle; set => SetProperty(ref _beneficiaryJobTitle, value); }
    public string BeneficiaryAddress { get => _beneficiaryAddress; set => SetProperty(ref _beneficiaryAddress, value); }
    public int FamilySituation { get => _familySituation; set => SetProperty(ref _familySituation, value); }
    public int DependentChildrenCount { get => _dependentChildrenCount; set => SetProperty(ref _dependentChildrenCount, value); }
    public DateTime WorkPeriodStart { get => _workPeriodStart; set => SetProperty(ref _workPeriodStart, value); }
    public DateTime WorkPeriodEnd { get => _workPeriodEnd; set => SetProperty(ref _workPeriodEnd, value); }
    public int WorkPeriodDays { get => _workPeriodDays; set => SetProperty(ref _workPeriodDays, value); }
    public decimal TaxableIncome { get => _taxableIncome; set => SetProperty(ref _taxableIncome, value); }
    public decimal BenefitsInKind { get => _benefitsInKind; set => SetProperty(ref _benefitsInKind, value); }
    public decimal GrossTaxableIncome { get => _grossTaxableIncome; set => SetProperty(ref _grossTaxableIncome, value); }
    public decimal ReinvestedIncome { get => _reinvestedIncome; set => SetProperty(ref _reinvestedIncome, value); }
    public decimal CommonRegimeWithheldAmount { get => _commonRegimeWithheldAmount; set => SetProperty(ref _commonRegimeWithheldAmount, value); }
    public decimal ForeignEmployeeWithheldAmount { get => _foreignEmployeeWithheldAmount; set => SetProperty(ref _foreignEmployeeWithheldAmount, value); }
    public decimal SocialSolidarityContribution { get => _socialSolidarityContribution; set => SetProperty(ref _socialSolidarityContribution, value); }
    public decimal A1NetPaidAmount { get => _a1NetPaidAmount; set => SetProperty(ref _a1NetPaidAmount, value); }
    public int A2AmountType { get => _a2AmountType; set => SetProperty(ref _a2AmountType, value); }
    public decimal GrossProfessionalAmount { get => _grossProfessionalAmount; set => SetProperty(ref _grossProfessionalAmount, value); }
    public decimal RealRegimeFeesAmount { get => _realRegimeFeesAmount; set => SetProperty(ref _realRegimeFeesAmount, value); }
    public decimal BoardAndSecuritiesAmount { get => _boardAndSecuritiesAmount; set => SetProperty(ref _boardAndSecuritiesAmount, value); }
    public decimal OccasionalWorkAmount { get => _occasionalWorkAmount; set => SetProperty(ref _occasionalWorkAmount, value); }
    public decimal RealEstateCapitalGainAmount { get => _realEstateCapitalGainAmount; set => SetProperty(ref _realEstateCapitalGainAmount, value); }
    public decimal HotelRentAmount { get => _hotelRentAmount; set => SetProperty(ref _hotelRentAmount, value); }
    public decimal ArtistRemunerationAmount { get => _artistRemunerationAmount; set => SetProperty(ref _artistRemunerationAmount, value); }
    public decimal PublicSectorVatWithheldAmount { get => _publicSectorVatWithheldAmount; set => SetProperty(ref _publicSectorVatWithheldAmount, value); }
    public decimal A2WithheldAmount { get => _a2WithheldAmount; set => SetProperty(ref _a2WithheldAmount, value); }
    public decimal A2NetPaidAmount { get => _a2NetPaidAmount; set => SetProperty(ref _a2NetPaidAmount, value); }
    public decimal SavingsAccountInterest { get => _savingsAccountInterest; set => SetProperty(ref _savingsAccountInterest, value); }
    public decimal OtherMovableCapitalIncome { get => _otherMovableCapitalIncome; set => SetProperty(ref _otherMovableCapitalIncome, value); }
    public decimal NonEstablishedBankLoanInterest { get => _nonEstablishedBankLoanInterest; set => SetProperty(ref _nonEstablishedBankLoanInterest, value); }
    public decimal A3WithheldAmount { get => _a3WithheldAmount; set => SetProperty(ref _a3WithheldAmount, value); }
    public decimal A3NetPaidAmount { get => _a3NetPaidAmount; set => SetProperty(ref _a3NetPaidAmount, value); }
    public int A4AmountType { get => _a4AmountType; set => SetProperty(ref _a4AmountType, value); }
    public decimal ProfessionalAmountRate { get => _professionalAmountRate; set => SetProperty(ref _professionalAmountRate, value); }
    public decimal ProfessionalAmount { get => _professionalAmount; set => SetProperty(ref _professionalAmount, value); }
    public decimal ConstructionWorkRate { get => _constructionWorkRate; set => SetProperty(ref _constructionWorkRate, value); }
    public decimal ConstructionWorkAmount { get => _constructionWorkAmount; set => SetProperty(ref _constructionWorkAmount, value); }
    public decimal A4RealEstateCapitalGainRate { get => _a4RealEstateCapitalGainRate; set => SetProperty(ref _a4RealEstateCapitalGainRate, value); }
    public decimal A4RealEstateCapitalGainAmount { get => _a4RealEstateCapitalGainAmount; set => SetProperty(ref _a4RealEstateCapitalGainAmount, value); }
    public decimal SecuritiesCapitalGainRate { get => _securitiesCapitalGainRate; set => SetProperty(ref _securitiesCapitalGainRate, value); }
    public decimal SecuritiesCapitalGainAmount { get => _securitiesCapitalGainAmount; set => SetProperty(ref _securitiesCapitalGainAmount, value); }
    public decimal SecuritiesIncomeRate { get => _securitiesIncomeRate; set => SetProperty(ref _securitiesIncomeRate, value); }
    public decimal SecuritiesIncomeAmount { get => _securitiesIncomeAmount; set => SetProperty(ref _securitiesIncomeAmount, value); }
    public decimal PrivilegedTaxRegimeAmount { get => _privilegedTaxRegimeAmount; set => SetProperty(ref _privilegedTaxRegimeAmount, value); }
    public decimal A4VatWithheldAmount { get => _a4VatWithheldAmount; set => SetProperty(ref _a4VatWithheldAmount, value); }
    public decimal A4WithheldAmount { get => _a4WithheldAmount; set => SetProperty(ref _a4WithheldAmount, value); }
    public decimal A4NetPaidAmount { get => _a4NetPaidAmount; set => SetProperty(ref _a4NetPaidAmount, value); }
    public decimal PurchasesFromTenPercentCompanies { get => _purchasesFromTenPercentCompanies; set => SetProperty(ref _purchasesFromTenPercentCompanies, value); }
    public decimal PurchasesFromFifteenPercentCompanies { get => _purchasesFromFifteenPercentCompanies; set => SetProperty(ref _purchasesFromFifteenPercentCompanies, value); }
    public decimal PurchasesFromTwoThirdsDeductionBusinesses { get => _purchasesFromTwoThirdsDeductionBusinesses; set => SetProperty(ref _purchasesFromTwoThirdsDeductionBusinesses, value); }
    public decimal PurchasesFromOtherBusinesses { get => _purchasesFromOtherBusinesses; set => SetProperty(ref _purchasesFromOtherBusinesses, value); }
    public decimal A5VatWithheldAmount { get => _a5VatWithheldAmount; set => SetProperty(ref _a5VatWithheldAmount, value); }
    public decimal DeliveryPlatformThreePercentWithheldAmount { get => _deliveryPlatformThreePercentWithheldAmount; set => SetProperty(ref _deliveryPlatformThreePercentWithheldAmount, value); }
    public decimal A5WithheldAmount { get => _a5WithheldAmount; set => SetProperty(ref _a5WithheldAmount, value); }
    public decimal A5NetPaidAmount { get => _a5NetPaidAmount; set => SetProperty(ref _a5NetPaidAmount, value); }
    public int RebateType { get => _rebateType; set => SetProperty(ref _rebateType, value); }
    public decimal RebateAmount { get => _rebateAmount; set => SetProperty(ref _rebateAmount, value); }
    public decimal FlatRegimeSalesAmount { get => _flatRegimeSalesAmount; set => SetProperty(ref _flatRegimeSalesAmount, value); }
    public decimal FlatRegimeSalesAdvanceAmount { get => _flatRegimeSalesAdvanceAmount; set => SetProperty(ref _flatRegimeSalesAdvanceAmount, value); }
    public decimal GamblingIncomeAmount { get => _gamblingIncomeAmount; set => SetProperty(ref _gamblingIncomeAmount, value); }
    public decimal GamblingWithheldAmount { get => _gamblingWithheldAmount; set => SetProperty(ref _gamblingWithheldAmount, value); }
    public decimal DistributionNetworkSalesAmount { get => _distributionNetworkSalesAmount; set => SetProperty(ref _distributionNetworkSalesAmount, value); }
    public decimal DistributionNetworkWithheldAmount { get => _distributionNetworkWithheldAmount; set => SetProperty(ref _distributionNetworkWithheldAmount, value); }
    public decimal CashCollectionsAmount { get => _cashCollectionsAmount; set => SetProperty(ref _cashCollectionsAmount, value); }
    public decimal AlcoholSalesAmount { get => _alcoholSalesAmount; set => SetProperty(ref _alcoholSalesAmount, value); }
    public decimal AlcoholSalesAdvanceAmount { get => _alcoholSalesAdvanceAmount; set => SetProperty(ref _alcoholSalesAdvanceAmount, value); }
    public int PaidAmountType { get => _paidAmountType; set => SetProperty(ref _paidAmountType, value); }
    public decimal A7PaidAmount { get => _a7PaidAmount; set => SetProperty(ref _a7PaidAmount, value); }
    public decimal A7WithheldAmount { get => _a7WithheldAmount; set => SetProperty(ref _a7WithheldAmount, value); }
    public decimal A7NetPaidAmount { get => _a7NetPaidAmount; set => SetProperty(ref _a7NetPaidAmount, value); }

    public void Configure(AnnexWorkbenchConfiguration configuration, AnnexWorkbenchNavigateHandler? navigateHandler)
    {
        _configuration = configuration;
        _navigateHandler = navigateHandler;
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(AnnexCode));
        OnPropertyChanged(nameof(IsA1));
        OnPropertyChanged(nameof(IsA2));
        OnPropertyChanged(nameof(IsA3));
        OnPropertyChanged(nameof(IsA4));
        OnPropertyChanged(nameof(IsA5));
        OnPropertyChanged(nameof(IsA6));
        OnPropertyChanged(nameof(IsA7));
        OnPropertyChanged(nameof(CurrentIdentifierTypes));
    }

    public async Task LoadAsync()
    {
        var declaration = _currentDeclarationService.Current;
        if (declaration is null)
        {
            HasDeclaration = false;
            CompanyName = "-";
            FiscalYear = "-";
            DeclarationTitle = "Aucune declaration active";
            DeclarationStatus = "-";
            ValidationStatus = "Non disponible";
            Lines.Clear();
            ValidationMessages.Clear();
            LineCount = 0;
            GrossTotal = 0;
            WithheldTotal = 0;
            NetTotal = 0;
            StatusMessage = "Veuillez creer ou selectionner une declaration employeur avant de gerer les annexes.";
            ResetForm();
            return;
        }

        try
        {
            IsBusy = true;
            HasDeclaration = true;
            CompanyName = declaration.ClientRaisonSociale ?? declaration.ClientCode ?? "-";
            FiscalYear = declaration.Year.ToString();
            DeclarationTitle = declaration.Title;
            DeclarationStatus = declaration.Status;
            await LoadLinesAndSummaryAsync(declaration.Id);
            StatusMessage = $"{Title} chargee : {Lines.Count} ligne(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur chargement annexe : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadLinesAndSummaryAsync(Guid declarationId)
    {
        Lines.Clear();
        ValidationMessages.Clear();

        switch (AnnexCode)
        {
            case "A1":
                foreach (var line in await _empccaAnnexesApiClient.GetA1LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.JobTitle ?? line.Beneficiary.Activity, line.Details.GrossTaxableIncome, line.Details.CommonRegimeWithheldAmount + line.Details.ForeignEmployeeWithheldAmount + line.Details.SocialSolidarityContribution, line.Details.NetPaidAmount, "Brouillon", line));
                }
                var a1Summary = await _empccaAnnexesApiClient.GetA1SummaryAsync(declarationId);
                LineCount = a1Summary.LineCount;
                GrossTotal = a1Summary.GrossTaxableIncomeTotal;
                WithheldTotal = a1Summary.CommonRegimeWithheldTotal + a1Summary.ForeignEmployeeWithheldTotal + a1Summary.SocialSolidarityContributionTotal;
                NetTotal = a1Summary.NetPaidTotal;
                break;
            case "A2":
                foreach (var line in await _empccaAnnexesApiClient.GetA2LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.GrossProfessionalAmount + line.Details.RealRegimeFeesAmount + line.Details.BoardAndSecuritiesAmount + line.Details.OccasionalWorkAmount + line.Details.RealEstateCapitalGainAmount + line.Details.HotelRentAmount + line.Details.ArtistRemunerationAmount, line.Details.WithheldAmount, line.Details.NetPaidAmount, "Brouillon", line));
                }
                var a2Summary = await _empccaAnnexesApiClient.GetA2SummaryAsync(declarationId);
                LineCount = a2Summary.LineCount;
                GrossTotal = a2Summary.GrossProfessionalTotal;
                WithheldTotal = a2Summary.WithheldTotal;
                NetTotal = a2Summary.NetPaidTotal;
                break;
            case "A3":
                foreach (var line in await _empccaAnnexesApiClient.GetA3LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.SavingsAccountInterest + line.Details.OtherMovableCapitalIncome + line.Details.NonEstablishedBankLoanInterest, line.Details.WithheldAmount, line.Details.NetPaidAmount, "Brouillon", line));
                }
                ApplySummary(await _empccaAnnexesApiClient.GetA3SummaryAsync(declarationId));
                break;
            case "A4":
                foreach (var line in await _empccaAnnexesApiClient.GetA4LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.ProfessionalAmount + line.Details.ConstructionWorkAmount + line.Details.RealEstateCapitalGainAmount + line.Details.SecuritiesCapitalGainAmount + line.Details.SecuritiesIncomeAmount + line.Details.PrivilegedTaxRegimeAmount, line.Details.WithheldAmount, line.Details.NetPaidAmount, "Brouillon", line));
                }
                ApplySummary(await _empccaAnnexesApiClient.GetA4SummaryAsync(declarationId));
                break;
            case "A5":
                foreach (var line in await _empccaAnnexesApiClient.GetA5LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.PurchasesFromTenPercentCompanies + line.Details.PurchasesFromFifteenPercentCompanies + line.Details.PurchasesFromTwoThirdsDeductionBusinesses + line.Details.PurchasesFromOtherBusinesses, line.Details.WithheldAmount, line.Details.NetPaidAmount, "Brouillon", line));
                }
                var a5Summary = await _empccaAnnexesApiClient.GetA5SummaryAsync(declarationId);
                LineCount = a5Summary.LineCount;
                GrossTotal = a5Summary.PurchasesTotal;
                WithheldTotal = a5Summary.WithheldTotal;
                NetTotal = a5Summary.NetPaidTotal;
                break;
            case "A6":
                foreach (var line in await _empccaAnnexesApiClient.GetA6LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.RebateAmount + line.Details.FlatRegimeSalesAmount + line.Details.GamblingIncomeAmount + line.Details.DistributionNetworkSalesAmount + line.Details.CashCollectionsAmount + line.Details.AlcoholSalesAmount, line.Details.FlatRegimeSalesAdvanceAmount + line.Details.GamblingWithheldAmount + line.Details.DistributionNetworkWithheldAmount + line.Details.AlcoholSalesAdvanceAmount, 0m, "Brouillon", line));
                }
                ApplySummary(await _empccaAnnexesApiClient.GetA6SummaryAsync(declarationId));
                break;
            case "A7":
                foreach (var line in await _empccaAnnexesApiClient.GetA7LinesAsync(declarationId))
                {
                    Lines.Add(new AnnexLineItem(line.Id, line.OrderNumber, IdentifierTypeLabel(line.Beneficiary.IdentifierType), line.Beneficiary.Identifier, line.Beneficiary.Name, line.Beneficiary.Activity, line.Details.PaidAmount, line.Details.WithheldAmount, line.Details.NetPaidAmount, "Brouillon", line));
                }
                ApplySummary(await _empccaAnnexesApiClient.GetA7SummaryAsync(declarationId));
                break;
        }

        if (_editingLineId.HasValue)
        {
            SelectedLine = Lines.FirstOrDefault(x => x.Id == _editingLineId.Value);
        }

        if (SelectedLine is null)
        {
            SetNextOrder();
        }
    }

    private async Task SaveAsync()
    {
        var declaration = _currentDeclarationService.Current;
        if (declaration is null)
        {
            StatusMessage = "Veuillez creer ou selectionner une declaration employeur avant d'enregistrer.";
            return;
        }

        try
        {
            IsBusy = true;
            switch (AnnexCode)
            {
                case "A1":
                    var a1 = BuildA1Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA1LineAsync(declaration.Id, _editingLineId.Value, a1);
                    else await _empccaAnnexesApiClient.CreateA1LineAsync(declaration.Id, a1);
                    break;
                case "A2":
                    var a2 = BuildA2Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA2LineAsync(declaration.Id, _editingLineId.Value, a2);
                    else await _empccaAnnexesApiClient.CreateA2LineAsync(declaration.Id, a2);
                    break;
                case "A3":
                    var a3 = BuildA3Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA3LineAsync(declaration.Id, _editingLineId.Value, a3);
                    else await _empccaAnnexesApiClient.CreateA3LineAsync(declaration.Id, a3);
                    break;
                case "A4":
                    var a4 = BuildA4Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA4LineAsync(declaration.Id, _editingLineId.Value, a4);
                    else await _empccaAnnexesApiClient.CreateA4LineAsync(declaration.Id, a4);
                    break;
                case "A5":
                    var a5 = BuildA5Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA5LineAsync(declaration.Id, _editingLineId.Value, a5);
                    else await _empccaAnnexesApiClient.CreateA5LineAsync(declaration.Id, a5);
                    break;
                case "A6":
                    var a6 = BuildA6Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA6LineAsync(declaration.Id, _editingLineId.Value, a6);
                    else await _empccaAnnexesApiClient.CreateA6LineAsync(declaration.Id, a6);
                    break;
                case "A7":
                    var a7 = BuildA7Request();
                    if (_editingLineId.HasValue) await _empccaAnnexesApiClient.UpdateA7LineAsync(declaration.Id, _editingLineId.Value, a7);
                    else await _empccaAnnexesApiClient.CreateA7LineAsync(declaration.Id, a7);
                    break;
            }

            await LoadLinesAndSummaryAsync(declaration.Id);
            ResetForm();
            StatusMessage = "Ligne enregistree avec succes.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur enregistrement : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task DeleteAsync()
    {
        var declaration = _currentDeclarationService.Current;
        if (declaration is null)
        {
            StatusMessage = "Veuillez selectionner une declaration.";
            return;
        }

        if (!_editingLineId.HasValue)
        {
            StatusMessage = "Selectionne une ligne d'annexe a supprimer.";
            return;
        }

        if (MessageBox.Show("Voulez-vous vraiment supprimer cette ligne d'annexe ?", "Confirmation suppression", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            StatusMessage = "Suppression annulee.";
            return;
        }

        try
        {
            IsBusy = true;
            switch (AnnexCode)
            {
                case "A1": await _empccaAnnexesApiClient.DeleteA1LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A2": await _empccaAnnexesApiClient.DeleteA2LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A3": await _empccaAnnexesApiClient.DeleteA3LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A4": await _empccaAnnexesApiClient.DeleteA4LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A5": await _empccaAnnexesApiClient.DeleteA5LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A6": await _empccaAnnexesApiClient.DeleteA6LineAsync(declaration.Id, _editingLineId.Value); break;
                case "A7": await _empccaAnnexesApiClient.DeleteA7LineAsync(declaration.Id, _editingLineId.Value); break;
            }

            await LoadLinesAndSummaryAsync(declaration.Id);
            ResetForm();
            StatusMessage = "Ligne supprimee avec succes.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur suppression : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ValidateAsync()
    {
        var declaration = _currentDeclarationService.Current;
        if (declaration is null)
        {
            StatusMessage = "Veuillez selectionner une declaration.";
            return;
        }

        try
        {
            IsBusy = true;
            EmpccaAnnexValidationDto result = AnnexCode switch
            {
                "A1" => await _empccaAnnexesApiClient.ValidateA1Async(declaration.Id),
                "A2" => await _empccaAnnexesApiClient.ValidateA2Async(declaration.Id),
                "A3" => await _empccaAnnexesApiClient.ValidateA3Async(declaration.Id),
                "A4" => await _empccaAnnexesApiClient.ValidateA4Async(declaration.Id),
                "A5" => await _empccaAnnexesApiClient.ValidateA5Async(declaration.Id),
                "A6" => await _empccaAnnexesApiClient.ValidateA6Async(declaration.Id),
                "A7" => await _empccaAnnexesApiClient.ValidateA7Async(declaration.Id),
                _ => new EmpccaAnnexValidationDto()
            };

            ValidationMessages.Clear();
            foreach (var issue in result.BlockingIssues)
            {
                ValidationMessages.Add(new AnnexValidationMessage("Blocking", issue));
            }

            foreach (var warning in result.Warnings)
            {
                ValidationMessages.Add(new AnnexValidationMessage("Warning", warning));
            }

            ValidationStatus = result.IsValid ? "Valide" : "Avec erreurs";
            StatusMessage = result.IsValid ? "Controle termine." : $"Controle termine : {result.BlockingIssues.Count} anomalie(s) bloquante(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur controle annexe : {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        _editingLineId = null;
        SelectedLine = null;
        SetNextOrder();
        BeneficiaryIdentifierType = CurrentIdentifierTypes.FirstOrDefault()?.Value ?? 2;
        BeneficiaryIdentifier = string.Empty;
        BeneficiaryName = string.Empty;
        BeneficiaryActivity = string.Empty;
        BeneficiaryJobTitle = string.Empty;
        BeneficiaryAddress = string.Empty;
        FamilySituation = 1;
        DependentChildrenCount = 0;
        WorkPeriodStart = new DateTime(_currentDeclarationService.Current?.Year ?? DateTime.Now.Year, 1, 1);
        WorkPeriodEnd = new DateTime(_currentDeclarationService.Current?.Year ?? DateTime.Now.Year, 12, 31);
        WorkPeriodDays = 365;
        TaxableIncome = 0m;
        BenefitsInKind = 0m;
        GrossTaxableIncome = 0m;
        ReinvestedIncome = 0m;
        CommonRegimeWithheldAmount = 0m;
        ForeignEmployeeWithheldAmount = 0m;
        SocialSolidarityContribution = 0m;
        A1NetPaidAmount = 0m;
        A2AmountType = 1;
        GrossProfessionalAmount = 0m;
        RealRegimeFeesAmount = 0m;
        BoardAndSecuritiesAmount = 0m;
        OccasionalWorkAmount = 0m;
        RealEstateCapitalGainAmount = 0m;
        HotelRentAmount = 0m;
        ArtistRemunerationAmount = 0m;
        PublicSectorVatWithheldAmount = 0m;
        A2WithheldAmount = 0m;
        A2NetPaidAmount = 0m;
        SavingsAccountInterest = 0m;
        OtherMovableCapitalIncome = 0m;
        NonEstablishedBankLoanInterest = 0m;
        A3WithheldAmount = 0m;
        A3NetPaidAmount = 0m;
        A4AmountType = 1;
        ProfessionalAmountRate = 0m;
        ProfessionalAmount = 0m;
        ConstructionWorkRate = 0m;
        ConstructionWorkAmount = 0m;
        A4RealEstateCapitalGainRate = 0m;
        A4RealEstateCapitalGainAmount = 0m;
        SecuritiesCapitalGainRate = 0m;
        SecuritiesCapitalGainAmount = 0m;
        SecuritiesIncomeRate = 0m;
        SecuritiesIncomeAmount = 0m;
        PrivilegedTaxRegimeAmount = 0m;
        A4VatWithheldAmount = 0m;
        A4WithheldAmount = 0m;
        A4NetPaidAmount = 0m;
        PurchasesFromTenPercentCompanies = 0m;
        PurchasesFromFifteenPercentCompanies = 0m;
        PurchasesFromTwoThirdsDeductionBusinesses = 0m;
        PurchasesFromOtherBusinesses = 0m;
        A5VatWithheldAmount = 0m;
        DeliveryPlatformThreePercentWithheldAmount = 0m;
        A5WithheldAmount = 0m;
        A5NetPaidAmount = 0m;
        RebateType = 0;
        RebateAmount = 0m;
        FlatRegimeSalesAmount = 0m;
        FlatRegimeSalesAdvanceAmount = 0m;
        GamblingIncomeAmount = 0m;
        GamblingWithheldAmount = 0m;
        DistributionNetworkSalesAmount = 0m;
        DistributionNetworkWithheldAmount = 0m;
        CashCollectionsAmount = 0m;
        AlcoholSalesAmount = 0m;
        AlcoholSalesAdvanceAmount = 0m;
        PaidAmountType = 1;
        A7PaidAmount = 0m;
        A7WithheldAmount = 0m;
        A7NetPaidAmount = 0m;
    }

    private void LoadLineIntoForm(object source)
    {
        _editingLineId = source switch
        {
            EmpccaAnnexA1LineDto a1 => a1.Id,
            EmpccaAnnexA2LineDto a2 => a2.Id,
            EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest> a3 => a3.Id,
            EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest> a4 => a4.Id,
            EmpccaAnnexA5LineDto a5 => a5.Id,
            EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest> a6 => a6.Id,
            EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest> a7 => a7.Id,
            _ => null
        };

        switch (source)
        {
            case EmpccaAnnexA1LineDto a1:
                LoadBeneficiary(a1.Beneficiary);
                OrderNumber = a1.OrderNumber;
                FamilySituation = a1.Details.FamilySituation;
                DependentChildrenCount = a1.Details.DependentChildrenCount;
        WorkPeriodStart = a1.Details.WorkPeriodStart.ToDateTime(TimeOnly.MinValue);
        WorkPeriodEnd = a1.Details.WorkPeriodEnd.ToDateTime(TimeOnly.MinValue);
                WorkPeriodDays = a1.Details.WorkPeriodDays;
                TaxableIncome = a1.Details.TaxableIncome;
                BenefitsInKind = a1.Details.BenefitsInKind;
                GrossTaxableIncome = a1.Details.GrossTaxableIncome;
                ReinvestedIncome = a1.Details.ReinvestedIncome;
                CommonRegimeWithheldAmount = a1.Details.CommonRegimeWithheldAmount;
                ForeignEmployeeWithheldAmount = a1.Details.ForeignEmployeeWithheldAmount;
                SocialSolidarityContribution = a1.Details.SocialSolidarityContribution;
                A1NetPaidAmount = a1.Details.NetPaidAmount;
                break;
            case EmpccaAnnexA2LineDto a2:
                LoadBeneficiary(a2.Beneficiary);
                OrderNumber = a2.OrderNumber;
                A2AmountType = a2.Details.AmountType;
                GrossProfessionalAmount = a2.Details.GrossProfessionalAmount;
                RealRegimeFeesAmount = a2.Details.RealRegimeFeesAmount;
                BoardAndSecuritiesAmount = a2.Details.BoardAndSecuritiesAmount;
                OccasionalWorkAmount = a2.Details.OccasionalWorkAmount;
                RealEstateCapitalGainAmount = a2.Details.RealEstateCapitalGainAmount;
                HotelRentAmount = a2.Details.HotelRentAmount;
                ArtistRemunerationAmount = a2.Details.ArtistRemunerationAmount;
                PublicSectorVatWithheldAmount = a2.Details.PublicSectorVatWithheldAmount;
                A2WithheldAmount = a2.Details.WithheldAmount;
                A2NetPaidAmount = a2.Details.NetPaidAmount;
                break;
            case EmpccaDetailedLineDto<CreateEmpccaAnnexA3LineRequest> a3:
                LoadBeneficiary(a3.Beneficiary);
                OrderNumber = a3.OrderNumber;
                SavingsAccountInterest = a3.Details.SavingsAccountInterest;
                OtherMovableCapitalIncome = a3.Details.OtherMovableCapitalIncome;
                NonEstablishedBankLoanInterest = a3.Details.NonEstablishedBankLoanInterest;
                A3WithheldAmount = a3.Details.WithheldAmount;
                A3NetPaidAmount = a3.Details.NetPaidAmount;
                break;
            case EmpccaDetailedLineDto<CreateEmpccaAnnexA4LineRequest> a4:
                LoadBeneficiary(a4.Beneficiary);
                OrderNumber = a4.OrderNumber;
                A4AmountType = a4.Details.AmountType;
                ProfessionalAmountRate = a4.Details.ProfessionalAmountRate;
                ProfessionalAmount = a4.Details.ProfessionalAmount;
                ConstructionWorkRate = a4.Details.ConstructionWorkRate;
                ConstructionWorkAmount = a4.Details.ConstructionWorkAmount;
                A4RealEstateCapitalGainRate = a4.Details.RealEstateCapitalGainRate;
                A4RealEstateCapitalGainAmount = a4.Details.RealEstateCapitalGainAmount;
                SecuritiesCapitalGainRate = a4.Details.SecuritiesCapitalGainRate;
                SecuritiesCapitalGainAmount = a4.Details.SecuritiesCapitalGainAmount;
                SecuritiesIncomeRate = a4.Details.SecuritiesIncomeRate;
                SecuritiesIncomeAmount = a4.Details.SecuritiesIncomeAmount;
                PrivilegedTaxRegimeAmount = a4.Details.PrivilegedTaxRegimeAmount;
                A4VatWithheldAmount = a4.Details.VatWithheldAmount;
                A4WithheldAmount = a4.Details.WithheldAmount;
                A4NetPaidAmount = a4.Details.NetPaidAmount;
                break;
            case EmpccaAnnexA5LineDto a5:
                LoadBeneficiary(a5.Beneficiary);
                OrderNumber = a5.OrderNumber;
                PurchasesFromTenPercentCompanies = a5.Details.PurchasesFromTenPercentCompanies;
                PurchasesFromFifteenPercentCompanies = a5.Details.PurchasesFromFifteenPercentCompanies;
                PurchasesFromTwoThirdsDeductionBusinesses = a5.Details.PurchasesFromTwoThirdsDeductionBusinesses;
                PurchasesFromOtherBusinesses = a5.Details.PurchasesFromOtherBusinesses;
                A5VatWithheldAmount = a5.Details.VatWithheldAmount;
                DeliveryPlatformThreePercentWithheldAmount = a5.Details.DeliveryPlatformThreePercentWithheldAmount;
                A5WithheldAmount = a5.Details.WithheldAmount;
                A5NetPaidAmount = a5.Details.NetPaidAmount;
                break;
            case EmpccaDetailedLineDto<CreateEmpccaAnnexA6LineRequest> a6:
                LoadBeneficiary(a6.Beneficiary);
                OrderNumber = a6.OrderNumber;
                RebateType = a6.Details.RebateType;
                RebateAmount = a6.Details.RebateAmount;
                FlatRegimeSalesAmount = a6.Details.FlatRegimeSalesAmount;
                FlatRegimeSalesAdvanceAmount = a6.Details.FlatRegimeSalesAdvanceAmount;
                GamblingIncomeAmount = a6.Details.GamblingIncomeAmount;
                GamblingWithheldAmount = a6.Details.GamblingWithheldAmount;
                DistributionNetworkSalesAmount = a6.Details.DistributionNetworkSalesAmount;
                DistributionNetworkWithheldAmount = a6.Details.DistributionNetworkWithheldAmount;
                CashCollectionsAmount = a6.Details.CashCollectionsAmount;
                AlcoholSalesAmount = a6.Details.AlcoholSalesAmount;
                AlcoholSalesAdvanceAmount = a6.Details.AlcoholSalesAdvanceAmount;
                break;
            case EmpccaDetailedLineDto<CreateEmpccaAnnexA7LineRequest> a7:
                LoadBeneficiary(a7.Beneficiary);
                OrderNumber = a7.OrderNumber;
                PaidAmountType = a7.Details.PaidAmountType;
                A7PaidAmount = a7.Details.PaidAmount;
                A7WithheldAmount = a7.Details.WithheldAmount;
                A7NetPaidAmount = a7.Details.NetPaidAmount;
                break;
        }
    }

    private void LoadBeneficiary(EmpccaBeneficiaryInput beneficiary)
    {
        BeneficiaryIdentifierType = beneficiary.IdentifierType;
        BeneficiaryIdentifier = beneficiary.Identifier;
        BeneficiaryName = beneficiary.Name;
        BeneficiaryActivity = beneficiary.Activity;
        BeneficiaryJobTitle = beneficiary.JobTitle;
        BeneficiaryAddress = beneficiary.Address;
    }

    private EmpccaBeneficiaryInput BuildBeneficiary() => new()
    {
        IdentifierType = BeneficiaryIdentifierType,
        Identifier = BeneficiaryIdentifier.Trim(),
        Name = BeneficiaryName.Trim(),
        Activity = string.IsNullOrWhiteSpace(BeneficiaryActivity) ? null : BeneficiaryActivity.Trim(),
        JobTitle = string.IsNullOrWhiteSpace(BeneficiaryJobTitle) ? null : BeneficiaryJobTitle.Trim(),
        Address = BeneficiaryAddress.Trim()
    };

    private CreateEmpccaAnnexA1LineRequest BuildA1Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        FamilySituation = FamilySituation,
        DependentChildrenCount = DependentChildrenCount,
        WorkPeriodStart = DateOnly.FromDateTime(WorkPeriodStart),
        WorkPeriodEnd = DateOnly.FromDateTime(WorkPeriodEnd),
        WorkPeriodDays = WorkPeriodDays,
        TaxableIncome = TaxableIncome,
        BenefitsInKind = BenefitsInKind,
        GrossTaxableIncome = GrossTaxableIncome,
        ReinvestedIncome = ReinvestedIncome,
        CommonRegimeWithheldAmount = CommonRegimeWithheldAmount,
        ForeignEmployeeWithheldAmount = ForeignEmployeeWithheldAmount,
        SocialSolidarityContribution = SocialSolidarityContribution,
        NetPaidAmount = A1NetPaidAmount
    };

    private CreateEmpccaAnnexA2LineRequest BuildA2Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        AmountType = A2AmountType,
        GrossProfessionalAmount = GrossProfessionalAmount,
        RealRegimeFeesAmount = RealRegimeFeesAmount,
        BoardAndSecuritiesAmount = BoardAndSecuritiesAmount,
        OccasionalWorkAmount = OccasionalWorkAmount,
        RealEstateCapitalGainAmount = RealEstateCapitalGainAmount,
        HotelRentAmount = HotelRentAmount,
        ArtistRemunerationAmount = ArtistRemunerationAmount,
        PublicSectorVatWithheldAmount = PublicSectorVatWithheldAmount,
        WithheldAmount = A2WithheldAmount,
        NetPaidAmount = A2NetPaidAmount
    };

    private CreateEmpccaAnnexA3LineRequest BuildA3Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        SavingsAccountInterest = SavingsAccountInterest,
        OtherMovableCapitalIncome = OtherMovableCapitalIncome,
        NonEstablishedBankLoanInterest = NonEstablishedBankLoanInterest,
        WithheldAmount = A3WithheldAmount,
        NetPaidAmount = A3NetPaidAmount
    };

    private CreateEmpccaAnnexA4LineRequest BuildA4Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        AmountType = A4AmountType,
        ProfessionalAmountRate = ProfessionalAmountRate,
        ProfessionalAmount = ProfessionalAmount,
        ConstructionWorkRate = ConstructionWorkRate,
        ConstructionWorkAmount = ConstructionWorkAmount,
        RealEstateCapitalGainRate = A4RealEstateCapitalGainRate,
        RealEstateCapitalGainAmount = A4RealEstateCapitalGainAmount,
        SecuritiesCapitalGainRate = SecuritiesCapitalGainRate,
        SecuritiesCapitalGainAmount = SecuritiesCapitalGainAmount,
        SecuritiesIncomeRate = SecuritiesIncomeRate,
        SecuritiesIncomeAmount = SecuritiesIncomeAmount,
        PrivilegedTaxRegimeAmount = PrivilegedTaxRegimeAmount,
        VatWithheldAmount = A4VatWithheldAmount,
        WithheldAmount = A4WithheldAmount,
        NetPaidAmount = A4NetPaidAmount
    };

    private CreateEmpccaAnnexA5LineRequest BuildA5Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        PurchasesFromTenPercentCompanies = PurchasesFromTenPercentCompanies,
        PurchasesFromFifteenPercentCompanies = PurchasesFromFifteenPercentCompanies,
        PurchasesFromTwoThirdsDeductionBusinesses = PurchasesFromTwoThirdsDeductionBusinesses,
        PurchasesFromOtherBusinesses = PurchasesFromOtherBusinesses,
        VatWithheldAmount = A5VatWithheldAmount,
        DeliveryPlatformThreePercentWithheldAmount = DeliveryPlatformThreePercentWithheldAmount,
        WithheldAmount = A5WithheldAmount,
        NetPaidAmount = A5NetPaidAmount
    };

    private CreateEmpccaAnnexA6LineRequest BuildA6Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        RebateType = RebateType,
        RebateAmount = RebateAmount,
        FlatRegimeSalesAmount = FlatRegimeSalesAmount,
        FlatRegimeSalesAdvanceAmount = FlatRegimeSalesAdvanceAmount,
        GamblingIncomeAmount = GamblingIncomeAmount,
        GamblingWithheldAmount = GamblingWithheldAmount,
        DistributionNetworkSalesAmount = DistributionNetworkSalesAmount,
        DistributionNetworkWithheldAmount = DistributionNetworkWithheldAmount,
        CashCollectionsAmount = CashCollectionsAmount,
        AlcoholSalesAmount = AlcoholSalesAmount,
        AlcoholSalesAdvanceAmount = AlcoholSalesAdvanceAmount
    };

    private CreateEmpccaAnnexA7LineRequest BuildA7Request() => new()
    {
        OrderNumber = OrderNumber,
        Beneficiary = BuildBeneficiary(),
        PaidAmountType = PaidAmountType,
        PaidAmount = A7PaidAmount,
        WithheldAmount = A7WithheldAmount,
        NetPaidAmount = A7NetPaidAmount
    };

    private void ApplySummary(EmpccaAnnexSummaryDto summary)
    {
        LineCount = summary.LineCount;
        GrossTotal = summary.GrossAmountTotal;
        WithheldTotal = summary.WithheldAmountTotal;
        NetTotal = summary.NetPaidAmountTotal;
    }

    private void SetNextOrder()
    {
        OrderNumber = Lines.Count == 0 ? 1 : Lines.Max(x => x.OrderNumber) + 1;
    }

    private static string IdentifierTypeLabel(int value) => value switch
    {
        1 => "Matricule Fiscale",
        2 => "CIN",
        3 => "Carte Sejour",
        4 => "Identifiant non resident",
        _ => "Autre"
    };
}

public sealed record AnnexWorkbenchConfiguration(string AnnexCode, string Title, string Description);
public delegate void AnnexWorkbenchNavigateHandler();
public sealed record AnnexOption(int Value, string Label);
public sealed record AnnexValidationMessage(string Severity, string Message);
public sealed record AnnexLineItem(Guid Id, int OrderNumber, string IdentifierType, string Identifier, string BeneficiaryName, string? Activity, decimal GrossAmount, decimal WithheldAmount, decimal NetAmount, string Status, object Source);
