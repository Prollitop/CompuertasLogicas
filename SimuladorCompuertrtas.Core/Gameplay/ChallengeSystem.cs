using System.Text.Json;
using SimuladorCompuertrtas.Core.Logic;
using SimuladorCompuertrtas.Core.Models;
using SimuladorCompuertrtas.Core.Services;

namespace SimuladorCompuertrtas.Core.Gameplay;

public enum ChallengeDifficulty { Easy, Medium, Hard }
public sealed record ChallengeDefinition(string Id, string Title, string Description, ChallengeDifficulty Difficulty, int InputCount, IReadOnlySet<string> AllowedTypes, Func<IReadOnlyList<LogicState>, LogicState> ExpectedOutput, int BaseXp, int UnlockAfter);
public sealed record ChallengeResult(bool IsSuccessful, int Stars, int XpAwarded, string Message);
public sealed class ChallengeCatalog
{
    public IReadOnlyList<ChallengeDefinition> Challenges { get; } =
    [ C("01","Enciende el LED","Conecta una entrada a una salida.",1,["switch","led"], x=>x[0],50,0), C("02","Las dos condiciones","La salida solo se enciende con A y B.",2,["switch","led","and"],x=> x.All(v=>v==LogicState.High)?LogicState.High:LogicState.Low,75,1), C("03","Una condición basta","La salida se enciende con al menos una entrada.",2,["switch","led","or"],x=>x.Any(v=>v==LogicState.High)?LogicState.High:LogicState.Low,75,2), C("04","Inversión","Invierte la señal de entrada.",1,["switch","led","not"],x=>x[0]==LogicState.High?LogicState.Low:LogicState.High,75,3), C("05","¿Son iguales?","Detecta entradas iguales.",2,["switch","led","xnor"],x=>x[0]==x[1]?LogicState.High:LogicState.Low,100,4), C("06","Solo una","Enciende solo con una entrada activa.",2,["switch","led","xor"],x=>x[0]!=x[1]?LogicState.High:LogicState.Low,100,5), C("07","Todo desde NAND","Construye NOT con NAND.",1,["switch","led","nand"],x=>x[0]==LogicState.High?LogicState.Low:LogicState.High,125,6), C("08","Todo desde NOR","Construye NOT con NOR.",1,["switch","led","nor"],x=>x[0]==LogicState.High?LogicState.Low:LogicState.High,125,7), C("09","Detector de combinación","Enciende solo cuando A es 1 y B es 0.",2,["switch","led","and","not"],x=>x[0]==LogicState.High&&x[1]==LogicState.Low?LogicState.High:LogicState.Low,150,8), C("10","Construye XOR","Construye XOR sin usar XOR.",2,["switch","led","and","or","not","nand","nor"],x=>x[0]!=x[1]?LogicState.High:LogicState.Low,175,9) ];
    private static ChallengeDefinition C(string id,string title,string description,int inputs,string[] allowed,Func<IReadOnlyList<LogicState>,LogicState> expected,int xp,int unlock) => new(id,title,description, unlock >= 6 ? ChallengeDifficulty.Hard : unlock >= 3 ? ChallengeDifficulty.Medium : ChallengeDifficulty.Easy,inputs,new HashSet<string>(allowed),expected,xp,unlock);
}
public sealed class ChallengeValidator
{
    private readonly CircuitSimulator _simulator = new();
    public ChallengeResult Validate(ChallengeDefinition challenge, Circuit circuit)
    {
        var invalid = circuit.Components.FirstOrDefault(component => !challenge.AllowedTypes.Contains(TypeOf(component)));
        if (invalid is not null) return new(false,0,0,$"{invalid.DisplayName} no está permitido en este desafío.");
        var inputs = circuit.Components.OfType<InputComponent>().Take(challenge.InputCount).ToArray(); var output = circuit.Components.OfType<OutputComponent>().FirstOrDefault();
        if (inputs.Length != challenge.InputCount || output is null) return new(false,0,0,"Agrega las entradas y el LED requeridos antes de comprobar.");
        for (var value=0; value < (1 << inputs.Length); value++) { for(var i=0;i<inputs.Length;i++) inputs[i].State=(value&(1<<(inputs.Length-i-1)))!=0?LogicState.High:LogicState.Low; try { _simulator.Evaluate(circuit); } catch(CircuitSimulationException) { return new(false,0,0,"El circuito está incompleto. Revisa todas las conexiones."); } if(output.Input.State!=challenge.ExpectedOutput(inputs.Select(input=>input.State).ToArray())) return new(false,0,0,"Aún no coincide con todas las combinaciones de la tabla de verdad."); }
        var stars = circuit.Components.Count <= challenge.InputCount + 2 ? 3 : circuit.Components.Count <= challenge.InputCount + 4 ? 2 : 1; return new(true,stars,challenge.BaseXp + (stars == 3 ? 25 : 0),"¡Excelente! Tu circuito cumple todas las combinaciones.");
    }
    private static string TypeOf(Component component) => component switch { InputComponent => "switch", OutputComponent => "led", LogicGate gate => gate.Definition.Id, _ => "unknown" };
}
public sealed class PlayerProgress
{
    public int Xp { get; set; }
    public HashSet<string> CompletedChallenges { get; set; } = [];
    public Dictionary<string,int> StarsByChallenge { get; set; } = [];
    public HashSet<string> Achievements { get; set; } = [];
    public int Level => Math.Min(5, Xp / 200 + 1);
    public string Rank => Level switch { 1=>"Aprendiz",2=>"Explorador",3=>"Constructor",4=>"Diseñador",_=>"Ingeniero lógico" };
    public int TotalStars => StarsByChallenge.Values.Sum();
}
public sealed class ProgressService(string? path = null)
{
    public string Path { get; } = path ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LogicLab","player_progress.json");
    public PlayerProgress Load() { try { return File.Exists(Path) ? JsonSerializer.Deserialize<PlayerProgress>(File.ReadAllText(Path)) ?? new() : new(); } catch { return new(); } }
    public void Save(PlayerProgress progress) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path)!); File.WriteAllText(Path,JsonSerializer.Serialize(progress)); }
    public void Record(PlayerProgress progress,ChallengeDefinition challenge,ChallengeResult result) { if(!result.IsSuccessful)return; var first=progress.CompletedChallenges.Add(challenge.Id); progress.StarsByChallenge[challenge.Id]=Math.Max(progress.StarsByChallenge.GetValueOrDefault(challenge.Id),result.Stars); if(first)progress.Xp+=result.XpAwarded; if(progress.CompletedChallenges.Count>=1)progress.Achievements.Add("first-circuit"); if(result.Stars==3)progress.Achievements.Add("three-stars"); if(progress.CompletedChallenges.Count>=5)progress.Achievements.Add("builder"); if(challenge.Id=="10")progress.Achievements.Add("xor-master"); Save(progress); }
}
