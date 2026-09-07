using System.Collections.ObjectModel; using System.Windows.Input; using SimuladorCompuertrtas.Commands; using SimuladorCompuertrtas.Core.Gameplay;
namespace SimuladorCompuertrtas.ViewModels;
public sealed class ChallengesViewModel : ViewModelBase
{
 private readonly CircuitEditorViewModel _editor; private readonly ChallengeValidator _validator=new(); private readonly ProgressService _progressService=new();
 public ChallengeCatalog Catalog { get; }=new(); public PlayerProgress Progress { get; private set; }
 public ChallengeDefinition? Active { get; private set; } public string ResultMessage { get; private set; }="Elige un desafío para comenzar."; public ICommand StartCommand {get;} public ICommand CheckCommand {get;} public ICommand ResetCommand {get;}
 private readonly Action _openEditor;
 public bool HasActiveChallenge => Active is not null;
 public ChallengesViewModel(CircuitEditorViewModel editor, Action openEditor){_editor=editor;_openEditor=openEditor;Progress=_progressService.Load();StartCommand=new RelayCommand(p=>Start(p as ChallengeDefinition));CheckCommand=new RelayCommand(_=>Check());ResetCommand=new RelayCommand(_=>{if(Active is not null)_editor.LoadChallengeShell(Active.InputCount);});}
 public bool IsUnlocked(ChallengeDefinition c)=>Progress.CompletedChallenges.Count>=c.UnlockAfter;
 private void Start(ChallengeDefinition? c){if(c is null)return;if(!IsUnlocked(c)){ResultMessage=$"Bloqueado: completa {c.UnlockAfter} desafíos primero.";OnPropertyChanged(nameof(ResultMessage));return;}Active=c;_editor.LoadChallengeShell(c.InputCount);ResultMessage=$"{c.Description} Construye la solución y pulsa Comprobar.";OnPropertyChanged(nameof(Active));OnPropertyChanged(nameof(HasActiveChallenge));OnPropertyChanged(nameof(ResultMessage));_openEditor();}
 private void Check(){if(Active is null){ResultMessage="Selecciona un desafío primero.";}else{var r=_validator.Validate(Active,_editor.Circuit);ResultMessage=r.Message+(r.IsSuccessful?$" ★{new string('★',r.Stars)} +{r.XpAwarded} XP":"");_progressService.Record(Progress,Active,r);OnPropertyChanged(nameof(Progress));}OnPropertyChanged(nameof(ResultMessage));}
}
