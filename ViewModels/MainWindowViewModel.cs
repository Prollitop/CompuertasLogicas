namespace SimuladorCompuertrtas.ViewModels;

/// <summary>Presentation entry point; later phases will add editor and learning view models here.</summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    public CircuitEditorViewModel Editor { get; } = new();
    public LearningViewModel Learning { get; } = new();
    public ChallengesViewModel Challenges { get; }
    private bool _isLearningMode;
    private bool _isChallengesMode;
    public MainWindowViewModel() => Challenges = new ChallengesViewModel(Editor, () => IsLaboratoryMode = true);
    public bool IsLearningMode { get => _isLearningMode; set { _isLearningMode = value; if(value)_isChallengesMode=false; OnPropertyChanged(); OnPropertyChanged(nameof(IsLaboratoryMode)); OnPropertyChanged(nameof(IsChallengesMode)); } }
    public bool IsLaboratoryMode { get => !IsLearningMode&&!IsChallengesMode; set { if (value){_isChallengesMode=false;IsLearningMode=false;} } }
    public bool IsChallengesMode { get=>_isChallengesMode; set{_isChallengesMode=value;if(value)_isLearningMode=false;OnPropertyChanged();OnPropertyChanged(nameof(IsLaboratoryMode));OnPropertyChanged(nameof(IsLearningMode));} }
}
