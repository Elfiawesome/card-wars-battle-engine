using CardWars.Core.Data;
using CardWars.Server.Session;

namespace CardWars.Server.Vanilla.Session;

public sealed class BattleInstanceProvider : IInstanceProvider
{
	public IServerInstance Create(InstanceCreateContext ctx)
		=> new BattleInstance { InstanceId = Guid.Parse(ctx.Key) };

	public IServerInstance Load(InstanceLoadContext ctx)
		=> DataTagMapper.FromTag<BattleInstance>(ctx.SavedData);
}
