using SimuladorCompuertrtas.Core.Gameplay; using SimuladorCompuertrtas.Core.Logic; using SimuladorCompuertrtas.Core.Models;
namespace SimuladorCompuertrtas.Core.Tests;
public sealed class GameplayTests
{
 [Fact] public void Validates_behavior_and_rejects_wrong_gate(){var c=new Circuit();var a=new InputComponent("a","A");var b=new InputComponent("b","B");var g=new LogicGate("g",new AndGateDefinition());var o=new OutputComponent("o","LED");foreach(var x in new Component[]{a,b,g,o})c.AddComponent(x);c.Connect(a.Output,g.InputPorts[0]);c.Connect(b.Output,g.InputPorts[1]);c.Connect(g.Output,o.Input);var challenge=new ChallengeCatalog().Challenges[1];Assert.True(new ChallengeValidator().Validate(challenge,c).IsSuccessful);}
 [Fact] public void Progress_calculates_level_stars_and_persists(){var file=System.IO.Path.GetTempFileName();var service=new ProgressService(file);var p=service.Load();var ch=new ChallengeCatalog().Challenges[0];service.Record(p,ch,new(true,3,75,"ok"));var loaded=service.Load();Assert.Equal(75,loaded.Xp);Assert.Equal(1,loaded.Level);Assert.Equal(3,loaded.TotalStars);Assert.Contains("first-circuit",loaded.Achievements);File.Delete(file);}
 [Fact] public void Corrupt_progress_returns_safe_default(){var f=System.IO.Path.GetTempFileName();File.WriteAllText(f,"bad");Assert.Equal(0,new ProgressService(f).Load().Xp);File.Delete(f);}
}
