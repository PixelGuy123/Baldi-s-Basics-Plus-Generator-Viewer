using System;

namespace BBP_Gen.Elements;

public abstract class RandomEvent(string name)
{
	public virtual void Initialize(Random rng)
	{
		rng.Next(); // Each one has this by default
	}

	public string Name { get; } = name;
}

public class PartyEvent() : RandomEvent("Party Event")
{
	public override void Initialize(Random rng)
	{
		base.Initialize(rng);
		rng.Next(); // Just one actually lol
	}
}

public class GenericEvent(string name) : RandomEvent(name);
